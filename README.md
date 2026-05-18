# SocialNetwork

A simple social network project with a .NET 9 Web API backend and a React/Vite frontend.

## Features implemented

- User registration and user listing
- Post creation and retrieval, including posting on another user's timeline
- Aggregated wall feed based on followed users
- Follow/unfollow functionality stored in a relation table
- Direct messages support with inbox retrieval
- Entity Framework Core with SQL Server persistence
- Swagger API docs for backend
- React frontend with routing and Axios integration
- Unit tests for API controllers using EF InMemory

## Backend

Start SQL Server with Docker:

```bash
docker run -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD=Your_password123 \
  -p 1433:1433 \
  --name socialnetwork-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Apply the database migration:

```bash
cd SocialNetwork.API
dotnet ef database update
```

```bash
cd SocialNetwork.API
dotnet restore
dotnet run
```

The backend runs on `http://localhost:5000` by default, with API routes under `/api`.

## Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend runs on `http://localhost:5173` by default.

## Notes

- The frontend expects the backend to be available at `http://localhost:5000/api`.
- The main frontend pages are `Wall`, `Post`, `Users`, `Messages`, and `Register`.
- API endpoints are implemented for users, posts, follows, and direct messages.

## Tests

```bash
dotnet test SocialNetwork.Tests/SocialNetwork.Tests.csproj
```

This project includes controller-level tests using an in-memory EF Core provider.
