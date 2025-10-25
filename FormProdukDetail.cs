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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        public int? ProdukId { get; set; } = null;

        private void LoadDataProduk()
        {
            if (ProdukId == null) return;

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT NamaProduk, Harga, Stok, KategoriId, Deskripsi 
                                   FROM Produk WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", ProdukId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtNamaProduk.Text = reader["NamaProduk"].ToString();
                        txtHarga.Text = reader["Harga"].ToString();
                        txtStok.Text = reader["Stok"].ToString();
                        txtDeskripsi.Text = reader["Deskripsi"].ToString();

                        if (reader["KategoriId"] != DBNull.Value)
                        {
                            int kategoriId = Convert.ToInt32(reader["KategoriId"]);
                            cmbKategori.SelectedValue = kategoriId;
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data produk: " + ex.Message);
                }
            }
        }

        private void FormProdukDetail_Load(object sender, EventArgs e)
        {
            LoadKategoriComboBox();

            if (ProdukId.HasValue)
            {
                this.Text = "Edit Produk";
                lblJudul.Text = "Edit Produk";
                LoadDataProduk();

                if (string.IsNullOrWhiteSpace(txtNamaProduk.Text))
                {
                    txtNamaProduk.Focus();
                }
            }
            else
            {
                this.Text = "Tambah Produk";
                lblJudul.Text = "Tambah Produk";
            }
        }

        private void LoadKategoriComboBox()
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Id, NamaKategori FROM Kategori";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, string> kategoriDict = new Dictionary<int, string>();
                    while (reader.Read())
                    {
                        kategoriDict.Add((int)reader["Id"], reader["NamaKategori"].ToString());
                    }

                    cmbKategori.DataSource = new BindingSource(kategoriDict, null);
                    cmbKategori.DisplayMember = "Value";
                    cmbKategori.ValueMember = "Key";

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat kategori: " + ex.Message);
                }
            }
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtNamaProduk.Text))
            {
                errorProvider1.SetError(txtNamaProduk, "Nama produk tidak boleh kosong.");
                valid = false;
            }

            if (!decimal.TryParse(txtHarga.Text, out decimal harga) || harga < 0)
            {
                errorProvider1.SetError(txtHarga, "Harga harus berupa angka positif.");
                valid = false;
            }

            if (!int.TryParse(txtStok.Text, out int stok) || stok < 0)
            {
                errorProvider1.SetError(txtStok, "Stok harus berupa angka ≥ 0.");
                valid = false;
            }

            if (cmbKategori.SelectedItem == null)
            {
                errorProvider1.SetError(cmbKategori, "Kategori wajib dipilih.");
                valid = false;
            }

            if (!valid) return;

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query;

                    if (ProdukId.HasValue)
                    {
                        query = @"UPDATE Produk 
                                 SET NamaProduk = @nama, 
                                     Harga = @harga, 
                                     Stok = @stok, 
                                     KategoriId = @kategori,
                                     Deskripsi = @deskripsi 
                                 WHERE Id = @id";
                    }
                    else
                    {
                        query = @"INSERT INTO Produk (NamaProduk, Harga, Stok, KategoriId, Deskripsi) 
                                 VALUES (@nama, @harga, @stok, @kategori, @deskripsi)";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nama", txtNamaProduk.Text.Trim());
                    cmd.Parameters.AddWithValue("@harga", harga);
                    cmd.Parameters.AddWithValue("@stok", stok);
                    cmd.Parameters.AddWithValue("@kategori", ((KeyValuePair<int, string>)cmbKategori.SelectedItem).Key);
                    cmd.Parameters.AddWithValue("@deskripsi", txtDeskripsi.Text.Trim());

                    if (ProdukId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@id", ProdukId.Value);
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        string message = ProdukId.HasValue ? "diperbarui" : "ditambahkan";
                        MessageBox.Show($"Produk berhasil {message}!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Tidak ada data yang berubah.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan produk: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtHarga_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtStok_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnBatal_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label1_Click_1(object sender, EventArgs e) { }
        private void lblKategori_Click(object sender, EventArgs e) { }
        private void cmbKategori_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label1_Click_2(object sender, EventArgs e) { }
        private void txtStok_TextChanged(object sender, EventArgs e) { }
    }
}