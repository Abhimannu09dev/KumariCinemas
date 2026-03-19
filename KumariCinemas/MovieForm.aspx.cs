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

        // ════════════════════════════════════════════
        // PAGE LOAD
        // ════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadMovies();
        }

        // ════════════════════════════════════════════
        // LOAD GRID
        // New column names: Title, Duration, Language, Genre, ReleaseDate
        // ════════════════════════════════════════════
        private void LoadMovies()
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Movie_Id, Title, Duration,
                                          Language, Genre, ReleaseDate
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

        // ════════════════════════════════════════════
        // SAVE — INSERT or UPDATE
        // ════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int movieId = int.Parse(hfMovieId.Value);
            bool isNew = (movieId == 0);

            // Parse release date
            DateTime releaseDate;
            if (!DateTime.TryParse(txtReleaseDate.Text.Trim(), out releaseDate))
            {
                ShowMessage("✗ Invalid release date. Use DD-MMM-YYYY (e.g. 30-Sep-2022).", isError: true);
                return;
            }

            // Duration is optional (nullable in schema)
            int? duration = null;
            if (!string.IsNullOrWhiteSpace(txtDuration.Text))
            {
                int parsedDuration;
                if (!int.TryParse(txtDuration.Text.Trim(), out parsedDuration) || parsedDuration < 1)
                {
                    ShowMessage("✗ Invalid duration. Enter a positive number.", isError: true);
                    return;
                }
                duration = parsedDuration;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // Get next Movie_Id from sequence
                        int newMovieId;
                        using (var seqCmd = new OracleCommand(
                            "SELECT seq_movie_id.NEXTVAL FROM DUAL", conn))
                        {
                            newMovieId = Convert.ToInt32(seqCmd.ExecuteScalar());
                        }

                        string sql = @"INSERT INTO Movie
                                           (Movie_Id, Title, Duration, Language, Genre, ReleaseDate)
                                       VALUES
                                           (:p_id, :p_title, :p_duration, :p_language,
                                            :p_genre, :p_releasedate)";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_id", OracleDbType.Int32).Value = newMovieId;
                            cmd.Parameters.Add(":p_title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                            cmd.Parameters.Add(":p_duration", OracleDbType.Int32).Value = duration.HasValue
                                                                                                 ? (object)duration.Value
                                                                                                 : DBNull.Value;
                            cmd.Parameters.Add(":p_language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                            cmd.Parameters.Add(":p_genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                            cmd.Parameters.Add(":p_releasedate", OracleDbType.Date).Value = releaseDate;
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("✓ Movie added successfully!", isError: false);
                    }
                    else
                    {
                        string sql = @"UPDATE Movie
                                       SET    Title       = :p_title,
                                              Duration    = :p_duration,
                                              Language    = :p_language,
                                              Genre       = :p_genre,
                                              ReleaseDate = :p_releasedate
                                       WHERE  Movie_Id    = :p_id";

                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":p_title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                            cmd.Parameters.Add(":p_duration", OracleDbType.Int32).Value = duration.HasValue
                                                                                                 ? (object)duration.Value
                                                                                                 : DBNull.Value;
                            cmd.Parameters.Add(":p_language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                            cmd.Parameters.Add(":p_genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                            cmd.Parameters.Add(":p_releasedate", OracleDbType.Date).Value = releaseDate;
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

        // ════════════════════════════════════════════
        // GRID ROW COMMAND — Edit / Delete
        // ════════════════════════════════════════════
        protected void gvMovies_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int movieId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditMovie")
                LoadMovieIntoForm(movieId);
            else if (e.CommandName == "DeleteMovie")
                DeleteMovie(movieId);
        }

        // ════════════════════════════════════════════
        // LOAD INTO FORM (edit)
        // ════════════════════════════════════════════
        private void LoadMovieIntoForm(int movieId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Movie_Id, Title, Duration,
                                          Language, Genre, ReleaseDate
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
                                txtTitle.Text = reader["Title"].ToString();
                                txtDuration.Text = reader["Duration"] == DBNull.Value
                                                    ? "" : reader["Duration"].ToString();

                                ddlLanguage.SelectedValue = reader["Language"].ToString();
                                ddlGenre.SelectedValue = reader["Genre"].ToString();

                                txtReleaseDate.Text = reader["ReleaseDate"] == DBNull.Value
                                    ? ""
                                    : Convert.ToDateTime(reader["ReleaseDate"]).ToString("dd-MMM-yyyy");

                                lblFormTitle.Text = "✏ Edit Movie (ID: " + movieId + ")";
                                btnSave.Text = "Update Movie";
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

        // ════════════════════════════════════════════
        // DELETE MOVIE
        // Junction table delete order:
        // Show_Ticket → Hall_Show → Booking_Movie → Show → Movie_City → Movie
        // ════════════════════════════════════════════
        private void DeleteMovie(int movieId)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 1. Delete Show_Ticket for all shows of this movie
                    Execute(conn, @"DELETE FROM Show_Ticket
                                    WHERE Show_Id IN
                                        (SELECT Show_Id FROM Show WHERE Movie_Id = :p_id)", movieId);

                    // 2. Delete Hall_Show for all shows of this movie
                    Execute(conn, @"DELETE FROM Hall_Show
                                    WHERE Show_Id IN
                                        (SELECT Show_Id FROM Show WHERE Movie_Id = :p_id)", movieId);

                    // 3. Delete Booking_Movie junction
                    Execute(conn, "DELETE FROM Booking_Movie WHERE Movie_Id = :p_id", movieId);

                    // 4. Delete Shows of this movie
                    Execute(conn, "DELETE FROM Show WHERE Movie_Id = :p_id", movieId);

                    // 5. Delete Movie_City junction
                    Execute(conn, "DELETE FROM Movie_City WHERE Movie_Id = :p_id", movieId);

                    // 6. Now safe to delete Movie
                    Execute(conn, "DELETE FROM Movie WHERE Movie_Id = :p_id", movieId);
                }

                ShowMessage("✓ Movie and all linked records deleted.", isError: false);
                LoadMovies();
            }
            catch (Exception ex)
            {
                ShowMessage("✗ Error deleting movie: " + ex.Message, isError: true);
            }
        }

        // ════════════════════════════════════════════
        // CLEAR FORM
        // ════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfMovieId.Value = "0";
            txtTitle.Text = "";
            txtDuration.Text = "";
            txtReleaseDate.Text = "";
            ddlGenre.SelectedIndex = 0;
            ddlLanguage.SelectedIndex = 0;
            lblFormTitle.Text = "Add New Movie";
            btnSave.Text = "Save Movie";
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