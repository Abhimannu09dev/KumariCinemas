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

        // ════════════════════════════════════════════
        // PAGE LOAD
        // ════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMovieDropdown();
                LoadHallDropdown();
                LoadShowtimeDropdown();
                LoadPricingDropdown();
                LoadShows();
            }
        }

        // ════════════════════════════════════════════
        // LOAD MOVIE DROPDOWN
        // ════════════════════════════════════════════
        private void LoadMovieDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "SELECT Movie_Id, Title FROM Movie ORDER BY Title", conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        ddlMovie.DataSource = dt;
                        ddlMovie.DataTextField = "Title";
                        ddlMovie.DataValueField = "Movie_Id";
                        ddlMovie.DataBind();
                        ddlMovie.Items.Insert(0, new ListItem("-- Select Movie --", "0"));
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading movies: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // LOAD HALL DROPDOWN
        // Shows "Hall A — QFX Cinemas" via Theater_Hall → Theater join
        // ════════════════════════════════════════════
        private void LoadHallDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT h.Hall_Id,
                               h.HallName || ' — ' || t.TheaterName AS Hall_Label
                        FROM   Hall         h
                        JOIN   Theater_Hall th ON th.Hall_Id    = h.Hall_Id
                        JOIN   Theater      t  ON t.Theater_Id  = th.Theater_Id
                        ORDER  BY t.TheaterName, h.HallName";

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
            catch (Exception ex) { ShowMessage("Error loading halls: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // LOAD SHOWTIME DROPDOWN
        // ════════════════════════════════════════════
        private void LoadShowtimeDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "SELECT Showtime_Id, Showtime_Name FROM Showtime ORDER BY Showtime_Id", conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        ddlShowtime.DataSource = dt;
                        ddlShowtime.DataTextField = "Showtime_Name";
                        ddlShowtime.DataValueField = "Showtime_Id";
                        ddlShowtime.DataBind();
                        ddlShowtime.Items.Insert(0, new ListItem("-- Select Showtime --", "0"));
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading showtimes: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // LOAD PRICING DROPDOWN
        // Shows "NPR 300.00 (Regular)" or "NPR 600.00 (Holiday)"
        // ════════════════════════════════════════════
        private void LoadPricingDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT Pricing_Id,
                               'NPR ' || TO_CHAR(Ticket_Price, 'FM99990.00') ||
                               CASE WHEN IsHolidayPricing = 'Y'
                                    THEN ' (Holiday)'
                                    ELSE ' (Regular)'
                               END AS Price_Label
                        FROM   Pricing
                        ORDER  BY Ticket_Price";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        ddlPricing.DataSource = dt;
                        ddlPricing.DataTextField = "Price_Label";
                        ddlPricing.DataValueField = "Pricing_Id";
                        ddlPricing.DataBind();
                        ddlPricing.Items.Insert(0, new ListItem("-- Select Pricing --", "0"));
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading pricing: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // LOAD SHOWS GRID
        // Join path: Show → Movie (direct FK)
        //            Show → Hall_Show → Hall → Theater_Hall → Theater
        //            Show → Showtime (direct FK)
        //            Show → Pricing (direct FK)
        // ════════════════════════════════════════════
        private void LoadShows()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT s.Show_Id,
                               m.Title                                      AS Movie_Title,
                               h.HallName || ' — ' || t.TheaterName         AS Hall_Info,
                               s.ShowDate,
                               st.Showtime_Name                             AS Showtime,
                               p.Ticket_Price
                        FROM   Show         s
                        JOIN   Movie        m  ON m.Movie_Id    = s.Movie_Id
                        JOIN   Showtime     st ON st.Showtime_Id = s.Showtime_Id
                        JOIN   Pricing      p  ON p.Pricing_Id  = s.Pricing_Id
                        JOIN   Hall_Show    hs ON hs.Show_Id    = s.Show_Id
                        JOIN   Hall         h  ON h.Hall_Id     = hs.Hall_Id
                        JOIN   Theater_Hall th ON th.Hall_Id    = h.Hall_Id
                        JOIN   Theater      t  ON t.Theater_Id  = th.Theater_Id
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
            catch (Exception ex) { ShowMessage("Error loading shows: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // INSERT: Show table + Hall_Show junction
        // UPDATE: Show table + Hall_Show junction
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int showId = int.Parse(hfShowId.Value);
            bool isNew = (showId == 0);

            DateTime showDate;
            if (!DateTime.TryParse(txtShowDate.Text.Trim(), out showDate))
            {
                ShowMessage("✗ Invalid date. Use DD-MMM-YYYY (e.g. 15-Jan-2025).", true);
                return;
            }

            int movieId = int.Parse(ddlMovie.SelectedValue);
            int hallId = int.Parse(ddlHall.SelectedValue);
            int showtimeId = int.Parse(ddlShowtime.SelectedValue);
            int pricingId = int.Parse(ddlPricing.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // STEP 1: Get next Show_Id from sequence
                        int newShowId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_show_id.NEXTVAL FROM DUAL", conn))
                        {
                            newShowId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        // STEP 2: INSERT into Show with ID as parameter
                        string sqlShow = @"
                            INSERT INTO Show (Show_Id, ShowDate, Movie_Id, Showtime_Id, Pricing_Id)
                            VALUES (:p_id, :p_date, :p_movieId, :p_showtimeId, :p_pricingId)";

                        using (var cmd = new OracleCommand(sqlShow, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newShowId;
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = showDate;
                            cmd.Parameters.Add(":p_movieId", OracleDbType.Int32).Value = movieId;
                            cmd.Parameters.Add(":p_showtimeId", OracleDbType.Int32).Value = showtimeId;
                            cmd.Parameters.Add(":p_pricingId", OracleDbType.Int32).Value = pricingId;
                            cmd.ExecuteNonQuery();
                        }

                        // STEP 2: INSERT into Hall_Show junction
                        using (var cmd = new OracleCommand(
                            "INSERT INTO Hall_Show (Hall_Id, Show_Id) VALUES (:p_hallId, :p_showId)", conn))
                        {
                            cmd.Parameters.Add(":p_hallId", OracleDbType.Int32).Value = hallId;
                            cmd.Parameters.Add(":p_showId", OracleDbType.Int32).Value = newShowId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Show added successfully! (ID: " + newShowId + ")", false);
                    }
                    else
                    {
                        // UPDATE Show fields
                        using (var cmd = new OracleCommand(@"
                            UPDATE Show
                            SET    ShowDate     = :p_date,
                                   Movie_Id     = :p_movieId,
                                   Showtime_Id  = :p_showtimeId,
                                   Pricing_Id   = :p_pricingId
                            WHERE  Show_Id      = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_date", OracleDbType.Date).Value = showDate;
                            cmd.Parameters.Add(":p_movieId", OracleDbType.Int32).Value = movieId;
                            cmd.Parameters.Add(":p_showtimeId", OracleDbType.Int32).Value = showtimeId;
                            cmd.Parameters.Add(":p_pricingId", OracleDbType.Int32).Value = pricingId;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showId;
                            cmd.ExecuteNonQuery();
                        }

                        // UPDATE Hall_Show junction — change which hall runs this show
                        using (var cmd = new OracleCommand(@"
                            UPDATE Hall_Show
                            SET    Hall_Id = :p_hallId
                            WHERE  Show_Id = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_hallId", OracleDbType.Int32).Value = hallId;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Show updated successfully!", false);
                    }
                }

                ClearForm();
                LoadShows();
            }
            catch (Exception ex) { ShowMessage("✗ Error saving show: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // GRID ROW COMMAND — Edit / Delete
        // ════════════════════════════════════════════
        protected void gvShows_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int showId = int.Parse(e.CommandArgument.ToString());
            if (e.CommandName == "EditShow") LoadShowIntoForm(showId);
            else if (e.CommandName == "DeleteShow") DeleteShow(showId);
        }

        // ════════════════════════════════════════════
        // LOAD INTO FORM (edit)
        // Gets Hall_Id from Hall_Show junction
        // ════════════════════════════════════════════
        private void LoadShowIntoForm(int showId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT s.Show_Id, s.ShowDate, s.Movie_Id,
                               s.Showtime_Id, s.Pricing_Id, hs.Hall_Id
                        FROM   Show      s
                        JOIN   Hall_Show hs ON hs.Show_Id = s.Show_Id
                        WHERE  s.Show_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showId;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                hfShowId.Value = r["Show_Id"].ToString();
                                ddlMovie.SelectedValue = r["Movie_Id"].ToString();
                                ddlHall.SelectedValue = r["Hall_Id"].ToString();
                                ddlShowtime.SelectedValue = r["Showtime_Id"].ToString();
                                ddlPricing.SelectedValue = r["Pricing_Id"].ToString();

                                DateTime d = Convert.ToDateTime(r["ShowDate"]);
                                txtShowDate.Text = d.ToString("dd-MMM-yyyy");

                                lblFormTitle.Text = "✏ Edit Show (ID: " + showId + ")";
                                btnSave.Text = "Update Show";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("✗ Error loading show: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // DELETE SHOW
        // FK order: Show_Ticket → Hall_Show → Show
        // ════════════════════════════════════════════
        private void DeleteShow(int showId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    Execute(conn, "DELETE FROM Show_Ticket WHERE Show_Id = :p_id", showId);
                    Execute(conn, "DELETE FROM Hall_Show   WHERE Show_Id = :p_id", showId);
                    Execute(conn, "DELETE FROM Show        WHERE Show_Id = :p_id", showId);
                }
                ShowMessage("✓ Show and linked tickets deleted.", false);
                LoadShows();
            }
            catch (Exception ex) { ShowMessage("✗ Error deleting show: " + ex.Message, true); }
        }

        // ════════════════════════════════════════════
        // CLEAR FORM
        // ════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            hfShowId.Value = "0";
            ddlMovie.SelectedIndex = 0;
            ddlHall.SelectedIndex = 0;
            ddlShowtime.SelectedIndex = 0;
            ddlPricing.SelectedIndex = 0;
            txtShowDate.Text = "";
            lblFormTitle.Text = "Add New Show";
            btnSave.Text = "Save Show";
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