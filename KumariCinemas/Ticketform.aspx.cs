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

        // ─────────────────────────────────────────────────────────────
        // PAGE LOAD
        // ─────────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBookingDropdown();
                LoadShowDropdown();
                LoadTickets();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD BOOKING DROPDOWN
        // Shows "Booking #1 — Abhimannu Kunwar (Confirmed)"
        // ─────────────────────────────────────────────────────────────
        private void LoadBookingDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT b.Booking_ID,
                                          'Booking #' || b.Booking_ID || ' — ' ||
                                          u.User_Name || ' (' || b.Booking_Status || ')' AS Booking_Label
                                   FROM   Booking b
                                   JOIN   ""User"" u ON b.User_Id = u.User_ID
                                   ORDER  BY b.Booking_ID";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        ddlBooking.DataSource = dt;
                        ddlBooking.DataTextField = "Booking_Label";
                        ddlBooking.DataValueField = "Booking_ID";
                        ddlBooking.DataBind();

                        ddlBooking.Items.Insert(0, new ListItem("-- Select Booking --", "0"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading bookings: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD SHOW DROPDOWN
        // Shows "Prem Geet 3 — Hall A (10-Jan-2025 10:00)"
        // ─────────────────────────────────────────────────────────────
        private void LoadShowDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT s.Show_Id,
                                          m.Movie_Name || ' — ' || h.Hall_Number ||
                                          ' (' || TO_CHAR(s.Show_Date, 'DD-Mon-YYYY') ||
                                          ' ' || TO_CHAR(s.Show_Time, 'HH24:MI') || ')' AS Show_Label
                                   FROM   Show     s
                                   JOIN   Movie    m ON s.Movie_Id = m.Movie_Id
                                   JOIN   Hall     h ON s.Hall_Id  = h.Hall_Id
                                   ORDER  BY s.Show_Date, s.Show_Time";

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
            catch (Exception ex)
            {
                ShowMessage("Error loading shows: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD GRID
        // JOINs Booking + User + Show + Movie for full readable info
        // ─────────────────────────────────────────────────────────────
        private void LoadTickets()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT t.Ticket_Id,
                                          'Booking #' || b.Booking_ID || ' (' || u.User_Name || ')' AS Booking_Info,
                                          m.Movie_Name || ' — ' || TO_CHAR(s.Show_Date, 'DD-Mon-YYYY') AS Show_Info,
                                          t.Seat_Number,
                                          t.Ticket_Price
                                   FROM   Ticket  t
                                   JOIN   Booking b ON t.Booking_Id = b.Booking_ID
                                   JOIN   ""User"" u ON b.User_Id   = u.User_ID
                                   JOIN   Show    s ON t.Show_Id    = s.Show_Id
                                   JOIN   Movie   m ON s.Movie_Id   = m.Movie_Id
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
            catch (Exception ex)
            {
                ShowMessage("Error loading tickets: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // SAVE — INSERT or UPDATE
        // ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int ticketId = int.Parse(hfTicketId.Value);
            bool isNew = (ticketId == 0);

            int selectedBookingId = int.Parse(ddlBooking.SelectedValue);
            int selectedShowId = int.Parse(ddlShow.SelectedValue);
            decimal price;

            if (!decimal.TryParse(txtPrice.Text.Trim(), out price))
            {
                ShowMessage("✗ Invalid price. Enter a number e.g. 300.00", isError: true);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql;

                    if (isNew)
                    {
                        int newId = GetNextTicketId(conn);

                        sql = @"INSERT INTO Ticket
                                    (Ticket_Id, Booking_Id, Show_Id, Seat_Number, Ticket_Price)
                                VALUES
                                    (:p_id, :p_bookingId, :p_showId, :p_seat, :p_price)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_bookingId", OracleDbType.Int32).Value = selectedBookingId;
                            cmd.Parameters.Add(":p_showId", OracleDbType.Int32).Value = selectedShowId;
                            cmd.Parameters.Add(":p_seat", OracleDbType.Varchar2).Value = txtSeatNumber.Text.Trim().ToUpper();
                            cmd.Parameters.Add(":p_price", OracleDbType.Decimal).Value = price;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Ticket added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        sql = @"UPDATE Ticket
                                SET Booking_Id   = :p_bookingId,
                                    Show_Id      = :p_showId,
                                    Seat_Number  = :p_seat,
                                    Ticket_Price = :p_price
                                WHERE Ticket_Id = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_bookingId", OracleDbType.Int32).Value = selectedBookingId;
                            cmd.Parameters.Add(":p_showId", OracleDbType.Int32).Value = selectedShowId;
                            cmd.Parameters.Add(":p_seat", OracleDbType.Varchar2).Value = txtSeatNumber.Text.Trim().ToUpper();
                            cmd.Parameters.Add(":p_price", OracleDbType.Decimal).Value = price;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Ticket updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadTickets();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error saving ticket: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GRID ROW COMMAND — Edit / Delete
        // ─────────────────────────────────────────────────────────────
        protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int ticketId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditTicket")
            {
                LoadTicketIntoForm(ticketId);
            }
            else if (e.CommandName == "DeleteTicket")
            {
                DeleteTicket(ticketId);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD INTO FORM (editing)
        // ─────────────────────────────────────────────────────────────
        private void LoadTicketIntoForm(int ticketId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Ticket_Id, Booking_Id, Show_Id,
                                          Seat_Number, Ticket_Price
                                   FROM   Ticket
                                   WHERE  Ticket_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfTicketId.Value = reader["Ticket_Id"].ToString();

                                ddlBooking.SelectedValue = reader["Booking_Id"].ToString();
                                ddlShow.SelectedValue = reader["Show_Id"].ToString();
                                txtSeatNumber.Text = reader["Seat_Number"].ToString();
                                txtPrice.Text = Convert.ToDecimal(reader["Ticket_Price"])
                                                                  .ToString("N2");

                                lblFormTitle.Text = "✏ Edit Ticket (ID: " + ticketId + ")";
                                btnSave.Text = "💾 Update Ticket";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error loading ticket: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE — Ticket has no children, safe to delete directly
        // ─────────────────────────────────────────────────────────────
        private void DeleteTicket(int ticketId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Ticket WHERE Ticket_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = ticketId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Ticket deleted successfully.", isError: false);
                LoadTickets();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting ticket: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // CLEAR FORM
        // ─────────────────────────────────────────────────────────────
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfTicketId.Value = "0";
            ddlBooking.SelectedIndex = 0;
            ddlShow.SelectedIndex = 0;
            txtSeatNumber.Text = "";
            txtPrice.Text = "";
            lblFormTitle.Text = "Add New Ticket";
            btnSave.Text = "💾 Save Ticket";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        private int GetNextTicketId(OracleConnection conn)
        {
            string sql = "SELECT NVL(MAX(Ticket_Id), 0) + 1 FROM Ticket";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}