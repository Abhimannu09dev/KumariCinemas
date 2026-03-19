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

        // ════════════════════════════════════════════
        // PAGE LOAD
        // ════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBookingDropdown();
                LoadPayments();
            }
        }

        // ════════════════════════════════════════════
        // LOAD BOOKING DROPDOWN
        // Join path: Booking → User_Booking → User
        // Shows "Booking #1 — Abhimannu (Confirmed)"
        // ════════════════════════════════════════════
        private void LoadBookingDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT b.Booking_Id,
                               'Booking #' || b.Booking_Id || ' — ' ||
                               u.Username || ' (' || b.BookingStatus || ')' AS Booking_Label
                        FROM   Booking      b
                        JOIN   User_Booking ub ON ub.Booking_Id = b.Booking_Id
                        JOIN   ""User""     u  ON u.User_Id     = ub.User_Id
                        ORDER  BY b.Booking_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        ddlBooking.DataSource = dt;
                        ddlBooking.DataTextField = "Booking_Label";
                        ddlBooking.DataValueField = "Booking_Id";
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

        // ════════════════════════════════════════════
        // LOAD PAYMENTS GRID
        // Join path: Payment → Booking_Payment → Booking → User_Booking → User
        // ════════════════════════════════════════════
        private void LoadPayments()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT p.Payment_Id,
                               'Booking #' || b.Booking_Id ||
                               ' (' || u.Username || ')' AS Booking_Info,
                               p.PaymentDate,
                               p.PaymentAmount,
                               p.PaymentStatus
                        FROM   Payment         p
                        JOIN   Booking_Payment bp ON bp.Payment_Id  = p.Payment_Id
                        JOIN   Booking         b  ON b.Booking_Id   = bp.Booking_Id
                        JOIN   User_Booking    ub ON ub.Booking_Id  = b.Booking_Id
                        JOIN   ""User""        u  ON u.User_Id      = ub.User_Id
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

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // INSERT: writes to Payment then Booking_Payment junction
        // UPDATE: updates Payment + Booking_Payment junction
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int paymentId = int.Parse(hfPaymentId.Value);
            bool isNew = (paymentId == 0);

            int selectedBookingId = int.Parse(ddlBooking.SelectedValue);

            DateTime paymentDate;
            if (!DateTime.TryParse(txtPaymentDate.Text.Trim(), out paymentDate))
            {
                ShowMessage("✗ Invalid date. Use DD-MMM-YYYY (e.g. 10-Jan-2025).", isError: true);
                return;
            }

            decimal amount;
            if (!decimal.TryParse(txtAmount.Text.Trim(), out amount) || amount <= 0)
            {
                ShowMessage("✗ Invalid amount. Enter a positive number e.g. 900.00", isError: true);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // STEP 1: Get next Payment_Id from sequence
                        int newPaymentId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_payment_id.NEXTVAL FROM DUAL", conn))
                        {
                            newPaymentId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        // STEP 2: INSERT into Payment with ID as parameter
                        string sqlPayment = @"
                            INSERT INTO Payment (Payment_Id, PaymentDate, PaymentAmount, PaymentStatus)
                            VALUES (:p_id, :p_date, :p_amount, :p_status)";

                        using (var cmd = new OracleCommand(sqlPayment, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newPaymentId;
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = paymentDate;
                            cmd.Parameters.Add(":p_amount", OracleDbType.Decimal).Value = amount;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }

                        // STEP 2: INSERT into Booking_Payment junction
                        string sqlJunction = @"
                            INSERT INTO Booking_Payment (Booking_Id, Payment_Id)
                            VALUES (:p_bookingId, :p_paymentId)";

                        using (var cmd = new OracleCommand(sqlJunction, conn))
                        {
                            cmd.Parameters.Add(":p_bookingId", OracleDbType.Int32).Value = selectedBookingId;
                            cmd.Parameters.Add(":p_paymentId", OracleDbType.Int32).Value = newPaymentId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Payment added successfully! (ID: " + newPaymentId + ")", isError: false);
                    }
                    else
                    {
                        // UPDATE Payment fields
                        string sqlUpdate = @"
                            UPDATE Payment
                            SET    PaymentDate   = :p_date,
                                   PaymentAmount = :p_amount,
                                   PaymentStatus = :p_status
                            WHERE  Payment_Id    = :p_id";

                        using (var cmd = new OracleCommand(sqlUpdate, conn))
                        {
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = paymentDate;
                            cmd.Parameters.Add(":p_amount", OracleDbType.Decimal).Value = amount;
                            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = paymentId;
                            cmd.ExecuteNonQuery();
                        }

                        // UPDATE Booking_Payment junction — change which booking this payment links to
                        string sqlJunction = @"
                            UPDATE Booking_Payment
                            SET    Booking_Id = :p_bookingId
                            WHERE  Payment_Id = :p_id";

                        using (var cmd = new OracleCommand(sqlJunction, conn))
                        {
                            cmd.Parameters.Add(":p_bookingId", OracleDbType.Int32).Value = selectedBookingId;
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

        // ════════════════════════════════════════════
        // GRID ROW COMMAND — Edit / Delete
        // ════════════════════════════════════════════
        protected void gvPayments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int paymentId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditPayment")
                LoadPaymentIntoForm(paymentId);
            else if (e.CommandName == "DeletePayment")
                DeletePayment(paymentId);
        }

        // ════════════════════════════════════════════
        // LOAD INTO FORM (edit)
        // Gets Booking_Id from Booking_Payment junction
        // ════════════════════════════════════════════
        private void LoadPaymentIntoForm(int paymentId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT p.Payment_Id,
                               p.PaymentDate,
                               p.PaymentAmount,
                               p.PaymentStatus,
                               bp.Booking_Id
                        FROM   Payment         p
                        JOIN   Booking_Payment bp ON bp.Payment_Id = p.Payment_Id
                        WHERE  p.Payment_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = paymentId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfPaymentId.Value = reader["Payment_Id"].ToString();

                                DateTime d = Convert.ToDateTime(reader["PaymentDate"]);
                                txtPaymentDate.Text = d.ToString("dd-MMM-yyyy");

                                txtAmount.Text = Convert.ToDecimal(reader["PaymentAmount"])
                                                        .ToString("N2");

                                ddlStatus.SelectedValue = reader["PaymentStatus"].ToString();
                                ddlBooking.SelectedValue = reader["Booking_Id"].ToString();

                                lblFormTitle.Text = "✏ Edit Payment (ID: " + paymentId + ")";
                                btnSave.Text = "Update Payment";
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

        // ════════════════════════════════════════════
        // DELETE
        // Must delete Booking_Payment junction first
        // ════════════════════════════════════════════
        private void DeletePayment(int paymentId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete from Booking_Payment junction first
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Booking_Payment WHERE Payment_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = paymentId;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Now safe to delete Payment
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

        // ════════════════════════════════════════════
        // CLEAR FORM
        // ════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfPaymentId.Value = "0";
            ddlBooking.SelectedIndex = 0;
            txtPaymentDate.Text = "";
            txtAmount.Text = "";
            ddlStatus.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Payment";
            btnSave.Text = "Save Payment";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }
    }
}