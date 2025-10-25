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
    public partial class FormProdukUtama : Form
    {
        public FormProdukUtama()
        {
            InitializeComponent();
        }

        private void LoadDataProduk()
        {
            dgvProduk.Rows.Clear();
            dgvProduk.Columns.Clear();

            dgvProduk.Columns.Add("Id", "ID");
            dgvProduk.Columns.Add("NamaProduk", "Nama Produk");
            dgvProduk.Columns.Add("Harga", "Harga");
            dgvProduk.Columns.Add("Stok", "Stok");
            dgvProduk.Columns.Add("Kategori", "Kategori");
            dgvProduk.Columns.Add("Deskripsi", "Deskripsi");

            dgvProduk.Columns["Id"].Visible = false;

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT p.Id, p.NamaProduk, p.Harga, p.Stok, k.NamaKategori, p.Deskripsi 
                                   FROM Produk p LEFT JOIN Kategori k ON p.KategoriId = k.Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        decimal harga = Convert.ToDecimal(reader["Harga"]);
                        string hargaFormatted = FormatRupiah(harga);

                        dgvProduk.Rows.Add(
                            reader["Id"],
                            reader["NamaProduk"],
                            hargaFormatted,
                            reader["Stok"],
                            reader["NamaKategori"],
                            reader["Deskripsi"]
                        );
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menampilkan data: " + ex.Message);
                }
            }

            UpdateButtonStatus();
        }

        private string FormatRupiah(decimal amount)
        {
            return "Rp. " + amount.ToString("N0").Replace(",", ".");
        }

        private int? GetSelectedProductId()
        {
            if (dgvProduk.SelectedRows.Count > 0 && dgvProduk.SelectedRows[0] != null)
            {
                var selectedRow = dgvProduk.SelectedRows[0];
                if (selectedRow.Cells["Id"].Value != null && selectedRow.Cells["Id"].Value != DBNull.Value)
                {
                    return Convert.ToInt32(selectedRow.Cells["Id"].Value);
                }
            }

            if (dgvProduk.CurrentRow != null && dgvProduk.CurrentRow.Cells["Id"].Value != null)
            {
                return Convert.ToInt32(dgvProduk.CurrentRow.Cells["Id"].Value);
            }

            return null;
        }

        private void UpdateButtonStatus()
        {
            bool hasSelection = GetSelectedProductId().HasValue;
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

        private void FormProdukUtama_Load(object sender, EventArgs e)
        {
            dgvProduk.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProduk.MultiSelect = false;
            dgvProduk.ReadOnly = true;
            dgvProduk.RowHeadersVisible = false;
            dgvProduk.AllowUserToAddRows = false;

            btnEdit.Enabled = false;
            btnHapus.Enabled = false;

            LoadDataProduk();
        }

        private void dgvProduk_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvProduk.Rows[e.RowIndex].Selected = true;
                UpdateButtonStatus();
            }
        }

        private void dgvProduk_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStatus();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            Form3 frm = new Form3();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadDataProduk();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            int? id = GetSelectedProductId();
            if (id == null)
            {
                MessageBox.Show("Pilih produk yang ingin diedit.");
                return;
            }

            Form3 form = new Form3();
            form.ProdukId = id;
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadDataProduk();
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            int? id = GetSelectedProductId();
            if (id == null)
            {
                MessageBox.Show("Pilih produk yang ingin dihapus.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Yakin ingin menghapus produk ini?",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = Koneksi.GetConnection())
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM Produk WHERE Id = @id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Produk berhasil dihapus!");
                        LoadDataProduk();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal menghapus produk: " + ex.Message);
                    }
                }
            }
        }

        private void btnKategori_Click(object sender, EventArgs e)
        {
            FormKategori formKategori = new FormKategori();
            formKategori.ShowDialog();

            LoadDataProduk();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadDataProduk();
        }

        private void dgvProduk_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }
    }
}