# KumariCinemas — Cinema Booking Management System

A full-stack web application for managing cinema bookings, built with **ASP.NET Web Forms** and **Oracle XE** as part of the CC6012 Data and Web Development coursework at London Metropolitan University / Islington College.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Database Schema](#database-schema)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Complex Queries](#complex-queries)
- [Screenshots](#screenshots)
- [Author](#author)

---

## Overview

KumariCinemas is a cinema management system that allows administrators to manage users, bookings, payments, movies, cities, theaters, halls, showtimes, pricing, shows and tickets through a clean web-based interface.

The database design follows a fully normalized **3NF schema** with **19 tables** (11 entity + 8 junction tables), designed from scratch through UNF → 1NF → 2NF → 3NF normalization steps.

---

## Features

### CRUD Management
| Module | Description |
|---|---|
| Users | Register and manage cinema users |
| Bookings | Create and track bookings per user |
| Payments | Record and manage payment records |
| Movies | Manage movie catalogue with genre and language |
| Cities | Manage cities where theaters operate |
| Theaters | Manage cinema theater brands |
| Halls | Manage screening halls per theater |
| Showtimes | Manage time slots (Morning, Afternoon, Evening, Night) |
| Pricing | Manage ticket pricing with holiday pricing support |
| Shows | Schedule movies in halls with showtimes and pricing |
| Tickets | Book and manage individual seat tickets |

### Complex Reports
| Report | Description |
|---|---|
| User Ticket Report | Show all paid tickets for a user within a 6-month period |
| Hall Movie Report | Show all movies and showtimes scheduled in a selected hall |
| Occupancy Performer | Show top 3 halls by seat occupancy % for a selected movie (paid tickets only) |

### Dashboard
- Live stat cards for Users, Bookings, Movies, Shows, Halls, Theaters, Tickets and Total Revenue
- Bar chart — Tickets sold per movie (Chart.js)
- Doughnut chart — Booking status breakdown (Chart.js)
- Recent bookings table

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | ASP.NET Web Forms (.aspx), HTML5, CSS3 |
| Backend | C# (.NET Framework 4.8) |
| Database | Oracle Database XE 21c |
| ORM / Driver | Oracle.ManagedDataAccess.Client (ODP.NET) |
| Charts | Chart.js (CDN) |
| Icons | Bootstrap Icons v1.11.3 (CDN) |
| IDE | Visual Studio 2022 |
| Version Control | Git / GitHub |

---

## Database Schema

The schema follows **3NF normalization** with 19 tables:

### Entity Tables (11)
```
User, Booking, Payment, Movie, City, Theater,
Hall, Showtime, Pricing, Show, Ticket
```

### Junction Tables (8)
```
User_Booking, Booking_Payment, Booking_Movie,
Movie_City, City_Theater, Theater_Hall,
Hall_Show, Show_Ticket
```

### Key Design Decisions
- All PKs use **Oracle Sequences** for auto-increment
- Junction tables resolve all **many-to-many** relationships
- `Pricing` table extracted from `Show` to remove transitive dependency (`IsHolidayPricing → Ticket_Price`)
- `Payment` table extracted from `Booking` to remove transitive dependency
- `Show` links directly to `Movie` as a direct FK (1 Show = 1 Movie)

---

## Project Structure

```
KumariCinemas/
│
├── Dashboard.aspx / .cs          # Main dashboard with charts and stats
│
├── ── CRUD Forms ──
├── UserForm.aspx / .cs           # User management
├── BookingForm.aspx / .cs        # Booking management
├── PaymentForm.aspx / .cs        # Payment management
├── MovieForm.aspx / .cs          # Movie catalogue
├── CityForm.aspx / .cs           # City management
├── TheaterForm.aspx / .cs        # Theater management
├── HallForm.aspx / .cs           # Hall management
├── ShowtimeForm.aspx / .cs       # Showtime slots
├── PricingForm.aspx / .cs        # Ticket pricing
├── ShowForm.aspx / .cs           # Show scheduling
├── TicketForm.aspx / .cs         # Ticket booking
│
├── ── Complex Query Forms ──
├── UserTicketForm.aspx / .cs     # Complex Query 1 — User tickets (6-month)
├── TheaterCityHallMovieForm.aspx / .cs  # Complex Query 2 — Hall movies
├── MovieOccupancyForm.aspx / .cs # Complex Query 3 — Top 3 occupancy
│
├── web.config                    # Oracle connection string + app settings
└── packages.config               # NuGet packages
```

---

## Getting Started

### Prerequisites
- Visual Studio 2022
- Oracle Database XE 21c
- Oracle SQL Developer
- .NET Framework 4.8

### 1. Clone the repository
```bash
git clone https://github.com/Abhimannu09dev/KumariCinemas.git
cd KumariCinemas
```

### 2. Set up Oracle Database
Open **Oracle SQL Developer** and run the following scripts in order:

```sql
-- Step 1: Create user/schema
CREATE USER kumaricinemas IDENTIFIED BY kumaricinemas;
GRANT CONNECT, RESOURCE, DBA TO kumaricinemas;

-- Step 2: Run DDL (creates all 19 tables, sequences, triggers, indexes)
-- File: KumariCinemas_DDL_Final.sql

-- Step 3: Run INSERT data
-- File: KumariCinemas_INSERT_Final.sql
```

### 3. Configure connection string
The `web.config` is already configured for local Oracle XE:
```xml
<add name="OracleConn"
     connectionString="Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)
     (HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)));
     User Id=kumari;Password=kumari;"
     providerName="Oracle.ManagedDataAccess.Client" />
```
Update `User Id` and `Password` if your Oracle schema uses different credentials.

### 4. Install NuGet Package
In Visual Studio: **Tools → NuGet Package Manager → Package Manager Console**
```
Install-Package Oracle.ManagedDataAccess
```

### 5. Run the application
Press **F5** or click **IIS Express** in Visual Studio. Navigate to:
```
http://localhost:[port]/Dashboard.aspx
```

---

##  Complex Queries

### Query 1 — User Ticket (6-Month Period)
```sql
SELECT t.Ticket_Id, u.Username, m.Title, s.ShowDate,
       st.Showtime_Name, p.Ticket_Price, h.HallName, th.TheaterName, c.CityName
FROM "User" u
JOIN Ticket t ON t.User_Id = u.User_Id
JOIN Show_Ticket stk ON stk.Ticket_Id = t.Ticket_Id
JOIN Show s ON s.Show_Id = stk.Show_Id
-- ... (full join chain through all junction tables)
WHERE u.User_Id = :p_userId
AND t.PaymentStatus = 'Paid'
AND s.ShowDate >= :p_dateFrom
AND s.ShowDate < ADD_MONTHS(:p_dateFrom, 6)
```

### Query 2 — TheaterCityHall Movie
```sql
SELECT h.HallName, t.TheaterName, c.CityName,
       m.Title, m.Genre, s.ShowDate, st.Showtime_Name
FROM Hall h
JOIN Theater_Hall thl ON thl.Hall_Id = h.Hall_Id
JOIN Theater t ON t.Theater_Id = thl.Theater_Id
-- ... (full join chain)
WHERE h.Hall_Id = :p_hallId
ORDER BY s.ShowDate, st.Showtime_Name
```

### Query 3 — Top 3 Hall Occupancy (Paid Tickets Only)
```sql
SELECT * FROM (
    SELECT th.TheaterName, c.CityName, h.HallName,
           h.HallCapacity, COUNT(tk.Ticket_Id) AS PaidTickets,
           ROUND((COUNT(tk.Ticket_Id) / h.HallCapacity) * 100, 2) AS OccupancyPercentage
    FROM Movie m
    JOIN Show s ON s.Movie_Id = m.Movie_Id
    -- ... (full join chain)
    JOIN Ticket tk ON tk.Ticket_Id = stk.Ticket_Id AND tk.PaymentStatus = 'Paid'
    WHERE m.Movie_Id = :p_movieId
    GROUP BY th.TheaterName, c.CityName, h.HallName, h.HallCapacity
    ORDER BY OccupancyPercentage DESC
) WHERE ROWNUM <= 3
```

---

##  Screenshots

<img width="900" alt="Dashboard" src="https://github.com/user-attachments/assets/fc5ab378-6378-43bd-8b2b-3b56d22f678a" />
<img width="900" alt="User Management" src="https://github.com/user-attachments/assets/5c523698-7665-4ca0-b6ff-d0d5af0f1184" />
<img width="900" alt="Booking Management" src="https://github.com/user-attachments/assets/62555dd9-a68d-462a-bc78-e6ef92173625" />


---

## Author

**Abhimannu Singh Kunwar**
- GitHub: [@Abhimannu09dev](https://github.com/Abhimannu09dev)

---

##  License

This project was created for academic purposes as part of CC6012 coursework at London Metropolitan University / Islington College.
