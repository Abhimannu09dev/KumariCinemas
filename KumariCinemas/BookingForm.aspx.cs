using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class BookingForm : Page
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
                LoadBookings();
            }
        }

        // ════════════════════════════════════════════
        // LOAD USER DROPDOWN
        // ════════════════════════════════════════════
        private void LoadUserDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT User_Id, Username
                                   FROM   ""User""
                                   ORDER  BY Username";

                    using (var cmd = new OracleCommand(sql, conn))
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
            catch (Exception ex)
            {
                ShowMessage("Error loading users: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // LOAD BOOKINGS GRID
        // Join path: Booking → User_Booking → User
        // ════════════════════════════════════════════
        private void LoadBookings()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT b.Booking_Id,
                               u.Username,
                               b.BookingDate,
                               b.BookingStatus
                        FROM   Booking      b
                        JOIN   User_Booking ub ON ub.Booking_Id = b.Booking_Id
                        JOIN   ""User""     u  ON u.User_Id     = ub.User_Id
                        ORDER  BY b.Booking_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvBookings.DataSource = dt;
                        gvBookings.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading bookings: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // INSERT: writes to Booking + User_Booking junction
        // UPDATE: only updates Booking — User_Booking updated separately
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int bookingId = int.Parse(hfBookingId.Value);
            bool isNew = (bookingId == 0);

            DateTime bookingDate;
            if (!DateTime.TryParse(txtBookingDate.Text.Trim(), out bookingDate))
            {
                ShowMessage("✗ Invalid date. Please use DD-MMM-YYYY (e.g. 10-Jan-2025).", isError: true);
                return;
            }

            int selectedUserId = int.Parse(ddlUser.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // STEP 1: Get next Booking_Id from sequence
                        int newBookingId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_booking_id.NEXTVAL FROM DUAL", conn))
                        {
                            newBookingId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        // STEP 2: INSERT into Booking with ID as parameter
                        string sqlBooking = @"INSERT INTO Booking
                                                 (Booking_Id, BookingDate, BookingStatus)
                                             VALUES
                                                 (:p_id, :p_date, :p_status)";

                        using (var cmd = new OracleCommand(sqlBooking, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newBookingId;
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = bookingDate;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }

                        // STEP 2: INSERT into User_Booking junction
                        string sqlJunction = @"INSERT INTO User_Booking (User_Id, Booking_Id)
                                               VALUES (:p_userId, :p_bookingId)";

                        using (var cmd = new OracleCommand(sqlJunction, conn))
                        {
                            cmd.Parameters.Add(":p_userId", OracleDbType.Int32).Value = selectedUserId;
                            cmd.Parameters.Add(":p_bookingId", OracleDbType.Int32).Value = newBookingId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Booking added successfully! (ID: " + newBookingId + ")", isError: false);
                    }
                    else
                    {
                        // UPDATE Booking fields
                        string sqlUpdate = @"UPDATE Booking
                                             SET    BookingDate   = :p_date,
                                                    BookingStatus = :p_status
                                             WHERE  Booking_Id    = :p_id";

                        using (var cmd = new OracleCommand(sqlUpdate, conn))
                        {
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = bookingDate;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = bookingId;
                            cmd.ExecuteNonQuery();
                        }

                        // UPDATE User_Booking junction — change which user owns this booking
                        string sqlJunction = @"UPDATE User_Booking
                                               SET    User_Id    = :p_userId
                                               WHERE  Booking_Id = :p_id";

                        using (var cmd = new OracleCommand(sqlJunction, conn))
                        {
                            cmd.Parameters.Add(":p_userId", OracleDbType.Int32).Value = selectedUserId;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = bookingId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Booking updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadBookings();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error saving booking: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // GRID ROW COMMAND — Edit / Delete
        // ════════════════════════════════════════════
        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int bookingId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditBooking")
                LoadBookingIntoForm(bookingId);
            else if (e.CommandName == "DeleteBooking")
                DeleteBooking(bookingId);
        }

        // ════════════════════════════════════════════
        // LOAD BOOKING INTO FORM (edit)
        // Gets User_Id from User_Booking junction
        // ════════════════════════════════════════════
        private void LoadBookingIntoForm(int bookingId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT b.Booking_Id,
                               b.BookingDate,
                               b.BookingStatus,
                               ub.User_Id
                        FROM   Booking      b
                        JOIN   User_Booking ub ON ub.Booking_Id = b.Booking_Id
                        WHERE  b.Booking_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = bookingId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfBookingId.Value = reader["Booking_Id"].ToString();

                                DateTime d = Convert.ToDateTime(reader["BookingDate"]);
                                txtBookingDate.Text = d.ToString("dd-MMM-yyyy");

                                ddlStatus.SelectedValue = reader["BookingStatus"].ToString();
                                ddlUser.SelectedValue = reader["User_Id"].ToString();

                                lblFormTitle.Text = "✏ Edit Booking (ID: " + bookingId + ")";
                                btnSave.Text = "Update Booking";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error loading booking: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // DELETE BOOKING
        // Must delete junction table rows first (FK order)
        // Order: Booking_Payment → Booking_Movie → User_Booking → Booking
        // ════════════════════════════════════════════
        private void DeleteBooking(int bookingId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete from Booking_Payment junction
                    Execute(conn, "DELETE FROM Booking_Payment WHERE Booking_Id = :p_id", bookingId);

                    // 2. Delete from Booking_Movie junction
                    Execute(conn, "DELETE FROM Booking_Movie WHERE Booking_Id = :p_id", bookingId);

                    // 3. Delete from User_Booking junction
                    Execute(conn, "DELETE FROM User_Booking WHERE Booking_Id = :p_id", bookingId);

                    // 4. Now safe to delete Booking itself
                    Execute(conn, "DELETE FROM Booking WHERE Booking_Id = :p_id", bookingId);
                }

                ShowMessage("✓ Booking and all linked records deleted.", isError: false);
                LoadBookings();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting booking: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // CLEAR FORM
        // ════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfBookingId.Value = "0";
            ddlUser.SelectedIndex = 0;
            txtBookingDate.Text = "";
            ddlStatus.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Booking";
            btnSave.Text = "Save Booking";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        // Helper — executes a simple DELETE with one :p_id parameter
        private void Execute(OracleConnection conn, string sql, int id)
        {
            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = id;
                cmd.ExecuteNonQuery();
            }
        }
    }
}