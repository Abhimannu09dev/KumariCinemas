<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaymentForm.aspx.cs" Inherits="KumariCinemas.PaymentForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Payment Management - KumariCenimas</title>
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

        .status-completed { color: green;  font-weight: bold; } 
        .status-pending   { color: orange; font-weight: bold; }
        .status-refunded  { color: #2980b9; font-weight: bold; }

        .msg-success { color: green; font-weight: bold; margin: 10px 0; }
        .msg-error   { color: red;   font-weight: bold; margin: 10px 0; }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <h2>💳 Payment Management</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Payment"></asp:Label></h3>

        <asp:HiddenField ID="hfPaymentId" runat="server" Value="0" />

        <div class="form-row">
            <label>Booking *</label>
            <asp:DropDownList ID="ddlBooking" runat="server" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="ddlBooking" runat="server"
                InitialValue="0"
                ErrorMessage="Please select a booking." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Payment Date *</label>
            <asp:TextBox ID="txtPaymentDate" runat="server" placeholder="DD-MMM-YYYY e.g. 08-Jan-2025" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtPaymentDate" runat="server"
                ErrorMessage="Payment date is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Payment Time *</label>
            <asp:TextBox ID="txtPaymentTime" runat="server" placeholder="HH:MM e.g. 09:15" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtPaymentTime" runat="server"
                ErrorMessage="Payment time is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Amount (NPR) *</label>
            <asp:TextBox ID="txtAmount" runat="server" MaxLength="10" placeholder="e.g. 600.00" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtAmount" runat="server"
                ErrorMessage="Amount is required." ForeColor="Red" Display="Dynamic" />
            <asp:RangeValidator ControlToValidate="txtAmount" runat="server"
                MinimumValue="1" MaximumValue="999999" Type="Double"
                ErrorMessage="Amount must be a number between 1 and 999999." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Payment Status *</label>
            <asp:DropDownList ID="ddlStatus" runat="server" Width="100%">
                <asp:ListItem Value="Completed">Completed</asp:ListItem>
                <asp:ListItem Value="Pending">Pending</asp:ListItem>
                <asp:ListItem Value="Refunded">Refunded</asp:ListItem>
            </asp:DropDownList>
        </div>

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save Payment"  CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form"    CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>

    <!-- ── PAYMENTS GRID ──────────────────────────────────────── -->
    <br />
    <asp:GridView ID="gvPayments" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No payments found."
        OnRowCommand="gvPayments_RowCommand">
        <Columns>
            <asp:BoundField DataField="Payment_Id"     HeaderText="ID"              />
            <asp:BoundField DataField="Booking_Info"   HeaderText="Booking"         />
            <asp:BoundField DataField="Payment_Date"   HeaderText="Date &amp; Time" DataFormatString="{0:dd-MMM-yyyy HH:mm}" />
            <asp:BoundField DataField="Payment_Amount" HeaderText="Amount (NPR)"    DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Payment_Status" HeaderText="Status"          />
            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditPayment"
                        CommandArgument='<%# Eval("Payment_Id") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeletePayment"
                        CommandArgument='<%# Eval("Payment_Id") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this payment record?');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
