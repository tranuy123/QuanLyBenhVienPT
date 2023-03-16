using BenhVienPT.Models;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace BenhVienPT
{
    public partial class FormQuanLyPT : Form
    {
        //kết nối 
        static String connString = @"Data Source=TRANUY\SQLEXPRESS;Initial Catalog=WebBenhVienPT;User ID=sa;Password=123";
        //khai báo
        SqlConnection sqlconnection = new SqlConnection(connString);
        SqlCommand sqlcommand;
        private TaiKhoan acc;

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
        public FormQuanLyPT(Models.TaiKhoan acc)
        {
            this.acc = acc;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            HienThiDSBenhAn();

            //  HienThiDSLichTruc();
            HienThiDSCaMo();
            LoadLichLam("");
            HienThiDSCaMoTC();
            HienthidanhsachcamoNull();
            Hienthidanhsachcamoall();
        }

       

        private void HienThiDSCaMo()
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "SELECT DISTINCT bh.mabenhan, bn.TenBN, b.tenbenh, b.mucdo, bh.trangthai, b.id AS idBenh, bh.id AS idBenhAn, cm.id AS idCaMo, lm.Ngay, lm.camo, cm.IDPhongMo " +
                 "FROM benhan bh " +
                 "INNER JOIN CaMo cm ON cm.IDBenhAn = bh.ID " +
                 "INNER JOIN BenhNhan bn ON bn.ID = bh.IDBenhNhan " +
                 "INNER JOIN chitietbenhan cb ON bh.id = cb.idbenhan " +
                 "INNER JOIN benh b ON cb.idbenh = b.id " +
                 "INNER JOIN LichMo lm ON lm.IDCM = cm.id " +
                 "INNER JOIN (" +
                 "    SELECT cb.idbenhan, MAX(b.mucdo) AS max_mucdo " +
                 "    FROM chitietbenhan cb " +
                 "    INNER JOIN benh b ON cb.idbenh = b.id " +
                 "    GROUP BY cb.idbenhan" +
                 ") max_benh ON cb.idbenhan = max_benh.idbenhan AND b.mucdo = max_benh.max_mucdo " +
                 "WHERE bh.trangthai = 1 and lm.ngay >= CAST(GETDATE() AS DATE) and cm.TinhTrang is null " +
                 "ORDER BY b.mucdo DESC";
                SqlDataReader reader = sqlcommand.ExecuteReader();
                livCaMo.Items.Clear();
                while (reader.Read())
                {
                    string MaBenhAn = reader.GetString(0);
                    string TenBenhNhan = reader.GetString(1);
                    string LoaiBenh = reader.GetString(2);
                    int mucDo = reader.GetInt32(3);
                    bool TrangThai = reader.GetBoolean(4);
                    int id = reader.GetInt32(5);
                    int idba = reader.GetInt32(6);
                    int idCaMo = reader.GetInt32(7);
                    DateTime ngay = reader.GetDateTime(8);
                    int iditgm = reader.GetInt32(9);
                    int ipm = reader.GetInt32(10);




                    ListViewItem lvi = new ListViewItem(MaBenhAn);
                    lvi.SubItems.Add(TenBenhNhan);
                    lvi.SubItems.Add(LoaiBenh);
                    lvi.SubItems.Add(mucDo.ToString());
                    lvi.SubItems.Add(TrangThai.ToString());
                    lvi.SubItems.Add(id.ToString());
                    lvi.SubItems.Add(idba.ToString());
                    lvi.SubItems.Add(idCaMo.ToString());
                    lvi.SubItems.Add(ngay.ToString("yyyy/MM/dd"));
                    lvi.SubItems.Add(iditgm.ToString());
                    lvi.SubItems.Add(ipm.ToString());


                    livCaMo.Items.Add(lvi);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HienThiDSBenhAn()
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "select BA.MaBenhAn, BN.TenBN, B.TenBenh, B.MucDo, BA.TrangThai, BA.GhiChu, BA.YLenh, BN.GioiTinh, BN.NgaySinh, BN.SDT  from BenhAn as BA inner join BenhNhan as BN on BA.IDBenhNhan = BN.ID inner join ChiTietBenhAn as CTBA on CTBA.IDBenhAn = BA.ID inner join Benh as B on B.ID = CTBA.IDBenh order by b.mucdo desc";
                SqlDataReader reader = sqlcommand.ExecuteReader();
                LivDSBenhAn.Items.Clear();
                while (reader.Read())
                {
                    string MaBenhAn = reader.GetString(0);
                    string TenBenhNhan = reader.GetString(1);
                    string LoaiBenh = reader.GetString(2);
                    int mucDo = reader.GetInt32(3);
                    bool TrangThai = reader.GetBoolean(4);
                    string GhiChu = reader.GetString(5);
                    string YLenh = reader.GetString(6);
                    string GioiTinh = reader.GetString(7);
                    DateTime NgaySinh = reader.GetDateTime(8);
                    string SDT = reader.GetString(9);          
           
                    ListViewItem lvi = new ListViewItem(MaBenhAn);
                    lvi.SubItems.Add(TenBenhNhan);
                    lvi.SubItems.Add(LoaiBenh);
                    lvi.SubItems.Add(mucDo.ToString());
                    if (!TrangThai) // nếu TrangThai == false
                    {
                        lvi.SubItems.Add(TrangThai.ToString());
                        lvi.BackColor = Color.LightGray; // thay đổi màu nền của dòng
                    }
                    else
                    {
                        lvi.SubItems.Add(TrangThai.ToString());

                    }
                    lvi.SubItems.Add(GhiChu);
                    lvi.SubItems.Add(YLenh);
                    lvi.SubItems.Add(GioiTinh);
                    lvi.SubItems.Add(NgaySinh.ToString("dd/MM/yyyy"));
                    lvi.SubItems.Add(SDT);       

                    LivDSBenhAn.Items.Add(lvi);
                }
                reader.Close();

            }
            catch(Exception ex )
            {
                MessageBox.Show(ex.Message);
            }
        }

      

        private void LivDSBenhAn_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtMaBA.ReadOnly = true;
            txtMaBA.ReadOnly = true;
            txtTenBN.ReadOnly = true;
            txtLoaiBenh.ReadOnly = true;
            txtTrangThai.ReadOnly = true;
            txtGioiTinh.ReadOnly = true;
            txtNgaySinh.ReadOnly = true;
            txtSDT.ReadOnly = true;
            txtMucDo.ReadOnly = true;
            txtGhiChu.ReadOnly = true;
            txtYLenh.ReadOnly = true;
            if (LivDSBenhAn.SelectedItems.Count == 0) return;
            ListViewItem lvi = LivDSBenhAn.SelectedItems[0];
            txtMaBA.Text = lvi.SubItems[0].Text;
            txtTenBN.Text = lvi.SubItems[1].Text;
            txtLoaiBenh.Text = lvi.SubItems[2].Text;
            txtMucDo.Text = lvi.SubItems[3].Text;
            txtTrangThai.Text = lvi.SubItems[4].Text;
            txtGhiChu.Text = lvi.SubItems[5].Text;
            txtYLenh.Text = lvi.SubItems[6].Text;
            txtGioiTinh.Text = lvi.SubItems[7].Text;
            txtNgaySinh.Text = lvi.SubItems[8].Text;
            txtSDT.Text = lvi.SubItems[9].Text;
    
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {

            if (txtTimKiem.Text != "")
            {
                // Tạo danh sách tạm thời chứa các mục thỏa mãn điều kiện tìm kiếm
                List<ListViewItem> matchingItems = new List<ListViewItem>();

                foreach (ListViewItem item in LivDSBenhAn.Items)
                {
                    if (item.SubItems[0].Text.ToLower().Contains(txtTimKiem.Text.ToLower()) ||
                        item.SubItems[1].Text.ToLower().Contains(txtTimKiem.Text.ToLower()) ||
                        item.SubItems[2].Text.ToLower().Contains(txtTimKiem.Text.ToLower()) ||
                         item.SubItems[3].Text.ToLower().Contains(txtTimKiem.Text.ToLower()))
                    {
                        item.BackColor = SystemColors.Highlight;
                        item.ForeColor = SystemColors.HighlightText;
                        matchingItems.Add(item);
                    }
                }

                // Loại bỏ các mục không thỏa mãn khỏi danh sách
                LivDSBenhAn.Items.Clear();
                LivDSBenhAn.Items.AddRange(matchingItems.ToArray());

                if (LivDSBenhAn.SelectedItems.Count == 1)
                {
                    LivDSBenhAn.Focus();
                }
            

        }
            else
            {
                RefreshAll();
            }         

        }

        private void RefreshAll()
        {
            HienThiDSBenhAn();
        }



        private void lbTaiKhoan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
          DialogResult y =  MessageBox.Show("Bạn có muốn đăng xuất?", "Thông báo!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (y == DialogResult.Yes)
            {
                MessageBox.Show("Đăng xuất thành công!");
                this.Hide();
                FormDangNhap logout = new FormDangNhap();
                logout.ShowDialog();
                this.Close();
            }
        }

        private void btnDatLich_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormDatLich formdatlich = new FormDatLich(acc);
            formdatlich.ShowDialog();
            this.Close();
        }


