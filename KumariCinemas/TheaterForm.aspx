<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TheaterForm.aspx.cs" Inherits="KumariCinemas.TheaterForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css"/>
    <title>Theater Management - KumariCinemas</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: Arial, sans-serif; background: #f4f4f4; }
        .navbar { background: #1B3A6B; padding: 0 24px; display: flex; align-items: center; gap: 4px; height: 52px; flex-wrap: wrap; }
        .navbar .brand { color: white; font-size: 1.1rem; font-weight: bold; margin-right: 16px; text-decoration: none; }
        .navbar a { color: rgba(255,255,255,0.85); text-decoration: none; padding: 5px 10px; border-radius: 4px; font-size: 0.82rem; }
        .navbar a:hover, .navbar a.active { background: rgba(255,255,255,0.2); color: white; }
        .navbar .nav-sep { color: rgba(255,255,255,0.3); font-size: 0.75rem; padding: 0 4px; }
        .page { padding: 24px; max-width: 1100px; margin: 0 auto; }
        h2 { color: #1B3A6B; margin-bottom: 16px; }
        .form-box { background: white; padding: 24px; margin-bottom: 24px; border: 1px solid #ddd; border-radius: 8px; max-width: 520px; box-shadow: 0 1px 4px rgba(0,0,0,0.07); }
        .form-box h3 { margin-top: 0; color: #1B3A6B; margin-bottom: 16px; }
        .form-row { margin-bottom: 12px; }
        .form-row label { display: block; font-weight: bold; margin-bottom: 4px; font-size: 0.9rem; }
        .form-row input[type=text], .form-row select { width: 100%; padding: 7px; border: 1px solid #ccc; border-radius: 4px; font-size: 0.9rem; }
        .form-row input[type=text]:focus { outline: none; border-color: #1B3A6B; }
        .btn-save   { background: #1B3A6B; color: white; padding: 8px 20px; border: none; cursor: pointer; border-radius: 4px; font-size: 0.9rem; }
        .btn-save:hover { background: #15305a; }
        .btn-clear  { background: #7f8c8d; color: white; padding: 8px 20px; border: none; cursor: pointer; border-radius: 4px; margin-left: 8px; font-size: 0.9rem; }
        .btn-edit   { background: #0A6B5E; color: white; padding: 4px 10px; border: none; cursor: pointer; border-radius: 3px; font-size: 0.82rem; }
        .btn-delete { background: #C0550A; color: white; padding: 4px 10px; border: none; cursor: pointer; border-radius: 3px; font-size: 0.82rem; }
        .msg-success { color: green; font-weight: bold; margin: 10px 0; display: block; }
        .msg-error   { color: red;   font-weight: bold; margin: 10px 0; display: block; }
        .grid-style { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 4px rgba(0,0,0,0.07); }
        .grid-style th { background: #1B3A6B; color: white; padding: 10px 12px; text-align: left; font-size: 0.85rem; }
        .grid-style td { padding: 8px 12px; border-bottom: 1px solid #eee; font-size: 0.85rem; }
        .grid-style tr:last-child td { border-bottom: none; }
        .grid-style tr:hover td { background: #f0f4fb; }
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
        <a href="TheaterForm.aspx" class="active">Theaters</a>
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
        <h2><i class="bi bi-shop"></i> Theater Management</h2>
        <asp:Label ID="lblMessage" runat="server" CssClass="msg-success" />

        <div class="form-box">
            <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Theater" /></h3>
            <asp:HiddenField ID="hfTheaterId" runat="server" Value="0" />

            <div class="form-row">
                <label>Theater Name *</label>
                <asp:TextBox ID="txtTheaterName" runat="server" MaxLength="150" placeholder="e.g. QFX Cinemas" />
                <asp:RequiredFieldValidator ControlToValidate="txtTheaterName" runat="server"
                    ErrorMessage="Theater name is required." ForeColor="Red" Display="Dynamic" />
            </div>

            <div class="form-row">
                <label>City *</label>
                <asp:DropDownList ID="ddlCity" runat="server" Width="100%" />
                <asp:RequiredFieldValidator ControlToValidate="ddlCity" runat="server"
                    InitialValue="0"
                    ErrorMessage="Please select a city." ForeColor="Red" Display="Dynamic" />
            </div>

            <div class="form-row">
                <label>Contact Number</label>
                <asp:TextBox ID="txtContact" runat="server" MaxLength="20" placeholder="e.g. 01-4444111" />
            </div>

            <div class="form-row">
                <label>Email</label>
                <asp:TextBox ID="txtEmail" runat="server" MaxLength="150" placeholder="e.g. info@qfx.com" />
            </div>

            <br />
            <asp:Button ID="btnSave"  runat="server" Text="Save Theater"  CssClass="btn-save"  OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear Form"    CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
        </div>

        <asp:GridView ID="gvTheaters" runat="server"
            AutoGenerateColumns="false"
            CssClass="grid-style"
            EmptyDataText="No theaters found."
            OnRowCommand="gvTheaters_RowCommand">
            <Columns>
                <asp:BoundField DataField="Theater_Id"   HeaderText="ID"      />
                <asp:BoundField DataField="TheaterName"  HeaderText="Theater" />
                <asp:BoundField DataField="CityName"     HeaderText="City"    />
                <asp:BoundField DataField="ContactNumber" HeaderText="Contact" />
                <asp:BoundField DataField="TheaterEmail" HeaderText="Email"   />
                <asp:TemplateField HeaderText="Edit">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="EditTheater"
                            CommandArgument='<%# Eval("Theater_Id") %>' CssClass="btn-edit"
                            Text='<i class="bi bi-pencil"></i> Edit' CausesValidation="false" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Delete">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="DeleteTheater"
                            CommandArgument='<%# Eval("Theater_Id") %>' CssClass="btn-delete"
                            Text='<i class="bi bi-trash3-fill"></i> Delete'
                            OnClientClick="return confirm('Delete this theater?');" CausesValidation="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
    </form>
</body>
</html>
