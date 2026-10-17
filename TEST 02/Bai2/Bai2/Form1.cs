using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Bai2
{
    public partial class Form1 : Form
    {
        private BindingList<Product> _productList;
        private BindingSource _bindingSource;
        private string _currentImagePath = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _productList = new BindingList<Product>();
            _bindingSource = new BindingSource();
            _bindingSource.DataSource = _productList;

            cboCategory.Items.AddRange(new string[] { "Linh kiện máy tính", "Điện thoại", "Thiết bị mạng", "Phụ kiện", "Khác" });

            SetupDataGridView();
            dgvProducts.DataSource = _bindingSource;

            timer1.Interval = 1000;
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            _productList.ListChanged += ProductList_ListChanged;

            ClearInputs();
            UpdateStatusStrip();
        }

        private void SetupDataGridView()
        {
            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Id", HeaderText = "Mã SP", Width = 80 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Name", HeaderText = "Tên sản phẩm", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Category", HeaderText = "Loại", Width = 100 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Price", HeaderText = "Đơn giá", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Quantity", HeaderText = "Số lượng", Width = 80 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "ImagePath", HeaderText = "Đường dẫn ảnh", Width = 150 });
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void ProductList_ListChanged(object sender, ListChangedEventArgs e)
        {
            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            int totalQuantity = 0;
            decimal totalValue = 0;

            foreach (var product in _productList)
            {
                totalQuantity += product.Quantity;
                totalValue += product.Quantity * product.Price;
            }

            lblTotalQuantity.Text = $"Tổng số lượng: {totalQuantity}";
            lblTotalValue.Text = $"Tổng giá trị: {totalValue:N0} VNĐ";
        }

        private bool ValidateInput()
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider1.SetError(txtName, "Tên sản phẩm không được để trống.");
                isValid = false;
            }

            if (cboCategory.SelectedIndex == -1)
            {
                errorProvider1.SetError(cboCategory, "Vui lòng chọn loại sản phẩm.");
                isValid = false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                errorProvider1.SetError(txtPrice, "Đơn giá phải là số hợp lệ và lớn hơn hoặc bằng 0.");
                isValid = false;
            }

            return isValid;
        }

        private void ClearInputs()
        {
            txtId.Text = "SP" + DateTime.Now.ToString("HHmmss");
            txtName.Clear();
            txtPrice.Clear();
            numQuantity.Value = 0;
            cboCategory.SelectedIndex = -1;
            picProductImage.Image = null;
            _currentImagePath = "";
            txtName.Focus();
            errorProvider1.Clear();
        }

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh sản phẩm";
                ofd.Filter = "Image Files (*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _currentImagePath = ofd.FileName;
                    try
                    {
                        picProductImage.Image = Image.FromFile(_currentImagePath);
                        picProductImage.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hiển thị ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            Product newProduct = new Product
            {
                Id = txtId.Text,
                Name = txtName.Text.Trim(),
                Category = cboCategory.SelectedItem?.ToString() ?? "",
                Price = decimal.Parse(txtPrice.Text),
                Quantity = (int)numQuantity.Value,
                ImagePath = _currentImagePath
            };

            _productList.Add(newProduct);
            ClearInputs();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                if (!ValidateInput()) return;

                Product selected = (Product)dgvProducts.CurrentRow.DataBoundItem;
                selected.Name = txtName.Text.Trim();
                selected.Category = cboCategory.SelectedItem?.ToString() ?? "";
                selected.Price = decimal.TryParse(txtPrice.Text, out decimal price) ? price : 0;
                selected.Quantity = (int)numQuantity.Value;
                selected.ImagePath = _currentImagePath;

                _bindingSource.ResetCurrentItem();
                ClearInputs();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                if (MessageBox.Show("Bạn có muốn xóa sản phẩm này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _productList.Remove((Product)dgvProducts.CurrentRow.DataBoundItem);
                    ClearInputs();
                }
            }
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null && dgvProducts.Focused)
            {
                Product selected = (Product)dgvProducts.CurrentRow.DataBoundItem;
                txtId.Text = selected.Id;
                txtName.Text = selected.Name;
                cboCategory.SelectedItem = selected.Category;
                txtPrice.Text = selected.Price.ToString();
                numQuantity.Value = selected.Quantity;
                _currentImagePath = selected.ImagePath;
                errorProvider1.Clear();

                if (!string.IsNullOrEmpty(_currentImagePath) && File.Exists(_currentImagePath))
                {
                    picProductImage.Image = Image.FromFile(_currentImagePath);
                    picProductImage.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    picProductImage.Image = null;
                }
            }
        }

        private void mnuOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                ofd.Title = "Open Data File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show($"Đã chọn file: {ofd.FileName}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void mnuSaveReport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                sfd.Title = "Save Report";
                sfd.FileName = "Report_" + DateTime.Now.ToString("ddMMyyyy");

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            sw.WriteLine("--- BÁO CÁO TỔNG HỢP KHO ---");
                            sw.WriteLine($"Thời gian xuất: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                            sw.WriteLine(lblTotalQuantity.Text);
                            sw.WriteLine(lblTotalValue.Text);
                            sw.WriteLine("----------------------------");
                        }
                        MessageBox.Show("Lưu báo cáo thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void mnuImportCSV_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV Files (*.csv)|*.csv";
                ofd.Title = "Chọn file dữ liệu nhập";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _productList.Clear();
                        string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);

                        for (int i = 1; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            string[] parts = line.Split(',');
                            if (parts.Length >= 6)
                            {
                                Product p = new Product
                                {
                                    Id = parts[0],
                                    Name = parts[1].Trim('"'),
                                    Category = parts[2],
                                    Price = decimal.Parse(parts[3]),
                                    Quantity = int.Parse(parts[4]),
                                    ImagePath = parts[5]
                                };
                                _productList.Add(p);
                            }
                        }
                        MessageBox.Show("Nhập dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi đọc file: Vui lòng kiểm tra định dạng dữ liệu.\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void mnuExportCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.Title = "Lưu báo cáo kho";
                sfd.FileName = "BaoCaoKho_" + DateTime.Now.ToString("ddMMyyyy");

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            sw.WriteLine("Id,Name,Category,Price,Quantity,ImagePath");
                            foreach (var p in _productList)
                            {
                                string safeName = p.Name.Contains(",") ? $"\"{p.Name}\"" : p.Name;
                                sw.WriteLine($"{p.Id},{safeName},{p.Category},{p.Price},{p.Quantity},{p.ImagePath}");
                            }
                        }
                        MessageBox.Show("Xuất báo cáo thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát chương trình?",
                                                  "Xác nhận thoát",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void lblTime_Click(object sender, EventArgs e)
        {

        }
    }
}
