using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectKI
{
    public partial class FormTransaksiPenjualan : Form
    {
        private void HitungTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItem.Rows)
            {
                // Skip new row yang kosong
                if (!row.IsNewRow)
                {
                    total += Convert.ToDecimal(row.Cells["Subtotal"].Value);
                }
            }
            lblTotal.Text = $"Total: Rp {total:N0}";
        }

        private decimal GetHargaProduk(int produkId)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Harga FROM Produk WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", produkId);
                var result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception($"Produk dengan ID {produkId} tidak ditemukan");
                }

                return Convert.ToDecimal(result);
            }
        }

        private int GetStokProduk(int produkId)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Stok FROM Produk WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", produkId);
                var result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception($"Produk dengan ID {produkId} tidak ditemukan");
                }

                return Convert.ToInt32(result);
            }
        }

        private void KurangiStokProduk(int produkId, int jumlah)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Produk SET Stok = Stok - @jumlah WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", produkId);
                cmd.Parameters.AddWithValue("@jumlah", jumlah);
                cmd.ExecuteNonQuery();
            }
        }

        public FormTransaksiPenjualan()
        {
            InitializeComponent();
        }

        private void cmbProduk_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtJumlah_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (cmbProduk.SelectedItem == null || !int.TryParse(txtJumlah.Text, out int jumlah) || jumlah <= 0)
            {
                MessageBox.Show("Pilih produk dan jumlah valid.");
                return;
            }

            try
            {
                var selected = (KeyValuePair<int, string>)cmbProduk.SelectedItem;
                int produkId = selected.Key;
                string namaProduk = selected.Value;

                // Cek stok tersedia
                int stok = GetStokProduk(produkId);
                if (jumlah > stok)
                {
                    MessageBox.Show($"Stok tidak mencukupi! Stok tersedia: {stok}");
                    return;
                }

                decimal harga = GetHargaProduk(produkId);
                decimal subtotal = harga * jumlah;
                dgvItem.Rows.Add(produkId, namaProduk, harga, jumlah, subtotal);
                HitungTotal();

                // Bersihkan input jumlah setelah berhasil tambah
                txtJumlah.Clear();
                txtJumlah.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void dgvItem_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle klik tombol hapus
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvItem.Columns["Hapus"].Index)
            {
                HapusBaris(e.RowIndex);
            }
        }

        private void HapusBaris(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dgvItem.Rows.Count && !dgvItem.Rows[rowIndex].IsNewRow)
            {
                dgvItem.Rows.RemoveAt(rowIndex);
                HitungTotal();
            }
        }

        private void lblTotal_Click(object sender, EventArgs e)
        {

        }

        private void Jumlah_Click(object sender, EventArgs e)
        {

        }

        private void FormTransaksiPenjualan_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, NamaProduk FROM Produk", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                Dictionary<int, string> produkDict = new Dictionary<int, string>();
                while (reader.Read())
                {
                    produkDict.Add((int)reader["Id"], reader["NamaProduk"].ToString());
                }
                cmbProduk.DataSource = new BindingSource(produkDict, null);
                cmbProduk.DisplayMember = "Value";
                cmbProduk.ValueMember = "Key";
            }

            // Setup kolom dgvItem
            dgvItem.Columns.Add("ProdukId", "ProdukId");
            dgvItem.Columns["ProdukId"].Visible = false;
            dgvItem.Columns.Add("NamaProduk", "Nama Produk");
            dgvItem.Columns.Add("Harga", "Harga");
            dgvItem.Columns.Add("Jumlah", "Jumlah");
            dgvItem.Columns.Add("Subtotal", "Subtotal");

            // Tambah kolom tombol hapus
            DataGridViewButtonColumn btnHapus = new DataGridViewButtonColumn();
            btnHapus.HeaderText = "Aksi";
            btnHapus.Text = "Hapus";
            btnHapus.Name = "Hapus";
            btnHapus.UseColumnTextForButtonValue = true;
            dgvItem.Columns.Add(btnHapus);

            // Set properties DataGridView
            dgvItem.AllowUserToAddRows = false;
            dgvItem.ReadOnly = true;
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (dgvItem.Rows.Count == 0)
            {
                MessageBox.Show("Belum ada item ditambahkan.");
                return;
            }

            // Validasi stok sebelum simpan
            try
            {
                foreach (DataGridViewRow row in dgvItem.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        int produkId = Convert.ToInt32(row.Cells["ProdukId"].Value);
                        int jumlah = Convert.ToInt32(row.Cells["Jumlah"].Value);
                        int stok = GetStokProduk(produkId);

                        if (jumlah > stok)
                        {
                            MessageBox.Show($"Stok produk {row.Cells["NamaProduk"].Value} tidak mencukupi! Stok tersedia: {stok}");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validasi stok: {ex.Message}");
                return;
            }

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlTransaction trx = conn.BeginTransaction();
                try
                {
                    // 1. Insert ke Penjualan
                    decimal total = 0;
                    foreach (DataGridViewRow row in dgvItem.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            total += Convert.ToDecimal(row.Cells["Subtotal"].Value);
                        }
                    }

                    SqlCommand cmdPenjualan = new SqlCommand(
                        "INSERT INTO Penjualan (Tanggal, TotalHarga) VALUES (@tgl, @total); SELECT SCOPE_IDENTITY();",
                        conn, trx);
                    cmdPenjualan.Parameters.AddWithValue("@tgl", DateTime.Now);
                    cmdPenjualan.Parameters.AddWithValue("@total", total);
                    int penjualanId = Convert.ToInt32(cmdPenjualan.ExecuteScalar());

                    // 2. Insert ke PenjualanDetail dan kurangi stok
                    foreach (DataGridViewRow row in dgvItem.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int produkId = Convert.ToInt32(row.Cells["ProdukId"].Value);
                        int jumlah = Convert.ToInt32(row.Cells["Jumlah"].Value);
                        decimal subtotal = Convert.ToDecimal(row.Cells["Subtotal"].Value);

                        // Insert ke PenjualanDetail
                        SqlCommand cmdDetail = new SqlCommand(
                            @"INSERT INTO PenjualanDetail (PenjualanId, ProdukId, Jumlah, Subtotal)
                              VALUES (@pjId, @prodId, @jumlah, @subtotal)",
                            conn, trx);
                        cmdDetail.Parameters.AddWithValue("@pjId", penjualanId);
                        cmdDetail.Parameters.AddWithValue("@prodId", produkId);
                        cmdDetail.Parameters.AddWithValue("@jumlah", jumlah);
                        cmdDetail.Parameters.AddWithValue("@subtotal", subtotal);
                        cmdDetail.ExecuteNonQuery();

                        // Kurangi stok produk
                        SqlCommand cmdUpdateStok = new SqlCommand(
                            "UPDATE Produk SET Stok = Stok - @jumlah WHERE Id = @id",
                            conn, trx);
                        cmdUpdateStok.Parameters.AddWithValue("@id", produkId);
                        cmdUpdateStok.Parameters.AddWithValue("@jumlah", jumlah);
                        cmdUpdateStok.ExecuteNonQuery();
                    }

                    trx.Commit();
                    MessageBox.Show("Transaksi berhasil disimpan!");
                    dgvItem.Rows.Clear();
                    HitungTotal();
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    MessageBox.Show("Gagal menyimpan transaksi: " + ex.Message);
                }
            }
        }

        // Event untuk tombol hapus baris (bisa juga dengan keyboard Delete)
        private void dgvItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dgvItem.CurrentRow != null && !dgvItem.CurrentRow.IsNewRow)
            {
                HapusBaris(dgvItem.CurrentRow.Index);
            }
        }
    }
}