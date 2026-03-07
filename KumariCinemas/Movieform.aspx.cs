using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinemas
{
    public partial class MovieForm : Page
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
                LoadMovies();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD GRID
        // ─────────────────────────────────────────────────────────────
        private void LoadMovies()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Movie_Id, Movie_Name, Movie_Duration,
                                          Movie_Genre, Movie_ReleaseDate, Movie_Language
                                   FROM   Movie
                                   ORDER  BY Movie_Id";

                    using (var cmd = new OracleCommand(sql, conn))
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvMovies.DataSource = dt;
                        gvMovies.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading movies: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // SAVE — INSERT or UPDATE
        // ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int movieId = int.Parse(hfMovieId.Value);
            bool isNew = (movieId == 0);

            // Parse optional release date
            DateTime? releaseDate = null;
            if (!string.IsNullOrWhiteSpace(txtReleaseDate.Text))
            {
                DateTime parsedDate;
                if (!DateTime.TryParse(txtReleaseDate.Text.Trim(), out parsedDate))
                {
                    ShowMessage("✗ Invalid release date. Use DD-MMM-YYYY (e.g. 30-Sep-2022).", isError: true);
                    return;
                }
                releaseDate = parsedDate;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql;

                    if (isNew)
                    {
                        int newId = GetNextMovieId(conn);

                        sql = @"INSERT INTO Movie
                                    (Movie_Id, Movie_Name, Movie_Duration,
                                     Movie_Genre, Movie_ReleaseDate, Movie_Language)
                                VALUES
                                    (:p_id, :p_name, :p_duration,
                                     :p_genre, :p_releasedate, :p_language)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":p_duration", OracleDbType.Int32).Value = int.Parse(txtDuration.Text.Trim());
                            cmd.Parameters.Add(":p_genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                            cmd.Parameters.Add(":p_releasedate", OracleDbType.Date).Value = releaseDate.HasValue
                                                                                                    ? (object)releaseDate.Value
                                                                                                    : DBNull.Value;
                            cmd.Parameters.Add(":p_language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Movie added successfully! (ID: " + newId + ")", isError: false);
                    }
                    else
                    {
                        sql = @"UPDATE Movie
                                SET Movie_Name        = :p_name,
                                    Movie_Duration    = :p_duration,
                                    Movie_Genre       = :p_genre,
                                    Movie_ReleaseDate = :p_releasedate,
                                    Movie_Language    = :p_language
                                WHERE Movie_Id = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":p_duration", OracleDbType.Int32).Value = int.Parse(txtDuration.Text.Trim());
                            cmd.Parameters.Add(":p_genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                            cmd.Parameters.Add(":p_releasedate", OracleDbType.Date).Value = releaseDate.HasValue
                                                                                                    ? (object)releaseDate.Value
                                                                                                    : DBNull.Value;
                            cmd.Parameters.Add(":p_language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = movieId;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Movie updated successfully!", isError: false);
                    }
                }

                ClearForm();
                LoadMovies();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error saving movie: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GRID ROW COMMAND — Edit / Delete
        // ─────────────────────────────────────────────────────────────
        protected void gvMovies_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int movieId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditMovie")
            {
                LoadMovieIntoForm(movieId);
            }
            else if (e.CommandName == "DeleteMovie")
            {
                DeleteMovie(movieId);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD INTO FORM (editing)
        // ─────────────────────────────────────────────────────────────
        private void LoadMovieIntoForm(int movieId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Movie_Id, Movie_Name, Movie_Duration,
                                          Movie_Genre, Movie_ReleaseDate, Movie_Language
                                   FROM   Movie
                                   WHERE  Movie_Id = :p_id";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = movieId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfMovieId.Value = reader["Movie_Id"].ToString();
                                txtName.Text = reader["Movie_Name"].ToString();
                                txtDuration.Text = reader["Movie_Duration"].ToString();

                                // Set Genre dropdown
                                ddlGenre.SelectedValue = reader["Movie_Genre"].ToString();
                                ddlLanguage.SelectedValue = reader["Movie_Language"].ToString();

                                // Release date is optional (nullable)
                                txtReleaseDate.Text = reader["Movie_ReleaseDate"] == DBNull.Value
                                    ? ""
                                    : Convert.ToDateTime(reader["Movie_ReleaseDate"]).ToString("dd-MMM-yyyy");

                                lblFormTitle.Text = "✏ Edit Movie (ID: " + movieId + ")";
                                btnSave.Text = "💾 Update Movie";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error loading movie: " + ex.Message, isError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE
        // Movie → Show → Ticket (cascade down before deleting Movie)
        // ─────────────────────────────────────────────────────────────
        private void DeleteMovie(int movieId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete Tickets linked to Shows of this Movie
                    using (var cmd = new OracleCommand(
                        @"DELETE FROM Ticket
                          WHERE Show_Id IN (
                              SELECT Show_Id FROM Show WHERE Movie_Id = :p_id)", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = movieId;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Delete Shows of this Movie
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Show WHERE Movie_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = movieId;
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Delete the Movie
                    using (var cmd = new OracleCommand(
                        "DELETE FROM Movie WHERE Movie_Id = :p_id", conn))
                    {
                        cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = movieId;
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✓ Movie and linked shows/tickets deleted.", isError: false);
                LoadMovies();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting movie: " + ex.Message, isError: true);
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
            hfMovieId.Value = "0";
            txtName.Text = "";
            txtDuration.Text = "";
            txtReleaseDate.Text = "";
            ddlGenre.SelectedIndex = 0;
            ddlLanguage.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Movie";
            btnSave.Text = "💾 Save Movie";
        }

        private void ShowMessage(string msg, bool isError)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = isError ? "msg-error" : "msg-success";
        }

        private int GetNextMovieId(OracleConnection conn)
        {
            string sql = "SELECT NVL(MAX(Movie_Id), 0) + 1 FROM Movie";
            using (var cmd = new OracleCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}