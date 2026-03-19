using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class CityForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadCities();
        }

        private void LoadCities()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT City_Id, CityName FROM City ORDER BY City_Id";
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvCities.DataSource = dt;
                        gvCities.DataBind();
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading cities: " + ex.Message, true); }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int cityId = int.Parse(hfCityId.Value);
            bool isNew = (cityId == 0);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        int newId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_city_id.NEXTVAL FROM DUAL", conn))
                        {
                            newId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }
                        using (var cmd = new OracleCommand(
                            "INSERT INTO City (City_Id, CityName) VALUES (:p_id, :p_name)", conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtCityName.Text.Trim();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new OracleCommand(
                            "UPDATE City SET CityName = :p_name WHERE City_Id = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtCityName.Text.Trim();
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = cityId;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                ShowMessage(isNew ? "✓ City added successfully!" : "✓ City updated successfully!", false);
                ClearForm();
                LoadCities();
            }
            catch (Exception ex) { ShowMessage("✗ Error saving city: " + ex.Message, true); }
        }

        protected void gvCities_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int cityId = int.Parse(e.CommandArgument.ToString());
            if (e.CommandName == "EditCity") LoadCityIntoForm(cityId);
            else if (e.CommandName == "DeleteCity") DeleteCity(cityId);
        }

        private void LoadCityIntoForm(int cityId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "SELECT City_Id, CityName FROM City WHERE City_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = cityId;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                hfCityId.Value = r["City_Id"].ToString();
                                txtCityName.Text = r["CityName"].ToString();
                                lblFormTitle.Text = "✏ Edit City (ID: " + cityId + ")";
                                btnSave.Text = "Update City";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("✗ Error loading city: " + ex.Message, true); }
        }

        private void DeleteCity(int cityId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    // Delete City_Theater junction first then Movie_City then City
                    Execute(conn, "DELETE FROM City_Theater WHERE City_Id = :p_id", cityId);
                    Execute(conn, "DELETE FROM Movie_City   WHERE City_Id = :p_id", cityId);
                    Execute(conn, "DELETE FROM City         WHERE City_Id = :p_id", cityId);
                }
                ShowMessage("✓ City deleted successfully.", false);
                LoadCities();
            }
            catch (Exception ex) { ShowMessage("✗ Error deleting city: " + ex.Message, true); }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            hfCityId.Value = "0";
            txtCityName.Text = "";
            lblFormTitle.Text = "Add New City";
            btnSave.Text = "Save City";
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