using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class MovieOccupancyForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadMovieDropdown();
        }

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
            catch (Exception ex) { ShowMessage("Error loading movies: " + ex.Message); }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlMovie.SelectedValue == "0")
            {
                ShowMessage("Please select a movie.");
                return;
            }

            int movieId = int.Parse(ddlMovie.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT *
                        FROM (
                            SELECT
                                ROWNUM                                          AS Rank,
                                Movie_Title,
                                Genre,
                                Language,
                                TheaterName,
                                CityName,
                                HallName,
                                HallCapacity,
                                PaidTickets,
                                OccupancyPercentage
                            FROM (
                                SELECT
                                    m.Title                                     AS Movie_Title,
                                    m.Genre,
                                    m.Language,
                                    t.TheaterName,
                                    c.CityName,
                                    h.HallName,
                                    h.HallCapacity,
                                    COUNT(tk.Ticket_Id)                         AS PaidTickets,
                                    ROUND(
                                        (COUNT(tk.Ticket_Id) / h.HallCapacity) * 100, 2
                                    )                                           AS OccupancyPercentage
                                FROM   Movie        m
                                JOIN   Show         s   ON s.Movie_Id     = m.Movie_Id
                                JOIN   Hall_Show    hs  ON hs.Show_Id     = s.Show_Id
                                JOIN   Hall         h   ON h.Hall_Id      = hs.Hall_Id
                                JOIN   Theater_Hall thl ON thl.Hall_Id    = h.Hall_Id
                                JOIN   Theater      t   ON t.Theater_Id   = thl.Theater_Id
                                JOIN   City_Theater ct  ON ct.Theater_Id  = t.Theater_Id
                                JOIN   City         c   ON c.City_Id      = ct.City_Id
                                JOIN   Show_Ticket  stk ON stk.Show_Id    = s.Show_Id
                                JOIN   Ticket       tk  ON tk.Ticket_Id   = stk.Ticket_Id
                                                       AND tk.PaymentStatus = 'Paid'
                                WHERE  m.Movie_Id = :p_movieId
                                GROUP  BY
                                    m.Title, m.Genre, m.Language,
                                    t.TheaterName, c.CityName,
                                    h.HallName, h.HallCapacity
                                ORDER  BY OccupancyPercentage DESC
                            )
                        )
                        WHERE Rank <= 3";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_movieId", OracleDbType.Int32).Value = movieId;

                        using (var da = new OracleDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            gvResults.DataSource = dt;
                            gvResults.DataBind();
                            gvResults.Visible = true;

                            lblResultCount.Text = $"<i class='bi bi-trophy-fill'></i>  Top {dt.Rows.Count} hall(s) by occupancy for <strong>{ddlMovie.SelectedItem.Text}</strong>";
                            lblResultCount.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error running query: " + ex.Message); }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlMovie.SelectedIndex = 0;
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