private void txtTimKiemCaMo_TextChanged(object sender, EventArgs e)
    {
        if (txtTimKiemCaMo.Text != "")
        {
            // Tạo danh sách tạm thời chứa các mục thỏa mãn điều kiện tìm kiếm
            List<ListViewItem> matchingItems = new List<ListViewItem>();
            string pattern = txtTimKiemCaMo.Text.ToLower().Trim();
            string noAccentPattern = RemoveVietnameseAccent(pattern);
            Regex regex = new Regex(pattern);

            foreach (ListViewItem item in livCaMo.Items)
            {
                string text = item.SubItems[0].Text.ToLower();
                string noAccentText = RemoveVietnameseAccent(text);
                if (regex.IsMatch(text) || regex.IsMatch(noAccentText))
                {
                    item.BackColor = SystemColors.Highlight;
                    item.ForeColor = SystemColors.HighlightText;
                    matchingItems.Add(item);
                }
            }

            // Loại bỏ các mục không thỏa mãn khỏi danh sách
            livCaMo.Items.Clear();
            livCaMo.Items.AddRange(matchingItems.ToArray());

            if (livCaMo.SelectedItems.Count == 1)
            {
                livCaMo.Focus();
            }
        }
        else
        {
            RefreshAll1();
        }
    }

    private static string RemoveVietnameseAccent(string text)
    {
        Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
        string result = text.Normalize(NormalizationForm.FormD);
        result = regex.Replace(result, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        return result;
    }

    private void RefreshAll1()
        {
            HienThiDSCaMo();
        }

        private void livCaMo_SelectedIndexChanged(object sender, EventArgs e)
        {
            cklBacSiCM.Items.Clear();
            cklYtaCM.Items.Clear();
            if (livCaMo.SelectedItems.Count == 0) return;
            ListViewItem lvi = livCaMo.SelectedItems[0];
            txtMaBACM.Text = lvi.SubItems[0].Text;
            txtTenBNCaMo.Text = lvi.SubItems[1].Text;
            string idtgm = livCaMo.SelectedItems[0].SubItems[9].Text;
            string idphong = livCaMo.SelectedItems[0].SubItems[10].Text;

            string idCaMo = livCaMo.SelectedItems[0].SubItems[7].Text;
            string idb = livCaMo.SelectedItems[0].SubItems[5].Text;
            string dinhdang = livCaMo.SelectedItems[0].SubItems[8].Text;

            

            HienThiThongTinCaMo(Convert.ToInt32(idCaMo));
            HienThiBS(Convert.ToInt32(idCaMo));
            HienThiYTa(Convert.ToInt32(idCaMo));
            HienThiDSPhongMo(idb,dinhdang);


        }

        private void HienThiThongTinCaMo(int idCaMo)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "SELECT DISTINCT lm.Ngay, lm.CaMo, pm.id, pm.TenPhongMo, tg.TenTGMo, pht.TenPhong " +
                                "FROM LichMo as lm " +
                                "INNER JOIN CaMo cm ON cm.ID = lm.IDCM " +
                                "INNER JOIN PhongMo pm ON pm.ID = cm.IDPhongMo " +
                                "INNER JOIN TGMo tg ON tg.id = lm.CaMo " +
                                "inner join PhongHoiTinh pht on pht.ID = cm.IDPhongHoiTinh " +
                                "WHERE cm.ID = @idCaMo";
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@idCaMo", idCaMo);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    DateTime ngay = reader.GetDateTime(0);
                    dtpTGCaMo.Value = ngay;
                    cbxPhongMoCM.Text = reader.GetString(3);
                    cbxCaMoCM.Text = reader.GetString(4);
                    cbxPhongHoiTinh.Text = reader.GetString(5);
                }
            }
        }

        private void HienThiBS(int idcamo)
        {

            Openconn();
            string query1 = "select distinct nv.tennv from lichmo as lm " +
                            "inner join camo cm on cm.id = lm.idcm " +
                            "inner join nhanvien nv on nv.id = lm.idnv " +
                            "inner join taikhoan tk on tk.id = nv.idtaikhoan " +
                            "inner join vaitro vt on vt.id = tk.idvaitro " +
                            "where cm.id = @idcamo and vt.id = 4";

            SqlCommand command1 = new SqlCommand(query1, sqlconnection);
            command1.Parameters.AddWithValue("@idcamo", idcamo);
            SqlDataReader reader1 = command1.ExecuteReader();
            if (reader1.HasRows)
            {
                while (reader1.Read())
                {
                    // assuming you have a checked list box named 'cklbacsicm'
                    cklBacSiCM.Items.Add(reader1["tennv"], true);
                }
            }
            reader1.Close();
        }

        private void HienThiYTa(int idcamo)
        {
            Openconn();
            string query1 = "select distinct nv.tennv from lichmo as lm " +
                            "inner join camo cm on cm.id = lm.idcm " +
                            "inner join nhanvien nv on nv.id = lm.idnv " +
                            "inner join taikhoan tk on tk.id = nv.idtaikhoan " +
                            "inner join vaitro vt on vt.id = tk.idvaitro " +
                            "where cm.id = @idcamo and vt.id = 2";

            SqlCommand command1 = new SqlCommand(query1, sqlconnection);
            command1.Parameters.AddWithValue("@idcamo", idcamo);
            SqlDataReader reader1 = command1.ExecuteReader();
            if (reader1.HasRows)
            {
                while (reader1.Read())
                {
                    // assuming you have a checked list box named 'cklbacsicm'
                    cklYtaCM.Items.Add(reader1["tennv"], true);
                }
            }
            reader1.Close();
            Closeconn();



        }
        private void HienThiDSPhongMo(string maBenh, string ngay)
        {
            Openconn();

            string query = "SELECT DISTINCT ctpm.IDPM, PM.TenPhongMo as Display FROM PhongMo as PM " +
                  "INNER JOIN ChiTietPhongBenh as CTPB ON CTPB.IDPM = PM.ID " +
                  "INNER JOIN Benh as B ON B.ID = CTPB.IDB " +
                  "INNER JOIN ChiTietPhongMo as ctpm on ctpm.IDPM = pm.id " +
                  "WHERE b.id = '" + maBenh + "' and ctpm.ngay = '" + ngay + "' and ctpm.TrangThai = 'false'";
            using (SqlCommand command = new SqlCommand(query, sqlconnection))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);



                    if (table.Rows.Count > 0)
                    {
                        cbxPhongMoCM.DataSource = table;
                        cbxPhongMoCM.ValueMember = "IDPM";
                        cbxPhongMoCM.DisplayMember = "Display";
                    }
                    else
                    {
                        cbxPhongMoCM.DataSource = null;
                    }
                }
            }
            Closeconn();

            // Đọc kết quả truy vấn và thêm các phòng còn trống vào ComboBox

        }
        private void HienThiDSCaMo(int maPM, string TGDatLich)
        {
            Openconn();

            string query = "SELECT CTPM.IDtgm as ID, TGM.TenTGMo FROM PhongMo as PM  INNER JOIN ChiTietPhongMo as CTPM ON CTPM.IDPM = PM.ID INNER JOIN TGMo as TGM ON TGM.ID = CTPM.IDTGM where CTPM.Ngay = '" + TGDatLich + "' and CTPM.TrangThai = 0 and PM.id = '" + maPM + "'";
            using (SqlCommand command = new SqlCommand(query, sqlconnection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);

                cbxCaMoCM.DataSource = table;
                cbxCaMoCM.ValueMember = "ID";
                cbxCaMoCM.DisplayMember = "TenTGMo";
            }
            sqlconnection.Close();
        }

        private void cbxPhongMoCM_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime TGDatLich1 = dtpTGCaMo.Value.Date;
            string dinhdang = TGDatLich1.ToString("yyyy-MM-dd");
            int selectedID = 0;
            object selectedItem = cbxPhongMoCM.SelectedItem;
            if (selectedItem != null)
            {
                DataRowView row = (DataRowView)selectedItem;
                selectedID = Convert.ToInt32(row["IDPM"]);
                HienThiDSCaMo(selectedID, dinhdang);
            }
        }

        private void dtpChonNgayLT_ValueChanged(object sender, EventArgs e)
        {
            DateTime selectedDate = dtpChonNgayLT.Value;
            string formattedDate = selectedDate.ToString("yyyy-MM-dd");
            LoadLichLam(formattedDate);
        }

        private void LoadLichLam(string day)
        {
            try
            {
                Openconn();
                int idnv = NhanVien.GetIdNV(acc.Id).Id;
                sqlcommand = sqlconnection.CreateCommand();
                if (day == "")
                {

                    sqlcommand.CommandText = "SELECT  LT.NgayTruc, TGM.TenTGMo FROM LichTruc as LT  inner join TGMo as TGM  on lt.IDTGMo = tgm.ID where IDNV = @idnv AND NgayTruc >= CONVERT(varchar, GETDATE(), 111)";
                }
                else
                {
                    sqlcommand.CommandText = "SELECT  LT.NgayTruc, TGM.TenTGMo FROM LichTruc as LT join TGMo as TGM  on LT.IDTGMo = TGM.ID where IDNV = @idnv AND NgayTruc = '" + day + "'";
                }
                sqlcommand.Parameters.AddWithValue("@idnv", idnv);
                SqlDataReader reader = sqlcommand.ExecuteReader();
                livLichTruc.Items.Clear();
                while (reader.Read())
                {

                    DateTime NgayTruc = reader.GetDateTime(0);
                    string thu = NgayTruc.ToString("dddd", new CultureInfo("vi-VN")); // Lấy tên của ngày trong tuần từ cột NgayTruc
                    string TenTGMo = reader.GetString(1);
                    ListViewItem lvi = new ListViewItem(NgayTruc.ToString("dd/MM/yyyy"));
                    lvi.SubItems.Add(thu); // Thêm giá trị của cột mới "Ngày trong tuần"
                    lvi.SubItems.Add(TenTGMo);
                    livLichTruc.Items.Add(lvi);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnluu_Click(object sender, EventArgs e)
        {
            string idba = livCaMo.SelectedItems[0].SubItems[6].Text;

            string idCaMo = livCaMo.SelectedItems[0].SubItems[7].Text;
            string idb = livCaMo.SelectedItems[0].SubItems[5].Text;
            string dinhdang = livCaMo.SelectedItems[0].SubItems[8].Text;
            string idtgm = livCaMo.SelectedItems[0].SubItems[9].Text;
            string idphong = livCaMo.SelectedItems[0].SubItems[10].Text;
            // Lấy danh sách các nhân viên được tick trong CheckedListBox BacSi
            List<string> selectedBacSi = cklBacSiCM.CheckedItems.OfType<string>().ToList();

            // Lấy danh sách các nhân viên được tick trong CheckedListBox Yta
            List<string> selectedYta = cklYtaCM.CheckedItems.OfType<string>().ToList();
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn hủy ca mổ này không?", "Xác nhận hủy ca mổ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            try
            {
                Openconn();
                //xóa trong bảng lịch mổ
                string querydeleteCamo = "delete from lichmo where idcm = @idcm";
                SqlCommand commandInsertCamo = new SqlCommand(querydeleteCamo, sqlconnection);
                commandInsertCamo.Parameters.AddWithValue("@idcm", idCaMo);
                commandInsertCamo.ExecuteNonQuery();
                //xóa trong bảng camo
                string querydeletelichmo = "delete from camo where id = @idcm";
                SqlCommand commanddeletelichmo = new SqlCommand(querydeletelichmo, sqlconnection);
                commanddeletelichmo.Parameters.AddWithValue("@idcm", idCaMo);
                commanddeletelichmo.ExecuteNonQuery();
                //update bảng chitietphongmo
                string queryUpdateChiTietPhongMo = "UPDATE ChiTietPhongMo SET TrangThai = 'false' WHERE IDPM = @idphongmo AND ngay = @ngay and idtgm = @idtgm";
                SqlCommand commandUpdateChiTietPhongMo = new SqlCommand(queryUpdateChiTietPhongMo, sqlconnection);
                commandUpdateChiTietPhongMo.Parameters.AddWithValue("@idphongmo", idphong);
                commandUpdateChiTietPhongMo.Parameters.AddWithValue("@ngay", dinhdang);
                commandUpdateChiTietPhongMo.Parameters.AddWithValue("@idtgm", idtgm);
                commandUpdateChiTietPhongMo.ExecuteNonQuery();
                //update benhan
                string queryUpdatebenhan = "UPDATE benhan SET TrangThai = 'false' WHERE id = @idphongmo ";
                SqlCommand commandUpdatebenhan = new SqlCommand(queryUpdatebenhan, sqlconnection);
                commandUpdatebenhan.Parameters.AddWithValue("@idphongmo", idba);


                commandUpdatebenhan.ExecuteNonQuery();
                //update  lichtruc
                foreach (string nv in selectedBacSi)
                {
                    string querySelectNV = "SELECT id FROM nhanvien WHERE TenNV = @TenNV";
                    SqlCommand cmdSelectNV = new SqlCommand(querySelectNV, sqlconnection);
                    cmdSelectNV.Parameters.AddWithValue("@TenNV", nv);
                    int idNV = (int)cmdSelectNV.ExecuteScalar();
                    // Cập nhật trạng thái cho bảng lichtruc
                    string queryUpdateLichTruc = "UPDATE lichtruc SET TrangThai = 'false' WHERE IDNV = @idnv AND Ngaytruc = @ngay AND idtgmo = @ca";
                    SqlCommand commandUpdateLichTruc = new SqlCommand(queryUpdateLichTruc, sqlconnection);
                    commandUpdateLichTruc.Parameters.AddWithValue("@idnv", idNV);
                    commandUpdateLichTruc.Parameters.AddWithValue("@ngay", dinhdang);
                    commandUpdateLichTruc.Parameters.AddWithValue("@ca", idtgm);
                    commandUpdateLichTruc.ExecuteNonQuery();
                        string thongbao = "insert into thongbao(idnv, tieude, noidung, ngay, trangthai) values (@idnv, N'thông báo hủy ca mổ', N'Ca mổ cho bệnh án ' + @idba + N' vào ngày ' + @ngay + N' đã bị hủy', getdate(), 0)";
                        SqlCommand commandUpdatethongbao = new SqlCommand(thongbao, sqlconnection);
                        commandUpdatethongbao.Parameters.AddWithValue("@idnv", idNV);
                        commandUpdatethongbao.Parameters.AddWithValue("@idba", idba);
                        commandUpdatethongbao.Parameters.AddWithValue("@ngay", dinhdang);
                        commandUpdatethongbao.ExecuteNonQuery();

                        commandUpdatethongbao.ExecuteNonQuery();
                        sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "select email from nhanvien where id = '" + idNV + "'";
                string count = Convert.ToString(sqlcommand.ExecuteScalar());
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress("quanlyphongmt@gmail.com");
                        mail.To.Add(count);
                        mail.Subject = $"Thông báo: Hủy ca mổ";
                        mail.Body = $"Xin chào,\n\nCa mổ diễn ra vào "+idtgm+" ngày "+dinhdang+"\n\nMà bạn tham gia đã bị hủy, bạn sẽ nhận được thông báo khi ca mổ mới được xắp xếp.\n\nTrân trọng,\nBộ phận phòng mổ.";
                        // Gửi email
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential("quanlyphongmt@gmail.com", "jbmrynnksnbbqckn");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                foreach (string nv in selectedYta)
                {
                    string querySelectNV = "SELECT id FROM nhanvien WHERE TenNV = @TenNV";
                    SqlCommand cmdSelectNV = new SqlCommand(querySelectNV, sqlconnection);
                    cmdSelectNV.Parameters.AddWithValue("@TenNV", nv);
                    int idNV = (int)cmdSelectNV.ExecuteScalar();
                    // Cập nhật trạng thái cho bảng lichtruc
                    string queryUpdateLichTruc = "UPDATE lichtruc SET TrangThai = 'false' WHERE IDNV = @idnv AND Ngaytruc = @ngay AND idtgmo = @ca";
                    SqlCommand commandUpdateLichTruc = new SqlCommand(queryUpdateLichTruc, sqlconnection);
                    commandUpdateLichTruc.Parameters.AddWithValue("@idnv", idNV);
                    commandUpdateLichTruc.Parameters.AddWithValue("@ngay", dinhdang);
                    commandUpdateLichTruc.Parameters.AddWithValue("@ca", idtgm);
                    commandUpdateLichTruc.ExecuteNonQuery();
                        string thongbao = "insert into thongbao(idnv, tieude, noidung, ngay, trangthai) values (@idnv, N'thông báo hủy ca mổ', N'Ca mổ cho bệnh án ' + @idba + N' vào ngày ' + @ngay + N' đã bị hủy', getdate(), 0)";
                        SqlCommand commandUpdatethongbao = new SqlCommand(thongbao, sqlconnection);
                        commandUpdatethongbao.Parameters.AddWithValue("@idnv", idNV);
                        commandUpdatethongbao.Parameters.AddWithValue("@idba", idba);
                        commandUpdatethongbao.Parameters.AddWithValue("@ngay", dinhdang);
                        commandUpdatethongbao.ExecuteNonQuery();
                        sqlcommand = sqlconnection.CreateCommand();
                        sqlcommand.CommandText = "select email from nhanvien where id = '" + idNV + "'";
                        string count = Convert.ToString(sqlcommand.ExecuteScalar());
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress("quanlyphongmt@gmail.com");
                        mail.To.Add(count);
                        mail.Subject = $"Thông báo: Hủy ca mổ";
                        mail.Body = $"Xin chào,\n\nCa mổ diễn ra vào " + idtgm + " ngày " + dinhdang + "\n\nMà bạn tham gia đã bị hủy, bạn sẽ nhận được thông báo khi ca mổ mới được xắp xếp.\n\nTrân trọng,\nBộ phận phòng mổ.";
                        // Gửi email
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential("quanlyphongmt@gmail.com", "jbmrynnksnbbqckn");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);

                    }
                    MessageBox.Show("Hủy ca mổ thành công");
                    cbxPhongMoCM.SelectedIndex = -1;
                    cbxCaMoCM.SelectedIndex = -1;
                    cbxPhongHoiTinh.SelectedIndex = -1;
                    cklBacSiCM.Items.Clear();
                    cklYtaCM.Items.Clear();
                    HienThiDSCaMo();
                    HienThiDSBenhAn();

                    //  HienThiDSLichTruc();
                    LoadLichLam("");
                    HienThiDSCaMoTC();
                    HienthidanhsachcamoNull();
                    Hienthidanhsachcamoall();


                }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }


        }

        private void HienThiDSCaMoTC()
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "SELECT cm.id, bn.tenbn, pht.tenphong, ba.ghichu, ba.ylenh, ba.id " +
                                            "FROM CaMo cm " +
                                            "INNER JOIN BenhAn ba ON ba.ID = cm.IDBenhAn " +
                                            "INNER JOIN PhongHoiTinh pht ON pht.ID = cm.IDPhongHoiTinh " +
                                            "INNER JOIN BenhNhan bn ON bn.ID = ba.IDBenhNhan " +
                                            "WHERE cm.TinhTrang = 1";
                SqlDataReader reader = sqlcommand.ExecuteReader();
                livCaMoTC.Items.Clear();
                while (reader.Read())
                {
                    int MaCaMo = reader.GetInt32(0);
                    string TenBN = reader.GetString(1);
                    string TenPhong = reader.GetString(2);
                    string GhiChu = reader.GetString(3);
                    string YLenh = reader.GetString(4);
                    int idba = reader.GetInt32(5);


                    ListViewItem lvi = new ListViewItem(MaCaMo.ToString());
                    lvi.SubItems.Add(TenBN);
                    lvi.SubItems.Add(TenPhong);
                    lvi.SubItems.Add(GhiChu);
                    lvi.SubItems.Add(YLenh);
                    lvi.SubItems.Add(idba.ToString());

                    livCaMoTC.Items.Add(lvi);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }

        private void livCaMoTC_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (livCaMoTC.SelectedItems.Count == 0) return;
            ListViewItem lvi = livCaMoTC.SelectedItems[0];
            txtmacamo.Text = lvi.SubItems[0].Text;
            txttenbenhnhan.Text = lvi.SubItems[1].Text;
            txtphonghoitinh.Text = lvi.SubItems[2].Text;
            txtghichucm.Text = lvi.SubItems[3].Text;
            txtylenhcm.Text = lvi.SubItems[4].Text;
            txtidbacm.Text = lvi.SubItems[5].Text;
        }

        private void btnLuuCM_Click(object sender, EventArgs e)
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "UPDATE BenhAn SET GhiChu = @GhiChu, YLenh = @YLenh WHERE id = @idba";
                sqlcommand.Parameters.AddWithValue("@GhiChu", txtghichucm.Text);
                sqlcommand.Parameters.AddWithValue("@YLenh", txtylenhcm.Text);
                sqlcommand.Parameters.AddWithValue("@idba", txtidbacm.Text);
                int kq = sqlcommand.ExecuteNonQuery();
                if (kq > 0)
                {
                    MessageBox.Show("Cập nhật thành công !");
                    HienThiDSCaMoTC();
                    this.txtidbacm.Clear();
                    this.txtmacamo.Clear();
                    this.txttenbenhnhan.Clear();
                    this.txtphonghoitinh.Clear();
                    this.txtghichucm.Clear();
                    this.txtylenhcm.Clear();
                    HienThiDSCaMo();
                    HienThiDSBenhAn();

                    //  HienThiDSLichTruc();
                    LoadLichLam("");
                    HienThiDSCaMoTC();
                    HienthidanhsachcamoNull();
                    Hienthidanhsachcamoall();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại !");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txttimkiemcm_TextChanged(object sender, EventArgs e)
        {
            if (txtTimKiemCaMo.Text != "")
            {
                // Tạo danh sách tạm thời chứa các mục thỏa mãn điều kiện tìm kiếm
                List<ListViewItem> matchingItems = new List<ListViewItem>();

                foreach (ListViewItem item in livCaMoTC.Items)
                {
                    if (item.SubItems[0].Text.ToLower().Contains(txtTimKiemCaMo.Text.ToLower()) ||
                        item.SubItems[1].Text.ToLower().Contains(txtTimKiemCaMo.Text.ToLower()) ||
                        item.SubItems[2].Text.ToLower().Contains(txtTimKiemCaMo.Text.ToLower()) )
                    {
                        item.BackColor = SystemColors.Highlight;
                        item.ForeColor = SystemColors.HighlightText;
                        matchingItems.Add(item);
                    }
                }

                // Loại bỏ các mục không thỏa mãn khỏi danh sách
                livCaMoTC.Items.Clear();
                livCaMoTC.Items.AddRange(matchingItems.ToArray());

                if (livCaMoTC.SelectedItems.Count == 1)
                {
                    livCaMoTC.Focus();
                }


            }
            else
            {
                RefreshAll2();
            }
        }

        private void RefreshAll2()
        {
            HienThiDSCaMoTC();
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            HienThiDSBenhAn();
        }

        private void buttonin_Click(object sender, EventArgs e)
        {
            using (ExcelPackage excel = new ExcelPackage())

            {
                // Tạo một bảng tính trong tệp Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var worksheet = excel.Workbook.Worksheets.Add("camo");
                worksheet.HeaderFooter.OddHeader.CenteredText = "Thong tin ca mổ";
                //Thêm phần tiêu đề vào đầu file Excel
                worksheet.HeaderFooter.OddHeader.CenteredText = "&\"Segoe UI,Regular Bold\"&14Thong tin ca mổ";                // Lấy dữ liệu từ ListView và đưa chúng vào bảng tính Excel
                for (int i = 0; i < livCaMo.Items.Count; i++)
                {
                    for (int j = 0; j < livCaMo.Columns.Count; j++)
                    {
                        if (i == 0) // Thêm tên cột vào dòng đầu tiên của bảng tính
                        {
                            worksheet.Cells[1, j + 1].Value = livCaMo.Columns[j].Text;
                        }
                        string cellValue = livCaMo.Items[i].SubItems[j].Text;
                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            worksheet.Cells[i + 2, j + 1].Value = cellValue;
                        }
                        else
                        {
                            worksheet.Cells[i + 2, j + 1].Value = 0; // Hoặc "" nếu bạn muốn gán giá trị trống
                        }
                    }
                }


                // Lưu tệp Excel
                string filePath = @"D:\BVPT\QuanLyPhongMo\excelcamo\camo.xlsx"; // Thay đổi đường dẫn tới thư mục lưu trữ của bạn
                FileInfo excelFile = new FileInfo(filePath);
                int stt = 1;

                while (excelFile.Exists)
                {
                    string newFilePath = $@"D:\BVPT\QuanLyPhongMo\excelcamo\camo{stt}.xlsx";
                    excelFile = new FileInfo(newFilePath);
                    stt++;
                }

                excel.SaveAs(excelFile);
                Process.Start(excelFile.FullName);

            }
        }

        private void livDSCMSauDatLich_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (livDSCMSauDatLich.SelectedItems.Count == 0) return;
            ListViewItem lvi = livDSCMSauDatLich.SelectedItems[0];
            txtMaCaMoTT.Text = lvi.SubItems[0].Text;
            txtTenBenhNhanTT.Text = lvi.SubItems[2].Text;
            txtPhongHTTT.Text = lvi.SubItems[3].Text;
            txtGhiChuTT.Text = lvi.SubItems[6].Text;
            txtYLenhTT.Text = lvi.SubItems[7].Text;
            txtidTT.Text = lvi.SubItems[8].Text;
        }
        private void HienthidanhsachcamoNull()
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "SELECT DISTINCT cm.id,ba.MaBenhAn, bn.tenbn, pm.tenphongmo,lm.Ngay,tg.TenTGMo, ba.ghichu, ba.ylenh,ba.id " +
               "FROM CaMo cm " +
               "INNER JOIN BenhAn ba ON ba.ID = cm.IDBenhAn " +
               "INNER JOIN PhongHoiTinh pht ON pht.ID = cm.IDPhongHoiTinh " +
               "INNER JOIN BenhNhan bn ON bn.ID = ba.IDBenhNhan " +
               "INNER JOIN LichMo lm ON lm.IDCM = cm.ID " +
               "INNER JOIN TGMo tg ON tg.ID = lm.CaMo " +
               "INNER JOIN PhongMo pm ON pm.ID = cm.IDPhongMo " +
               "WHERE cm.TinhTrang is null";
                SqlDataReader reader = sqlcommand.ExecuteReader();
                livDSCMSauDatLich.Items.Clear();
                while (reader.Read())
                {
                    int MaCaMo = reader.GetInt32(0);
                    string MaBA = reader.GetString(1);
                    string TenBN = reader.GetString(2);
                    string TenPhong = reader.GetString(3);
                    DateTime ngay = reader.GetDateTime(4);
                    string tentgm = reader.GetString(5);
                    string GhiChu = reader.GetString(6);
                    string YLenh = reader.GetString(7);
                    int idba = reader.GetInt32(8);


                    ListViewItem lvi = new ListViewItem(MaCaMo.ToString());
                    lvi.SubItems.Add(MaBA);

                    lvi.SubItems.Add(TenBN);
                    lvi.SubItems.Add(TenPhong);
                    lvi.SubItems.Add(ngay.ToString("yyyy/MM/dd"));
                    lvi.SubItems.Add(tentgm);

                    lvi.SubItems.Add(GhiChu);
                    lvi.SubItems.Add(YLenh);
                    lvi.SubItems.Add(idba.ToString());


                    livDSCMSauDatLich.Items.Add(lvi);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        private void cbxTT_SelectedIndexChanged(object sender, EventArgs e)
        {

           
        }

        private void btnCapNhatTT_Click(object sender, EventArgs e)
        {
            bool checktc = checkBoxTC.Checked;
            bool checktb = checkBoxTB.Checked;
            if (checktc)
            {
                try
                {
                    Openconn();
                    sqlcommand = sqlconnection.CreateCommand();
                    sqlcommand.CommandText = "UPDATE camo SET tinhtrang = 1 WHERE id = @idba";
                    sqlcommand.Parameters.AddWithValue("@idba", txtMaCaMoTT.Text);
                 
                    int kq = sqlcommand.ExecuteNonQuery();
                    if (kq > 0)
                    {
                        MessageBox.Show("Cập nhật thành công !");
                        HienThiDSCaMoTC();
                        this.txtidbacm.Clear();
                        this.txtmacamo.Clear();
                        this.txttenbenhnhan.Clear();
                        this.txtphonghoitinh.Clear();
                        this.txtghichucm.Clear();
                        this.txtylenhcm.Clear();
                        HienthidanhsachcamoNull();
                        this.txtMaCaMoTT.Clear();
                        this.txtTenBenhNhanTT.Clear();
                        this.txtGhiChuTT.Clear();
                        this.txtYLenhTT.Clear();
                        this.txtPhongHTTT.Clear();
                        this.txtTenBenhNhanTT.Clear();
                        checkBoxTC.Checked = false;
                        checkBoxTB.Checked = false;
                        HienThiDSCaMo();
                        HienThiDSBenhAn();

                        //  HienThiDSLichTruc();
                        LoadLichLam("");
                        HienThiDSCaMoTC();
                        Hienthidanhsachcamoall();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thất bại !");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    Openconn();
                    sqlcommand = sqlconnection.CreateCommand();
                    sqlcommand.CommandText = "UPDATE camo SET tinhtrang = 0 WHERE id = @idba";
                    sqlcommand.Parameters.AddWithValue("@idba", txtMaCaMoTT.Text);

                    int kq = sqlcommand.ExecuteNonQuery();
                    string queryInsertLichMo = "update benhan set ghichu = @ghichu where id = @idba";
                    SqlCommand commandInsertLichMo = new SqlCommand(queryInsertLichMo, sqlconnection);
                    commandInsertLichMo.Parameters.AddWithValue("@ghichu", txtGhiChuTT.Text);
                    commandInsertLichMo.Parameters.AddWithValue("@idba", txtidTT.Text);
                   
                    commandInsertLichMo.ExecuteNonQuery();
                    if (kq > 0)
                    {
                        MessageBox.Show("Cập nhật thành công !");
                        HienThiDSCaMoTC();
                        this.txtidbacm.Clear();
                        this.txtmacamo.Clear();
                        this.txttenbenhnhan.Clear();
                        this.txtphonghoitinh.Clear();
                        this.txtghichucm.Clear();
                        this.txtylenhcm.Clear();
                        HienthidanhsachcamoNull();
                        this.txtMaCaMoTT.Clear();
                        this.txtTenBenhNhanTT.Clear();
                        this.txtGhiChuTT.Clear();
                        this.txtYLenhTT.Clear();
                        this.txtPhongHTTT.Clear();
                        this.txtTenBenhNhanTT.Clear();
                        checkBoxTC.Checked = false;
                        checkBoxTB.Checked = false;
                        HienThiDSCaMo();
                        HienThiDSBenhAn();

                        //  HienThiDSLichTruc();
                        LoadLichLam("");
                        HienThiDSCaMoTC();
                        Hienthidanhsachcamoall();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thất bại !");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
               
            }
        }

        private void checkBoxTC_CheckedChanged(object sender, EventArgs e)
        {
            bool checktt = checkBoxTC.Checked;
            if (checktt)
            {
                checkBoxTB.Checked = false;
            }
        }

        private void checkBoxTB_CheckedChanged(object sender, EventArgs e)
        {
            bool checktb = checkBoxTB.Checked;
            if (checktb)
            {
                checkBoxTC.Checked = false;
                txtGhiChuTT.Enabled = true;
            }
        }
        private void Hienthidanhsachcamoall()
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "SELECT DISTINCT cm.id,ba.MaBenhAn, bn.tenbn, pm.tenphongmo,lm.Ngay,tg.TenTGMo, ba.ghichu, ba.ylenh,cm.tinhtrang,ba.id " +
               "FROM CaMo cm " +
               "INNER JOIN BenhAn ba ON ba.ID = cm.IDBenhAn " +
               "INNER JOIN PhongHoiTinh pht ON pht.ID = cm.IDPhongHoiTinh " +
               "INNER JOIN BenhNhan bn ON bn.ID = ba.IDBenhNhan " +
               "INNER JOIN LichMo lm ON lm.IDCM = cm.ID " +
               "INNER JOIN TGMo tg ON tg.ID = lm.CaMo " +
               "INNER JOIN PhongMo pm ON pm.ID = cm.IDPhongMo " +
               "WHERE cm.TinhTrang is not null";
                SqlDataReader reader = sqlcommand.ExecuteReader();
                livXemLichSuPT.Items.Clear();
                while (reader.Read())
                {
                    int MaCaMo = reader.GetInt32(0);
                    string MaBA = reader.GetString(1);
                    string TenBN = reader.GetString(2);
                    string TenPhong = reader.GetString(3);
                    DateTime ngay = reader.GetDateTime(4);
                    string tentgm = reader.GetString(5);
                    string GhiChu = reader.GetString(6);
                    string YLenh = reader.GetString(7);
                    
                    bool tt = reader.GetBoolean(8);
                    int idba = reader.GetInt32(9);


                    ListViewItem lvi = new ListViewItem(MaCaMo.ToString());
                    lvi.SubItems.Add(MaBA);

                    lvi.SubItems.Add(TenBN);
                    lvi.SubItems.Add(TenPhong);
                    lvi.SubItems.Add(ngay.ToString("yyyy/MM/dd"));
                    lvi.SubItems.Add(tentgm);

                    lvi.SubItems.Add(GhiChu);
                    lvi.SubItems.Add(YLenh);
                    lvi.SubItems.Add(idba.ToString());
                    if (!tt) // nếu TrangThai == false
                    {
                        lvi.SubItems.Add(tt.ToString());
                        lvi.BackColor = Color.LightGray; // thay đổi màu nền của dòng
                    }
                    else
                    {
                        lvi.SubItems.Add(tt.ToString());

                    }


                    livXemLichSuPT.Items.Add(lvi);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Hienthidanhsachcamoalltheongay(string start,string end)
        {
            try
            {
                Openconn();
                sqlcommand = sqlconnection.CreateCommand();
                sqlcommand.CommandText = "SELECT DISTINCT cm.id,ba.MaBenhAn, bn.tenbn, pm.tenphongmo,lm.Ngay,tg.TenTGMo, ba.ghichu, ba.ylenh,cm.tinhtrang,ba.id " +
               "FROM CaMo cm " +
               "INNER JOIN BenhAn ba ON ba.ID = cm.IDBenhAn " +
               "INNER JOIN PhongHoiTinh pht ON pht.ID = cm.IDPhongHoiTinh " +
               "INNER JOIN BenhNhan bn ON bn.ID = ba.IDBenhNhan " +
               "INNER JOIN LichMo lm ON lm.IDCM = cm.ID " +
               "INNER JOIN TGMo tg ON tg.ID = lm.CaMo " +
               "INNER JOIN PhongMo pm ON pm.ID = cm.IDPhongMo " +
               "WHERE cm.TinhTrang is not null and lm.ngay >= '"+start+"' and lm.ngay <='"+end+"'";
                SqlDataReader reader = sqlcommand.ExecuteReader();
                livXemLichSuPT.Items.Clear();
                while (reader.Read())
                {
                    int MaCaMo = reader.GetInt32(0);
                    string MaBA = reader.GetString(1);
                    string TenBN = reader.GetString(2);
                    string TenPhong = reader.GetString(3);
                    DateTime ngay = reader.GetDateTime(4);
                    string tentgm = reader.GetString(5);
                    string GhiChu = reader.GetString(6);
                    string YLenh = reader.GetString(7);
                    int idba = reader.GetInt32(9);
                    bool tt = reader.GetBoolean(8);


                    ListViewItem lvi = new ListViewItem(MaCaMo.ToString());
                    lvi.SubItems.Add(MaBA);

                    lvi.SubItems.Add(TenBN);
                    lvi.SubItems.Add(TenPhong);
                    lvi.SubItems.Add(ngay.ToString("yyyy/MM/dd"));
                    lvi.SubItems.Add(tentgm);

                    lvi.SubItems.Add(GhiChu);
                    lvi.SubItems.Add(YLenh);
                    lvi.SubItems.Add(idba.ToString());
                    if (!tt) // nếu TrangThai == false
                    {
                        lvi.SubItems.Add(tt.ToString());
                        lvi.BackColor = Color.LightGray; // thay đổi màu nền của dòng
                    }
                    else
                    {
                        lvi.SubItems.Add(tt.ToString());

                    }


                    livXemLichSuPT.Items.Add(lvi);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void livXemLichSuPT_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel12_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dtpNgayBatDau_ValueChanged(object sender, EventArgs e)
        {
            DateTime start = dtpNgayBatDau.Value.Date;
            string formatstart = start.ToString("yyyy-MM-dd");
            DateTime end = dtpNgayKetThuc.Value.Date;
            string formatend = end.ToString("yyyy-MM-dd");
            Hienthidanhsachcamoalltheongay(formatstart, formatend);
        }

        private void dtpNgayKetThuc_ValueChanged(object sender, EventArgs e)
        {
            DateTime start = dtpNgayBatDau.Value.Date;
            string formatstart = start.ToString("yyyy-MM-dd");
            DateTime end = dtpNgayKetThuc.Value.Date;
            string formatend = end.ToString("yyyy-MM-dd");
            Hienthidanhsachcamoalltheongay(formatstart, formatend);
        }

        private void btnXemTatCa_Click(object sender, EventArgs e)
        {
            Hienthidanhsachcamoall();
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            using (ExcelPackage excel = new ExcelPackage())
            {
                // Tạo một bảng tính trong tệp Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var worksheet = excel.Workbook.Worksheets.Add("thongtincamo");

                // Lấy dữ liệu từ ListView và đưa chúng vào bảng tính Excel
                for (int i = 0; i < livXemLichSuPT.Items.Count; i++)
                {
                    for (int j = 0; j < livXemLichSuPT.Columns.Count; j++)
                    {
                        if (i == 0) // Thêm tên cột vào dòng đầu tiên của bảng tính
                        {
                            worksheet.Cells[1, j + 1].Value = livXemLichSuPT.Columns[j].Text;
                        }
                        string cellValue = livXemLichSuPT.Items[i].SubItems[j].Text;
                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            worksheet.Cells[i + 2, j + 1].Value = cellValue;
                        }
                        else
                        {
                            worksheet.Cells[i + 2, j + 1].Value = 0; // Hoặc "" nếu bạn muốn gán giá trị trống
                        }
                    }
                }


                // Lưu tệp Excel
                string filePath = @"D:\BVPT\QuanLyPhongMo\thongtincamo.xlsx"; // Thay đổi đường dẫn tới thư mục lưu trữ của bạn
                FileInfo excelFile = new FileInfo(filePath);
                int stt = 1;

                while (excelFile.Exists)
                {
                    string newFilePath = $@"D:\BVPT\QuanLyPhongMo\thongtincamo{stt}.xlsx";
                    excelFile = new FileInfo(newFilePath);
                    stt++;
                }

                excel.SaveAs(excelFile);
                Process.Start(excelFile.FullName);

            }
        }
    }

}

