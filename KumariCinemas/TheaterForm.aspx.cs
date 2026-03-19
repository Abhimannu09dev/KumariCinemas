using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class TheaterForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCityDropdown();
                LoadTheaters();
            }
        }

        // ════════════════════════════════════════════
        // LOAD CITY DROPDOWN
        // Theater links to City via City_Theater junction
        // ════════════════════════════════════════════
        private void LoadCityDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "SELECT City_Id, CityName FROM City ORDER BY CityName", conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        ddlCity.DataSource = dt;
                        ddlCity.DataTextField = "CityName";
                        ddlCity.DataValueField = "City_Id";
                        ddlCity.DataBind();
                        ddlCity.Items.Insert(0, new ListItem("-- Select City --", "0"));
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading cities: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // LOAD THEATERS GRID
        // Join path: Theater → City_Theater → City
        // ════════════════════════════════════════════
        private void LoadTheaters()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT t.Theater_Id,
                               t.TheaterName,
                               c.CityName,
                               t.ContactNumber,
                               t.TheaterEmail
                        FROM   Theater      t
                        JOIN   City_Theater ct ON ct.Theater_Id = t.Theater_Id
                        JOIN   City         c  ON c.City_Id     = ct.City_Id
                        ORDER  BY t.Theater_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvTheaters.DataSource = dt;
                        gvTheaters.DataBind();
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading theaters: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // INSERT: writes to Theater then City_Theater junction
        // UPDATE: updates Theater + City_Theater junction
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int theaterId = int.Parse(hfTheaterId.Value);
            bool isNew = (theaterId == 0);
            int cityId = int.Parse(ddlCity.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // STEP 1: Get next Theater_Id from sequence
                        int newTheaterId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_theater_id.NEXTVAL FROM DUAL", conn))
                        {
                            newTheaterId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        // STEP 2: INSERT into Theater with ID as parameter
                        string sqlTheater = @"
                            INSERT INTO Theater (Theater_Id, TheaterName, ContactNumber, TheaterEmail)
                            VALUES (:p_id, :p_name, :p_contact, :p_email)";

                        using (var cmd = new OracleCommand(sqlTheater, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newTheaterId;
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtTheaterName.Text.Trim();
                            cmd.Parameters.Add(":p_contact", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtContact.Text)
                                                                                             ? (object)DBNull.Value : txtContact.Text.Trim();
                            cmd.Parameters.Add(":p_email", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtEmail.Text)
                                                                                             ? (object)DBNull.Value : txtEmail.Text.Trim();
                            cmd.ExecuteNonQuery();
                        }

                        // STEP 2: INSERT into City_Theater junction
                        using (var cmd = new OracleCommand(
                            "INSERT INTO City_Theater (City_Id, Theater_Id) VALUES (:p_cityId, :p_theaterId)", conn))
                        {
                            cmd.Parameters.Add(":p_cityId", OracleDbType.Int32).Value = cityId;
                            cmd.Parameters.Add(":p_theaterId", OracleDbType.Int32).Value = newTheaterId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Theater added successfully! (ID: " + newTheaterId + ")", false);
                    }
                    else
                    {
                        // UPDATE Theater fields
                        using (var cmd = new OracleCommand(@"
                            UPDATE Theater
                            SET    TheaterName   = :p_name,
                                   ContactNumber = :p_contact,
                                   TheaterEmail  = :p_email
                            WHERE  Theater_Id    = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtTheaterName.Text.Trim();
                            cmd.Parameters.Add(":p_contact", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtContact.Text)
                                                                                             ? (object)DBNull.Value : txtContact.Text.Trim();
                            cmd.Parameters.Add(":p_email", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtEmail.Text)
                                                                                             ? (object)DBNull.Value : txtEmail.Text.Trim();
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = theaterId;
                            cmd.ExecuteNonQuery();
                        }

                        // UPDATE City_Theater junction
                        using (var cmd = new OracleCommand(@"
                            UPDATE City_Theater
                            SET    City_Id    = :p_cityId
                            WHERE  Theater_Id = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_cityId", OracleDbType.Int32).Value = cityId;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = theaterId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Theater updated successfully!", false);
                    }
                }

                ClearForm();
                LoadTheaters();
            }
            catch (Exception ex) { ShowMessage("✗ Error saving theater: " + ex.Message, true); }
        }

        protected void gvTheaters_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int theaterId = int.Parse(e.CommandArgument.ToString());
            if (e.CommandName == "EditTheater") LoadTheaterIntoForm(theaterId);
            else if (e.CommandName == "DeleteTheater") DeleteTheater(theaterId);
        }

        private void LoadTheaterIntoForm(int theaterId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT t.Theater_Id, t.TheaterName, t.ContactNumber,
                               t.TheaterEmail, ct.City_Id
                        FROM   Theater      t
                        JOIN   City_Theater ct ON ct.Theater_Id = t.Theater_Id
                        WHERE  t.Theater_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = theaterId;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                hfTheaterId.Value = r["Theater_Id"].ToString();
                                txtTheaterName.Text = r["TheaterName"].ToString();
                                txtContact.Text = r["ContactNumber"] == DBNull.Value ? "" : r["ContactNumber"].ToString();
                                txtEmail.Text = r["TheaterEmail"] == DBNull.Value ? "" : r["TheaterEmail"].ToString();
                                ddlCity.SelectedValue = r["City_Id"].ToString();
                                lblFormTitle.Text = "✏ Edit Theater (ID: " + theaterId + ")";
                                btnSave.Text = "Update Theater";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("✗ Error loading theater: " + ex.Message, true); }
        }

        private void DeleteTheater(int theaterId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    // Delete Theater_Hall junction then City_Theater then Theater
                    Execute(conn, "DELETE FROM Theater_Hall WHERE Theater_Id = :p_id", theaterId);
                    Execute(conn, "DELETE FROM City_Theater WHERE Theater_Id = :p_id", theaterId);
                    Execute(conn, "DELETE FROM Theater      WHERE Theater_Id = :p_id", theaterId);
                }
                ShowMessage("✓ Theater deleted successfully.", false);
                LoadTheaters();
            }
            catch (Exception ex) { ShowMessage("✗ Error deleting theater: " + ex.Message, true); }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            hfTheaterId.Value = "0";
            txtTheaterName.Text = "";
            txtContact.Text = "";
            txtEmail.Text = "";
            ddlCity.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Theater";
            btnSave.Text = "Save Theater";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

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