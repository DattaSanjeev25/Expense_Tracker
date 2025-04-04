# Expense Tracker Application

A full-stack expense tracking application built with Angular 19 and .NET 9.0.

## Features

- Transaction management (Create, Read, Update, Delete)
- JWT-based authentication with role-based access control
- Filter transactions by date, month, or year
- Dashboard with transaction analytics
- Responsive UI with tab-based navigation

## Tech Stack

### Frontend
- Angular 19
- TypeScript
- NgRx for state management
- Angular Material for UI components

### Backend
- .NET 9.0
- Entity Framework Core
- SQLite database
- JWT Authentication
- Swagger API documentation

## Project Structure

```
expense-tracker/
├── src/                    # Angular frontend application
│   ├── app/               # Application components
│   ├── assets/            # Static assets
│   └── environments/      # Environment configurations
└── ExpenseTracker.API/    # .NET backend application
    ├── Controllers/       # API controllers
    ├── Models/           # Data models
    ├── Services/         # Business logic
    └── Data/             # Database context and migrations
```

## Getting Started

### Prerequisites
- Node.js (v18 or later)
- .NET 9.0 SDK
- Angular CLI (v19)

### Backend Setup
1. Navigate to the backend directory:
   ```bash
   cd ExpenseTracker.API
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Run database migrations:
   ```bash
   dotnet ef database update
   ```
4. Start the API:
   ```bash
   dotnet run
   ```

### Frontend Setup
1. Navigate to the frontend directory:
   ```bash
   cd expense-tracker
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   ng serve
   ```

## API Documentation
Once the backend is running, access the Swagger documentation at:
```
https://localhost:7001/swagger
```

## Security
- JWT-based authentication
- Role-based access control (RBAC)
- Secure password hashing
- HTTPS enabled

## Deployment
Detailed deployment instructions can be found in the `DEPLOYMENT.md` file. 