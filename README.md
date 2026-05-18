# SocialNetwork

A simple social network project with a .NET 9 Web API backend and a React/Vite frontend.

## Features implemented

- User registration and user listing
- Post creation and retrieval
- Follow/unfollow functionality
- Direct messages support
- Entity Framework Core with SQLite
- Swagger API docs for backend
- React frontend with routing and Axios integration
- Unit tests for API controllers using EF InMemory

## Backend

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
- The main frontend pages are `Feed` and `Register`.
- API endpoints are implemented for users, posts, follows, and direct messages.

## Tests

```bash
dotnet test SocialNetwork.Tests/SocialNetwork.Tests.csproj
```

This project includes controller-level tests using an in-memory EF Core provider.
