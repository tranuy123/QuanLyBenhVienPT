using BenhVienPT.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BenhVienPT
{
    public partial class ThongBao : Form
    {
        public ThongBao()
        {
            InitializeComponent();
        }
        private int idnv ; 

        private void ThongBao_Load(object sender, EventArgs e)
        {

        }
        public ThongBao (int id)
        {
            InitializeComponent();
            HienThiThongbao(id);
            this.idnv = id;
        }
        //kết nối 
        static String connString = @"Data Source=TRANUY\SQLEXPRESS;Initial Catalog=WebBenhVienPT;User ID=sa;Password=123";
        //khai báo
        SqlConnection sqlconnection = new SqlConnection(connString);
        SqlCommand sqlcommand;

        //Mở kết nối      
        private void Openconn()
        {
            if (sqlconnection == null)
            {
                sqlconnection = new SqlConnection(connString);
            }
            if (sqlconnection.State == ConnectionState.Closed)
            {
                sqlconnection.Open();
            }
        }
        //Đóng kết nối
        private void Closeconn()
        {
            if (sqlconnection.State == ConnectionState.Open && sqlconnection != null)
            {
                sqlconnection.Close();
            }
        }

        
        private void HienThiThongbao(int id)
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "select * from thongbao where idnv = '"+id+"'";
                SqlDataReader reader = sqlcommand.ExecuteReader();
                listViewTB.Items.Clear();
                while (reader.Read())
                {
                    int  ID = reader.GetInt32(0);
                    int idnv = reader.GetInt32(1);
                    string nd = reader.GetString(2);
                    DateTime ngay = reader.GetDateTime(3);
                    bool TrangThai = reader.GetBoolean(4);
                    string tieude = reader.GetString(5);



                    ListViewItem lvi = new ListViewItem(ngay.ToString());
                    lvi.SubItems.Add(tieude);
                    lvi.SubItems.Add(nd);
                    lvi.SubItems.Add(ID.ToString());
                    if (!TrangThai) // nếu TrangThai là false thì đặt màu xám cho ListViewItem
                    {
                        lvi.BackColor = Color.Gray;
                    }



                    listViewTB.Items.Add(lvi);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listViewTB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string idtb = listViewTB.SelectedItems[0].SubItems[3].Text;

            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "UPDATE thongbao SET trangthai = 1 WHERE id = @idtb";
                sqlcommand.Parameters.AddWithValue("@idtb", idtb);

                sqlcommand.ExecuteNonQuery();
                HienThiThongbao(idnv);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
