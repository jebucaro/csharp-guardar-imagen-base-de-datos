using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Notes
{
    public partial class MainForm : Form
    {
        SqlConnection con;

        public MainForm()
        {
            Font = SystemFonts.MessageBoxFont;

            InitializeComponent();

            CrearConexion();
        }

        void CrearConexion()
        {
            SqlConnectionStringBuilder cs = new SqlConnectionStringBuilder
            {
                DataSource = @"localhost",
                InitialCatalog = "samples",
                IntegratedSecurity = true
            };

            con = new SqlConnection(cs.ConnectionString);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult r = DlgBrowseImage.ShowDialog();
            if (r == DialogResult.OK)
            {
                TxtPath.Text = DlgBrowseImage.FileName;
                pictureBox1.Load(TxtPath.Text);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {

            byte[] data = File.ReadAllBytes(TxtPath.Text);
            string qry = "insert into Notes (Title, ImageData) values (@prTitle, @prImageData)";

            try
            {
                using (SqlCommand sqlCommand = new SqlCommand(qry, con))
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@prTitle", TxtTitle.Text));
                    sqlCommand.Parameters.Add(new SqlParameter("@prImageData", data));

                    con.Open();
                    sqlCommand.ExecuteNonQuery();
                }

                MessageBox.Show("Success", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string qry = "select Title, ImageData from Notes where Id = @prId";
            try
            {
                using (SqlCommand sqlCommand = new SqlCommand(qry, con))
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@prId", NudId.Value));

                    con.Open();

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        if (sqlDataReader.Read())
                        {
                            LblTitle.Text = sqlDataReader.GetString(0);
                            byte[] imgData = (byte[])sqlDataReader.GetValue(1);

                            using (MemoryStream ms = new MemoryStream(imgData, 0, imgData.Length))
                            {
                                pictureBox2.Image = Image.FromStream(ms, true);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Id { NudId.Value.ToString() } does not exists", "View", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
    }
}
