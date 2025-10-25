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
    public partial class FormKategori : Form
    {
        public FormKategori()
        {
            InitializeComponent();
        }

        private void LoadDataKategori()
        {
            dgvKategori.Rows.Clear();
            dgvKategori.Columns.Clear();

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT k.Id, k.NamaKategori, COUNT(p.Id) as JumlahProduk 
                                   FROM Kategori k 
                                   LEFT JOIN Produk p ON k.Id = p.KategoriId 
                                   GROUP BY k.Id, k.NamaKategori";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    
                    dgvKategori.Columns.Add("Id", "ID");
                    dgvKategori.Columns.Add("NamaKategori", "Nama Kategori");
                    dgvKategori.Columns.Add("JumlahProduk", "Jumlah Produk");

                    dgvKategori.Columns["Id"].Visible = false;

                    while (reader.Read())
                    {
                        int jumlahProduk = Convert.ToInt32(reader["JumlahProduk"]);
                        dgvKategori.Rows.Add(
                            reader["Id"],
                            reader["NamaKategori"],
                            jumlahProduk + " produk"
                        );
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data kategori: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            UpdateButtonStatus();
        }

        private void UpdateButtonStatus()
        {
            bool hasSelection = dgvKategori.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnHapus.Enabled = hasSelection;

            if (!hasSelection)
            {
                btnEdit.BackColor = Color.LightGray;
                btnHapus.BackColor = Color.LightGray;
            }
            else
            {
                btnEdit.BackColor = SystemColors.Control;
                btnHapus.BackColor = SystemColors.Control;
            }
        }

        private void FormKategori_Load(object sender, EventArgs e)
        {
            dgvKategori.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKategori.MultiSelect = false;
            dgvKategori.ReadOnly = true;
            dgvKategori.RowHeadersVisible = false;
            dgvKategori.AllowUserToAddRows = false;

            dgvProdukTerkait.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProdukTerkait.MultiSelect = false;
            dgvProdukTerkait.ReadOnly = true;
            dgvProdukTerkait.RowHeadersVisible = false;
            dgvProdukTerkait.AllowUserToAddRows = false;

            LoadDataKategori();

            btnEdit.Enabled = false;
            btnHapus.Enabled = false;

            lblTotal.Text = "Total: 0 produk";
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNamaKategori.Text))
            {
                MessageBox.Show("Nama kategori tidak boleh kosong.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaKategori.Focus();
                return;
            }

            if (txtNamaKategori.Text.Trim().Length < 3)
            {
                MessageBox.Show("Nama kategori minimal 3 karakter.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaKategori.Focus();
                return;
            }

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Kategori (NamaKategori) VALUES (@nama)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nama", txtNamaKategori.Text.Trim());
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Kategori berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtNamaKategori.Clear();
                    LoadDataKategori();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambahkan kategori: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvKategori.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih kategori terlebih dahulu.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvKategori.SelectedRows[0].Cells["Id"].Value);
            string nama = txtNamaKategori.Text.Trim();

            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show("Nama kategori tidak boleh kosong.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaKategori.Focus();
                return;
            }

            if (nama.Length < 3)
            {
                MessageBox.Show("Nama kategori minimal 3 karakter.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaKategori.Focus();
                return;
            }

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Kategori SET NamaKategori = @nama WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Kategori berhasil diubah.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtNamaKategori.Clear();
                    LoadDataKategori();

                    dgvKategori.ClearSelection();
                    UpdateButtonStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mengubah kategori: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (dgvKategori.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih kategori yang ingin dihapus.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvKategori.SelectedRows[0].Cells["Id"].Value);
            string namaKategori = dgvKategori.SelectedRows[0].Cells["NamaKategori"].Value.ToString();

            int jumlahProduk = GetJumlahProdukByKategori(id);
            if (jumlahProduk > 0)
            {
                MessageBox.Show($"Tidak dapat menghapus kategori '{namaKategori}' karena masih memiliki {jumlahProduk} produk.\nHapus atau pindahkan produk terlebih dahulu.",
                              "Tidak Dapat Dihapus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Yakin ingin menghapus kategori '{namaKategori}'?",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection conn = Koneksi.GetConnection())
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM Kategori WHERE Id = @id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Kategori berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtNamaKategori.Clear();
                        LoadDataKategori();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal menghapus kategori: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private int GetJumlahProdukByKategori(int kategoriId)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Produk WHERE KategoriId = @kategoriId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@kategoriId", kategoriId);
                    return (int)cmd.ExecuteScalar();
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        private void dgvKategori_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKategori.SelectedRows.Count == 0)
            {
                
                dgvProdukTerkait.Rows.Clear();
                dgvProdukTerkait.Columns.Clear();
                txtNamaKategori.Clear();
                lblTotal.Text = "Total: 0 produk";
                UpdateButtonStatus();
                return;
            }

            try
            {
                int kategoriId = Convert.ToInt32(dgvKategori.SelectedRows[0].Cells["Id"].Value);
                string namaKategori = dgvKategori.SelectedRows[0].Cells["NamaKategori"].Value.ToString();
                txtNamaKategori.Text = namaKategori;

                UpdateButtonStatus();
                LoadProdukTerkait(kategoriId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data kategori: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProdukTerkait(int kategoriId)
        {
            dgvProdukTerkait.Rows.Clear();
            dgvProdukTerkait.Columns.Clear();

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT NamaProduk, Harga, Stok FROM Produk WHERE KategoriId = @kategoriId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@kategoriId", kategoriId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    
                    dgvProdukTerkait.Columns.Add("NamaProduk", "Nama Produk");
                    dgvProdukTerkait.Columns.Add("Harga", "Harga");
                    dgvProdukTerkait.Columns.Add("Stok", "Stok");

                    int totalProduk = 0;
                    while (reader.Read())
                    {
                        decimal harga = Convert.ToDecimal(reader["Harga"]);
                        string hargaFormatted = FormatRupiah(harga);

                        dgvProdukTerkait.Rows.Add(
                            reader["NamaProduk"].ToString(),
                            hargaFormatted,
                            reader["Stok"].ToString()
                        );
                        totalProduk++;
                    }
                    reader.Close();

                    
                    lblTotal.Text = $"Total: {totalProduk} produk";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat produk terkait: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblTotal.Text = "Total: 0 produk";
                }
            }
        }

        private string FormatRupiah(decimal amount)
        {
            return "Rp. " + amount.ToString("N0").Replace(",", ".");
        }

        private void dgvKategori_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvKategori.Rows[e.RowIndex].Selected = true;
            }
        }

        private void lblTotal_Click(object sender, EventArgs e)
        {
            
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void Form2_Load(object sender, EventArgs e) { }
        private void dgvProdukTerkait_CellContentClick_1(object sender, DataGridViewCellEventArgs e) { }
        private void txtNamaKategori_TextChanged(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}