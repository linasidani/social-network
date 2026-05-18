# Testning och coverage

Här beskriver jag hur jag har testat applikationen och vad som är kvar att göra.

## Översikt

| Del | Status |
|-----|--------|
| Funktionalitet | Alla 6 funktioner klara |
| Tester | 83 tester, alla passerar |
| Kodkvalitet | Följer Clean Code |
| Git-Flow | 8 branches mergade |
| Video | Klar |

## Hur jag har testat

Jag har jobbat med testdriven utveckling (TDD) och skrivit enhetstester för all affärslogik. Det blev totalt 83 tester som alla passerar.

### Tester för services

- **PostService** — testar validering, wall, timeline och radering
- **FollowService** — testar följ/avfölj, dubletter och cirkulära relationer
- **DirectMessageService** — testar validering, konversationer och inbox
- **UserService** — testar registrering, inloggning, hashning och tokens
- **AuthService** — testar PBKDF2-hashning och JWT-generering

### Tester för controllers

Integrationstester som kör hela request-flödet med en InMemory-databas. Mock-logger används för att undvika sidoeffekter.

## Köra tester med coverage

Så här kör du alla tester och genererar coverage-rapport:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Rapporten hamnar i `TestResults/coverage.cobertura.xml`

## Edge cases jag har testat

Dessa speciella fall är täckta:

- Cirkulära följ-relationer (A följer B, B följer A)
- Tomma meddelanden och meddelanden med bara mellanslag
- Försök att följa en användare som inte finns
- Dublettföljning (försök att följa samma person två gånger)
- Verifiering av PBKDF2-hashning

## Statisk kodanalys

Jag har aktiverat .NET analyzers i projektfilen:

```xml
<EnableNETAnalyzers>true</EnableNETAnalyzers>
<AnalysisMode>Recommended</AnalysisMode>
```

### Kända varningar

Det finns 14 varningar av typen CA1000 (statiska metoder på generiska typer). Detta gäller ServiceResult-klassen och är ett medvetet val — det är ett accepterat mönster för fabriksmetoder.

## Checklista inför inlämning

### Krav från uppgiften

Alla sex funktioner är klara:

- Posta inlägg — med validering av längd och användare
- Läsa tidslinje — i kronologisk ordning  
- Följa användare — via följ-knapp i frontend
- Vägg — visar egna inlägg plus inlägg från följda användare
- Direktmeddelanden — privata, syns inte i wall
- Persistens — all data sparas i SQL Server

### Testkrav

- 83 enhetstester som alla passerar
- Edge cases är täckta (cirkulära relationer, tomma meddelanden, dubletter)

### Kodkvalitet

- Tydliga namn (se ARKITECTURE.md)
- Små funktioner (max ~30 rader)
- Separerad logik (services vs controllers)

### Versionshantering

- 8 feature-branches mergade via pull requests
- Tydliga commit-meddelanden

### Videoinspelning

Videon är inspelad och innehåller:

- Demo av applikationen (alla funktioner visas)
- Kodgenomgång (arkitektur, Clean Code-principer)
- Körning av tester med coverage-resultat
- Git-historik (commits, branches, PRs)

Tidslängd: 10-15 minuter

### Länkar för inlämning

- GitHub Repository: https://github.com/linasidani/social-network
- GitHub Projects: https://github.com/linasidani/social-network/projects
- Video: (lägg till länk här)
