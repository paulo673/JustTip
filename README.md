# JustTip

A full-stack Roster & Tip Management Application built with Clean Architecture. JustTip manages employee shifts and implements sophisticated daily tip splitting logic, ensuring fair distribution based on hours worked each day.

## Table of Contents

- [Purpose](#purpose)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Running with Docker (Recommended)](#running-with-docker-recommended)
  - [Running Locally](#running-locally)
- [Tip Calculation Logic](#tip-calculation-logic)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)
- [Testing](#testing)

## Purpose

JustTip solves the challenge of fair tip distribution in businesses where tip amounts vary significantly by day. Unlike simple weekly pooling systems that divide total tips by total hours, JustTip implements **Daily Tip Pools** that ensure employees working high-tip days (like Friday or Saturday) earn proportionally more than those working low-tip days.

### The Problem with Weekly Pooling

Consider this scenario:
- **Tuesday**: €50 in tips, Alice works 5 hours alone
- **Friday**: €500 in tips, Bob works 5 hours alone

With simple weekly pooling: Both earn €275 (€550 / 10 hours × 5 hours each)

With JustTip's daily pooling:
- Alice earns €50 (all Tuesday tips)
- Bob earns €500 (all Friday tips)

This accurately reflects the value each employee contributed on their respective days.

## Key Features

- **Weekly Roster Management**: View and manage employee shifts across a 7-day week
- **Shift Scheduling**: Create, update, and delete shifts with overlap validation
- **Daily Tip Entry**: Record tip amounts for each day of the week
- **Fair Tip Distribution**: Calculate payouts based on daily tip pools and hours worked
- **Retroactive Protection**: Prevent modifications to past shifts
- **Real-time Calculations**: Instant payout previews as you enter tips

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Backend** | .NET 10, ASP.NET Core Web API |
| **Frontend** | React 19, TypeScript, Vite |
| **UI Components** | ShadCN/UI, Tailwind CSS |
| **State Management** | TanStack Query |
| **Database** | PostgreSQL 15 |
| **ORM** | Entity Framework Core 10 |
| **Testing** | xUnit, NSubstitute |
| **Infrastructure** | Docker, Docker Compose |

## Architecture

JustTip follows a **Clean Architecture** pattern organized as a layered monolith:

```
┌─────────────────────────────────────────────────────────┐
│                    JustTip.Web                          │
│              (React Frontend - UI Layer)                │
└─────────────────────────┬───────────────────────────────┘
                          │ HTTP/REST
┌─────────────────────────▼───────────────────────────────┐
│                    JustTip.Api                          │
│           (ASP.NET Core - Presentation Layer)           │
└─────────────────────────┬───────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                   JustTip.Core                          │
│      (Domain Entities, Services, Interfaces)            │
│              Zero external dependencies                 │
└─────────────────────────┬───────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│               JustTip.Infrastructure                    │
│         (EF Core, Repositories, Data Access)            │
└─────────────────────────┬───────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                    PostgreSQL                           │
│                    (Database)                           │
└─────────────────────────────────────────────────────────┘
```

### Domain Entities

- **Employee**: Staff members with names and associated shifts
- **Shift**: Scheduled work periods with date, start time, and end time
- **DailyTipPool**: Tip amounts recorded per specific date

## Prerequisites

### For Docker Setup (Recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)

### For Local Development
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) with npm
- [PostgreSQL 15+](https://www.postgresql.org/download/) (or use Docker for database only)

## Getting Started

### Running with Docker (Recommended)

The easiest way to run JustTip is using Docker Compose, which starts all services (database, API, and web frontend) with a single command.

```bash
# Clone the repository
git clone https://github.com/paulo673/JustTip.git
cd JustTip

# Start all services
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

Once started, access the application:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/scalar/v1

To stop the services:
```bash
docker-compose down

# To also remove the database volume
docker-compose down -v
```

### Running Locally

#### 1. Start the Database

Using Docker (recommended):
```bash
docker-compose -f docker-compose-db.yml up -d
```

Or configure a local PostgreSQL instance with:
- Database: `justtip_db`
- Username: `postgres`
- Password: `postgres`
- Port: `5432`

#### 2. Run the Backend API

```bash
# Navigate to the API project
cd src/JustTip.Api

# Run the API (applies migrations automatically)
dotnet run
```

The API will be available at `http://localhost:5000`.

#### 3. Run the Frontend

```bash
# Navigate to the web project
cd src/JustTip.Web

# Install dependencies
npm install

# Start the development server
npm run dev
```

The frontend will be available at `http://localhost:3000`.

## Tip Calculation Logic

JustTip implements a **Daily Tip Pool** algorithm that ensures fair distribution based on actual daily contributions. This is the core differentiator from simple weekly pooling systems.

### Algorithm Overview

For each week, the system:

1. **Groups shifts by date** - All shifts are organized by their calendar date
2. **Calculates daily hourly rates** - For each day: `Hourly Rate = Daily Tips / Total Hours Worked That Day`
3. **Computes individual earnings** - For each employee on each day: `Daily Earning = Hourly Rate × Employee Hours That Day`
4. **Sums weekly payouts** - Each employee's weekly payout is the sum of their daily earnings

### Mathematical Formula

```
For each day d in the week:
    HourlyRate(d) = TipPool(d) / Σ(Hours worked by all employees on d)

For each employee e:
    DailyEarning(e, d) = HourlyRate(d) × Hours(e, d)
    WeeklyPayout(e) = Σ(DailyEarning(e, d)) for all days d
```

### Detailed Example

Consider a week with two employees, Alice and Bob:

| Day | Tips | Alice Hours | Bob Hours | Total Hours | Hourly Rate |
|-----|------|-------------|-----------|-------------|-------------|
| Monday | €100 | 5 | 3 | 8 | €12.50/hr   |
| Tuesday | €80  | 4 | 4 | 8 | €10.00/hr   |
| Friday | €300 | 0 | 8 | 8 | €37.50/hr   |
| Saturday | €400 | 6 | 6 | 12 | €33.33/hr   |

**Daily Earnings Calculation:**

*Alice:*
- Monday: €12.50 × 5 = €62.50
- Tuesday: €10.00 × 4 = €40.00
- Friday: €37.50 × 0 = €0.00
- Saturday: €33.33 × 6 = €200.00
- **Weekly Total: €302.50**

*Bob:*
- Monday: €12.50 × 3 = €37.50
- Tuesday: €10.00 × 4 = €40.00
- Friday: €37.50 × 8 = €300.00
- Saturday: €33.33 × 6 = €200.00
- **Weekly Total: €577.50**

**Comparison with Simple Weekly Pooling:**

If using simple weekly pooling (€880 total tips / 40 total hours = €22/hr):
- Alice: €22 × 15 hours = €330
- Bob: €22 × 25 hours = €550

Notice how daily pooling rewards Bob more for working the high-tip Friday, while simple pooling would undervalue his contribution.

### Edge Cases Handled

| Scenario | Behavior                                                       |
|----------|----------------------------------------------------------------|
| Day with tips but no shifts | Tips for that day are not distributed (preserved)              |
| Day with shifts but no tips | Employees earn €0 for that day but hours still count in totals |
| Multiple shifts per employee per day | Hours are summed before calculating that employee's share      |
| Overnight shifts | Duration correctly calculated across midnight                  |

### Rounding

All payout amounts are rounded to 2 decimal places using standard rounding (MidpointRounding.ToEven) to avoid accumulating rounding errors.

## API Endpoints

### Roster Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/roster?weekStart={date}` | Get weekly roster |
| GET | `/api/roster/{id}` | Get shift by ID |
| POST | `/api/roster` | Create a new shift |
| PUT | `/api/roster/{id}` | Update a shift |
| DELETE | `/api/roster/{id}` | Delete a shift |

### Tip Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tips/weekly?weekStart={date}` | Calculate weekly payouts |
| GET | `/api/tips/daily?weekStart={date}` | Get daily tip entries |
| POST | `/api/tips/daily` | Save daily tip entries |

### Employees

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/employees` | Get all employees |

## Project Structure

```
JustTip/
├── src/
│   ├── JustTip.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/          # API endpoints
│   │   ├── Program.cs            # Application entry point
│   │   └── Dockerfile
│   │
│   ├── JustTip.Core/             # Domain layer (no dependencies)
│   │   ├── DTOs/                 # Data transfer objects
│   │   ├── Entities/             # Domain entities
│   │   ├── Exceptions/           # Custom exceptions
│   │   ├── Interfaces/           # Repository & service contracts
│   │   └── Services/             # Business logic
│   │
│   ├── JustTip.Infrastructure/   # Data access layer
│   │   ├── Data/                 # DbContext & migrations
│   │   └── Repositories/         # Repository implementations
│   │
│   └── JustTip.Web/              # React frontend
│       ├── src/
│       │   ├── components/       # UI components
│       │   ├── hooks/            # Custom React hooks
│       │   ├── lib/              # Utilities & API client
│       │   └── types/            # TypeScript types
│       └── Dockerfile
│
├── tests/
│   └── JustTip.Tests/            # xUnit test project
│       └── Services/             # Service unit tests
│
├── docker-compose.yml            # Full stack orchestration
├── docker-compose-db.yml         # Database only
├── docker-compose-api.yml        # API only
├── docker-compose-ui.yml         # Frontend only
└── JustTip.sln                   # Solution file
```

## Testing

The project includes comprehensive unit tests for the tip calculation logic.

### Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

- **TipCalculationServiceTests**: Core tip distribution algorithm tests
- **RosterServiceOverlappingTests**: Shift overlap validation tests
- **ShiftOverlapScenariosTests**: Edge cases for shift scheduling

### Key Test Scenarios

- High-tip days pay more per hour than low-tip days
- Equal hours on same day results in equal pay
- Multiple shifts per employee per day are summed correctly
- Days without shifts don't affect tip distribution
- Proper rounding to 2 decimal places
