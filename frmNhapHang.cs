using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace QLTH1
{
    public partial class frmNhapHang : Form
    {
        string sCon = "Data Source=DPLN-2410;Initial Catalog=QUAN_LY_TAP_HOA0;Integrated Security=True;TrustServerCertificate=True";
        public frmNhapHang()
        {
            InitializeComponent();
            this.Load += frmNhapHang_Load;
            //dataGridView1.CellClick += dataGridView1_CellContentClick;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {

        }

        private void btnHuy_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void frmNhapHang_Load(object sender, EventArgs e)
        {
            LoadMaSP();
            cbMaSP.SelectedIndex = -1; // Không chọn sẵn sản phẩm nào
            txtQC.Text = "";
            LoadDanhSachHoaDon();

        }

        private void LoadDanhSachHoaDon()
        {
            using (SqlConnection con = new SqlConnection(sCon))
            {
                try
                {
                    con.Open();
                    string sQuery = @"SELECT 
                        HDN.MAHD_NH, 
                        HDN.MANV, 
                        HDN.MANCC, 
                        HDN.NGAYGIO_NH, 
                        HDN.TRANGTHAI, 
                        HDN.TONGTIEN_TRUOCTHUE,
                        HDN.TIEN_THANHTOAN,
                        HDN.THUE_SUAT,
                        LH.MALO,
                        LH.MASP, 
                        LH.THUNG, 
                        LH.LE, 
                        LH.QC,
                        LH.HSD,
                        LH.DONGIA_NH,
                        LH.CHIETKHAU
                      FROM HOADON_NHAP HDN 
                      LEFT JOIN LOHANG LH ON LH.MAHD_NH = HDN.MAHD_NH
                      WHERE HDN.TRANGTHAI = 1 
                      AND (LH.TRANGTHAI = 1)";
                    SqlDataAdapter adapter = new SqlDataAdapter(sQuery, con);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "LOHANG");
                    dataGridView1.DataSource = ds.Tables["LOHANG"];
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xảy ra lỗi khi tải danh sách hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            

        }

        //private object ParseTrangThai(string txt)
        //{
            
        //}


        private void btnLuu_Click(object sender, EventArgs e)
        {
            string maNCC = txtMaNCC.Text.Trim();
            if (!string.IsNullOrEmpty(maNCC))
            {
                HienThiThongTinNCC(maNCC); // Hàm này lấy thông tin NCC từ CSDL rồi hiển thị lên groupBox
            }
            else
            {
                MessageBox.Show("Vui lòng nhập mã nhà cung cấp!");
            }
            string maNV = txtMaNV.Text.Trim();
            DateTime ngayNhap = dtNgayGio.Value;
            decimal thue = Convert.ToDecimal(txtThue.Text);

            using (SqlConnection conn = new SqlConnection(sCon))
            using (SqlCommand cmd = new SqlCommand("themHoaDonNhap", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@maNCC", maNCC);
                cmd.Parameters.AddWithValue("@maNV", maNV);
                cmd.Parameters.AddWithValue("@ngayGio_NH", ngayNhap);
                cmd.Parameters.AddWithValue("@thueSuat", thue);
                cmd.Parameters.AddWithValue("@tien_thanhtoan", DBNull.Value); // NULL lúc đầu

                SqlParameter maHD = new SqlParameter("@maHD_NH", SqlDbType.Char, 10)
                {
                    Direction = ParameterDirection.Output
                };
                SqlParameter thongbao = new SqlParameter("@thongbao", SqlDbType.NVarChar, 500)
                {
                    Direction = ParameterDirection.Output
                };
                SqlParameter retVal = new SqlParameter("@ret_val", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(maHD);
                cmd.Parameters.Add(thongbao);
                cmd.Parameters.Add(retVal);

                conn.Open();
                cmd.ExecuteNonQuery();

                if ((bool)retVal.Value)
                {
                    txtMaHDN.Text = maHD.Value.ToString(); // hiện mã hóa đơn mới
                    MessageBox.Show(thongbao.Value.ToString(), "Thông báo");

                    groupBoxHD.Enabled = false;
                    groupBoxChiTiet.Enabled = true; // cho nhập lô hàng
                }
                else
                {
                    MessageBox.Show(thongbao.Value.ToString(), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void HienThiThongTinNCC(string maNCC)
        {
            using (SqlConnection con = new SqlConnection(sCon))
            {
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("TimKiemNhaCungCap", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Dùng đúng tên tham số
                    cmd.Parameters.AddWithValue("@MaNCC", maNCC);
                    cmd.Parameters.AddWithValue("@TenNCC", DBNull.Value);
                    cmd.Parameters.AddWithValue("SDT_NCC", DBNull.Value);

                    SqlParameter thongBao = new SqlParameter("@ThongBao", SqlDbType.NVarChar, 500)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(thongBao);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.SelectCommand.CommandTimeout = 120;

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    //dataGridView1.DataSource = dt;


                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        txtTenNCC.Text = row["Ten_NCC"].ToString();
                        txtSDT.Text = row["SoDienThoai"].ToString();
                        txtDiaChi.Text = row["DiaChi"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải lại thông tin: " + ex.Message);
                }
            }
        }


        private void btnThem_Click_1(object sender, EventArgs e)
        {
            try
            {
                string maHD = txtMaHDN.Text;
                string maSP = cbMaSP.SelectedValue.ToString();
                decimal donGia = Convert.ToDecimal(txtDonGia.Text);
                int thung = int.Parse(txtThung.Text);
                int le = int.Parse(txtLe.Text);
                decimal chietKhau = Convert.ToDecimal(txtCK.Text);
                DateTime hsd = dtHSD.Value;

                using (SqlConnection conn = new SqlConnection(sCon))
                using (SqlCommand cmd = new SqlCommand("ThemLoHang", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // ⚠️ Tăng thời gian timeout tránh lỗi "Execution Timeout Expired"
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.AddWithValue("@maSP", maSP);
                    cmd.Parameters.AddWithValue("@maHD_NH", maHD);
                    cmd.Parameters.AddWithValue("@donGia_NH", donGia);
                    cmd.Parameters.AddWithValue("@thung", thung);
                    cmd.Parameters.AddWithValue("@le", le);
                    cmd.Parameters.AddWithValue("@chietKhau", chietKhau);
                    cmd.Parameters.AddWithValue("@HSD", hsd);

                    SqlParameter thongbao = new SqlParameter("@thongbao", SqlDbType.NVarChar, 500)
                    {
                        Direction = ParameterDirection.Output
                    };
                    SqlParameter retVal = new SqlParameter("@ret_val", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(thongbao);
                    cmd.Parameters.Add(retVal);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    if ((bool)retVal.Value)
                    {
                        MessageBox.Show(thongbao.Value.ToString(), "✅ Thành công");

                        // Cập nhật tổng tiền từ trigger
                        LoadTongTien(maHD);

                        // Cho phép nhập tiền thanh toán
                        txtTien.Enabled = true;
                        btnCapNhat.Enabled = true; // Nếu có nút cập nhật riêng
                        groupBoxHD.Enabled = true;   // 👉 Nếu bạn muốn bật toàn bộ group nhập tiền

                        // Reset các trường chi tiết
                        ResetChiTiet();

                        // Focus vào txtTien
                        txtTien.Focus();
                    }
                    else
                    {
                        MessageBox.Show(thongbao.Value.ToString(), "❌ Lỗi");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi hệ thống");
            }

        }

        private void LoadTongTien(string maHD)
        {
            using (SqlConnection conn = new SqlConnection(sCon))
            using (SqlCommand cmd = new SqlCommand("SELECT TONGTIEN_TRUOCTHUE FROM HOADON_NHAP WHERE MAHD_NH = @maHD", conn))
            {
                cmd.Parameters.AddWithValue("@maHD", maHD);
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    txtTongTien.Text = string.Format("{0:N0}", result); // Hiển thị số đẹp (1,000,000)
                }
                else
                {
                    txtTongTien.Text = "0";
                }
            }
        }

        private void ResetChiTiet()
        {
            cbMaSP.SelectedIndex = 0;
            txtDonGia.Clear();
            txtThung.Clear();
            txtLe.Clear();
            txtCK.Clear();
            dtHSD.Value = DateTime.Now;
        }



        private void cbMaSP_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadQuyChuan();
        }

        private void LoadQuyChuanTheoSanPham(string maSP)
        {
            
        }

        private void LoadMaSP()
        {
            using (SqlConnection con = new SqlConnection(sCon))
            {
                string query = "SELECT MASP FROM HANGHOA ORDER BY CAST(SUBSTRING(MASP, 3, LEN(MASP)) AS INT)";
                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cbMaSP.DataSource = dt;
                cbMaSP.DisplayMember = "MASP";     // 👈 Hiển thị mã sản phẩm
                cbMaSP.ValueMember = "MASP";       // 👈 Giá trị cũng là mã sản phẩm
            }
        }


        private void txtMaSP_Leave(object sender, EventArgs e)
        {
            LoadQuyChuan();
        }

        private void txtMaSP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadQuyChuan(); // Gọi hàm đã viết

                // Chuyển sang control tiếp theo
                this.SelectNextControl((Control)sender, true, true, true, true);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LoadQuyChuan()
        {
            string maSP = cbMaSP.Text.Trim();
            if (!string.IsNullOrEmpty(maSP))
            {
                SqlConnection con = new SqlConnection(sCon);
                {
                    string sql = "SELECT QC FROM HANGHOA WHERE MASP = @masp";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@masp", maSP);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        txtQC.Text = result.ToString();
                    else
                        txtQC.Text = "";
                }
            }
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                string maHD = txtMaHDN.Text.Trim();
                decimal tienThanhToan = Convert.ToDecimal(txtTien.Text);

                using (SqlConnection con = new SqlConnection(sCon))
                using (SqlCommand cmd = new SqlCommand("CapNhatTienThanhToan", con)) // Bạn cần tạo SP này
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@maHD", maHD);
                    cmd.Parameters.AddWithValue("@tienThanhToan", tienThanhToan);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Đã cập nhật tiền thanh toán thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật tiền thanh toán: " + ex.Message);
            }
        }

        private void btnXong_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hoàn tất nhập hàng!");

            // Reset toàn bộ form
            txtMaNCC.Clear();
            txtMaNV.Clear();
            txtThue.Clear();
            txtTenNCC.Clear();
            txtSDT.Clear();
            txtDiaChi.Clear();
            txtMaHDN.Clear();
            txtTongTien.Clear();
            txtTien.Clear();
            txtTien.Enabled = false;
            cbMaSP.SelectedIndex = -1;
            ResetChiTiet();

            groupBoxHD.Enabled = true;
        }

        private void ResetForm()
        {
            MessageBox.Show("✅ Hóa đơn nhập đã được hoàn tất!", "Thông báo");

            // Reset giao diện
            txtMaHDN.Clear();
            txtMaNCC.Clear();
            txtTenNCC.Clear();
            txtSDT.Clear();
            txtDiaChi.Clear();
            txtMaNV.Clear();
            txtThue.Clear();
            txtTongTien.Clear();
            txtTien.Clear();
            groupBoxHD.Enabled = true;
            txtTien.Enabled = false;

           
        }

        private void btnHuy_Click_1(object sender, EventArgs e)
        {

        }

        private void txtMaLO_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

