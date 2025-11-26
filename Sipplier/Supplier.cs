using EventManagmentSystem.Classes;
using MaterialSkin;
using MaterialSkin.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace EventManagmentSystem.Sipplier
{
    public partial class Supplier : MaterialForm
    {
        private string supplierId;
        private string supplierName;
        private string supplierLogin;
        private string eventId;
        private string estimateId;
        public Supplier(string login)
        {
            InitializeComponent();
            supplierLogin = login;
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            GetSupplier();
            MessageBox.Show($"Приветствую {supplierName}");
            GetEstimate();
            LoadSupplierEventsDetailed();
            LoadSupplierEarnings();
        }
        private void LoadSupplierEventsDetailed()
        {
            if (string.IsNullOrEmpty(supplierId))
                return;

            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                string query = @"SELECT 
                e.`Event_id`,
                e.`Наименование` as 'Мероприятие',
                DATE_FORMAT(e.`Дата мероприятия`, '%d.%m.%Y') as 'Дата',
                s.`Количество`,
                s.`Цена за единицу`,
                (s.`Количество` * s.`Цена за единицу`) as 'Сумма',
                sv.`Наименование` as 'Услуга',
                sv.`Единица измерения` as 'Ед_изм'
                FROM `estimate` s
                INNER JOIN `event` e ON s.`Мероприятие` = e.`Event_id`
                INNER JOIN `supplier_of_services` sup ON s.`Поставщик` = sup.`Supplier_id`
                INNER JOIN `services` sv ON sup.`Услуга` = sv.`Service_id`
                WHERE s.`Поставщик` = @supplierId
                ORDER BY e.`Дата мероприятия` DESC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@supplierId", supplierId);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView6.DataSource = dt;
                CalculateTotalEstimate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке мероприятий поставщика: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void CalculateTotalEstimate()
        {
            decimal total = 0;

            foreach (DataGridViewRow row in dataGridView6.Rows)
            {
                if (row.Cells["Сумма"].Value != null)
                {
                    total += Convert.ToDecimal(row.Cells["Сумма"].Value);
                }
            }

            label22.Text = $"Всего смета составляет: {total:C}";
        }
        private void LoadSupplierEarnings()
        {
            if (string.IsNullOrEmpty(supplierId))
            {
                MessageBox.Show("Поставщик не найден!");
                return;
            }
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"SELECT 
                            SUM(s.`Количество` * s.`Цена за единицу`) as TotalAmount,
                            COUNT(DISTINCT s.`Мероприятие`) as EventsCount
                        FROM `estimate` s
                        WHERE s.`Поставщик` = @supplierId";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@supplierId", supplierId);
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    decimal totalAmount = reader["TotalAmount"] != DBNull.Value ?
                                        Convert.ToDecimal(reader["TotalAmount"]) : 0;
                    int eventsCount = reader["EventsCount"] != DBNull.Value ?
                                    Convert.ToInt32(reader["EventsCount"]) : 0;
                    label24.Text = $"Ваш общий заработок: {totalAmount:C}";
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о заработке: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GetSupplier()
        {
            string log = supplierLogin;

            DBUser db = new DBUser();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `Логин` = @log", db.getConnection());
            command.Parameters.AddWithValue("@log", log);
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                supplierName = dt.Rows[0]["От кого"].ToString();
                GetSupp();
            }
            else
            {
                MessageBox.Show("Пользователь не найден!");
            }
        }
        private void GetSupp()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT `Supplier_id` FROM `supplier_of_services` WHERE `Наименование` = @ln", db.getConnection());
            command.Parameters.AddWithValue("@ln", supplierName);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                supplierId = dt.Rows[0]["Supplier_id"].ToString();
            }
            db.closeConnection();
        }
        private void GetEstimate()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `estimate` WHERE `Поставщик` = @id", db.getConnection());
            command.Parameters.AddWithValue("@id", supplierId);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if(dt.Rows.Count > 0)
            {
                eventId = dt.Rows[0]["Мероприятие"].ToString();
            }
            dataGridView5.DataSource = dt;
        }
        private void GetEvent()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `event` WHERE `Event_id` = @id", db.getConnection());
            command.Parameters.AddWithValue("@id", eventId);
            DataTable dt = new DataTable();
            adapter.SelectCommand= command;
            adapter.Fill(dt);
            dataGridView4.DataSource = dt;
        }
        private void GetGuests()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `guest` WHERE `Мероприятие` = @id", db.getConnection());
            command.Parameters.AddWithValue("@id", eventId);
            DataTable dt = new DataTable();
            adapter.SelectCommand = command;
            adapter.Fill(dt);
            dataGridView1.DataSource = dt;
        }

        private void dataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView5.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView5.SelectedRows[0];
                textBox18.Text = selectedRow.Cells["Количество"].Value?.ToString() ?? "";
                textBox17.Text = selectedRow.Cells["Цена за единицу"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Estimate_id"].Value != null &&
                    selectedRow.Cells["Estimate_id"].Value != DBNull.Value)
                {
                    estimateId = selectedRow.Cells["Estimate_id"].Value.ToString();
                }
                    if (selectedRow.Cells["Мероприятие"].Value != null &&
                    selectedRow.Cells["Мероприятие"].Value != DBNull.Value)
                {
                    eventId = selectedRow.Cells["Мероприятие"].Value.ToString();
                    GetEvent();
                }
            }
        }

        private void dataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView4.SelectedRows.Count == 0)
                return;

            DataGridViewRow selectedRow = dataGridView4.SelectedRows[0];
            if (selectedRow == null || selectedRow.Cells["Наименование"].Value == null)
                return;

            try
            {
                textBox15.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
                textBox14.Text = selectedRow.Cells["Бюджет"].Value?.ToString() ?? "";
                textBox16.Text = selectedRow.Cells["Дата мероприятия"].Value?.ToString() ?? "";
                textBox13.Text = selectedRow.Cells["Основные пожелания"].Value?.ToString() ?? "";
                GetGuests();
                if (selectedRow.Cells["Подтверждение гостей"].Value != null &&
                    Convert.ToInt32(selectedRow.Cells["Подтверждение гостей"].Value) > 0)
                {
                    label6.Text = "Статус: список гостей подтвержден";
                }
                else
                {
                    label6.Text = "Статус: список гостей не подтвержден";
                }
                if (selectedRow.Cells["Подтверждение сметы"].Value != null &&
                    Convert.ToInt32(selectedRow.Cells["Подтверждение сметы"].Value) > 0)
                {
                    label1.Text = "Статус: смета подтверждена";
                }
                else
                {
                    label1.Text = "Статус: смета не подтверждена";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных мероприятия: {ex.Message}");
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            if (selectedRow == null || selectedRow.Cells["Фамилия"].Value == null)
                return;
            try
            {
                textBox2.Text = selectedRow.Cells["Фамилия"].Value?.ToString() ?? "";
                textBox3.Text = selectedRow.Cells["Имя"].Value?.ToString() ?? "";
                textBox4.Text = selectedRow.Cells["Отчество"].Value?.ToString() ?? "";
                textBox5.Text = selectedRow.Cells["Контактные данные"].Value?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }
        private void materialButton1_Click(object sender, EventArgs e)
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("UPDATE `supplier_of_services` SET `Готовность` = 'В процессе выполнения' WHERE `Supplier_id` = @id;", db.getConnection());
            command.Parameters.AddWithValue("@id", supplierId);
            db.openConnection();
            int rowsAffected = command.ExecuteNonQuery();
            db.closeConnection();
            if (rowsAffected > 0)
            {
                MessageBox.Show("Вы начали выполнять заказ!");
                GetEvent();
            }
            else
            {
                MessageBox.Show("Не удалось начать выполнение заказа.");
            }
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("UPDATE `supplier_of_services` SET `Готовность` = 'Готов' WHERE `Supplier_id` = @id;", db.getConnection());
            command.Parameters.AddWithValue("@id", supplierId);
            db.openConnection();
            int rowsAffected = command.ExecuteNonQuery();
            db.closeConnection();
            if (rowsAffected > 0)
            {
                MessageBox.Show("Вы выполнили свой заказ!");
            }
            else
            {
                MessageBox.Show("Не удалось выполнить заказ.");
            }
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            int count = Convert.ToInt32(textBox17.Text);
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("UPDATE `estimate` SET `Цена за единицу` = @count WHERE `Estimate_id` = @id;", db.getConnection());
            command.Parameters.AddWithValue("@id", estimateId);
            command.Parameters.AddWithValue("@count", count);
            db.openConnection();
            int rowsAffected = command.ExecuteNonQuery();
            db.closeConnection();
            if (rowsAffected > 0)
            {
                MessageBox.Show("Цена прикреплена успешно!");
                GetEstimate();
            }
            else
            {
                MessageBox.Show("Не удалось прикрепить цену.");
            }
        }

        private void materialButton23_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 form1 = new Form1();
            form1.ShowDialog();
        }

        private void materialButton24_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из приложения?", "Подтверждение выхода",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
