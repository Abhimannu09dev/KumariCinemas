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

        // ════════════════════════════════════════════
        // PAGE LOAD
        // ════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTheaterDropdown();
                LoadHalls();
            }
        }

        // ════════════════════════════════════════════
        // LOAD THEATER DROPDOWN
        // Hall links to Theater via Theater_Hall junction
        // ════════════════════════════════════════════
        private void LoadTheaterDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Theater_Id, TheaterName
                                   FROM   Theater
                                   ORDER  BY TheaterName";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        ddlTheater.DataSource = dt;
                        ddlTheater.DataTextField = "TheaterName";
                        ddlTheater.DataValueField = "Theater_Id";
                        ddlTheater.DataBind();
                        ddlTheater.Items.Insert(0, new ListItem("-- Select Theater --", "0"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading theaters: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // LOAD HALLS GRID
        // Join path: Hall → Theater_Hall → Theater
        // ════════════════════════════════════════════
        private void LoadHalls()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT h.Hall_Id,
                               h.HallName,
                               h.HallCapacity,
                               t.TheaterName
                        FROM   Hall         h
                        JOIN   Theater_Hall th ON th.Hall_Id    = h.Hall_Id
                        JOIN   Theater      t  ON t.Theater_Id  = th.Theater_Id
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

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // INSERT: writes to Hall then Theater_Hall junction
        // UPDATE: updates Hall + Theater_Hall junction
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int hallId = int.Parse(hfHallId.Value);
            bool isNew = (hallId == 0);

            int selectedTheaterId = int.Parse(ddlTheater.SelectedValue);

            int capacity;
            if (!int.TryParse(txtCapacity.Text.Trim(), out capacity) || capacity < 1)
            {
                ShowMessage("✗ Invalid capacity. Enter a positive number.", isError: true);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // STEP 1: Get next Hall_Id from sequence
                        int newHallId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_hall_id.NEXTVAL FROM DUAL", conn))
                        {
                            newHallId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        // STEP 2: INSERT into Hall with ID as parameter
                        string sqlHall = @"INSERT INTO Hall (Hall_Id, HallName, HallCapacity)
                                           VALUES (:p_id, :p_name, :p_capacity)";

                        using (var cmd = new OracleCommand(sqlHall, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newHallId;
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtHallName.Text.Trim();
                            cmd.Parameters.Add(":p_capacity", OracleDbType.Int32).Value = capacity;
                            cmd.ExecuteNonQuery();
                        }

                        // STEP 2: INSERT into Theater_Hall junction
                        string sqlJunction = @"INSERT INTO Theater_Hall (Theater_Id, Hall_Id)
                                               VALUES (:p_theaterId, :p_hallId)";

                        using (var cmd = new OracleCommand(sqlJunction, conn))
                        {
                            cmd.Parameters.Add(":p_theaterId", OracleDbType.Int32).Value = selectedTheaterId;
                            cmd.Parameters.Add(":p_hallId", OracleDbType.Int32).Value = newHallId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Hall added successfully! (ID: " + newHallId + ")", isError: false);
                    }
                    else
                    {
                        // UPDATE Hall fields
                        string sqlUpdate = @"UPDATE Hall
                                             SET    HallName     = :p_name,
                                                    HallCapacity = :p_capacity
                                             WHERE  Hall_Id      = :p_id";

                        using (var cmd = new OracleCommand(sqlUpdate, conn))
                        {
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtHallName.Text.Trim();
                            cmd.Parameters.Add(":p_capacity", OracleDbType.Int32).Value = capacity;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = hallId;
                            cmd.ExecuteNonQuery();
                        }

                        // UPDATE Theater_Hall junction — change which theater owns this hall
                        string sqlJunction = @"UPDATE Theater_Hall
                                               SET    Theater_Id = :p_theaterId
                                               WHERE  Hall_Id    = :p_id";

                        using (var cmd = new OracleCommand(sqlJunction, conn))
                        {
                            cmd.Parameters.Add(":p_theaterId", OracleDbType.Int32).Value = selectedTheaterId;
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

        // ════════════════════════════════════════════
        // GRID ROW COMMAND — Edit / Delete
        // ════════════════════════════════════════════
        protected void gvHalls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int hallId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditHall")
                LoadHallIntoForm(hallId);
            else if (e.CommandName == "DeleteHall")
                DeleteHall(hallId);
        }

        // ════════════════════════════════════════════
        // LOAD INTO FORM (edit)
        // Gets Theater_Id from Theater_Hall junction
        // ════════════════════════════════════════════
        private void LoadHallIntoForm(int hallId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT h.Hall_Id,
                               h.HallName,
                               h.HallCapacity,
                               th.Theater_Id
                        FROM   Hall         h
                        JOIN   Theater_Hall th ON th.Hall_Id = h.Hall_Id
                        WHERE  h.Hall_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = hallId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfHallId.Value = reader["Hall_Id"].ToString();
                                txtHallName.Text = reader["HallName"].ToString();
                                txtCapacity.Text = reader["HallCapacity"].ToString();
                                ddlTheater.SelectedValue = reader["Theater_Id"].ToString();

                                lblFormTitle.Text = "✏ Edit Hall (ID: " + hallId + ")";
                                btnSave.Text = "Update Hall";
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

        // ════════════════════════════════════════════
        // DELETE HALL
        // FK delete order:
        // Show_Ticket → Hall_Show → Theater_Hall → Hall
        // ════════════════════════════════════════════
        private void DeleteHall(int hallId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete Show_Ticket for all shows in this hall
                    Execute(conn, @"DELETE FROM Show_Ticket
                                    WHERE Show_Id IN
                                        (SELECT Show_Id FROM Hall_Show WHERE Hall_Id = :p_id)", hallId);

                    // 2. Delete Hall_Show junction
                    Execute(conn, "DELETE FROM Hall_Show WHERE Hall_Id = :p_id", hallId);

                    // 3. Delete Theater_Hall junction
                    Execute(conn, "DELETE FROM Theater_Hall WHERE Hall_Id = :p_id", hallId);

                    // 4. Now safe to delete Hall
                    Execute(conn, "DELETE FROM Hall WHERE Hall_Id = :p_id", hallId);
                }

                ShowMessage("✓ Hall and all linked records deleted.", isError: false);
                LoadHalls();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting hall: " + ex.Message, isError: true);
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
            hfHallId.Value = "0";
            ddlTheater.SelectedIndex = 0;
            txtHallName.Text = "";
            txtCapacity.Text = "";
            lblFormTitle.Text = "Add New Hall";
            btnSave.Text = "Save Hall";
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