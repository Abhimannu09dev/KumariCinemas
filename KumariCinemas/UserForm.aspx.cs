using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class UserForm : Page
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
                LoadUsers();
        }

        // ════════════════════════════════════════════
        // LOAD GRID
        // ════════════════════════════════════════════
        private void LoadUsers()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    // NOTE: "User" needs double quotes — USER is reserved in Oracle
                    string sql = @"SELECT User_Id, Username, Email, Phone,
                                          Address, CreatedAt
                                   FROM   ""User""
                                   ORDER  BY User_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvUsers.DataSource = dt;
                        gvUsers.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading users: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int userId = int.Parse(hfUserId.Value);
            bool isNew = (userId == 0);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // STEP 1: Get next ID from sequence
                        int newId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_user_id.NEXTVAL FROM DUAL", conn))
                        {
                            newId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        // STEP 2: INSERT with the ID as a parameter
                        string sql = @"INSERT INTO ""User""
                       (User_Id, Username, Email, Phone, Password, Address)
                   VALUES
                       (:p_id, :p_username, :p_email, :p_phone,
                        :p_password, :p_address)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_username", OracleDbType.Varchar2).Value = txtUsername.Text.Trim();
                            cmd.Parameters.Add(":p_email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                            cmd.Parameters.Add(":p_phone", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtPhone.Text)
                                                                                              ? (object)DBNull.Value
                                                                                              : txtPhone.Text.Trim();
                            cmd.Parameters.Add(":p_password", OracleDbType.Varchar2).Value = txtPassword.Text.Trim();
                            cmd.Parameters.Add(":p_address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text)
                                                                                              ? (object)DBNull.Value
                                                                                              : txtAddress.Text.Trim();
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ User added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        // UPDATE — password only updated if field is not empty
                        string sql = @"UPDATE ""User""
                                       SET    Username = :p_username,
                                              Email    = :p_email,
                                              Phone    = :p_phone,
                                              Address  = :p_address"
                                     + (string.IsNullOrEmpty(txtPassword.Text)
                                            ? "" : ", Password = :p_password") +
                                     @" WHERE User_Id  = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_username", OracleDbType.Varchar2).Value = txtUsername.Text.Trim();
                            cmd.Parameters.Add(":p_email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                            cmd.Parameters.Add(":p_phone", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtPhone.Text)
                                                                                              ? (object)DBNull.Value
                                                                                              : txtPhone.Text.Trim();
                            cmd.Parameters.Add(":p_address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text)
                                                                                              ? (object)DBNull.Value
                                                                                              : txtAddress.Text.Trim();
                            if (!string.IsNullOrEmpty(txtPassword.Text))
                                cmd.Parameters.Add(":p_password", OracleDbType.Varchar2).Value = txtPassword.Text.Trim();

                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = userId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ User updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving user: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // GRID ROW COMMAND — Edit / Delete
        // ════════════════════════════════════════════
        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditUser")
                LoadUserIntoForm(userId);
            else if (e.CommandName == "DeleteUser")
                DeleteUser(userId);
        }

        // ════════════════════════════════════════════
        // LOAD USER INTO FORM (edit)
        // ════════════════════════════════════════════
        private void LoadUserIntoForm(int userId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT User_Id, Username, Email, Phone, Address
                                   FROM   ""User""
                                   WHERE  User_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = userId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfUserId.Value = reader["User_Id"].ToString();
                                txtUsername.Text = reader["Username"].ToString();
                                txtEmail.Text = reader["Email"].ToString();
                                txtPhone.Text = reader["Phone"] == DBNull.Value
                                                    ? "" : reader["Phone"].ToString();
                                txtAddress.Text = reader["Address"] == DBNull.Value
                                                    ? "" : reader["Address"].ToString();
                                txtPassword.Text = ""; // never show password

                                lblFormTitle.Text = "✏ Edit User (ID: " + userId + ")";
                                btnSave.Text = "Update User";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading user: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // DELETE USER
        // Cannot delete if user has entries in User_Booking
        // ════════════════════════════════════════════
        private void DeleteUser(int userId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"DELETE FROM ""User"" WHERE User_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = userId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ User deleted successfully.", isError: false);
                LoadUsers();
            }
            catch (OracleException oex) when (oex.Number == 2292)
            {
                // ORA-02292: child records exist in User_Booking or Ticket
                ShowMessage("✗ Cannot delete — this user has existing bookings or tickets.", isError: true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting user: " + ex.Message, isError: true);
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
            hfUserId.Value = "0";
            txtUsername.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtPassword.Text = "";
            txtAddress.Text = "";
            lblFormTitle.Text = "Add New User";
            btnSave.Text = "Save User";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }
    }
}