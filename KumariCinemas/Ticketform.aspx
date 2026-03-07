<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TicketForm.aspx.cs" Inherits="KumariCinemas.TicketForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Ticket Management - KumariCenimas</title>
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

    <h2>🎟️ Ticket Management</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Ticket"></asp:Label></h3>

        <asp:HiddenField ID="hfTicketId" runat="server" Value="0" />

        <div class="form-row">
            <label>Booking *</label>
            <asp:DropDownList ID="ddlBooking" runat="server" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="ddlBooking" runat="server"
                InitialValue="0"
                ErrorMessage="Please select a booking." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Show *</label>
            <asp:DropDownList ID="ddlShow" runat="server" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="ddlShow" runat="server"
                InitialValue="0"
                ErrorMessage="Please select a show." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Seat Number *</label>
            <asp:TextBox ID="txtSeatNumber" runat="server" MaxLength="5" placeholder="e.g. A1" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtSeatNumber" runat="server"
                ErrorMessage="Seat number is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Ticket Price (NPR) *</label>
            <asp:TextBox ID="txtPrice" runat="server" MaxLength="10" placeholder="e.g. 300.00" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtPrice" runat="server"
                ErrorMessage="Price is required." ForeColor="Red" Display="Dynamic" />
            <asp:RangeValidator ControlToValidate="txtPrice" runat="server"
                MinimumValue="1" MaximumValue="99999" Type="Double"
                ErrorMessage="Price must be a number between 1 and 99999." ForeColor="Red" Display="Dynamic" />
        </div>

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save Ticket"  CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form"   CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>

    <!-- ── TICKETS GRID ───────────────────────────────────────── -->
    <br />
    <asp:GridView ID="gvTickets" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No tickets found."
        OnRowCommand="gvTickets_RowCommand">
        <Columns>
            <asp:BoundField DataField="Ticket_Id"    HeaderText="ID"           />
            <asp:BoundField DataField="Booking_Info" HeaderText="Booking"      />
            <asp:BoundField DataField="Show_Info"    HeaderText="Show"         />
            <asp:BoundField DataField="Seat_Number"  HeaderText="Seat"         />
            <asp:BoundField DataField="Ticket_Price" HeaderText="Price (NPR)"  DataFormatString="{0:N2}" />
            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditTicket"
                        CommandArgument='<%# Eval("Ticket_Id") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeleteTicket"
                        CommandArgument='<%# Eval("Ticket_Id") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this ticket?');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
