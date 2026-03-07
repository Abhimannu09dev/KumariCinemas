using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class HallForm : Page
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
                LoadLocationDropdown();
                LoadHalls();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD LOCATION DROPDOWN
        // ─────────────────────────────────────────────────────────────
        private void LoadLocationDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Location_Id, Location_Name
                                   FROM   Location
                                   ORDER  BY Location_Name";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        ddlLocation.DataSource = dt;
                        ddlLocation.DataTextField = "Location_Name";
                        ddlLocation.DataValueField = "Location_Id";
                        ddlLocation.DataBind();

                        ddlLocation.Items.Insert(0, new ListItem("-- Select Location --", "0"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading locations: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD GRID — JOINs Location to show name instead of ID
        // ─────────────────────────────────────────────────────────────
        private void LoadHalls()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT h.Hall_Id,
                                          l.Location_Name,
                                          h.Hall_Number,
                                          h.Hall_Capacity
                                   FROM   Hall h
                                   JOIN   Location l ON h.Location_Id = l.Location_Id
                                   ORDER  BY h.Hall_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvHalls.DataSource = dt;
                        gvHalls.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading halls: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // SAVE — INSERT or UPDATE
        // ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int hallId = int.Parse(hfHallId.Value);
            bool isNew = (hallId == 0);

            int selectedLocationId = int.Parse(ddlLocation.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql;

                    if (isNew)
                    {
                        int newId = GetNextHallId(conn);

                        sql = @"INSERT INTO Hall
                                    (Hall_Id, Location_Id, Hall_Number, Hall_Capacity)
                                VALUES
                                    (:p_id, :p_locationId, :p_number, :p_capacity)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_locationId", OracleDbType.Int32).Value = selectedLocationId;
                            cmd.Parameters.Add(":p_number", OracleDbType.Varchar2).Value = txtHallNumber.Text.Trim();
                            cmd.Parameters.Add(":p_capacity", OracleDbType.Int32).Value = int.Parse(txtCapacity.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Hall added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        sql = @"UPDATE Hall
                                SET Location_Id   = :p_locationId,
                                    Hall_Number   = :p_number,
                                    Hall_Capacity = :p_capacity
                                WHERE Hall_Id = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_locationId", OracleDbType.Int32).Value = selectedLocationId;
                            cmd.Parameters.Add(":p_number", OracleDbType.Varchar2).Value = txtHallNumber.Text.Trim();
                            cmd.Parameters.Add(":p_capacity", OracleDbType.Int32).Value = int.Parse(txtCapacity.Text.Trim());
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = hallId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Hall updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadHalls();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error saving hall: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GRID ROW COMMAND — Edit / Delete
        // ─────────────────────────────────────────────────────────────
        protected void gvHalls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int hallId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditHall")
            {
                LoadHallIntoForm(hallId);
            }
            else if (e.CommandName == "DeleteHall")
            {
                DeleteHall(hallId);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD INTO FORM (editing)
        // ─────────────────────────────────────────────────────────────
        private void LoadHallIntoForm(int hallId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Hall_Id, Location_Id, Hall_Number, Hall_Capacity
                                   FROM   Hall
                                   WHERE  Hall_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = hallId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfHallId.Value = reader["Hall_Id"].ToString();

                                // Set location dropdown to the saved value
                                ddlLocation.SelectedValue = reader["Location_Id"].ToString();

                                txtHallNumber.Text = reader["Hall_Number"].ToString();
                                txtCapacity.Text = reader["Hall_Capacity"].ToString();

                                lblFormTitle.Text = "✏ Edit Hall (ID: " + hallId + ")";
                                btnSave.Text = "💾 Update Hall";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error loading hall: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE
        // Hall → Show → Ticket (cascade before deleting Hall)
        // ─────────────────────────────────────────────────────────────
        private void DeleteHall(int hallId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete Tickets linked to Shows in this Hall
                    using (var cmd = new OracleCommand(
                        @"DELETE FROM Ticket
                          WHERE Show_Id IN (
                              SELECT Show_Id FROM Show WHERE Hall_Id = :p_id)", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = hallId;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Delete Shows in this Hall
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Show WHERE Hall_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = hallId;
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Delete the Hall itself
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Hall WHERE Hall_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = hallId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Hall and linked shows/tickets deleted.", isError: false);
                LoadHalls();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting hall: " + ex.Message, isError: true);
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
            hfHallId.Value = "0";
            ddlLocation.SelectedIndex = 0;
            txtHallNumber.Text = "";
            txtCapacity.Text = "";
            lblFormTitle.Text = "Add New Hall";
            btnSave.Text = "💾 Save Hall";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        private int GetNextHallId(OracleConnection conn)
        {
            string sql = "SELECT NVL(MAX(Hall_Id), 0) + 1 FROM Hall";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}