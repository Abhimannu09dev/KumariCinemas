using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace KumariCinemas
{
    public partial class UserForm : Page
    {
        // ── Get connection string from web.config ─────────────────────
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
                LoadUsers();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD GRID — reads all users from Oracle
        // ─────────────────────────────────────────────────────────────
        private void LoadUsers()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // NOTE: "User" needs double quotes because USER is
                    // a reserved word in Oracle
                    string sql = @"SELECT User_ID, User_Name, User_Contact,
                                          User_Address, User_Email
                                   FROM ""User""
                                   ORDER BY User_ID";

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

        // ─────────────────────────────────────────────────────────────
        // SAVE BUTTON — handles both INSERT (new) and UPDATE (edit)
        // ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int userId = int.Parse(hfUserId.Value);
            bool isNew = (userId == 0);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    string sql;

                    if (isNew)
                    {
                        // ── INSERT ────────────────────────────────────
                        // Get next available ID
                        int newId = GetNextUserId(conn);

                        sql = @"INSERT INTO ""User""
                                    (User_ID, User_Name, User_Contact, User_Address, User_Email)
                                VALUES
                                    (:p_id, :p_name, :p_contact, :p_address, :p_email)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            // Oracle uses :paramName — NOT @paramName like SQL Server!
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":p_contact", OracleDbType.Varchar2).Value = txtContact.Text.Trim();
                            cmd.Parameters.Add(":p_address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text)
                                                                                                    ? (object)DBNull.Value
                                                                                                    : txtAddress.Text.Trim();
                            cmd.Parameters.Add(":p_email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();

                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ User added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        // ── UPDATE ────────────────────────────────────
                        sql = @"UPDATE ""User""
                                SET User_Name    = :p_name,
                                    User_Contact = :p_contact,
                                    User_Address = :p_address,
                                    User_Email   = :p_email
                                WHERE User_ID = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":p_contact", OracleDbType.Varchar2).Value = txtContact.Text.Trim();
                            cmd.Parameters.Add(":p_address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text)
                                                                                                ? (object)DBNull.Value
                                                                                                : txtAddress.Text.Trim();
                            cmd.Parameters.Add(":p_email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = userId;

                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ User updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadUsers();  // refresh grid
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving user: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GRID ROW COMMAND — handles Edit and Delete button clicks
        // ─────────────────────────────────────────────────────────────
        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditUser")
            {
                LoadUserIntoForm(userId);
            }
            else if (e.CommandName == "DeleteUser")
            {
                DeleteUser(userId);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD USER INTO FORM (for editing)
        // ─────────────────────────────────────────────────────────────
        private void LoadUserIntoForm(int userId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT User_ID, User_Name, User_Contact,
                                          User_Address, User_Email
                                   FROM ""User""
                                   WHERE User_ID = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = userId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfUserId.Value = reader["User_ID"].ToString();
                                txtName.Text = reader["User_Name"].ToString();
                                txtContact.Text = reader["User_Contact"].ToString();
                                txtAddress.Text = reader["User_Address"] == DBNull.Value
                                                     ? "" : reader["User_Address"].ToString();
                                txtEmail.Text = reader["User_Email"].ToString();

                                lblFormTitle.Text = "✏ Edit User (ID: " + userId + ")";
                                btnSave.Text = "💾 Update User";
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

        // ─────────────────────────────────────────────────────────────
        // DELETE USER
        // ─────────────────────────────────────────────────────────────
        private void DeleteUser(int userId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"DELETE FROM ""User"" WHERE User_ID = :p_id";

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
                // ORA-02292: child records exist (user has bookings)
                ShowMessage("✗ Cannot delete — this user has existing bookings. Cancel their bookings first.", isError: true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting user: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // CLEAR FORM BUTTON
        // ─────────────────────────────────────────────────────────────
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // ─────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────
        private void ClearForm()
        {
            hfUserId.Value = "0";
            txtName.Text = "";
            txtContact.Text = "";
            txtAddress.Text = "";
            txtEmail.Text = "";
            lblFormTitle.Text = "Add New User";
            btnSave.Text = "💾 Save User";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        // Gets MAX(User_ID) + 1 as the next ID
        // (In production you'd use a SEQUENCE, but this matches your current schema)
        private int GetNextUserId(OracleConnection conn)
        {
            string sql = @"SELECT NVL(MAX(User_ID), 0) + 1 FROM ""User""";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}