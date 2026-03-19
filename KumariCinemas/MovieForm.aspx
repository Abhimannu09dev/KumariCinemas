<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MovieForm.aspx.cs" Inherits="KumariCinemas.MovieForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css"/>
    <title>Movie Management - KumariCinemas</title>
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
        .form-row input[type=text]:focus, .form-row select:focus { outline: none; border-color: #1B3A6B; }

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
        <a href="MovieForm.aspx" class="active">Movies</a>
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

        <h2><i class="bi bi-camera-film"></i> Movie Management</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="msg-success" />

        <div class="form-box">
            <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Movie" /></h3>

            <asp:HiddenField ID="hfMovieId" runat="server" Value="0" />

            <div class="form-row">
                <label>Title *</label>
                <asp:TextBox ID="txtTitle" runat="server" MaxLength="200" placeholder="e.g. Prem Geet 3" />
                <asp:RequiredFieldValidator ControlToValidate="txtTitle" runat="server"
                    ErrorMessage="Title is required." ForeColor="Red" Display="Dynamic" />
            </div>

            <div class="form-row">
                <label>Duration (minutes)</label>
                <asp:TextBox ID="txtDuration" runat="server" MaxLength="5" placeholder="e.g. 135" />
                <asp:RangeValidator ControlToValidate="txtDuration" runat="server"
                    MinimumValue="1" MaximumValue="500" Type="Integer"
                    ErrorMessage="Duration must be between 1 and 500." ForeColor="Red" Display="Dynamic" />
            </div>

            <div class="form-row">
                <label>Language *</label>
                <asp:DropDownList ID="ddlLanguage" runat="server" Width="100%">
                    <asp:ListItem Value="Nepali">Nepali</asp:ListItem>
                    <asp:ListItem Value="Hindi">Hindi</asp:ListItem>
                    <asp:ListItem Value="English">English</asp:ListItem>
                    <asp:ListItem Value="Maithili">Maithili</asp:ListItem>
                    <asp:ListItem Value="Newari">Newari</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="form-row">
                <label>Genre *</label>
                <asp:DropDownList ID="ddlGenre" runat="server" Width="100%">
                    <asp:ListItem Value="Romance">Romance</asp:ListItem>
                    <asp:ListItem Value="Action">Action</asp:ListItem>
                    <asp:ListItem Value="Drama">Drama</asp:ListItem>
                    <asp:ListItem Value="Comedy">Comedy</asp:ListItem>
                    <asp:ListItem Value="Adventure">Adventure</asp:ListItem>
                    <asp:ListItem Value="Thriller">Thriller</asp:ListItem>
                    <asp:ListItem Value="Horror">Horror</asp:ListItem>
                    <asp:ListItem Value="Documentary">Documentary</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="form-row">
                <label>Release Date *</label>
                <asp:TextBox ID="txtReleaseDate" runat="server" placeholder="e.g. 30-Sep-2022" />
                <asp:RequiredFieldValidator ControlToValidate="txtReleaseDate" runat="server"
                    ErrorMessage="Release date is required." ForeColor="Red" Display="Dynamic" />
            </div>

            <br />
            <asp:Button ID="btnSave"  runat="server" Text="Save Movie"  CssClass="btn-save"  OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear Form"  CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
        </div>

        <asp:GridView ID="gvMovies" runat="server"
            AutoGenerateColumns="false"
            CssClass="grid-style"
            EmptyDataText="No movies found."
            OnRowCommand="gvMovies_RowCommand">
            <Columns>
                <asp:BoundField DataField="Movie_Id"    HeaderText="ID"           />
                <asp:BoundField DataField="Title"       HeaderText="Title"        />
                <asp:BoundField DataField="Duration"    HeaderText="Mins"         />
                <asp:BoundField DataField="Language"    HeaderText="Language"     />
                <asp:BoundField DataField="Genre"       HeaderText="Genre"        />
                <asp:BoundField DataField="ReleaseDate" HeaderText="Release Date" DataFormatString="{0:dd-MMM-yyyy}" />

                <asp:TemplateField HeaderText="Edit">
                    <ItemTemplate>
                        <asp:LinkButton runat="server"
                            CommandName="EditMovie"
                            CommandArgument='<%# Eval("Movie_Id") %>'
                            CssClass="btn-edit"
                            Text='<i class="bi bi-pencil"></i> Edit'
                            CausesValidation="false" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Delete">
                    <ItemTemplate>
                        <asp:LinkButton runat="server"
                            CommandName="DeleteMovie"
                            CommandArgument='<%# Eval("Movie_Id") %>'
                            CssClass="btn-delete"
                            Text='<i class="bi bi-trash3-fill"></i> Delete'
                            OnClientClick="return confirm('Delete this movie and all linked records?');"
                            CausesValidation="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

    </div>
    </form>
</body>
</html>
