using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ImportPriceList
{
    public partial class Form1 : Form
    {
        const string CONNECTION = @"Data Source=172.16.1.10;Password=2825@Stcc-Sa.Com;User ID=sa;Initial Catalog=Sonic_Data_Test;Persist Security Info=True";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx" })
            {
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    txt_path.Text = ofd.FileName;
                    button2.Enabled = true;
                }
            }
        }

        public DataTable ReadExcel(string fileName)
        {
            string conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=YES';";
            DataTable dtexcel = new DataTable();
            using(OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [PriceList$]", con);
                    oleAdpt.Fill(dtexcel);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString());
                }
                return dtexcel;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = ReadExcel(txt_path.Text);
                txt_message.Text = "Number of rows: " + dt.Rows.Count.ToString();
                foreach (DataRow row in dt.Rows)
                {
                    UpdatePrice(
                        row["ItemCode"].ToString(), 
                        row["UOM"].ToString(), 
                        Convert.ToDecimal(row["Price"]), 
                        Convert.ToDecimal(row["MinPrice"]),
                        Convert.ToDecimal(row["MaxPrice"]),
                        Convert.ToDecimal(row["CostPrice"])
                    );
                }

                button2.Enabled = false;
                txt_path.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public void UpdatePrice(string itemCode, string uom, decimal price, decimal minPrice, decimal maxPrice, decimal costPrice)
        {
            minPrice = minPrice == -1 ? price : minPrice;
            maxPrice = maxPrice == -1 ? price : maxPrice;

            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            conn.ConnectionString = CONNECTION;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdatePriceList";

            cmd.Parameters.AddWithValue("@ItemCode", itemCode);
            cmd.Parameters.AddWithValue("@UOM", uom);
            cmd.Parameters.AddWithValue("@Price", price);
            cmd.Parameters.AddWithValue("@MinPrice", minPrice);
            cmd.Parameters.AddWithValue("@MaxPrice", maxPrice);
            cmd.Parameters.AddWithValue("@CostPrice", costPrice);
            cmd.Parameters.Add("@Message", SqlDbType.VarChar, 4000);
            cmd.Parameters["@Message"].Direction = ParameterDirection.Output;

            try
            {
                conn.Open();
                int i = cmd.ExecuteNonQuery();
                string msg = Convert.ToString(cmd.Parameters["@Message"].Value);

                txt_message.AppendText(Environment.NewLine);
                txt_message.AppendText(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
