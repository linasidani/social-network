# Arkitekturbeskrivning

Här förklarar jag hur social network-applikationen är uppbyggd och vilka tekniska val jag har gjort.

## Vad jag har byggt

Det här är en fullstack-applikation där användare kan registrera sig, posta meddelanden, följa varandra och skicka privata meddelanden.

## Tekniker jag använder

| Del | Teknik | Varför |
|-----|--------|--------|
| Frontend | React 19 + Vite | Snabb, modern, lätt att arbeta med |
| Backend | .NET 9 Web API | Robusst, bra prestanda, typ-säkert |
| Databas | SQL Server i Docker | Enkel att köra lokalt, kraftfull |
| Databas-access | Entity Framework Core 9 | Slipp skriva SQL för hand |
| Inloggning | JWT tokens | Modern standard, stateless |
| Tester | xUnit + Moq | Standard i .NET-världen |

## Hur koden är organiserad

### Backend

Jag har delat upp koden i olika lager så det blir lättare att underhålla:

```
SocialNetwork.API/
├── Controllers/     # Tar emot HTTP-anrop och skickar svar
├── Services/        # Här händer allt viktiga (logik, regler, validering)
├── Data/            # Kopplingen till databasen
├── Models/          # Klasser som representerar tabeller i databasen
└── DTOs/            # Objekt jag skickar till/från frontend
```

**Varför denna uppdelning?**
- Controllers ska bara bry sig om HTTP (URL, statuskoder)
- Services innehåller all viktig logik (går att testa i isolation)
- Models är enkla data-klasser
- Data-lagret hanterar databasen

### Frontend

```
frontend/src/
├── components/      # React-komponenter (Feed, Users, Messages...)
├── services/        # Pratar med backend (apiService.js)
└── App.jsx          # Huvudfilen med navigering
```

## Viktiga mönster jag använder

### 1. Repository Pattern
EF Core fungerar som mitt "repository" — jag behöver inte skriva SQL själv. Services frågar databasen via LINQ.

### 2. Service Pattern
Varje del av appen har sin egen service-klass:
- `UserService` — registrering, inloggning, användarhantering
- `PostService` — skapa inlägg, hämta wall/timeline
- `FollowService` — följa och avfölja användare
- `DirectMessageService` — privata meddelanden
- `AuthService` — hashning av lösenord, skapa JWT-tokens

**Fördel:** Varje service har ett ansvar. Lätt att hitta koden och testa den.

### 3. DTO Pattern
Jag skickar inte rakt av mina databas-modeller till frontend. Istället skapar jag specifika objekt (DTOs) som bara innehåller det som behövs:
- `CreatePostDto` — vad som behövs för att skapa inlägg
- `PostDto` — vad frontend får se om ett inlägg
- `LoginUserDto` — inloggningsuppgifter

**Fördel:** Frontend behöver inte veta hur databasen ser ut. Jag kan ändra databasen utan att påverka frontend.

### 4. Result Pattern
Services returnerar alltid ett `ServiceResult<T>` istället för att kasta exceptions:

```csharp
// Så här kan det se ut:
return ServiceResult<PostDto>.Success(post);
return ServiceResult<PostDto>.NotFound("Användaren hittades inte");
return ServiceResult<PostDto>.BadRequest("Meddelandet får inte vara tomt");
```

**Fördel:** Tydliga felmeddelanden till användaren. Controller vet exakt vad som gick fel.

## Databasstruktur

### Tabeller och relationer

En användare kan ha:
- Många inlägg (som författare)
- Många inlägg på sin tidslinje
- Många följare
- Följa många andra
- Skicka och ta emot många privata meddelanden

Följ-relationen är en "många-till-många" mellan användare.

### Migreringar

Jag använder EF Core Code-First — jag skriver C#-klasser först och EF skapar databas-tabellerna:

```bash
# Skapa en ny migrering
dotnet ef migrations add NamnPåMigrering

# Uppdatera databasen
dotnet ef database update
```

## Inloggning och säkerhet

**Så här fungerar det:**
1. Användare registrerar sig → lösenord hashas med PBKDF2 (100 000 iterationer)
2. Vid inloggning kollas hash mot det sparade
3. Vid match skapas en JWT-token (giltig 60 minuter)
4. Frontend sparar token och skickar med vid varje anrop
5. Backend kollar token innan den svarar på skyddade endpoints

## Tester

Jag har skrivit **83 enhetstester** som kollar:
- Att validering fungerar (tomma meddelanden, för långa, osv)
- Att affärslogiken är korrekt (cirkulära följ-relationer, dubletter)
- Att edge cases hanteras (ogiltiga användare, saknade resurser)

Tester körs mot en "InMemory"-databas så de är snabba och isolerade.

## Säkerhetstänk

- **Lösenord** — hashas aldrig sparas i klartext
- **SQL injection** — EF Core använder parameteriserade queries
- **XSS** — React escapar automatiskt farlig output
- **CORS** — Begränsar vilka sidor som får prata med API:et
- **Input-validering** — All input kollas innan den når databasen

## Skalbarhet

API:et är **stateless** — jag sparar ingen info om användaren på servern mellan anrop. Allt finns i JWT-token. Det betyder att jag kan köra flera API-servrar bakom en load balancer om jag vill skala upp.

## Clean Code-principer

Baserat på Robert C. Martins bok "Clean Code". Här är de principer jag har följt:

### 1. Tydliga namn
Namn ska beskriva vad saker gör, inte hur:

```csharp
// Bra — man förstår direkt
public async Task<ServiceResult<PostDto>> CreateAsync(CreatePostDto dto, int authorId)

// Dåligt — för generiskt
public async Task<Result> Create(Input input, int id)
```

### 2. Små funktioner
Max ungefär 30 rader per funktion. Varje funktion gör **en sak**.

### 3. Undvik upprepning (DRY)
ServiceResult används av alla services istället för att skapa UserResult, PostResult, etc.

### 4. Separerad logik
Controllers hanterar bara HTTP. All affärslogik ligger i services.

### 5. Tydlig felhantering
ServiceResult-mönstret ger tydliga felmeddelanden utan att exponera stack traces.

### 6. Dependency Injection
Services får sina beroenden via constructor — lätt att testa och byta ut.

### 7. Ett ansvar per klass (SRP)
Varje service har ett ansvar: PostService för inlägg, FollowService för relationer, osv.

### 8. Testbar kod
83 enhetstester som kör på under 10 sekunder. Inga dolda beroenden eller global state.

### 9. Få kommentarer
Koden ska förklara sig själv. Enda kommentarerna är XML-docs för Swagger.

### 10. Konsekvens
Samma mönster överallt: CreateAsync, GetByIdAsync, ServiceResult<T>, osv.

| Princip | Status |
|---------|--------|
| Tydliga namn | Ja |
| Små funktioner | Ja |
| DRY | Ja |
| Separerad logik | Ja |
| Tydlig felhantering | Ja |
| Dependency Injection | Ja |
| SRP | Ja |
| Testbar kod | 83 tester |
| Minimala kommentarer | Ja |
| Konsekvens | Ja |
