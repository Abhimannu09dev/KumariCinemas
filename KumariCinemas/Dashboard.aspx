<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="KumariCinemas.Dashboard" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css"/>
    <title>Dashboard - KumariCinemas</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: Arial, sans-serif; background: #f4f4f4; }

        /* ── Navbar ── */
        .navbar {
            background: #1B3A6B;
            padding: 0 24px;
            display: flex;
            align-items: center;
            gap: 4px;
            height: 52px;
            flex-wrap: wrap;
        }
        .navbar .brand {
            color: white;
            font-size: 1.1rem;
            font-weight: bold;
            margin-right: 16px;
            text-decoration: none;
        }
        .navbar a {
            color: rgba(255,255,255,0.85);
            text-decoration: none;
            padding: 5px 10px;
            border-radius: 4px;
            font-size: 0.82rem;
            transition: background 0.2s;
        }
        .navbar a:hover, .navbar a.active {
            background: rgba(255,255,255,0.2);
            color: white;
        }
        .navbar .nav-sep {
            color: rgba(255,255,255,0.3);
            font-size: 0.75rem;
            padding: 0 4px;
        }

        /* ── Page wrapper ── */
        .page { padding: 24px; max-width: 1300px; margin: 0 auto; }
        .page-title { font-size: 1.4rem; font-weight: bold; color: #333; margin-bottom: 6px; }
        .page-sub   { color: #888; font-size: 0.88rem; margin-bottom: 24px; }

        /* ── Stat cards ── */
        .cards {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
            gap: 14px;
            margin-bottom: 32px;
        }
        .card {
            background: white;
            border-radius: 8px;
            padding: 18px;
            border-left: 5px solid #1B3A6B;
            box-shadow: 0 1px 4px rgba(0,0,0,0.08);
            display: flex;
            flex-direction: column;
            gap: 5px;
            text-decoration: none;
            color: inherit;
            transition: box-shadow 0.2s, transform 0.2s;
        }
        .card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.14); transform: translateY(-2px); }
        .card-icon  { font-size: 1.5rem; }
        .card-count { font-size: 1.9rem; font-weight: bold; color: #1B3A6B; }
        .card-label { font-size: 0.8rem; color: #777; text-transform: uppercase; letter-spacing: 0.05em; }

        .card.teal   { border-left-color: #0A6B5E; } .card.teal   .card-count { color: #0A6B5E; }
        .card.orange { border-left-color: #C0550A; } .card.orange .card-count { color: #C0550A; }
        .card.green  { border-left-color: #27ae60; } .card.green  .card-count { color: #27ae60; }
        .card.purple { border-left-color: #8e44ad; } .card.purple .card-count { color: #8e44ad; }
        .card.red    { border-left-color: #c0392b; } .card.red    .card-count { color: #c0392b; }

        /* ── Charts row ── */
        .charts {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin-bottom: 32px;
        }
        .chart-box {
            background: white;
            border-radius: 8px;
            padding: 20px;
            box-shadow: 0 1px 4px rgba(0,0,0,0.08);
        }
        .chart-title {
            font-size: 0.95rem;
            font-weight: bold;
            color: #333;
            margin-bottom: 16px;
            padding-bottom: 8px;
            border-bottom: 2px solid #f4f4f4;
        }

        /* ── Recent bookings table ── */
        .section-title { font-size: 1rem; font-weight: bold; color: #333; margin-bottom: 12px; }
        .recent-table {
            width: 100%; border-collapse: collapse; background: white;
            border-radius: 8px; overflow: hidden;
            box-shadow: 0 1px 4px rgba(0,0,0,0.08);
        }
        .recent-table th { background: #1B3A6B; color: white; padding: 10px 14px; text-align: left; font-size: 0.85rem; }
        .recent-table td { padding: 9px 14px; border-bottom: 1px solid #f0f0f0; font-size: 0.85rem; }
        .recent-table tr:last-child td { border-bottom: none; }
        .recent-table tr:hover td { background: #f0f4fb; }

        .badge { display: inline-block; padding: 2px 8px; border-radius: 10px; font-size: 0.75rem; font-weight: bold; }
        .badge-confirmed { background: #d5f5e3; color: #1e8449; }
        .badge-cancelled { background: #fadbd8; color: #922b21; }
        .badge-pending   { background: #fef9e7; color: #b7950b; }

        @media (max-width: 700px) { .charts { grid-template-columns: 1fr; } }
    </style>
</head>
<body>

    <!-- ── Navbar ── -->
    <div class="navbar">
        <a href="Dashboard.aspx" class="brand">&#127916; KumariCinemas</a>
        <a href="Dashboard.aspx" class="active">Dashboard</a>
        <span class="nav-sep">|</span>
        <a href="UserForm.aspx">Users</a>
        <a href="BookingForm.aspx">Bookings</a>
        <a href="PaymentForm.aspx">Payments</a>
        <span class="nav-sep">|</span>
        <a href="MovieForm.aspx">Movies</a>
        <a href="CityForm.aspx">Cities</a>
        <a href="TheaterForm.aspx">Theaters</a>
        <a href="HallForm.aspx">Halls</a>
        <span class="nav-sep">|</span>
        <a href="ShowtimeForm.aspx">Showtimes</a>
        <a href="PricingForm.aspx">Pricing</a>
        <a href="ShowForm.aspx">Shows</a>
        <a href="TicketForm.aspx">Tickets</a>
        <span class="nav-sep">|</span>
        <a href="UserTicketForm.aspx">User Ticket</a>
        <a href="TheaterCityHallMovieForm.aspx">Hall Movie</a>
        <a href="MovieOccupancyForm.aspx">Occupancy</a>
    </div>

    <form id="form1" runat="server">
    <div class="page">

        <div class="page-title">Dashboard</div>
        <div class="page-sub">Welcome to KumariCinemas Management System — overview of all data</div>

        <!-- ── Stat Cards ── -->
        <div class="cards">
            <a href="UserForm.aspx" class="card green">
                <span class="card-icon"><i class="bi bi-person-fill"></i></span>
                <span class="card-count"><asp:Label ID="lblUsers" runat="server" Text="0" /></span>
                <span class="card-label">Users</span>
            </a>
            <a href="BookingForm.aspx" class="card">
                <span class="card-icon"><i class="bi bi-journal-bookmark-fill"></i></span>
                <span class="card-count"><asp:Label ID="lblBookings" runat="server" Text="0" /></span>
                <span class="card-label">Bookings</span>
            </a>
            <a href="MovieForm.aspx" class="card teal">
                <span class="card-icon"><i class="bi bi-camera-film"></i></span>
                <span class="card-count"><asp:Label ID="lblMovies" runat="server" Text="0" /></span>
                <span class="card-label">Movies</span>
            </a>
            <a href="ShowForm.aspx" class="card orange">
                <span class="card-icon"><i class="bi bi-film"></i></span>
                <span class="card-count"><asp:Label ID="lblShows" runat="server" Text="0" /></span>
                <span class="card-label">Shows</span>
            </a>
            <a href="HallForm.aspx" class="card purple">
                <span class="card-icon"><i class="bi bi-building-fill"></i></span>
                <span class="card-count"><asp:Label ID="lblHalls" runat="server" Text="0" /></span>
                <span class="card-label">Halls</span>
            </a>
            <a href="TheaterForm.aspx" class="card teal">
                <span class="card-icon"><i class="bi bi-shop"></i></span>
                <span class="card-count"><asp:Label ID="lblTheaters" runat="server" Text="0" /></span>
                <span class="card-label">Theaters</span>
            </a>
            <a href="TicketForm.aspx" class="card orange">
                <span class="card-icon"><i class="bi bi-ticket-fill"></i></span>
                <span class="card-count"><asp:Label ID="lblTickets" runat="server" Text="0" /></span>
                <span class="card-label">Tickets</span>
            </a>
            <a href="PaymentForm.aspx" class="card green">
                <span class="card-icon"><i class="bi bi-credit-card-fill"></i></span>
                <span class="card-count">NPR <asp:Label ID="lblRevenue" runat="server" Text="0" /></span>
                <span class="card-label">Total Revenue</span>
            </a>
        </div>

        <!-- ── Charts ── -->
        <div class="charts">
            <div class="chart-box">
                <div class="chart-title"><i class="bi bi-bar-chart-fill"></i> Tickets Sold per Movie</div>
                <canvas id="movieChart" height="220"></canvas>
            </div>
            <div class="chart-box">
                <div class="chart-title"><i class="bi bi-pie-chart-fill"></i> Booking Status Breakdown</div>
                <canvas id="statusChart" height="220"></canvas>
            </div>
        </div>

        <!-- ── Recent Bookings ── -->
        <div class="section-title"><i class="bi bi-clock-history"></i> Recent Bookings</div>
        <asp:GridView ID="gvRecent" runat="server"
            AutoGenerateColumns="false"
            CssClass="recent-table"
            EmptyDataText="No bookings yet.">
            <Columns>
                <asp:BoundField DataField="Booking_Id"    HeaderText="ID"      />
                <asp:BoundField DataField="Username"      HeaderText="User"    />
                <asp:BoundField DataField="Movie_Title"   HeaderText="Movie"   />
                <asp:BoundField DataField="BookingDate"   HeaderText="Date"    DataFormatString="{0:dd-MMM-yyyy}" />
                <asp:BoundField DataField="BookingStatus" HeaderText="Status"  />
            </Columns>
        </asp:GridView>

        <asp:HiddenField ID="hfMovieLabels"  runat="server" />
        <asp:HiddenField ID="hfMovieCounts"  runat="server" />
        <asp:HiddenField ID="hfStatusLabels" runat="server" />
        <asp:HiddenField ID="hfStatusCounts" runat="server" />

    </div>
    </form>

    <script>
        // ── Bar chart: tickets sold per movie
        var movieLabels = document.getElementById('<%= hfMovieLabels.ClientID %>').value.split('|');
        var movieCounts = document.getElementById('<%= hfMovieCounts.ClientID %>').value.split('|').map(Number);

        new Chart(document.getElementById('movieChart'), {
            type: 'bar',
            data: {
                labels: movieLabels,
                datasets: [{
                    label: 'Tickets Sold',
                    data: movieCounts,
                    backgroundColor: '#0A6B5E',
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
            }
        });

        // ── Doughnut chart: booking status breakdown
        var statusLabels = document.getElementById('<%= hfStatusLabels.ClientID %>').value.split('|');
        var statusCounts = document.getElementById('<%= hfStatusCounts.ClientID %>').value.split('|').map(Number);

        new Chart(document.getElementById('statusChart'), {
            type: 'doughnut',
            data: {
                labels: statusLabels,
                datasets: [{
                    data: statusCounts,
                    backgroundColor: ['#1B3A6B', '#C0550A', '#0A6B5E'],
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { position: 'bottom' } }
            }
        });
    </script>

</body>
</html>
