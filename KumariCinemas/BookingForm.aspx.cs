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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUserDropdown();   // populate User dropdown first
                LoadBookings();       // then load the grid
            }
        }

        // LOAD USER DROPDOWN
        // Populates ddlUser with all users from "User" table
        private void LoadUserDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT User_ID, User_Name
                                   FROM ""User""
                                   ORDER BY User_Name";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        ddlUser.DataSource = dt;
                        ddlUser.DataTextField = "User_Name";   // shown in dropdown
                        ddlUser.DataValueField = "User_ID";     // stored as value
                        ddlUser.DataBind();

                        // Add a blank "-- Select User --" option at the top
                        ddlUser.Items.Insert(0, new ListItem("-- Select User --", "0"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading users: " + ex.Message, isError: true);
            }
        }


        // LOAD BOOKINGS GRID
        // JOINs with "User" to show the user name instead of just ID
        private void LoadBookings()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT b.Booking_ID,
                                          u.User_Name,
                                          b.Booking_Date,
                                          b.Booking_Status
                                   FROM   Booking b
                                   JOIN   ""User"" u ON b.User_Id = u.User_ID
                                   ORDER  BY b.Booking_ID";

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

        
        // SAVE BUTTON
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int bookingId = int.Parse(hfBookingId.Value);
            bool isNew = (bookingId == 0);

            // Parse the date the user typed
            DateTime bookingDate;
            if (!DateTime.TryParse(txtBookingDate.Text.Trim(), out bookingDate))
            {
                ShowMessage("✗ Invalid date format. Please use DD-MMM-YYYY (e.g. 10-Jan-2025).", isError: true);
                return;
            }

            int selectedUserId = int.Parse(ddlUser.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql;

                    if (isNew)
                    {
                        // ── INSERT 
                        int newId = GetNextBookingId(conn);

                        sql = @"INSERT INTO Booking
                                    (Booking_ID, User_Id, Booking_Date, Booking_Status)
                                VALUES
                                    (:p_id, :p_userId, :p_date, :p_status)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_userId", OracleDbType.Int32).Value = selectedUserId;
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = bookingDate;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Booking added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        // ── UPDATE 
                        sql = @"UPDATE Booking
                                SET User_Id        = :p_userId,
                                    Booking_Date   = :p_date,
                                    Booking_Status = :p_status
                                WHERE Booking_ID = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_userId", OracleDbType.Int32).Value = selectedUserId;
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = bookingDate;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
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

        // GRID ROW COMMAND — Edit / Delete
        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int bookingId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditBooking")
            {
                LoadBookingIntoForm(bookingId);
            }
            else if (e.CommandName == "DeleteBooking")
            {
                DeleteBooking(bookingId);
            }
        }

        // LOAD BOOKING INTO FORM (for editing)
        private void LoadBookingIntoForm(int bookingId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Booking_ID, User_Id, Booking_Date, Booking_Status
                                   FROM   Booking
                                   WHERE  Booking_ID = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = bookingId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfBookingId.Value = reader["Booking_ID"].ToString();

                                // Set user dropdown to match saved User_Id
                                ddlUser.SelectedValue = reader["User_Id"].ToString();

                                // Format date for the textbox
                                DateTime d = Convert.ToDateTime(reader["Booking_Date"]);
                                txtBookingDate.Text = d.ToString("dd-MMM-yyyy");

                                // Set status dropdown
                                ddlStatus.SelectedValue = reader["Booking_Status"].ToString();

                                lblFormTitle.Text = "✏ Edit Booking (ID: " + bookingId + ")";
                                btnSave.Text = "💾 Update Booking";
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

        // DELETE BOOKING
        // Must delete child Ticket + Payment rows first (FK constraint)
        private void DeleteBooking(int bookingId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // Delete child records first to avoid FK violation
                    // 1. Delete Tickets linked to this booking
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Ticket WHERE Booking_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = bookingId;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Delete Payments linked to this booking
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Payment WHERE Booking_ID = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = bookingId;
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Now safe to delete the Booking itself
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Booking WHERE Booking_ID = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = bookingId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Booking (and linked tickets/payments) deleted.", isError: false);
                LoadBookings();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting booking: " + ex.Message, isError: true);
            }
        }

        
        // CLEAR FORM BUTTON
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // HELPERS\
        private void ClearForm()
        {
            hfBookingId.Value = "0";
            ddlUser.SelectedIndex = 0;          // back to Select User 
            txtBookingDate.Text = "";
            ddlStatus.SelectedIndex = 0;        // back to "Confirmed"
            lblFormTitle.Text = "Add New Booking";
            btnSave.Text = "💾 Save Booking";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        // Gets MAX(Booking_ID) + 1 as next ID
        private int GetNextBookingId(OracleConnection conn)
        {
            string sql = "SELECT NVL(MAX(Booking_ID), 0) + 1 FROM Booking";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}