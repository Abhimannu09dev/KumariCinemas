<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserForm.aspx.cs" Inherits="KumariCinemas.UserForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>User Management - KumariCenimas</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background: #f4f4f4; }
        h2   { color: #333; }

        /* GridView styling */
        .grid-style { width: 100%; border-collapse: collapse; background: white; }
        .grid-style th {
            background-color: #c0392b;
            color: white;
            padding: 10px;
            text-align: left;
        }
        .grid-style td { padding: 8px 10px; border-bottom: 1px solid #ddd; }
        .grid-style tr:hover td { background-color: #fdf2f2; }

        /* Form area */
        .form-box {
            background: white;
            padding: 20px;
            margin-top: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
            max-width: 500px;
        }
        .form-box h3 { margin-top: 0; color: #c0392b; }
        .form-row    { margin-bottom: 12px; }
        .form-row label { display: block; font-weight: bold; margin-bottom: 4px; }
        .form-row asp\:TextBox, .form-row input { width: 100%; padding: 6px; box-sizing: border-box; }

        .btn-save   { background: #c0392b; color: white; padding: 8px 20px; border: none; cursor: pointer; border-radius: 3px; }
        .btn-clear  { background: #7f8c8d; color: white; padding: 8px 20px; border: none; cursor: pointer; border-radius: 3px; margin-left: 8px; }
        .btn-edit   { background: #2980b9; color: white; padding: 4px 10px; border: none; cursor: pointer; border-radius: 3px; }
        .btn-delete { background: #c0392b; color: white; padding: 4px 10px; border: none; cursor: pointer; border-radius: 3px; }

        .msg-success { color: green;  font-weight: bold; margin: 10px 0; }
        .msg-error   { color: red;    font-weight: bold; margin: 10px 0; }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <h2>👤 User Management</h2>

    <!-- Status message -->
    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New User"></asp:Label></h3>

        <!-- Hidden field to track which user we're editing (0 = new) -->
        <asp:HiddenField ID="hfUserId" runat="server" Value="0" />

        <div class="form-row">
            <label>Full Name *</label>
            <asp:TextBox ID="txtName" runat="server" MaxLength="225" placeholder="e.g. Sita Sharma" />
            <asp:RequiredFieldValidator ControlToValidate="txtName" runat="server"
                ErrorMessage="Name is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Contact Number *</label>
            <asp:TextBox ID="txtContact" runat="server" MaxLength="15" placeholder="e.g. 9841234567" />
            <asp:RequiredFieldValidator ControlToValidate="txtContact" runat="server"
                ErrorMessage="Contact is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Address</label>
            <asp:TextBox ID="txtAddress" runat="server" MaxLength="225" placeholder="e.g. Thamel, Kathmandu" />
        </div>

        <div class="form-row">
            <label>Email *</label>
            <asp:TextBox ID="txtEmail" runat="server" MaxLength="225" placeholder="e.g. name@gmail.com" />
            <asp:RequiredFieldValidator ControlToValidate="txtEmail" runat="server"
                ErrorMessage="Email is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save User"  CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form" CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>


    <br />
    <asp:GridView ID="gvUsers" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No users found."
        OnRowCommand="gvUsers_RowCommand">

        <Columns>
            <asp:BoundField DataField="User_ID"      HeaderText="ID"      />
            <asp:BoundField DataField="User_Name"    HeaderText="Name"    />
            <asp:BoundField DataField="User_Contact" HeaderText="Contact" />
            <asp:BoundField DataField="User_Address" HeaderText="Address" />
            <asp:BoundField DataField="User_Email"   HeaderText="Email"   />

           
            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditUser"
                        CommandArgument='<%# Eval("User_ID") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>

          
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeleteUser"
                        CommandArgument='<%# Eval("User_ID") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this user?');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
