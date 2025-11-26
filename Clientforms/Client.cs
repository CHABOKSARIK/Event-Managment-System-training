using EventManagmentSystem.Classes;
using MaterialSkin;
using MaterialSkin.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EventManagmentSystem.Client
{
    public partial class Client : MaterialForm
    {
        private string FirstNameClient;
        private string LastNameClient;
        private string clientId;
        private int currentEventId;
        private int selectedSupplierId;
        private int selectedServiceId;
        private int selectedEstimateId;
        private string userLogin;
        bool GuestsAccepted = false;
        bool EstimateAccepted = false;
        public Client(string login)
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            userLogin = login;
            GetUser();
            LoadPayments();
            LoadEventData();
            textBox19.Text = $"{LastNameClient} {FirstNameClient}";
            CheckStatuses();
            LoadNearestEvent();
            LoadTotalPayments();
            LoadClientEvents();
            dataGridView1.ReadOnly = true;
            dataGridView2.ReadOnly = true;
            dataGridView5.ReadOnly = true;
            dataGridView6.ReadOnly = true;
        }
        private void LoadClientEvents()
        {
            if (string.IsNullOrEmpty(clientId))
            {
                MessageBox.Show("Клиент не найден!");
                return;
            }

            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                string query = @"SELECT `Event_id`, 
                                CONCAT(`Наименование`, ' (', DATE_FORMAT(`Дата мероприятия`, '%d.%m.%Y'), ')') as DisplayName
                        FROM `event` 
                        WHERE `Клиент` = @clientId 
                        ORDER BY `Дата мероприятия` DESC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@clientId", clientId);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                comboBox1.DataSource = dt;
                comboBox1.DisplayMember = "DisplayName";
                comboBox1.ValueMember = "Event_id";

                if (dt.Rows.Count == 0)
                {
                    comboBox1.Text = "Нет доступных мероприятий";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке мероприятий: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadPayments()
        {
                if (string.IsNullOrEmpty(clientId))
                {
                    MessageBox.Show("Клиент не найден!");
                    return;
                }
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string query = @"SELECT 
                            op.`Pay_id`,
                            e.`Наименование` as 'Мероприятие',
                            op.`Оплата` as 'Сумма',
                            op.`Дата оплаты` as 'Дата оплаты',
                            CONCAT(e.`Наименование`, ' (', DATE_FORMAT(e.`Дата мероприятия`, '%d.%m.%Y'), ')') as 'Мероприятие_полное'
                        FROM `Pay` op
                        INNER JOIN `event` e ON op.`Мероприятие` = e.`Event_id`
                        WHERE e.`Клиент` = @clientId
                        ORDER BY op.`Дата оплаты` DESC";
                    MySqlCommand command = new MySqlCommand(query, db.getConnection());
                    command.Parameters.AddWithValue("@clientId", clientId);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView6.DataSource = dt;
                    if (dataGridView6.Columns.Count > 0)
                    {
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке истории оплат: {ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
            }
        private void LoadTotalPayments()
        {
            DBEvent db = new DBEvent();
            if (string.IsNullOrEmpty(clientId))
                return;
            try
            {
                db.openConnection();
                string query = @"SELECT SUM(op.`Оплата`) as `Полная оплата`
                        FROM `Pay` op
                        INNER JOIN `event` e ON op.`Мероприятие` = e.`Event_id`
                        WHERE e.`Клиент` = @clientId";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@clientId", clientId);
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    decimal totalPaid = Convert.ToDecimal(result);
                    label24.Text = $"Вы всего оплатили: {totalPaid:C}";
                }
                else
                {
                    label24.Text = "Вы всего оплатили: 0 руб.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке суммы оплат: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadNearestEvent()
        {
            if (string.IsNullOrEmpty(clientId))
                return;

            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"SELECT `Наименование`, `Дата мероприятия`, `бюджет`, `Основные пожелания`
                        FROM `event` 
                        WHERE `Клиент` = @clientId 
                          AND `Дата мероприятия` >= CURDATE()
                        ORDER BY `Дата мероприятия` ASC 
                        LIMIT 1";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@clientId", clientId);
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string eventName = reader["Наименование"].ToString();
                    DateTime eventDate = Convert.ToDateTime(reader["Дата мероприятия"]);
                    decimal budget = Convert.ToDecimal(reader["бюджет"]);
                    string indicators = reader["Основные пожелания"].ToString();
                    textBox1.Text = $"Название: {eventName}\r\n" +
                                              $"Дата: {eventDate:dd.MM.yyyy}\r\n" +
                                              $"Бюджет: {budget:C}\r\n" +
                                              $"Показатели: {indicators}";
                }
                else
                {
                    textBox1.Text = "На данный момент не назначено мероприятий";
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ближайшего мероприятия: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void CheckStatuses()
        {
            if (GuestsAccepted)
            {
                label7.Text = "Список подтвержден";
            }
            else
            {
                label7.Text = "Список не подтвержден";
            }
            if (EstimateAccepted)
            {
                label8.Text = "Смета подтверждена";
            }
            else
            {
                label8.Text = "Смета не подтверждена";
            }
        }
        private void GetUser()
        {
            string log = userLogin; 

            DBUser db = new DBUser();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `Логин` = @log", db.getConnection());
            command.Parameters.AddWithValue("@log", log);
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                FirstNameClient = dt.Rows[0]["Имя"].ToString();
                LastNameClient = dt.Rows[0]["Фамилия"].ToString();
                GetClient();
            }
            else
            {
                MessageBox.Show("Пользователь не найден!");
            }
        }
        private void GetClient()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT `Client_id` FROM `client` WHERE `Фамилия` = @ln AND `Имя` = @fn", db.getConnection());
            command.Parameters.AddWithValue("@ln", LastNameClient);
            command.Parameters.AddWithValue("@fn", FirstNameClient);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if(dt.Rows.Count > 0)
            {
                clientId = dt.Rows[0]["Client_id"].ToString();
            }
            db.closeConnection();
        }
        private void LoadEventData()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `event` WHERE `Клиент` = @clientid", db.getConnection());
            command.Parameters.AddWithValue("@clientid", clientId);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dataGridView2.DataSource = dt;
            }
            db.closeConnection();
        }
        private void LoadGuests()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `guest` WHERE `Мероприятие` = @eventid", db.getConnection());
            command.Parameters.AddWithValue("@eventid", currentEventId);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt;
            }
            db.closeConnection();
        }
        private void LoadEstimates()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `estimate` WHERE `Мероприятие` = @eventid", db.getConnection());
            command.Parameters.AddWithValue("@eventid", currentEventId);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dataGridView5.DataSource = dt;
            }
            db.closeConnection();
        }
        private void LoadEventSuppliers()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `supplier_of_services` WHERE `Supplier_id` = @id", db.getConnection());
            command.Parameters.AddWithValue("@id", selectedSupplierId);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                textBox7.Text = dt.Rows[0]["Наименование"].ToString();
                textBox9.Text = dt.Rows[0]["Контактные данные"].ToString();
                textBox10.Text = dt.Rows[0]["Реквизиты"].ToString();
                selectedServiceId = Convert.ToInt32(dt.Rows[0]["Услуга"]);
            }
            db.closeConnection();
            LoadEventService();
        }
        private void LoadEventService()
        {
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `services` WHERE `Service_id` = @id", db.getConnection());
            command.Parameters.AddWithValue("@id", selectedServiceId);
            db.openConnection();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if(dt.Rows.Count > 0)
            {
                textBox11.Text = dt.Rows[0]["Наименование"].ToString();
                textBox12.Text = dt.Rows[0]["Единица измерения"].ToString();
            }
        }
        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                textBox15.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
                textBox14.Text = selectedRow.Cells["Бюджет"].Value?.ToString() ?? "";
                textBox16.Text = selectedRow.Cells["Дата мероприятия"].Value?.ToString() ?? "";
                textBox13.Text = selectedRow.Cells["Основные пожелания"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Event_id"].Value != null &&
                    selectedRow.Cells["Event_id"].Value != DBNull.Value)
                {
                    currentEventId = Convert.ToInt32(selectedRow.Cells["Event_id"].Value);
                    LoadGuests();
                    LoadEstimates();
                }
                GuestsAccepted = Convert.ToInt32(selectedRow.Cells["Подтверждение гостей"].Value) > 0;
                EstimateAccepted = Convert.ToInt32(selectedRow.Cells["Подтверждение сметы"].Value) > 0;
                CheckStatuses();
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                textBox2.Text = selectedRow.Cells["Фамилия"].Value?.ToString() ?? "";
                textBox3.Text = selectedRow.Cells["Имя"].Value?.ToString() ?? "";
                textBox4.Text = selectedRow.Cells["Отчество"].Value?.ToString() ?? "";
                textBox5.Text = selectedRow.Cells["Контактные данные"].Value?.ToString() ?? "";
            }
        }
        private void dataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView5.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView5.SelectedRows[0];
                textBox18.Text = selectedRow.Cells["Количество"].Value?.ToString() ?? "";
                textBox17.Text = selectedRow.Cells["Цена за единицу"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Поставщик"].Value != null &&
                    selectedRow.Cells["Поставщик"].Value != DBNull.Value)
                {
                    selectedSupplierId = Convert.ToInt32(selectedRow.Cells["Поставщик"].Value);
                    LoadEventSuppliers();
                }
                if (selectedRow.Cells["Estimate_id"].Value != null &&
                    selectedRow.Cells["Estimate_id"].Value != DBNull.Value)
                {
                    selectedEstimateId = Convert.ToInt32(selectedRow.Cells["Estimate_id"].Value);
                }
            }
        }
        private void materialButton1_Click(object sender, EventArgs e)
        {
            DBEvent db = new DBEvent();
            MySqlCommand command = new MySqlCommand("UPDATE `event` SET `Подтверждение гостей` = '1' WHERE `Event_id` = @id;", db.getConnection());
            command.Parameters.AddWithValue("@id", currentEventId);
            db.openConnection();
            int rowsAffected = command.ExecuteNonQuery();
            db.closeConnection();

            if (rowsAffected > 0)
            {
                GuestsAccepted = true;
                MessageBox.Show("Список гостей подтвержден!");
                CheckStatuses();
                LoadEventData();
            }
            else
            {
                MessageBox.Show("Не удалось подтвердить список гостей. Возможно, мероприятие с таким ID не найдено.");
            }
        }
        private void materialButton2_Click(object sender, EventArgs e)
        {
            DBEvent db = new DBEvent();
            MySqlCommand command = new MySqlCommand("UPDATE `event` SET `Подтверждение сметы` = '1' WHERE `Event_id` = @id;", db.getConnection());
            command.Parameters.AddWithValue("@id", currentEventId);
            db.openConnection();
            int rowsAffected = command.ExecuteNonQuery();
            db.closeConnection();

            if (rowsAffected > 0)
            {
                EstimateAccepted = true;
                MessageBox.Show("Смета подтверждена!");
                CheckStatuses();
                LoadEventData();
            }
            else
            {
                MessageBox.Show("Не удалось подтвердить смету. Возможно, мероприятие с таким ID не найдено.");
            }
        }
        private void materialButton3_Click(object sender, EventArgs e)
        {
            string wish = textBox13.Text;
            DBEvent db = new DBEvent();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("UPDATE `event` SET `Основные пожелания` = @wish WHERE `Event_id` = @id;", db.getConnection());
            command.Parameters.AddWithValue("@wish", wish);
            command.Parameters.AddWithValue("@id", currentEventId);
            db.openConnection();
            int rowsAffected = command.ExecuteNonQuery();
            db.closeConnection();
            if (rowsAffected > 0)
            {
                MessageBox.Show("Пожелания сохранены!");
                CheckStatuses();
                LoadEventData();
            }
            else
            {
                MessageBox.Show("Не удалось сохранить пожелания. Возможно проблема в таблице мероприятия.");
            }
        }
        private void materialButton12_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ОТЧЕТ ПО МЕРОПРИЯТИЯМ С ФИНАНСОВОЙ АНАЛИТИКОЙ");
            reportForm.ShowDialog();
        }
        private void materialButton15_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ФИНАНСОВЫЙ ОТЧЕТ ПО МЕСЯЦАМ");
            reportForm.ShowDialog();
        }
        private void materialButton16_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ОТЧЕТ ПО ПОСТАВЩИКАМ И УСЛУГАМ");
            reportForm.ShowDialog();
        }
        private void materialButton13_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ОТЧЕТ ПО КЛИЕНТАМ И ИХ МЕРОПРИЯТИЯМ");
            reportForm.ShowDialog();
        }
        private void materialButton14_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ОТЧЕТ ПО ГОСТЯМ МЕРОПРИЯТИЙ");
            reportForm.ShowDialog();
        }
        private void materialButton17_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ДЕТАЛИЗИРОВАННАЯ СМЕТА МЕРОПРИЯТИЙ");
            reportForm.ShowDialog();
        }
        private void materialButton18_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ОТЧЕТ ПО ПОПУЛЯРНОСТИ УСЛУГ");
            reportForm.ShowDialog();
        }
        private void materialButton19_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("ОТЧЕТ ПО НЕЗАВЕРШЕННЫМ ОПЛАТАМ");
            reportForm.ShowDialog();
        }
        private void materialButton20_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("СТАТИСТИКА ПО МЕРОПРИЯТИЯМ И СЕЗОНАМ");
            reportForm.ShowDialog();
        }
        private void materialButton21_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm("СВОДНЫЙ ОТЧЕТ ПО ЭФФЕКТИВНОСТИ ПОСТАВЩИКОВ");
            reportForm.ShowDialog();
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

        private void materialButton22_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Сначала выберите мероприятие из списка!");
                return;
            }
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите сумму оплаты:",
                "Оплата мероприятия",
                "0",
                -1, -1);
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Оплата отменена.");
                return;
            }
            if (!decimal.TryParse(input, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму оплаты (положительное число)!");
                return;
            }
            string selectedEventId = comboBox1.SelectedValue.ToString();
            ProcessPayment(selectedEventId, amount);
        }
        private void ProcessPayment(string eventId, decimal amount)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"INSERT INTO `Pay` (`Мероприятие`, `Оплата`, `Дата оплаты`)
                        VALUES (@eventId, @amount, @paymentDate)";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@amount", amount);
                command.Parameters.AddWithValue("@paymentDate", DateTime.Now);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"Оплата в размере {amount:C} успешно внесена!");
                    LoadTotalPayments();
                    LoadPayments();
                }
                else
                {
                    MessageBox.Show("Не удалось внести оплату.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при внесении оплаты: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
    }
}
