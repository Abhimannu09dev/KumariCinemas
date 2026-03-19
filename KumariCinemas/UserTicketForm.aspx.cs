using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class UserTicketForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadUserDropdown();
        }

        private void LoadUserDropdown()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        @"SELECT User_Id, Username FROM ""User"" ORDER BY Username", conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        ddlUser.DataSource = dt;
                        ddlUser.DataTextField = "Username";
                        ddlUser.DataValueField = "User_Id";
                        ddlUser.DataBind();
                        ddlUser.Items.Insert(0, new ListItem("-- Select User --", "0"));
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading users: " + ex.Message); }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlUser.SelectedValue == "0")
            {
                ShowMessage("Please select a user.");
                return;
            }

            DateTime dateFrom;
            if (!DateTime.TryParse(txtDateFrom.Text.Trim(), out dateFrom))
            {
                ShowMessage("Invalid date. Use DD-MMM-YYYY (e.g. 01-Aug-2024).");
                return;
            }

            int userId = int.Parse(ddlUser.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT
                            t.Ticket_Id,
                            u.Username,
                            u.Email,
                            u.Phone,
                            t.Seat_Number,
                            t.PaymentStatus,
                            m.Title          AS Movie_Title,
                            s.ShowDate,
                            st.Showtime_Name AS Showtime,
                            p.Ticket_Price,
                            p.IsHolidayPricing AS IsHoliday,
                            h.HallName,
                            th.TheaterName,
                            c.CityName
                        FROM   ""User""     u
                        JOIN   Ticket       t   ON t.User_Id      = u.User_Id
                        JOIN   Show_Ticket  stk ON stk.Ticket_Id  = t.Ticket_Id
                        JOIN   Show         s   ON s.Show_Id      = stk.Show_Id
                        JOIN   Movie        m   ON m.Movie_Id     = s.Movie_Id
                        JOIN   Showtime     st  ON st.Showtime_Id = s.Showtime_Id
                        JOIN   Pricing      p   ON p.Pricing_Id   = s.Pricing_Id
                        JOIN   Hall_Show    hs  ON hs.Show_Id     = s.Show_Id
                        JOIN   Hall         h   ON h.Hall_Id      = hs.Hall_Id
                        JOIN   Theater_Hall thl ON thl.Hall_Id    = h.Hall_Id
                        JOIN   Theater      th  ON th.Theater_Id  = thl.Theater_Id
                        JOIN   City_Theater ct  ON ct.Theater_Id  = th.Theater_Id
                        JOIN   City         c   ON c.City_Id      = ct.City_Id
                        WHERE  u.User_Id        = :p_userId
                        AND    t.PaymentStatus  = 'Paid'
                        AND    s.ShowDate      >= :p_dateFrom
                        AND    s.ShowDate       < ADD_MONTHS(:p_dateFrom, 6)
                        ORDER  BY s.ShowDate ASC";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_userId", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add(":p_dateFrom", OracleDbType.Date).Value = dateFrom;

                        using (var da = new OracleDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            gvResults.DataSource = dt;
                            gvResults.DataBind();
                            gvResults.Visible = true;

                            lblResultCount.Text = $"<i class='bi bi-ticket-fill'></i>  Found {dt.Rows.Count} paid ticket(s) for <strong>{ddlUser.SelectedItem.Text}</strong> from {dateFrom:dd-MMM-yyyy} to {dateFrom.AddMonths(6).AddDays(-1):dd-MMM-yyyy}";
                            lblResultCount.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error running query: " + ex.Message); }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlUser.SelectedIndex = 0;
            txtDateFrom.Text = "";
            gvResults.Visible = false;
            lblResultCount.Visible = false;
            lblMessage.Text = "";
        }

        private void ShowMessage(string msg)
        {
            lblMessage.Text = "⚠ " + msg;
            lblMessage.CssClass = "msg-error";
        }
    }
}