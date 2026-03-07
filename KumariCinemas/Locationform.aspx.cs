using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace KumariCinemas
{
    public partial class LocationForm : Page
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
                LoadLocations();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD GRID
        // ─────────────────────────────────────────────────────────────
        private void LoadLocations()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Location_Id, Location_Name, Location_Address
                                   FROM   Location
                                   ORDER  BY Location_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvLocations.DataSource = dt;
                        gvLocations.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading locations: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // SAVE — INSERT or UPDATE
        // ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int locationId = int.Parse(hfLocationId.Value);
            bool isNew = (locationId == 0);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql;

                    if (isNew)
                    {
                        int newId = GetNextLocationId(conn);

                        sql = @"INSERT INTO Location
                                    (Location_Id, Location_Name, Location_Address)
                                VALUES
                                    (:p_id, :p_name, :p_address)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":p_address", OracleDbType.Varchar2).Value = txtAddress.Text.Trim();
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Location added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        sql = @"UPDATE Location
                                SET Location_Name    = :p_name,
                                    Location_Address = :p_address
                                WHERE Location_Id = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":p_address", OracleDbType.Varchar2).Value = txtAddress.Text.Trim();
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = locationId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Location updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadLocations();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error saving location: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GRID ROW COMMAND — Edit / Delete
        // ─────────────────────────────────────────────────────────────
        protected void gvLocations_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int locationId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditLocation")
            {
                LoadLocationIntoForm(locationId);
            }
            else if (e.CommandName == "DeleteLocation")
            {
                DeleteLocation(locationId);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD INTO FORM (editing)
        // ─────────────────────────────────────────────────────────────
        private void LoadLocationIntoForm(int locationId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Location_Id, Location_Name, Location_Address
                                   FROM   Location
                                   WHERE  Location_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = locationId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfLocationId.Value = reader["Location_Id"].ToString();
                                txtName.Text = reader["Location_Name"].ToString();
                                txtAddress.Text = reader["Location_Address"].ToString();

                                lblFormTitle.Text = "✏ Edit Location (ID: " + locationId + ")";
                                btnSave.Text = "💾 Update Location";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error loading location: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE
        // Hall has FK → Location, so delete child Halls first
        // Show has FK → Hall, Ticket → Show, so cascade all the way down
        // ─────────────────────────────────────────────────────────────
        private void DeleteLocation(int locationId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete Tickets linked to Shows in Halls at this Location
                    using (var cmd = new OracleCommand(
                        @"DELETE FROM Ticket
                          WHERE Show_Id IN (
                              SELECT s.Show_Id FROM Show s
                              JOIN Hall h ON s.Hall_Id = h.Hall_Id
                              WHERE h.Location_Id = :p_id)", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = locationId;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Delete Shows linked to Halls at this Location
                    using (var cmd = new OracleCommand(
                        @"DELETE FROM Show
                          WHERE Hall_Id IN (
                              SELECT Hall_Id FROM Hall WHERE Location_Id = :p_id)", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = locationId;
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Delete Halls at this Location
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Hall WHERE Location_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = locationId;
                        cmd.ExecuteNonQuery();
                    }

                    // 4. Delete the Location itself
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Location WHERE Location_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = locationId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Location and all linked halls/shows deleted.", isError: false);
                LoadLocations();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting location: " + ex.Message, isError: true);
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
            hfLocationId.Value = "0";
            txtName.Text = "";
            txtAddress.Text = "";
            lblFormTitle.Text = "Add New Location";
            btnSave.Text = "💾 Save Location";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        private int GetNextLocationId(OracleConnection conn)
        {
            string sql = "SELECT NVL(MAX(Location_Id), 0) + 1 FROM Location";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}