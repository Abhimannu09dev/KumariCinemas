using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class PricingForm : Page
    {
        private string connStr = ConfigurationManager
                                    .ConnectionStrings["OracleConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadPricing();
        }

        private void LoadPricing()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "SELECT Pricing_Id, Ticket_Price, IsHolidayPricing FROM Pricing ORDER BY Pricing_Id", conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvPricing.DataSource = dt;
                        gvPricing.DataBind();
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Error loading pricing: " + ex.Message, true); }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int pricingId = int.Parse(hfPricingId.Value);
            bool isNew = (pricingId == 0);

            decimal price;
            if (!decimal.TryParse(txtTicketPrice.Text.Trim(), out price) || price <= 0)
            {
                ShowMessage("✗ Invalid price. Enter a positive number.", true);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        int newId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_pricing_id.NEXTVAL FROM DUAL", conn))
                        {
                            newId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }
                        using (var cmd = new OracleCommand(
                            "INSERT INTO Pricing (Pricing_Id, Ticket_Price, IsHolidayPricing) VALUES (:p_id, :p_price, :p_holiday)", conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_price", OracleDbType.Decimal).Value = price;
                            cmd.Parameters.Add(":p_holiday", OracleDbType.Char).Value = ddlHoliday.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new OracleCommand(
                            "UPDATE Pricing SET Ticket_Price = :p_price, IsHolidayPricing = :p_holiday WHERE Pricing_Id = :p_id", conn))
                        {
                            cmd.Parameters.Add(":p_price", OracleDbType.Decimal).Value = price;
                            cmd.Parameters.Add(":p_holiday", OracleDbType.Char).Value = ddlHoliday.SelectedValue;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = pricingId;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                ShowMessage(isNew ? "✓ Pricing added!" : "✓ Pricing updated!", false);
                ClearForm();
                LoadPricing();
            }
            catch (Exception ex) { ShowMessage("✗ Error saving pricing: " + ex.Message, true); }
        }

        protected void gvPricing_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = int.Parse(e.CommandArgument.ToString());
            if (e.CommandName == "EditPricing") LoadIntoForm(id);
            else if (e.CommandName == "DeletePricing") DeletePricing(id);
        }

        private void LoadIntoForm(int id)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "SELECT Pricing_Id, Ticket_Price, IsHolidayPricing FROM Pricing WHERE Pricing_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = id;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                hfPricingId.Value = r["Pricing_Id"].ToString();
                                txtTicketPrice.Text = Convert.ToDecimal(r["Ticket_Price"]).ToString("N2");
                                ddlHoliday.SelectedValue = r["IsHolidayPricing"].ToString().Trim();
                                lblFormTitle.Text = "✏ Edit Pricing (ID: " + id + ")";
                                btnSave.Text = "Update Pricing";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("✗ Error loading pricing: " + ex.Message, true); }
        }

        private void DeletePricing(int id)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Pricing WHERE Pricing_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
                ShowMessage("✓ Pricing deleted.", false);
                LoadPricing();
            }
            catch (OracleException oex) when (oex.Number == 2292)
            {
                ShowMessage("✗ Cannot delete — this pricing is used by existing shows.", true);
            }
            catch (Exception ex) { ShowMessage("✗ Error deleting pricing: " + ex.Message, true); }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            hfPricingId.Value = "0";
            txtTicketPrice.Text = "";
            ddlHoliday.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Pricing";
            btnSave.Text = "Save Pricing";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }
    }
}