<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HallForm.aspx.cs" Inherits="KumariCinemas.HallForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Hall Management - KumariCenimas</title>
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

    <h2>🏛️ Hall Management</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Hall"></asp:Label></h3>

        <asp:HiddenField ID="hfHallId" runat="server" Value="0" />

        <div class="form-row">
            <label>Location *</label>
            <asp:DropDownList ID="ddlLocation" runat="server" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="ddlLocation" runat="server"
                InitialValue="0"
                ErrorMessage="Please select a location." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Hall Number *</label>
            <asp:TextBox ID="txtHallNumber" runat="server" MaxLength="10" placeholder="e.g. Hall A" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtHallNumber" runat="server"
                ErrorMessage="Hall number is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Capacity (seats) *</label>
            <asp:TextBox ID="txtCapacity" runat="server" MaxLength="5" placeholder="e.g. 150" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtCapacity" runat="server"
                ErrorMessage="Capacity is required." ForeColor="Red" Display="Dynamic" />
            <asp:RangeValidator ControlToValidate="txtCapacity" runat="server"
                MinimumValue="1" MaximumValue="2000" Type="Integer"
                ErrorMessage="Capacity must be a number between 1 and 2000." ForeColor="Red" Display="Dynamic" />
        </div>

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save Hall"  CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form" CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>

    <!-- ── HALLS GRID ─────────────────────────────────────────── -->
    <br />
    <asp:GridView ID="gvHalls" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No halls found."
        OnRowCommand="gvHalls_RowCommand">
        <Columns>
            <asp:BoundField DataField="Hall_Id"       HeaderText="ID"           />
            <asp:BoundField DataField="Location_Name" HeaderText="Location"     />
            <asp:BoundField DataField="Hall_Number"   HeaderText="Hall Number"  />
            <asp:BoundField DataField="Hall_Capacity" HeaderText="Capacity"     />
            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditHall"
                        CommandArgument='<%# Eval("Hall_Id") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeleteHall"
                        CommandArgument='<%# Eval("Hall_Id") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this hall? Linked shows and tickets will also be removed.');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
