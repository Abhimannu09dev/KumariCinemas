<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MovieOccupancyForm.aspx.cs" Inherits="KumariCinemas.MovieOccupancyForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css"/>
    <title>Movie Occupancy Report - KumariCinemas</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: Arial, sans-serif; background: #f4f4f4; }
        .navbar { background: #1B3A6B; padding: 0 24px; display: flex; align-items: center; gap: 4px; height: 52px; flex-wrap: wrap; }
        .navbar .brand { color: white; font-size: 1.1rem; font-weight: bold; margin-right: 16px; text-decoration: none; }
        .navbar a { color: rgba(255,255,255,0.85); text-decoration: none; padding: 5px 10px; border-radius: 4px; font-size: 0.82rem; }
        .navbar a:hover, .navbar a.active { background: rgba(255,255,255,0.2); color: white; }
        .navbar .nav-sep { color: rgba(255,255,255,0.3); font-size: 0.75rem; padding: 0 4px; }
        .page { padding: 24px; max-width: 1200px; margin: 0 auto; }
        h2 { color: #1B3A6B; margin-bottom: 6px; }
        .page-sub { color: #888; font-size: 0.88rem; margin-bottom: 24px; }

        .filter-box { background: white; padding: 24px; margin-bottom: 24px; border: 1px solid #ddd; border-radius: 8px; box-shadow: 0 1px 4px rgba(0,0,0,0.07); }
        .filter-box h3 { margin-top: 0; color: #1B3A6B; margin-bottom: 16px; font-size: 1rem; }
        .filter-row { display: flex; gap: 16px; align-items: flex-end; flex-wrap: wrap; }
        .filter-field { display: flex; flex-direction: column; gap: 4px; }
        .filter-field label { font-weight: bold; font-size: 0.88rem; }
        .filter-field select { padding: 7px 10px; border: 1px solid #ccc; border-radius: 4px; font-size: 0.9rem; min-width: 260px; }
        .filter-field select:focus { outline: none; border-color: #1B3A6B; }

        .btn-search { background: #1B3A6B; color: white; padding: 8px 24px; border: none; cursor: pointer; border-radius: 4px; font-size: 0.9rem; }
        .btn-search:hover { background: #15305a; }
        .btn-clear  { background: #7f8c8d; color: white; padding: 8px 20px; border: none; cursor: pointer; border-radius: 4px; font-size: 0.9rem; }

        .msg-error { color: red; font-weight: bold; margin: 10px 0; display: block; }
        .result-header { font-size: 1rem; font-weight: bold; color: #333; margin-bottom: 10px; }

        /* Podium styling for top-3 */
        .grid-style { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 4px rgba(0,0,0,0.07); }
        .grid-style th { background: #C0550A; color: white; padding: 10px 12px; text-align: left; font-size: 0.85rem; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; font-size: 0.85rem; }
        .grid-style tr:last-child td { border-bottom: none; }
        .grid-style tr:nth-child(1) td { background: #fff8f0; font-weight: bold; font-size: 0.9rem; }
        .grid-style tr:nth-child(2) td { background: #fdfcfb; }
        .grid-style tr:nth-child(3) td { background: #fdfcfb; }

        .rank-1 { color: #C0550A; font-size: 1.3rem; }
        .rank-2 { color: #888;    font-size: 1.1rem; }
        .rank-3 { color: #cd7f32; font-size: 1.1rem; }

        .occupancy-bar { display: inline-block; height: 14px; background: #C0550A; border-radius: 3px; vertical-align: middle; margin-right: 6px; }
    </style>
</head>
<body>
    <div class="navbar">
        <a href="Dashboard.aspx" class="brand">&#127916; KumariCinemas</a>
        <a href="Dashboard.aspx">Dashboard</a>
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
        <a href="MovieOccupancyForm.aspx" class="active">Occupancy</a>
    </div>

    <form id="form1" runat="server">
    <div class="page">

        <h2><i class="bi bi-bar-chart-fill"></i> Movie TheaterCityHall Occupancy Performer</h2>
        <div class="page-sub">Complex Query 3 — For any movie, show the top 3 halls with maximum seat occupancy % based on paid tickets only.</div>

        <asp:Label ID="lblMessage" runat="server" CssClass="msg-error" />

        <div class="filter-box">
            <h3><i class="bi bi-funnel-fill"></i> Search Filter</h3>
            <div class="filter-row">
                <div class="filter-field">
                    <label>Movie *</label>
                    <asp:DropDownList ID="ddlMovie" runat="server" />
                </div>
                <div class="filter-field" style="justify-content:flex-end;">
                    <asp:Button ID="btnSearch" runat="server" Text="Search Top 3" CssClass="btn-search" OnClick="btnSearch_Click" CausesValidation="false" />
                    <asp:Button ID="btnClear"  runat="server" Text="Clear"        CssClass="btn-clear"  OnClick="btnClear_Click"  CausesValidation="false" Style="margin-left:8px;" />
                </div>
            </div>
            <div style="margin-top:8px; font-size:0.82rem; color:#888;">
                <i class="bi bi-info-circle"></i> Occupancy % = (Paid Tickets ÷ Hall Capacity) × 100. Only <strong>Paid</strong> tickets are counted.
            </div>
        </div>

        <asp:Label ID="lblResultCount" runat="server" CssClass="result-header" Visible="false" />

        <asp:GridView ID="gvResults" runat="server"
            AutoGenerateColumns="false"
            CssClass="grid-style"
            EmptyDataText="No paid ticket data found for this movie."
            Visible="false">
            <Columns>
                <asp:BoundField DataField="Rank"                HeaderText="Rank"            />
                <asp:BoundField DataField="Movie_Title"         HeaderText="Movie"           />
                <asp:BoundField DataField="Genre"               HeaderText="Genre"           />
                <asp:BoundField DataField="Language"            HeaderText="Language"        />
                <asp:BoundField DataField="TheaterName"         HeaderText="Theater"         />
                <asp:BoundField DataField="CityName"            HeaderText="City"            />
                <asp:BoundField DataField="HallName"            HeaderText="Hall"            />
                <asp:BoundField DataField="HallCapacity"        HeaderText="Capacity"        />
                <asp:BoundField DataField="PaidTickets"         HeaderText="Paid Tickets"    />
                <asp:BoundField DataField="OccupancyPercentage" HeaderText="Occupancy %"     DataFormatString="{0:N2}" />
            </Columns>
        </asp:GridView>

    </div>
    </form>
</body>
</html>
