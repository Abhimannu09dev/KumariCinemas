using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class Dashboard : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"]
                                    .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStatCards();
                LoadMovieChart();
                LoadStatusChart();
                LoadRecentBookings();
            }
        }

        
        private void LoadStatCards()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT
                            (SELECT COUNT(*) FROM ""User"")    AS user_count,
                            (SELECT COUNT(*) FROM Booking)     AS booking_count,
                            (SELECT COUNT(*) FROM Movie)       AS movie_count,
                            (SELECT COUNT(*) FROM Show)        AS show_count,
                            (SELECT COUNT(*) FROM Hall)        AS hall_count,
                            (SELECT COUNT(*) FROM Theater)     AS theater_count,
                            (SELECT COUNT(*) FROM Ticket)      AS ticket_count,
                            (SELECT NVL(SUM(PaymentAmount), 0)
                             FROM   Payment
                             WHERE  PaymentStatus = 'Paid')    AS total_revenue
                        FROM DUAL";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblUsers.Text = reader["user_count"].ToString();
                            lblBookings.Text = reader["booking_count"].ToString();
                            lblMovies.Text = reader["movie_count"].ToString();
                            lblShows.Text = reader["show_count"].ToString();
                            lblHalls.Text = reader["hall_count"].ToString();
                            lblTheaters.Text = reader["theater_count"].ToString();
                            lblTickets.Text = reader["ticket_count"].ToString();

                            decimal revenue = Convert.ToDecimal(reader["total_revenue"]);
                            lblRevenue.Text = revenue.ToString("N0");
                        }
                    }
                }
            }
            catch
            {
                lblUsers.Text = lblBookings.Text = lblMovies.Text =
                lblShows.Text = lblHalls.Text = lblTheaters.Text =
                lblTickets.Text = lblRevenue.Text = "—";
            }
        }

        private void LoadMovieChart()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT m.Title,
                               COUNT(t.Ticket_Id) AS Ticket_Count
                        FROM   Movie m
                        LEFT JOIN Show        s   ON s.Movie_Id    = m.Movie_Id
                        LEFT JOIN Show_Ticket st  ON st.Show_Id    = s.Show_Id
                        LEFT JOIN Ticket      t   ON t.Ticket_Id   = st.Ticket_Id
                                                 AND t.PaymentStatus = 'Paid'
                        GROUP BY m.Title
                        ORDER BY Ticket_Count DESC";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        var labels = dt.AsEnumerable()
                                       .Select(r => r["Title"].ToString().Replace("|", ""));
                        var counts = dt.AsEnumerable()
                                       .Select(r => r["Ticket_Count"].ToString());

                        hfMovieLabels.Value = string.Join("|", labels);
                        hfMovieCounts.Value = string.Join("|", counts);
                    }
                }
            }
            catch
            {
                hfMovieLabels.Value = "No Data";
                hfMovieCounts.Value = "0";
            }
        }

        private void LoadStatusChart()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT BookingStatus,
                               COUNT(*) AS Status_Count
                        FROM   Booking
                        GROUP  BY BookingStatus
                        ORDER  BY BookingStatus";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        var labels = dt.AsEnumerable()
                                       .Select(r => r["BookingStatus"].ToString());
                        var counts = dt.AsEnumerable()
                                       .Select(r => r["Status_Count"].ToString());

                        hfStatusLabels.Value = string.Join("|", labels);
                        hfStatusCounts.Value = string.Join("|", counts);
                    }
                }
            }
            catch
            {
                hfStatusLabels.Value = "No Data";
                hfStatusCounts.Value = "0";
            }
        }

        private void LoadRecentBookings()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT * FROM (
                            SELECT
                                b.Booking_Id,
                                u.Username,
                                (SELECT m.Title
                                 FROM   Booking_Movie bm
                                 JOIN   Movie         m  ON m.Movie_Id = bm.Movie_Id
                                 WHERE  bm.Booking_Id = b.Booking_Id
                                 AND    ROWNUM = 1)  AS Movie_Title,
                                b.BookingDate,
                                b.BookingStatus
                            FROM   Booking      b
                            JOIN   User_Booking ub ON ub.Booking_Id = b.Booking_Id
                            JOIN   ""User""     u  ON u.User_Id     = ub.User_Id
                            ORDER  BY b.Booking_Id DESC
                        ) WHERE ROWNUM <= 5";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvRecent.DataSource = dt;
                        gvRecent.DataBind();
                    }
                }
            }
            catch
            {
                // Leave grid empty on error
            }
        }
    }
}