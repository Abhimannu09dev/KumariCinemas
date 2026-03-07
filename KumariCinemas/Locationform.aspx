<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LocationForm.aspx.cs" Inherits="KumariCinemas.LocationForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Location Management - KumariCenimas</title>
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

    <h2>📍 Location Management</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Location"></asp:Label></h3>

        <asp:HiddenField ID="hfLocationId" runat="server" Value="0" />

        <div class="form-row">
            <label>Location Name *</label>
            <asp:TextBox ID="txtName" runat="server" MaxLength="100" placeholder="e.g. QFX Civil Mall" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtName" runat="server"
                ErrorMessage="Location name is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Address *</label>
            <asp:TextBox ID="txtAddress" runat="server" MaxLength="100" placeholder="e.g. Civil Mall, Sundhara, Kathmandu" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtAddress" runat="server"
                ErrorMessage="Address is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save Location" CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form"    CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>

    <!-- ── LOCATIONS GRID ─────────────────────────────────────── -->
    <br />
    <asp:GridView ID="gvLocations" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No locations found."
        OnRowCommand="gvLocations_RowCommand">
        <Columns>
            <asp:BoundField DataField="Location_Id"      HeaderText="ID"       />
            <asp:BoundField DataField="Location_Name"    HeaderText="Name"     />
            <asp:BoundField DataField="Location_Address" HeaderText="Address"  />
            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditLocation"
                        CommandArgument='<%# Eval("Location_Id") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeleteLocation"
                        CommandArgument='<%# Eval("Location_Id") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this location? This will also remove linked halls.');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
