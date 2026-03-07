<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ShowForm.aspx.cs" Inherits="KumariCinemas.ShowForm" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Show Management - KumariCenimas</title>
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

    <h2>🎞️ Show Management</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="msg-success"></asp:Label>

    <!-- ── ADD / EDIT FORM ─────────────────────────────────────── -->
    <div class="form-box">
        <h3><asp:Label ID="lblFormTitle" runat="server" Text="Add New Show"></asp:Label></h3>

        <asp:HiddenField ID="hfShowId" runat="server" Value="0" />

        <div class="form-row">
            <label>Movie *</label>
            <asp:DropDownList ID="ddlMovie" runat="server" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="ddlMovie" runat="server"
                InitialValue="0"
                ErrorMessage="Please select a movie." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Hall *</label>
            <asp:DropDownList ID="ddlHall" runat="server" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="ddlHall" runat="server"
                InitialValue="0"
                ErrorMessage="Please select a hall." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Show Date *</label>
            <asp:TextBox ID="txtShowDate" runat="server" placeholder="DD-MMM-YYYY e.g. 10-Jan-2025" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtShowDate" runat="server"
                ErrorMessage="Show date is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-row">
            <label>Show Time *</label>
            <asp:TextBox ID="txtShowTime" runat="server" placeholder="HH:MM e.g. 14:30" Width="100%" />
            <asp:RequiredFieldValidator ControlToValidate="txtShowTime" runat="server"
                ErrorMessage="Show time is required." ForeColor="Red" Display="Dynamic" />
        </div>

        <asp:Button ID="btnSave"  runat="server" Text="💾 Save Show"  CssClass="btn-save"  OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="✖ Clear Form" CssClass="btn-clear" OnClick="btnClear_Click" CausesValidation="false" />
    </div>

    <!-- ── SHOWS GRID ─────────────────────────────────────────── -->
    <br />
    <asp:GridView ID="gvShows" runat="server"
        AutoGenerateColumns="false"
        CssClass="grid-style"
        EmptyDataText="No shows found."
        OnRowCommand="gvShows_RowCommand">
        <Columns>
            <asp:BoundField DataField="Show_Id"     HeaderText="ID"         />
            <asp:BoundField DataField="Movie_Name"  HeaderText="Movie"      />
            <asp:BoundField DataField="Hall_Info"   HeaderText="Hall"       />
            <asp:BoundField DataField="Show_Date"   HeaderText="Date"       DataFormatString="{0:dd-MMM-yyyy}" />
            <asp:BoundField DataField="Show_Time"   HeaderText="Time"       DataFormatString="{0:HH:mm}"       />
            <asp:TemplateField HeaderText="Edit">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="EditShow"
                        CommandArgument='<%# Eval("Show_Id") %>'
                        CssClass="btn-edit"
                        Text="✏ Edit"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CommandName="DeleteShow"
                        CommandArgument='<%# Eval("Show_Id") %>'
                        CssClass="btn-delete"
                        Text="🗑 Delete"
                        OnClientClick="return confirm('Delete this show? Linked tickets will also be removed.');"
                        CausesValidation="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</form>
</body>
</html>
