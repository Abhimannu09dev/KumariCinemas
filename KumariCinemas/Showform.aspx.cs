using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class ShowForm : Page
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
                LoadMovieDropdown();
                LoadHallDropdown();
                LoadShows();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD MOVIE DROPDOWN
        // ─────────────────────────────────────────────────────────────
        private void LoadMovieDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Movie_Id, Movie_Name
                                   FROM   Movie
                                   ORDER  BY Movie_Name";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        ddlMovie.DataSource = dt;
                        ddlMovie.DataTextField = "Movie_Name";
                        ddlMovie.DataValueField = "Movie_Id";
                        ddlMovie.DataBind();

                        ddlMovie.Items.Insert(0, new ListItem("-- Select Movie --", "0"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading movies: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD HALL DROPDOWN
        // Shows Hall Number + Location Name together e.g. "Hall A — QFX Civil Mall"
        // ─────────────────────────────────────────────────────────────
        private void LoadHallDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT h.Hall_Id,
                                          h.Hall_Number || ' — ' || l.Location_Name AS Hall_Label
                                   FROM   Hall h
                                   JOIN   Location l ON h.Location_Id = l.Location_Id
                                   ORDER  BY l.Location_Name, h.Hall_Number";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        ddlHall.DataSource = dt;
                        ddlHall.DataTextField = "Hall_Label";
                        ddlHall.DataValueField = "Hall_Id";
                        ddlHall.DataBind();

                        ddlHall.Items.Insert(0, new ListItem("-- Select Hall --", "0"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading halls: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD GRID
        // JOINs Movie + Hall + Location for readable display
        // ─────────────────────────────────────────────────────────────
        private void LoadShows()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT s.Show_Id,
                                          m.Movie_Name,
                                          h.Hall_Number || ' — ' || l.Location_Name AS Hall_Info,
                                          s.Show_Date,
                                          s.Show_Time
                                   FROM   Show     s
                                   JOIN   Movie    m ON s.Movie_Id   = m.Movie_Id
                                   JOIN   Hall     h ON s.Hall_Id    = h.Hall_Id
                                   JOIN   Location l ON h.Location_Id = l.Location_Id
                                   ORDER  BY s.Show_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvShows.DataSource = dt;
                        gvShows.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading shows: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // SAVE — INSERT or UPDATE
        // ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int showId = int.Parse(hfShowId.Value);
            bool isNew = (showId == 0);

            // Parse Show Date
            DateTime showDate;
            if (!DateTime.TryParse(txtShowDate.Text.Trim(), out showDate))
            {
                ShowMessage("✗ Invalid date. Use DD-MMM-YYYY (e.g. 10-Jan-2025).", isError: true);
                return;
            }

            // Parse Show Time — combine date + time into one TIMESTAMP
            TimeSpan showTime;
            if (!TimeSpan.TryParse(txtShowTime.Text.Trim(), out showTime))
            {
                ShowMessage("✗ Invalid time. Use HH:MM (e.g. 14:30).", isError: true);
                return;
            }

            // Combine date + time into a single DateTime for the TIMESTAMP column
            DateTime showTimestamp = showDate.Date + showTime;

            int selectedMovieId = int.Parse(ddlMovie.SelectedValue);
            int selectedHallId = int.Parse(ddlHall.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql;

                    if (isNew)
                    {
                        int newId = GetNextShowId(conn);

                        sql = @"INSERT INTO Show
                                    (Show_Id, Movie_Id, Hall_Id, Show_Date, Show_Time)
                                VALUES
                                    (:p_id, :p_movieId, :p_hallId, :p_date, :p_time)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_movieId", OracleDbType.Int32).Value = selectedMovieId;
                            cmd.Parameters.Add(":p_hallId", OracleDbType.Int32).Value = selectedHallId;
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = showDate;
                            cmd.Parameters.Add(":p_time", OracleDbType.TimeStamp).Value = showTimestamp;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Show added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        sql = @"UPDATE Show
                                SET Movie_Id  = :p_movieId,
                                    Hall_Id   = :p_hallId,
                                    Show_Date = :p_date,
                                    Show_Time = :p_time
                                WHERE Show_Id = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_movieId", OracleDbType.Int32).Value = selectedMovieId;
                            cmd.Parameters.Add(":p_hallId", OracleDbType.Int32).Value = selectedHallId;
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = showDate;
                            cmd.Parameters.Add(":p_time", OracleDbType.TimeStamp).Value = showTimestamp;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Show updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadShows();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error saving show: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GRID ROW COMMAND — Edit / Delete
        // ─────────────────────────────────────────────────────────────
        protected void gvShows_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int showId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditShow")
            {
                LoadShowIntoForm(showId);
            }
            else if (e.CommandName == "DeleteShow")
            {
                DeleteShow(showId);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD INTO FORM (editing)
        // ─────────────────────────────────────────────────────────────
        private void LoadShowIntoForm(int showId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Show_Id, Movie_Id, Hall_Id, Show_Date, Show_Time
                                   FROM   Show
                                   WHERE  Show_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfShowId.Value = reader["Show_Id"].ToString();

                                ddlMovie.SelectedValue = reader["Movie_Id"].ToString();
                                ddlHall.SelectedValue = reader["Hall_Id"].ToString();

                                DateTime date = Convert.ToDateTime(reader["Show_Date"]);
                                txtShowDate.Text = date.ToString("dd-MMM-yyyy");

                                // Show_Time is a TIMESTAMP — extract just the time part
                                DateTime time = Convert.ToDateTime(reader["Show_Time"]);
                                txtShowTime.Text = time.ToString("HH:mm");

                                lblFormTitle.Text = "✏ Edit Show (ID: " + showId + ")";
                                btnSave.Text = "💾 Update Show";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error loading show: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE — removes Tickets first, then Show
        // ─────────────────────────────────────────────────────────────
        private void DeleteShow(int showId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete Tickets linked to this Show
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Ticket WHERE Show_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showId;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Delete the Show itself
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Show WHERE Show_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Show and linked tickets deleted.", isError: false);
                LoadShows();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting show: " + ex.Message, isError: true);
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
            hfShowId.Value = "0";
            ddlMovie.SelectedIndex = 0;
            ddlHall.SelectedIndex = 0;
            txtShowDate.Text = "";
            txtShowTime.Text = "";
            lblFormTitle.Text = "Add New Show";
            btnSave.Text = "💾 Save Show";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        private int GetNextShowId(OracleConnection conn)
        {
            string sql = "SELECT NVL(MAX(Show_Id), 0) + 1 FROM Show";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}