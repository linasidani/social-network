# Teststrategi och resultat

## Testtäckning

Projektet använder **xUnit** för enhetstester med fokus på:

### Service-lager (TDD)
- **PostService** — validering, wall, timeline, radering
- **FollowService** — följ/avfölj, dubletter, cirkulära relationer
- **DirectMessageService** — validering, konversationer, inbox
- **UserService** — registrering, login, hashning, tokens
- **AuthService** — PBKDF2-hashning, JWT-generering

### Controller-tester
- Integrationstester med EF InMemory
- API-endpoints validering

## Kör tester med coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Edge cases täckta

- ✅ Cirkulära "följ"-relationer (A↔B)
- ✅ Tomma meddelanden och whitespace
- ✅ Ogiltiga användare vid inlägg/DM
- ✅ Dublettföljning
- ✅ Lösenordshashning med PBKDF2

## Statisk kodanalys

.NET analyzers aktiverade i `SocialNetwork.API.csproj`:
```xml
<EnableNETAnalyzers>true</EnableNETAnalyzers>
<AnalysisMode>Recommended</AnalysisMode>
```

### Kända varningar
- CA1000: Statiska metoder på generiska typer (ServiceResult-fabriker) — accepterat mönster
