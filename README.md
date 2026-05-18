# Social Network

En social network-applikation med .NET 9 Web API backend och React/Vite frontend.

## Funktioner

- **Registrering och inloggning** — JWT-baserad autentisering med säker lösenordshashning
- **Posta inlägg** — Skapa meddelanden på egen eller andras tidslinje
- **Tidslinje** — Se alla inlägg på en specifik användares profil
- **Följa/avfölja** — Följ andra användare och se deras inlägg i din wall
- **Wall** — Aggregat-flöde med egna inlägg och inlägg från följda användare
- **Direktmeddelanden** — Privata meddelanden mellan användare (inte synliga i wall)
- **Persistens** — SQL Server med Entity Framework Core

## Teknisk dokumentation

- [ARKITECTURE.md](ARKITECTURE.md) — Beskrivning av arkitektur och designmönster
- [TESTING.md](TESTING.md) — Teststrategi, coverage och checklista

## Snabbstart

### Backend

Starta SQL Server med Docker:

```bash
docker run -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD=Your_password123 \
  -p 1433:1433 \
  --name socialnetwork-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Applicera databasmigrering:

```bash
cd SocialNetwork.API
dotnet ef database update
```

```bash
cd SocialNetwork.API
dotnet restore
dotnet run
```

Backend körs på `http://localhost:5001` med API-routes under `/api`.
Swagger-dokumentation: `http://localhost:5001/swagger`

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend körs på `http://localhost:5173`.

## Tester

Kör alla tester (inklusive coverage):

```bash
dotnet test --collect:"XPlat Code Coverage"
```

83 enhetstester för service-lagret (TDD) + integrationstester för controllers.
Coverage-rapport genereras i `TestResults/coverage.cobertura.xml`
