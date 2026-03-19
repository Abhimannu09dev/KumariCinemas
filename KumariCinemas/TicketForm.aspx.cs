using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class TicketForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"]
                                    .ConnectionString;

        // ════════════════════════════════════════════
        // PAGE LOAD
        // ════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUserDropdown();
                LoadShowDropdown();
                LoadTickets();
            }
        }

        // ════════════════════════════════════════════
        // LOAD USER DROPDOWN
        // Ticket links directly to User via User_Id FK
        // ════════════════════════════════════════════
        private void LoadUserDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        @"SELECT User_Id, Username FROM ""User"" ORDER BY Username", conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        ddlUser.DataSource = dt;
                        ddlUser.DataTextField = "Username";
                        ddlUser.DataValueField = "User_Id";
                        ddlUser.DataBind();
                        ddlUser.Items.Insert(0, new ListItem("-- Select User --", "0"));
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading users: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // LOAD SHOW DROPDOWN
        // Shows "Bulbul — Hall A — QFX (15-Jan-2025 Morning)"
        // Join path: Show → Movie (direct) → Hall_Show → Hall → Theater_Hall → Theater → Showtime
        // ════════════════════════════════════════════
        private void LoadShowDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT s.Show_Id,
                               m.Title || ' — ' || h.HallName || ' (' ||
                               TO_CHAR(s.ShowDate, 'DD-Mon-YYYY') ||
                               ' ' || st.Showtime_Name || ')' AS Show_Label
                        FROM   Show         s
                        JOIN   Movie        m  ON m.Movie_Id    = s.Movie_Id
                        JOIN   Showtime     st ON st.Showtime_Id = s.Showtime_Id
                        JOIN   Hall_Show    hs ON hs.Show_Id    = s.Show_Id
                        JOIN   Hall         h  ON h.Hall_Id     = hs.Hall_Id
                        ORDER  BY s.ShowDate, st.Showtime_Name";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        ddlShow.DataSource = dt;
                        ddlShow.DataTextField = "Show_Label";
                        ddlShow.DataValueField = "Show_Id";
                        ddlShow.DataBind();
                        ddlShow.Items.Insert(0, new ListItem("-- Select Show --", "0"));
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading shows: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // LOAD TICKETS GRID
        // Join path: Ticket → User (direct FK)
        //            Ticket → Show_Ticket → Show → Movie → Showtime
        // ════════════════════════════════════════════
        private void LoadTickets()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT t.Ticket_Id,
                               u.Username,
                               m.Title || ' — ' || TO_CHAR(s.ShowDate, 'DD-Mon-YYYY')
                                       || ' ' || st.Showtime_Name AS Show_Info,
                               t.Seat_Number,
                               t.PaymentStatus
                        FROM   Ticket      t
                        JOIN   ""User""    u  ON u.User_Id     = t.User_Id
                        JOIN   Show_Ticket stk ON stk.Ticket_Id = t.Ticket_Id
                        JOIN   Show        s  ON s.Show_Id     = stk.Show_Id
                        JOIN   Movie       m  ON m.Movie_Id    = s.Movie_Id
                        JOIN   Showtime    st ON st.Showtime_Id = s.Showtime_Id
                        ORDER  BY t.Ticket_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvTickets.DataSource = dt;
                        gvTickets.DataBind();
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading tickets: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // INSERT: Ticket table + Show_Ticket junction
        // UPDATE: Ticket table + Show_Ticket junction
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int ticketId = int.Parse(hfTicketId.Value);
            bool isNew = (ticketId == 0);

            int userId = int.Parse(ddlUser.SelectedValue);
            int showId = int.Parse(ddlShow.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // STEP 1: Get next Ticket_Id from sequence
                        int newTicketId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_ticket_id.NEXTVAL FROM DUAL", conn))
                        {
                            newTicketId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        // STEP 2: INSERT into Ticket with ID as parameter
                        string sqlTicket = @"
                            INSERT INTO Ticket (Ticket_Id, Seat_Number, User_Id, PaymentStatus)
                            VALUES (:p_id, :p_seat, :p_userId, :p_status)";

                        using (var cmd = new OracleCommand(sqlTicket, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newTicketId;
                            cmd.Parameters.Add(":p_seat", OracleDbType.Varchar2).Value = txtSeatNumber.Text.Trim().ToUpper();
                            cmd.Parameters.Add(":p_userId", OracleDbType.Int32).Value = userId;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlPaymentStatus.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }

                        // STEP 2: INSERT into Show_Ticket junction
                        using (var cmd = new OracleCommand(
                            "INSERT INTO Show_Ticket (Show_Id, Ticket_Id) VALUES (:p_showId, :p_ticketId)", conn))
                        {
                            cmd.Parameters.Add(":p_showId", OracleDbType.Int32).Value = showId;
                            cmd.Parameters.Add(":p_ticketId", OracleDbType.Int32).Value = newTicketId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Ticket added successfully! (ID: " + newTicketId + ")", false);
                    }
                    else
                    {
                        // UPDATE Ticket fields
                        using (var cmd = new OracleCommand(@"
                            UPDATE Ticket
                            SET    Seat_Number   = :p_seat,
                                   User_Id       = :p_userId,
                                   PaymentStatus = :p_status
                            WHERE  Ticket_Id     = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_seat", OracleDbType.Varchar2).Value = txtSeatNumber.Text.Trim().ToUpper();
                            cmd.Parameters.Add(":p_userId", OracleDbType.Int32).Value = userId;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlPaymentStatus.SelectedValue;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;
                            cmd.ExecuteNonQuery();
                        }

                        // UPDATE Show_Ticket junction — change which show this ticket belongs to
                        using (var cmd = new OracleCommand(@"
                            UPDATE Show_Ticket
                            SET    Show_Id   = :p_showId
                            WHERE  Ticket_Id = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_showId", OracleDbType.Int32).Value = showId;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Ticket updated successfully!", false);
                    }
                }

                ClearForm();
                LoadTickets();
            }
            catch (Exception ex) { ShowMessage("✗ Error saving ticket: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // GRID ROW COMMAND — Edit / Delete
        // ════════════════════════════════════════════
        protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int ticketId = int.Parse(e.CommandArgument.ToString());
            if (e.CommandName == "EditTicket") LoadTicketIntoForm(ticketId);
            else if (e.CommandName == "DeleteTicket") DeleteTicket(ticketId);
        }

        // ════════════════════════════════════════════
        // LOAD INTO FORM (edit)
        // Gets Show_Id from Show_Ticket junction
        // ════════════════════════════════════════════
        private void LoadTicketIntoForm(int ticketId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT t.Ticket_Id,
                               t.Seat_Number,
                               t.User_Id,
                               t.PaymentStatus,
                               stk.Show_Id
                        FROM   Ticket      t
                        JOIN   Show_Ticket stk ON stk.Ticket_Id = t.Ticket_Id
                        WHERE  t.Ticket_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                hfTicketId.Value = r["Ticket_Id"].ToString();
                                txtSeatNumber.Text = r["Seat_Number"].ToString();
                                ddlUser.SelectedValue = r["User_Id"].ToString();
                                ddlPaymentStatus.SelectedValue = r["PaymentStatus"].ToString();
                                ddlShow.SelectedValue = r["Show_Id"].ToString();
                                lblFormTitle.Text = "✏ Edit Ticket (ID: " + ticketId + ")";
                                btnSave.Text = "Update Ticket";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("✗ Error loading ticket: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // DELETE TICKET
        // FK order: Show_Ticket junction first, then Ticket
        // ════════════════════════════════════════════
        private void DeleteTicket(int ticketId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete Show_Ticket junction first
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Show_Ticket WHERE Ticket_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Now safe to delete Ticket
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Ticket WHERE Ticket_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Ticket deleted successfully.", false);
                LoadTickets();
            }
            catch (Exception ex) { ShowMessage("✗ Error deleting ticket: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // CLEAR FORM
        // ════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            hfTicketId.Value = "0";
            ddlUser.SelectedIndex = 0;
            ddlShow.SelectedIndex = 0;
            txtSeatNumber.Text = "";
            ddlPaymentStatus.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Ticket";
            btnSave.Text = "Save Ticket";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }
    }
}