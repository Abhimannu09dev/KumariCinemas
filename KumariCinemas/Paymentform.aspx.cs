using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class PaymentForm : Page
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
                LoadPayments();
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
        // LOAD GRID
        // JOINs Booking + User for readable display
        // ─────────────────────────────────────────────────────────────
        private void LoadPayments()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT p.Payment_Id,
                                          'Booking #' || b.Booking_ID ||
                                          ' (' || u.User_Name || ')' AS Booking_Info,
                                          p.Payment_Date,
                                          p.Payment_Amount,
                                          p.Payment_Status
                                   FROM   Payment p
                                   JOIN   Booking b ON p.Booking_ID = b.Booking_ID
                                   JOIN   ""User"" u ON b.User_Id   = u.User_ID
                                   ORDER  BY p.Payment_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvPayments.DataSource = dt;
                        gvPayments.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading payments: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // SAVE — INSERT or UPDATE
        // ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int paymentId = int.Parse(hfPaymentId.Value);
            bool isNew = (paymentId == 0);

            int selectedBookingId = int.Parse(ddlBooking.SelectedValue);

            // Parse date
            DateTime paymentDate;
            if (!DateTime.TryParse(txtPaymentDate.Text.Trim(), out paymentDate))
            {
                ShowMessage("✗ Invalid date. Use DD-MMM-YYYY (e.g. 08-Jan-2025).", isError: true);
                return;
            }

            // Parse time and combine into timestamp
            TimeSpan paymentTime;
            if (!TimeSpan.TryParse(txtPaymentTime.Text.Trim(), out paymentTime))
            {
                ShowMessage("✗ Invalid time. Use HH:MM (e.g. 09:15).", isError: true);
                return;
            }

            DateTime paymentTimestamp = paymentDate.Date + paymentTime;

            // Parse amount
            decimal amount;
            if (!decimal.TryParse(txtAmount.Text.Trim(), out amount))
            {
                ShowMessage("✗ Invalid amount. Enter a number e.g. 600.00", isError: true);
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
                        int newId = GetNextPaymentId(conn);

                        sql = @"INSERT INTO Payment
                                    (Payment_Id, Booking_ID, Payment_Date,
                                     Payment_Amount, Payment_Status)
                                VALUES
                                    (:p_id, :p_bookingId, :p_date,
                                     :p_amount, :p_status)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_bookingId", OracleDbType.Int32).Value = selectedBookingId;
                            cmd.Parameters.Add(":p_date", OracleDbType.TimeStamp).Value = paymentTimestamp;
                            cmd.Parameters.Add(":p_amount", OracleDbType.Decimal).Value = amount;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Payment added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        sql = @"UPDATE Payment
                                SET Booking_ID     = :p_bookingId,
                                    Payment_Date   = :p_date,
                                    Payment_Amount = :p_amount,
                                    Payment_Status = :p_status
                                WHERE Payment_Id = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_bookingId", OracleDbType.Int32).Value = selectedBookingId;
                            cmd.Parameters.Add(":p_date", OracleDbType.TimeStamp).Value = paymentTimestamp;
                            cmd.Parameters.Add(":p_amount", OracleDbType.Decimal).Value = amount;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = paymentId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Payment updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadPayments();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error saving payment: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GRID ROW COMMAND — Edit / Delete
        // ─────────────────────────────────────────────────────────────
        protected void gvPayments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int paymentId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditPayment")
            {
                LoadPaymentIntoForm(paymentId);
            }
            else if (e.CommandName == "DeletePayment")
            {
                DeletePayment(paymentId);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD INTO FORM (editing)
        // ─────────────────────────────────────────────────────────────
        private void LoadPaymentIntoForm(int paymentId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Payment_Id, Booking_ID, Payment_Date,
                                          Payment_Amount, Payment_Status
                                   FROM   Payment
                                   WHERE  Payment_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = paymentId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfPaymentId.Value = reader["Payment_Id"].ToString();

                                ddlBooking.SelectedValue = reader["Booking_ID"].ToString();

                                // Split TIMESTAMP back into date + time fields
                                DateTime ts = Convert.ToDateTime(reader["Payment_Date"]);
                                txtPaymentDate.Text = ts.ToString("dd-MMM-yyyy");
                                txtPaymentTime.Text = ts.ToString("HH:mm");

                                txtAmount.Text = Convert.ToDecimal(reader["Payment_Amount"])
                                                        .ToString("N2");

                                ddlStatus.SelectedValue = reader["Payment_Status"].ToString();

                                lblFormTitle.Text = "✏ Edit Payment (ID: " + paymentId + ")";
                                btnSave.Text = "💾 Update Payment";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error loading payment: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE — Payment has no children, safe to delete directly
        // ─────────────────────────────────────────────────────────────
        private void DeletePayment(int paymentId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Payment WHERE Payment_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = paymentId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Payment deleted successfully.", isError: false);
                LoadPayments();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting payment: " + ex.Message, isError: true);
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
            hfPaymentId.Value = "0";
            ddlBooking.SelectedIndex = 0;
            txtPaymentDate.Text = "";
            txtPaymentTime.Text = "";
            txtAmount.Text = "";
            ddlStatus.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Payment";
            btnSave.Text = "💾 Save Payment";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        private int GetNextPaymentId(OracleConnection conn)
        {
            string sql = "SELECT NVL(MAX(Payment_Id), 0) + 1 FROM Payment";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}