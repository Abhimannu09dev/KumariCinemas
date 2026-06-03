# 🎬 KumariCinemas — Cinema Booking Management System

[![C#](https://img.shields.io/badge/C%23-.NET_4.8-239120?logo=csharp&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET](https://img.shields.io/badge/ASP.NET-Web_Forms-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/web-forms/)
[![Oracle](https://img.shields.io/badge/Oracle-XE_21c-F80000?logo=oracle&logoColor=white)](https://www.oracle.com/database/technologies/xe-downloads.html)
[![Chart.js](https://img.shields.io/badge/Chart.js-4.x-FF6384?logo=chartdotjs&logoColor=white)](https://www.chartjs.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.txt)

> Built for **CC6012 Data and Web Development** — London Metropolitan University / Islington College

---

## 📖 Project Description

**KumariCinemas** is a full-stack web-based cinema booking management system that allows administrators to manage the entire operation of a cinema chain — from cities and theaters down to individual seat tickets — through a clean, data-driven web interface.

The system covers 11 CRUD modules spanning the full booking lifecycle (Users → Bookings → Payments → Movies → Cities → Theaters → Halls → Showtimes → Pricing → Shows → Tickets), three complex SQL reports with multi-table joins, and a live dashboard with Chart.js visualisations.

### What makes it technically significant

The database was designed from scratch through a full normalisation process — **UNF → 1NF → 2NF → 3NF** — resulting in a **19-table schema** (11 entity tables + 8 junction tables) with Oracle Sequences for all PKs, proper FK constraints throughout, and documented justification for every transitive dependency that was extracted into its own table.

---

## ✨ Features

### 📋 CRUD Management (11 Modules)

| Module | Description |
|---|---|
| **Users** | Register and manage cinema customer accounts |
| **Bookings** | Create and track bookings per user with status (Confirmed / Pending / Cancelled) |
| **Payments** | Record and manage payment records linked to bookings |
| **Movies** | Manage the full movie catalogue with genre, language, and duration |
| **Cities** | Manage cities where the cinema chain operates |
| **Theaters** | Manage cinema theater brands across cities |
| **Halls** | Manage individual screening halls per theater with capacity |
| **Showtimes** | Manage named time slots (Morning, Afternoon, Evening, Night) |
| **Pricing** | Manage ticket pricing including holiday pricing support |
| **Shows** | Schedule movies into halls with a showtime, date, and pricing plan |
| **Tickets** | Book individual seat tickets with payment status tracking |

---

### 📊 Complex SQL Reports (3 Queries)

| Report | Description |
|---|---|
| **User Ticket Report** | All paid tickets for a selected user within a 6-month window — full join chain across User → Ticket → Show → Movie → Hall → Theater → City |
| **Hall Movie Report** | All movies and showtimes currently scheduled in a selected hall — full join chain across Hall → Theater → City → Show → Movie → Showtime |
| **Top 3 Hall Occupancy** | Top 3 halls by seat occupancy % for a selected movie (paid tickets only) — uses `ROWNUM <= 3` with `ROUND((PaidTickets / HallCapacity) * 100, 2)` |

---

### 📈 Dashboard

- **8 Live Stat Cards** — total count for Users, Bookings, Movies, Shows, Halls, Theaters, Tickets, and **Total Revenue**
- **Bar Chart** — Tickets sold per movie (Chart.js)
- **Doughnut Chart** — Booking status breakdown: Confirmed / Pending / Cancelled (Chart.js)
- **Recent Bookings Table** — Latest bookings with user, movie, and status at a glance

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Language** | C# (.NET Framework 4.8) |
| **Frontend / View** | ASP.NET Web Forms (.aspx) + HTML5 + CSS3 |
| **Database** | Oracle Database XE 21c |
| **DB Driver / ORM** | Oracle.ManagedDataAccess.Client (ODP.NET) |
| **Charts** | Chart.js (CDN) |
| **Icons** | Bootstrap Icons v1.11.3 (CDN) |
| **IDE** | Visual Studio 2022 |
| **Version Control** | Git / GitHub |

---

## 🏗️ Architecture Overview

KumariCinemas uses the **ASP.NET Web Forms code-behind pattern** — each `.aspx` page has a paired `.aspx.cs` code-behind file that handles both the business logic and direct Oracle data access via ODP.NET.

```
Browser (HTTP Request)
        │
        ▼
┌──────────────────────────────────────────────────┐
│           IIS / IIS Express (ASP.NET)            │
│                                                  │
│  ┌───────────────────────────────────────────┐   │
│  │  .aspx  (View — HTML + Web Controls)      │   │
│  │  DropDownList, GridView, TextBox, Button  │   │
│  └───────────────────┬───────────────────────┘   │
│                      │  PostBack event            │
│  ┌───────────────────▼───────────────────────┐   │
│  │  .aspx.cs  (Code-Behind Controller)       │   │
│  │  - Page_Load()                            │   │
│  │  - Button_Click event handlers            │   │
│  │  - OracleConnection / OracleCommand       │   │
│  │  - OracleDataReader / DataAdapter         │   │
│  └───────────────────┬───────────────────────┘   │
└──────────────────────│───────────────────────────┘
                       │  ODP.NET (JDBC-style)
                       ▼
              ┌─────────────────────┐
              │  Oracle XE 21c      │
              │  19 tables          │
              │  Sequences (PKs)    │
              │  FK constraints     │
              └─────────────────────┘
```

---

## 🗄️ Database Schema

The schema was designed through a complete normalisation process:

```
UNF (raw flat data)
 └── 1NF (atomic values, no repeating groups)
      └── 2NF (removed partial dependencies)
           └── 3NF (removed transitive dependencies → Pricing, Payment extracted)
```

### Entity Tables (11)

```
User          Booking       Payment       Movie
City          Theater       Hall          Showtime
Pricing       Show          Ticket
```

### Junction Tables (8)

All many-to-many relationships are resolved through explicit junction tables:

```
User_Booking      Booking_Payment     Booking_Movie
Movie_City        City_Theater        Theater_Hall
Hall_Show         Show_Ticket
```

### Key Design Decisions

| Decision | Justification |
|---|---|
| `Pricing` extracted from `Show` | Removes transitive dependency: `IsHolidayPricing → Ticket_Price` has nothing to do with `Show_Id` |
| `Payment` extracted from `Booking` | Removes transitive dependency: payment details depend on `Payment_Id`, not `Booking_Id` |
| `Show` has a direct FK to `Movie` | One show = one movie. The `Hall_Show` junction handles the Hall → Show relationship |
| All PKs use Oracle Sequences | No `IDENTITY` column equivalent in Oracle XE without sequences |
| Junction tables for every M2M | Enforces referential integrity at the DB level rather than relying on application logic |

### Schema Diagram (simplified)

```
User ──< User_Booking >── Booking ──< Booking_Payment >── Payment
                              │
                              └──< Booking_Movie >── Movie ──< Movie_City >── City ──< City_Theater >── Theater
                                                                                                              │
                                                                                                   Theater_Hall >── Hall
                                                                                                              │
                                                                                                       Hall_Show >── Show ──── Movie (direct FK)
                                                                                                                         │
                                                                                                                  Show_Ticket >── Ticket ──── User (direct FK)
                                                                                                                                      │
                                                                                                                                   Pricing (FK on Show)
```

### Oracle Sequence Pattern

Every table uses an Oracle Sequence for its primary key:

```sql
-- Example: User table
CREATE SEQUENCE user_seq START WITH 1 INCREMENT BY 1;

CREATE TABLE "User" (
    User_Id       NUMBER DEFAULT user_seq.NEXTVAL PRIMARY KEY,
    Username      VARCHAR2(100) NOT NULL UNIQUE,
    Email         VARCHAR2(150) NOT NULL UNIQUE,
    PhoneNumber   VARCHAR2(20),
    CreatedAt     DATE DEFAULT SYSDATE
);
```

---

## 📁 Project Structure

```
KumariCinemas/
│
├── KumariCinemas.slnx                      # Visual Studio solution file
│
└── KumariCinemas/                          # ASP.NET Web Forms project
    │
    ├── Dashboard.aspx / .cs               # Main dashboard (stat cards + Chart.js)
    │
    ├── ── CRUD Forms (11 modules) ──
    ├── UserForm.aspx / .cs                # User management
    ├── BookingForm.aspx / .cs             # Booking management
    ├── PaymentForm.aspx / .cs             # Payment management
    ├── MovieForm.aspx / .cs               # Movie catalogue
    ├── CityForm.aspx / .cs                # City management
    ├── TheaterForm.aspx / .cs             # Theater management
    ├── HallForm.aspx / .cs                # Hall management
    ├── ShowtimeForm.aspx / .cs            # Showtime slot management
    ├── PricingForm.aspx / .cs             # Ticket pricing (incl. holiday)
    ├── ShowForm.aspx / .cs                # Show scheduling
    ├── TicketForm.aspx / .cs              # Individual ticket booking
    │
    ├── ── Complex Report Forms (3 queries) ──
    ├── UserTicketForm.aspx / .cs          # Report 1 — User tickets (6-month)
    ├── TheaterCityHallMovieForm.aspx / .cs # Report 2 — Hall movies & showtimes
    ├── MovieOccupancyForm.aspx / .cs      # Report 3 — Top 3 halls by occupancy %
    │
    ├── web.config                         # Oracle connection string
    ├── packages.config                    # NuGet package list
    └── Site.Master / Site.Master.cs       # Shared master page layout
```

---

## ⚙️ Installation Guide

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **ASP.NET and web development** workload
- [Oracle Database XE 21c](https://www.oracle.com/database/technologies/xe-downloads.html)
- [Oracle SQL Developer](https://www.oracle.com/database/sqldeveloper/technologies/download/) (for running SQL scripts)
- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) (usually pre-installed on Windows)

---

### 1. Clone the repository

```bash
git clone https://github.com/Abhimannu09dev/KumariCinemas.git
cd KumariCinemas
```

---

### 2. Set up Oracle Database

Open **Oracle SQL Developer**, connect to your Oracle XE instance, and run the following in order:

**Step 1 — Create the schema user:**
```sql
CREATE USER kumaricinemas IDENTIFIED BY kumaricinemas;
GRANT CONNECT, RESOURCE, DBA TO kumaricinemas;
```

**Step 2 — Run the DDL script** (creates all 19 tables, sequences, constraints, and indexes):
```sql
-- Run: KumariCinemas_DDL_Final.sql
```

**Step 3 — Run the INSERT script** (populates tables with seed data):
```sql
-- Run: KumariCinemas_INSERT_Final.sql
```

---

### 3. Configure the connection string

Open `web.config` — it is already configured for a local Oracle XE instance:

```xml
<add name="OracleConn"
     connectionString="Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)
     (HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)));
     User Id=kumari;Password=kumari;"
     providerName="Oracle.ManagedDataAccess.Client" />
```

Update `User Id` and `Password` if your Oracle schema uses different credentials.

---

### 4. Install the Oracle NuGet package

In Visual Studio: **Tools → NuGet Package Manager → Package Manager Console**

```powershell
Install-Package Oracle.ManagedDataAccess
```

Or restore all packages automatically:

```bash
nuget restore KumariCinemas.slnx
```

---

### 5. Run the application

Press **F5** in Visual Studio (or click **IIS Express**). Navigate to:

```
http://localhost:[port]/Dashboard.aspx
```

> **💡 Tip:** Make sure Oracle XE is running before starting the app. If Oracle service is stopped, open **Services** (Windows) and start `OracleServiceXE`.

---

## 🧭 Application Pages

| Page | Description |
|---|---|
| `Dashboard.aspx` | Live stats, Chart.js charts, recent bookings table |
| `UserForm.aspx` | Create, view, update, delete users |
| `BookingForm.aspx` | Create and manage bookings with status tracking |
| `PaymentForm.aspx` | Record payments linked to bookings |
| `MovieForm.aspx` | Movie catalogue — title, genre, language, duration |
| `CityForm.aspx` | Cities where the cinema chain operates |
| `TheaterForm.aspx` | Theater brands and their city association |
| `HallForm.aspx` | Screening halls within theaters — capacity and name |
| `ShowtimeForm.aspx` | Named time slots (Morning / Afternoon / Evening / Night) |
| `PricingForm.aspx` | Ticket pricing with holiday pricing flag |
| `ShowForm.aspx` | Schedule a movie into a hall with showtime and pricing |
| `TicketForm.aspx` | Book a ticket — links user, show, and payment status |
| `UserTicketForm.aspx` | **Report 1** — Paid tickets for a user over a 6-month window |
| `TheaterCityHallMovieForm.aspx` | **Report 2** — All movies and showtimes in a selected hall |
| `MovieOccupancyForm.aspx` | **Report 3** — Top 3 halls by seat occupancy % for a movie |

---

## 🧩 Complex SQL Queries

### Query 1 — User Ticket Report (6-Month Period)

Returns all paid tickets for a selected user within a 6-month window from a chosen start date. Joins through the full booking and show chain.

```sql
SELECT t.Ticket_Id, u.Username, m.Title, s.ShowDate,
       st.Showtime_Name, p.Ticket_Price, h.HallName,
       th.TheaterName, c.CityName
FROM "User" u
JOIN Ticket t       ON t.User_Id = u.User_Id
JOIN Show_Ticket stk ON stk.Ticket_Id = t.Ticket_Id
JOIN Show s         ON s.Show_Id = stk.Show_Id
JOIN Hall_Show hs   ON hs.Show_Id = s.Show_Id
JOIN Hall h         ON h.Hall_Id = hs.Hall_Id
JOIN Theater_Hall thl ON thl.Hall_Id = h.Hall_Id
JOIN Theater th     ON th.Theater_Id = thl.Theater_Id
JOIN City_Theater ct ON ct.Theater_Id = th.Theater_Id
JOIN City c         ON c.City_Id = ct.City_Id
JOIN Movie m        ON m.Movie_Id = s.Movie_Id
JOIN Pricing p      ON p.Pricing_Id = s.Pricing_Id
JOIN Showtime st    ON st.Showtime_Id = s.Showtime_Id
WHERE u.User_Id = :p_userId
  AND t.PaymentStatus = 'Paid'
  AND s.ShowDate >= :p_dateFrom
  AND s.ShowDate <  ADD_MONTHS(:p_dateFrom, 6)
ORDER BY s.ShowDate
```

---

### Query 2 — Hall Movie Report

Returns all movies and their showtimes currently scheduled in a selected hall, with the full location chain (Hall → Theater → City).

```sql
SELECT h.HallName, t.TheaterName, c.CityName,
       m.Title, m.Genre, s.ShowDate, st.Showtime_Name
FROM Hall h
JOIN Theater_Hall thl ON thl.Hall_Id = h.Hall_Id
JOIN Theater t        ON t.Theater_Id = thl.Theater_Id
JOIN City_Theater ct  ON ct.Theater_Id = t.Theater_Id
JOIN City c           ON c.City_Id = ct.City_Id
JOIN Hall_Show hs     ON hs.Hall_Id = h.Hall_Id
JOIN Show s           ON s.Show_Id = hs.Show_Id
JOIN Movie m          ON m.Movie_Id = s.Movie_Id
JOIN Showtime st      ON st.Showtime_Id = s.Showtime_Id
WHERE h.Hall_Id = :p_hallId
ORDER BY s.ShowDate, st.Showtime_Name
```

---

### Query 3 — Top 3 Hall Occupancy (Paid Tickets Only)

Ranks all halls showing a selected movie by seat occupancy percentage (paid tickets / hall capacity × 100), returning only the top 3 using Oracle's `ROWNUM`.

```sql
SELECT * FROM (
    SELECT th.TheaterName, c.CityName, h.HallName,
           h.HallCapacity,
           COUNT(tk.Ticket_Id) AS PaidTickets,
           ROUND((COUNT(tk.Ticket_Id) / h.HallCapacity) * 100, 2) AS OccupancyPercentage
    FROM Movie m
    JOIN Show s         ON s.Movie_Id = m.Movie_Id
    JOIN Hall_Show hs   ON hs.Show_Id = s.Show_Id
    JOIN Hall h         ON h.Hall_Id = hs.Hall_Id
    JOIN Theater_Hall thl ON thl.Hall_Id = h.Hall_Id
    JOIN Theater th     ON th.Theater_Id = thl.Theater_Id
    JOIN City_Theater ct  ON ct.Theater_Id = th.Theater_Id
    JOIN City c         ON c.City_Id = ct.City_Id
    JOIN Show_Ticket stk  ON stk.Show_Id = s.Show_Id
    JOIN Ticket tk      ON tk.Ticket_Id = stk.Ticket_Id
                       AND tk.PaymentStatus = 'Paid'
    WHERE m.Movie_Id = :p_movieId
    GROUP BY th.TheaterName, c.CityName, h.HallName, h.HallCapacity
    ORDER BY OccupancyPercentage DESC
)
WHERE ROWNUM <= 3
```

---

## 📊 Dashboard Charts

The dashboard uses **Chart.js** (loaded via CDN) rendered inside `<canvas>` elements on `Dashboard.aspx`. Data is fetched server-side via ODP.NET and serialised to JSON directly in the code-behind, then injected into the page:

**Bar Chart — Tickets sold per movie:**
Groups `Ticket` records by `Movie.Title`, counts total tickets, and renders a bar per movie.

**Doughnut Chart — Booking status breakdown:**
Groups `Booking` records by `BookingStatus` (Confirmed / Pending / Cancelled) and renders three segments with percentage labels.

---

## 📸 Screenshots

| View | Description |
|---|---|
| ![Dashboard](https://private-user-images.githubusercontent.com/155934864/566023776-fc5ab378-6378-43bd-8b2b-3b56d22f678a.png) | Main dashboard — stat cards, revenue total, Chart.js charts, recent bookings |
| ![User Management](https://private-user-images.githubusercontent.com/155934864/566023958-5c523698-7665-4ca0-b6ff-d0d5af0f1184.png) | User CRUD form with GridView table |
| ![Booking Management](https://private-user-images.githubusercontent.com/155934864/566024148-62555dd9-a68d-462a-bc78-e6ef92173625.png) | Booking management interface with status tracking |

---

## 🚀 Future Improvements

- [ ] Add user authentication and login — currently no session/auth layer
- [ ] Build a public-facing customer portal for self-service booking
- [ ] Add online seat selection UI (visual seating map)
- [ ] Integrate a payment gateway (Khalti / eSewa for Nepal market)
- [ ] Migrate from Web Forms to ASP.NET Core Razor Pages or MVC
- [ ] Replace Oracle XE with a cloud-hosted database (Oracle Autonomous DB / PostgreSQL)
- [ ] Add server-side pagination to all GridView tables
- [ ] Export reports to PDF / Excel

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Follow the existing Web Forms code-behind pattern
4. Test against a local Oracle XE instance before submitting
5. Commit: `git commit -m "feat: add your feature"`
6. Open a Pull Request

---

## 📄 License

This project is licensed under the **MIT License** — see [LICENSE.txt](./LICENSE.txt) for details.

---

## 👨‍💻 Author

**Abhimannu Singh Kunwar**
BSc Computing — Islington College (London Metropolitan University)
[GitHub](https://github.com/Abhimannu09dev) · [LinkedIn](https://www.linkedin.com/in/abhimannu-singh-kunwar-5a9096268/)

> *Built with ASP.NET Web Forms and Oracle XE 21c for CC6012 Data and Web Development — Islington College / London Metropolitan University.*
