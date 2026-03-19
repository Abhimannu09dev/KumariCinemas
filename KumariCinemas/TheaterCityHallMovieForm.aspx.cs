using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class TheaterCityHallMovieForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadHallDropdown();
        }

        // ════════════════════════════════════════════
        // LOAD HALL DROPDOWN
        // Shows "Hall A — QFX Cinemas (Kathmandu)"
        // Join path: Hall → Theater_Hall → Theater → City_Theater → City
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
                               h.HallName || ' — ' || t.TheaterName ||
                               ' (' || c.CityName || ')' AS Hall_Label
                        FROM   Hall         h
                        JOIN   Theater_Hall thl ON thl.Hall_Id    = h.Hall_Id
                        JOIN   Theater      t   ON t.Theater_Id   = thl.Theater_Id
                        JOIN   City_Theater ct  ON ct.Theater_Id  = t.Theater_Id
                        JOIN   City         c   ON c.City_Id      = ct.City_Id
                        ORDER  BY c.CityName, t.TheaterName, h.HallName";

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
            catch (Exception ex) { ShowMessage("Error loading halls: " + ex.Message); }
        }

        // ════════════════════════════════════════════
        // COMPLEX QUERY 2 — TheaterCityHall Movie
        // For any hall, show movie and showtime details
        // Join path:
        //   Hall → Theater_Hall → Theater → City_Theater → City
        //   Hall → Hall_Show → Show
        //   Show → Movie (direct FK)
        //   Show → Showtime (direct FK)
        //   Show → Pricing (direct FK)
        // ════════════════════════════════════════════
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlHall.SelectedValue == "0")
            {
                ShowMessage("Please select a hall.");
                return;
            }

            int hallId = int.Parse(ddlHall.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT
                            h.Hall_Id,
                            h.HallName,
                            h.HallCapacity,
                            t.TheaterName,
                            c.CityName,
                            m.Title          AS Movie_Title,
                            m.Genre,
                            m.Language,
                            m.Duration,
                            s.ShowDate,
                            st.Showtime_Name AS Showtime,
                            p.Ticket_Price,
                            p.IsHolidayPricing AS IsHoliday
                        FROM   Hall         h
                        JOIN   Theater_Hall thl ON thl.Hall_Id    = h.Hall_Id
                        JOIN   Theater      t   ON t.Theater_Id   = thl.Theater_Id
                        JOIN   City_Theater ct  ON ct.Theater_Id  = t.Theater_Id
                        JOIN   City         c   ON c.City_Id      = ct.City_Id
                        JOIN   Hall_Show    hs  ON hs.Hall_Id     = h.Hall_Id
                        JOIN   Show         s   ON s.Show_Id      = hs.Show_Id
                        JOIN   Movie        m   ON m.Movie_Id     = s.Movie_Id
                        JOIN   Showtime     st  ON st.Showtime_Id = s.Showtime_Id
                        JOIN   Pricing      p   ON p.Pricing_Id   = s.Pricing_Id
                        WHERE  h.Hall_Id = :p_hallId
                        ORDER  BY s.ShowDate ASC, st.Showtime_Name ASC";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_hallId", OracleDbType.Int32).Value = hallId;

                        using (var da = new OracleDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            gvResults.DataSource = dt;
                            gvResults.DataBind();
                            gvResults.Visible = true;

                            lblResultCount.Text = $"<i class='bi bi-building-fill'></i>  Found {dt.Rows.Count} show(s) for <strong>{ddlHall.SelectedItem.Text}</strong>";
                            lblResultCount.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error running query: " + ex.Message); }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlHall.SelectedIndex = 0;
            gvResults.Visible = false;
            lblResultCount.Visible = false;
            lblMessage.Text = "";
        }

        private void ShowMessage(string msg)
        {
            lblMessage.Text = "⚠ " + msg;
        }
    }
}