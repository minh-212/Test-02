using System;
using System.ComponentModel;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Test_02_bài_1
{
    public partial class Form1 : Form
    {
        // Sử dụng BindingList để tự động thông báo thay đổi cho Grid
        private BindingList<Device> _deviceList;
        private BindingSource _bindingSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. Khởi tạo danh sách và BindingSource
            _deviceList = new BindingList<Device>();
            _bindingSource = new BindingSource();
            _bindingSource.DataSource = _deviceList;

            // 2. Load dữ liệu mẫu cho ComboBox
            string[] categories = { "Điện thoại", "Máy tính xách tay", "Linh kiện PC", "Thiết bị âm thanh", "Phụ kiện" };
            cboCategory.Items.AddRange(categories);

            // 3. Cấu hình DataGridView hoàn toàn bằng code
            SetupDataGridView();

            // Gán dữ liệu cho Grid
            dgvDevices.DataSource = _bindingSource;

            // Khởi tạo trạng thái ban đầu
            ClearInputs();
        }

        private void SetupDataGridView()
        {
            dgvDevices.AutoGenerateColumns = false; // Tắt tự động sinh cột
            dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Chọn nguyên dòng
            dgvDevices.ReadOnly = true; // Không cho sửa trực tiếp trên Grid
            dgvDevices.AllowUserToAddRows = false;

            // Cột Mã thiết bị
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Id",
                HeaderText = "Mã TB",
                Width = 80
            });

            // Cột Tên thiết bị (Tự động co giãn)
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Name",
                HeaderText = "Tên thiết bị",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Cột Loại thiết bị
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Category",
                HeaderText = "Loại",
                Width = 120
            });

            // Cột Đơn giá (Định dạng tiền tệ)
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Price",
                HeaderText = "Đơn giá",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } // Định dạng số hoặc "C0" cho tiền tệ
            });

            // Cột Số lượng
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Quantity",
                HeaderText = "Tồn kho",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
        }

        // Kiểm tra hợp lệ dữ liệu (Validation)
        private bool ValidateInput()
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtDeviceName.Text))
            {
                errorProvider1.SetError(txtDeviceName, "Tên thiết bị không được để trống.");
                isValid = false;
            }

            if (cboCategory.SelectedIndex == -1)
            {
                errorProvider1.SetError(cboCategory, "Vui lòng chọn loại thiết bị.");
                isValid = false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                errorProvider1.SetError(txtPrice, "Đơn giá phải là số hợp lệ và lớn hơn hoặc bằng 0.");
                isValid = false;
            }

            return isValid;
        }

        // Sinh mã thiết bị tự động (Dựa trên thời gian)
        private string GenerateId()
        {
            return "TB" + DateTime.Now.ToString("HHmmssff");
        }

        // Làm sạch Form
        private void ClearInputs()
        {
            txtDeviceId.Text = GenerateId();
            txtDeviceName.Clear();
            cboCategory.SelectedIndex = -1;
            txtPrice.Clear();
            numQuantity.Value = 0;
            if (dgvDevices.Rows.Count > 0)
                dgvDevices.ClearSelection();
            errorProvider1.Clear();
            txtDeviceName.Focus();
        }

        // Xử lý Thêm mới
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            Device newDevice = new Device
            {
                Id = txtDeviceId.Text,
                Name = txtDeviceName.Text.Trim(),
                Category = cboCategory.SelectedItem.ToString(),
                Price = decimal.Parse(txtPrice.Text),
                Quantity = (int)numQuantity.Value
            };

            // BindingList tự động phản ánh thay đổi lên Grid
            _deviceList.Add(newDevice);
            ClearInputs();
        }

        // Xử lý Cập nhật
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvDevices.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn thiết bị cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            // Lấy đối tượng đang được chọn
            Device selectedDevice = (Device)dgvDevices.CurrentRow.DataBoundItem;

            // Cập nhật thông tin
            selectedDevice.Name = txtDeviceName.Text.Trim();
            selectedDevice.Category = cboCategory.SelectedItem.ToString();
            selectedDevice.Price = decimal.Parse(txtPrice.Text);
            selectedDevice.Quantity = (int)numQuantity.Value;

            // Ép BindingSource cập nhật lại giao diện (Đồng bộ 2 chiều)
            _bindingSource.ResetCurrentItem();

            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputs();
        }

        // Xử lý Xóa
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDevices.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn thiết bị cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa thiết bị này không?",
                                                  "Xác nhận xóa",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Device selectedDevice = (Device)dgvDevices.CurrentRow.DataBoundItem;
                _deviceList.Remove(selectedDevice); // Xóa khỏi danh sách, Grid tự động cập nhật
                ClearInputs();
            }
        }

        // Xử lý Làm mới
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        // Xử lý đổ dữ liệu ngược lên Control khi click vào Grid
        private void dgvDevices_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDevices.CurrentRow != null && dgvDevices.Focused)
            {
                Device selectedDevice = (Device)dgvDevices.CurrentRow.DataBoundItem;

                txtDeviceId.Text = selectedDevice.Id;
                txtDeviceName.Text = selectedDevice.Name;
                cboCategory.SelectedItem = selectedDevice.Category;
                txtPrice.Text = selectedDevice.Price.ToString();
                numQuantity.Value = selectedDevice.Quantity;
            }
        }
    }
}
       // private void txtDeviceId_TextChanged(object sender, EventArgs e)
