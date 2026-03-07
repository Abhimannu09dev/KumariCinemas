<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BookingForm.aspx.cs" Inherits="KumariCinemas.BookingForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Booking Management - KumariCenimas</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background: #f4f4f4; }
        h2   { color: #333; }

        /* GridView */
        .grid-style { width: 100%; border-collapse: collapse; background: white; }
        .grid-style th { background-color: #c0392b; color: white; padding: 10px; text-align: left; }
        .grid-style td { padding: 8px 10px; border-bottom: 1px solid #ddd; }
        .grid-style tr:hover td { background-color: #fdf2f2; }

        /* Status badge colours in grid */
        .status-confirmed { color: green;  font-weight: bold; }
        .status-cancelled { color: red;    font-weight: bold; }
        .status-pending   { color: orange; font-weight: bold; }

        /* Form */
        .form-box { background: white; padding: 20px; margin-top: 20px;
                    border: 1px solid #ddd; border-radius: 5px; max-width: 500px; }
        .form-box h3  { margin-top: 0; color: #c0392b; }
        .form-row     { margin-bottom: 12px; }
        .form-row label { display: block; font-weight: bold; margin-bottom: 4px; }
        .form-row asp\:TextBox,
        .form-row asp\:DropDownList,
        .form-row select,
        .form-row input  { width: 100%; padding: 6px; box-sizing: border-box; }

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

    <h2>📋 Booking Management</h2>

    <!-- Status message -->
    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Booking"></asp:Label></h3>

        <!-- Hidden field: 0 = new booking, else = editing existing -->
        <asp:HiddenField ID="hfBookingId" runat="server" Value="0" />

        <div class="form-row">
            <label>User *</label>
            <asp:DropDownList ID="ddlUser" runat="server" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="ddlUser" runat="server"
                InitialValue="0"
                ErrorMessage="Please select a user." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Booking Date *</label>
            <asp:TextBox ID="txtBookingDate" runat="server" placeholder="DD-MMM-YYYY e.g. 10-Jan-2025" />
            <asp:RequiredFieldValidator ControlToValidate="txtBookingDate" runat="server"
                ErrorMessage="Booking date is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Status *</label>
            <asp:DropDownList ID="ddlStatus" runat="server" Width="100%">
                <asp:ListItem Value="Confirmed">Confirmed</asp:ListItem>
                <asp:ListItem Value="Pending">Pending</asp:ListItem>
                <asp:ListItem Value="Cancelled">Cancelled</asp:ListItem>
            </asp:DropDownList>
        </div>

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save Booking" CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form"   CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>

    <!-- ── BOOKINGS GRID ───────────────────────────────────────── -->
    <br />
    <asp:GridView ID="gvBookings" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No bookings found."
        OnRowCommand="gvBookings_RowCommand">

        <Columns>
            <asp:BoundField DataField="Booking_ID"     HeaderText="ID"           />
            <asp:BoundField DataField="User_Name"      HeaderText="User"         />
            <asp:BoundField DataField="Booking_Date"   HeaderText="Booking Date" DataFormatString="{0:dd-MMM-yyyy}" />
            <asp:BoundField DataField="Booking_Status" HeaderText="Status"       />

            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditBooking"
                        CommandArgument='<%# Eval("Booking_ID") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeleteBooking"
                        CommandArgument='<%# Eval("Booking_ID") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this booking? This will also delete linked tickets and payments.');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
