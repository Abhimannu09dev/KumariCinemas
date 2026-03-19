using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class ShowtimeForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadShowtimes();
        }

        private void LoadShowtimes()
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
                        gvShowtimes.DataSource = dt;
                        gvShowtimes.DataBind();
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading showtimes: " + ex.Message, true); }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int showtimeId = int.Parse(hfShowtimeId.Value);
            bool isNew = (showtimeId == 0);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        int newId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_showtime_id.NEXTVAL FROM DUAL", conn))
                        {
                            newId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }
                        using (var cmd = new OracleCommand(
                            "INSERT INTO Showtime (Showtime_Id, Showtime_Name) VALUES (:p_id, :p_name)", conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtShowtimeName.Text.Trim();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new OracleCommand(
                            "UPDATE Showtime SET Showtime_Name = :p_name WHERE Showtime_Id = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtShowtimeName.Text.Trim();
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = showtimeId;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                ShowMessage(isNew ? "✓ Showtime added!" : "✓ Showtime updated!", false);
                ClearForm();
                LoadShowtimes();
            }
            catch (Exception ex) { ShowMessage("✗ Error saving showtime: " + ex.Message, true); }
        }

        protected void gvShowtimes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = int.Parse(e.CommandArgument.ToString());
            if (e.CommandName == "EditShowtime") LoadIntoForm(id);
            else if (e.CommandName == "DeleteShowtime") DeleteShowtime(id);
        }

        private void LoadIntoForm(int id)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "SELECT Showtime_Id, Showtime_Name FROM Showtime WHERE Showtime_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = id;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                hfShowtimeId.Value = r["Showtime_Id"].ToString();
                                txtShowtimeName.Text = r["Showtime_Name"].ToString();
                                lblFormTitle.Text = "✏ Edit Showtime (ID: " + id + ")";
                                btnSave.Text = "Update Showtime";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("✗ Error loading showtime: " + ex.Message, true); }
        }

        private void DeleteShowtime(int id)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Showtime WHERE Showtime_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
                ShowMessage("✓ Showtime deleted.", false);
                LoadShowtimes();
            }
            catch (OracleException oex) when (oex.Number == 2292)
            {
                ShowMessage("✗ Cannot delete — this showtime is linked to existing shows.", true);
            }
            catch (Exception ex) { ShowMessage("✗ Error deleting showtime: " + ex.Message, true); }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            hfShowtimeId.Value = "0";
            txtShowtimeName.Text = "";
            lblFormTitle.Text = "Add New Showtime";
            btnSave.Text = "Save Showtime";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }
    }
}