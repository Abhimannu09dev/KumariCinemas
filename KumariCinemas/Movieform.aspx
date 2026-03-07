<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MovieForm.aspx.cs" Inherits="KumariCinemas.MovieForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Movie Management - KumariCenimas</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background: #f4f4f4; }
        h2   { color: #333; }

        .grid-style { width: 100%; border-collapse: collapse; background: white; }
        .grid-style th { background-color: #c0392b; color: white; padding: 10px; text-align: left; }
        .grid-style td { padding: 8px 10px; border-bottom: 1px solid #ddd; }
        .grid-style tr:hover td { background-color: #fdf2f2; }

        .form-box { background: white; padding: 20px; margin-top: 20px;
                    border: 1px solid #ddd; border-radius: 5px; max-width: 500px; }
        .form-box h3 { margin-top: 0; color: #c0392b; }
        .form-row    { margin-bottom: 12px; }
        .form-row label { display: block; font-weight: bold; margin-bottom: 4px; }

        .btn-save   { background: #c0392b; color: white; padding: 8px 20px; border: none; cursor: pointer; border-radius: 3px; }
        .btn-clear  { background: #7f8c8d; color: white; padding: 8px 20px; border: none; cursor: pointer; border-radius: 3px; margin-left: 8px; }
        .btn-edit   { background: #2980b9; color: white; padding: 4px 10px; border: none; cursor: pointer; border-radius: 3px; }
        .btn-delete { background: #c0392b; color: white; padding: 4px 10px; border: none; cursor: pointer; border-radius: 3px; }

        .msg-success { color: green; font-weight: bold; margin: 10px 0; }
        .msg-error   { color: red;   font-weight: bold; margin: 10px 0; }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <h2>🎬 Movie Management</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Movie"></asp:Label></h3>

        <asp:HiddenField ID="hfMovieId" runat="server" Value="0" />

        <div class="form-row">
            <label>Movie Name *</label>
            <asp:TextBox ID="txtName" runat="server" MaxLength="225" placeholder="e.g. Prem Geet 3" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtName" runat="server"
                ErrorMessage="Movie name is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Duration (minutes) *</label>
            <asp:TextBox ID="txtDuration" runat="server" MaxLength="5" placeholder="e.g. 135" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtDuration" runat="server"
                ErrorMessage="Duration is required." ForeColor="Red" Display="Dynamic" />
            <asp:RangeValidator ControlToValidate="txtDuration" runat="server"
                MinimumValue="1" MaximumValue="500" Type="Integer"
                ErrorMessage="Duration must be a number between 1 and 500." ForeColor="Red" Display="Dynamic" />
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
            <label>Release Date</label>
            <asp:TextBox ID="txtReleaseDate" runat="server" placeholder="DD-MMM-YYYY e.g. 30-Sep-2022" Width="100%" />
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

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save Movie"  CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form"  CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>

    <!-- ── MOVIES GRID ────────────────────────────────────────── -->
    <br />
    <asp:GridView ID="gvMovies" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No movies found."
        OnRowCommand="gvMovies_RowCommand">
        <Columns>
            <asp:BoundField DataField="Movie_Id"          HeaderText="ID"           />
            <asp:BoundField DataField="Movie_Name"        HeaderText="Title"        />
            <asp:BoundField DataField="Movie_Duration"    HeaderText="Mins"         />
            <asp:BoundField DataField="Movie_Genre"       HeaderText="Genre"        />
            <asp:BoundField DataField="Movie_ReleaseDate" HeaderText="Release Date" DataFormatString="{0:dd-MMM-yyyy}" />
            <asp:BoundField DataField="Movie_Language"    HeaderText="Language"     />
            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditMovie"
                        CommandArgument='<%# Eval("Movie_Id") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeleteMovie"
                        CommandArgument='<%# Eval("Movie_Id") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this movie? Linked shows and tickets will also be removed.');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
