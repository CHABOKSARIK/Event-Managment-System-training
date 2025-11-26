using EventManagmentSystem.Classes;
using Google.Protobuf.WellKnownTypes;
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
using System.Web;
using System.Windows.Forms;
using SD = System.Data;

namespace EventManagmentSystem
{
    public partial class Koordinator : MaterialForm
    {
        public int selectedClientId = -1;
        public int idEvent = -1;
        public bool Selected = false;
        public int idService = -1;
        public int idEstimate = -1;
        public int estimateEvent = 0;
        public int estimateSupplier = 0;

        public Koordinator()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            LoadData();
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView3.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView4.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView5.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView6.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
            dataGridView2.ReadOnly = true;
            dataGridView3.ReadOnly = true;
            dataGridView4.ReadOnly = true;
            dataGridView5.ReadOnly = true;
            dataGridView6.ReadOnly = true;
        }
        private void LoadPaymentData()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                p.`Pay_id`,
                p.`Оплата`,
                p.`Дата оплаты` as 'Дата оплаты',
                e.`Наименование` as 'Мероприятие',
                c.`Фамилия`,
                c.`Имя`
                FROM `pay` p
                LEFT JOIN `event` e ON p.`Мероприятие` = e.`Event_id`
                LEFT JOIN `client` c ON e.`Клиент` = c.`Client_id`
                ORDER BY p.`Дата оплаты` DESC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView6.DataSource = table;
                if (dataGridView6.Columns.Contains("Pay_id"))
                {
                    dataGridView6.Columns["Pay_id"].Visible = false;
                }
                UpdatePaymentStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных об оплатах: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void UpdatePaymentStatistics()
        {
            decimal totalPaid = 0;

            foreach (DataGridViewRow row in dataGridView6.Rows)
            {
                if (row.Cells["Оплата"].Value != null &&
                    decimal.TryParse(row.Cells["Оплата"].Value.ToString(), out decimal amount))
                {
                    totalPaid += amount;
                }
            }
            label24.Text = $"Всего оплачено: {totalPaid} руб.";
        }
        private void SearchPaymentByClient()
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите ФИО клиента для поиска оплат:", "Поиск оплат по клиенту", "", -1, -1);

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                string query = @"
                SELECT 
                p.`pay_id`,
                p.`Оплата`,
                p.`Дата оплаты` as 'Дата оплаты',
                e.`Наименование` as 'Мероприятие',
                c.`Фамилия`,
                c.`Имя`
                FROM `pay` p
                LEFT JOIN `event` e ON p.`Мероприятие` = e.`Event_id`
                LEFT JOIN `client` c ON e.`Клиент` = c.`Client_id`
                WHERE c.`Фамилия` LIKE @search OR c.`Имя` LIKE @search
                ORDER BY p.`Дата оплаты` DESC";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridView6.DataSource = table;
                decimal clientTotal = 0;
                foreach (DataRow row in table.Rows)
                {
                    if (decimal.TryParse(row["Оплата"].ToString(), out decimal amount))
                    {
                        clientTotal += amount;
                    }
                }
                MessageBox.Show($"От данного клиента всего оплачено: {clientTotal} руб.", "Результат поиска");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска оплат: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadNearestEvents()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                `Наименование`,
                `Дата мероприятия`,
                `Бюджет`,
                `Основные пожелания`
                FROM `event` 
                WHERE `Дата мероприятия` >= CURDATE()
                ORDER BY `Дата мероприятия` ASC 
                LIMIT 3";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                UpdateNearestEventsUI(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ближайших мероприятий: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void UpdateNearestEventsUI(DataTable eventsTable)
        {
            if (eventsTable.Rows.Count == 0)
            {
                textBox21.Text = "На данный момент не назначено мероприятий";
                return;
            }

            StringBuilder eventsText = new StringBuilder();
            foreach (DataRow eventRow in eventsTable.Rows)
            {
                string eventName = eventRow["Наименование"].ToString();
                DateTime eventDate = Convert.ToDateTime(eventRow["Дата мероприятия"]);
                string budget = eventRow["Бюджет"].ToString();
                string wishes = eventRow["Основные пожелания"]?.ToString() ?? "";

                eventsText.AppendLine($"{eventName}");
                eventsText.AppendLine($"{eventDate:dd.MM.yyyy HH:mm}");
                eventsText.AppendLine($"Бюджет: {budget} руб.");

                if (!string.IsNullOrEmpty(wishes))
                {
                    eventsText.AppendLine($"{wishes}");
                }
                eventsText.AppendLine("────────────────────");
            }

            textBox21.Text = eventsText.ToString();
        }
        private void CheckAndShowNotifications()
        {
            List<string> notifications = new List<string>();

            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
            SELECT `Наименование`, `Дата мероприятия`
            FROM `event` 
            WHERE DATEDIFF(`Дата мероприятия`, CURDATE()) BETWEEN 0 AND 7
            ORDER BY `Дата мероприятия` ASC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string eventName = reader["Наименование"].ToString();
                    DateTime eventDate = Convert.ToDateTime(reader["Дата мероприятия"]);
                    int daysLeft = (eventDate - DateTime.Today).Days;

                    if (daysLeft == 0)
                        notifications.Add($"СЕГОДНЯ: {eventName}");
                    else if (daysLeft == 1)
                        notifications.Add($"ЗАВТРА: {eventName}");
                    else
                        notifications.Add($"ЧЕРЕЗ {daysLeft} ДНЕЙ: {eventName}");
                }
                reader.Close();
                query = @"
                SELECT COUNT(*) 
                FROM `event` e 
                LEFT JOIN `estimate` est ON e.`Event_id` = est.`Мероприятие` 
                WHERE est.`Estimate_id` IS NULL 
                AND e.`Дата мероприятия` >= CURDATE()";
                command = new MySqlCommand(query, db.getConnection());
                int noEstimateCount = Convert.ToInt32(command.ExecuteScalar());

                if (noEstimateCount > 0)
                {
                    notifications.Add($"БЕЗ СМЕТЫ: {noEstimateCount} мероприятие(ий)");
                }
                query = @"
                SELECT COUNT(*) 
                FROM `event` e 
                LEFT JOIN `estimate` est ON e.`Event_id` = est.`Мероприятие` 
                WHERE est.`Поставщик` IS NULL 
                AND e.`Дата мероприятия` >= CURDATE()";
                command = new MySqlCommand(query, db.getConnection());
                int noSupplierCount = Convert.ToInt32(command.ExecuteScalar());

                if (noSupplierCount > 0)
                {
                    notifications.Add($"БЕЗ ПОСТАВЩИКОВ: {noSupplierCount} мероприятие(ий)");
                }
                query = "SELECT COUNT(*) FROM `event` WHERE `Дата мероприятия` < CURDATE()";
                command = new MySqlCommand(query, db.getConnection());
                int pastEvents = Convert.ToInt32(command.ExecuteScalar());

                query = "SELECT COUNT(*) FROM `event` WHERE `Дата мероприятия` >= CURDATE()";
                command = new MySqlCommand(query, db.getConnection());
                int futureEvents = Convert.ToInt32(command.ExecuteScalar());

                notifications.Add($"СТАТИСТИКА: {pastEvents} прошлых, {futureEvents} будущих мероприятий");
            }
            catch (Exception ex)
            {
                notifications.Add($"Ошибка загрузки уведомлений: {ex.Message}");

                notifications.Add($"Рекомендация: Проверьте структуру таблиц в базе данных");
            }
            finally
            {
                db.closeConnection();
            }
            UpdateNotificationsUI(notifications);
        }
        private void UpdateNotificationsUI(List<string> notifications)
        {
            if (notifications.Count == 0)
            {
                textBox20.Text = "На данный момент уведомлений пока нет";
                return;
            }

            StringBuilder notificationsText = new StringBuilder();
            notificationsText.AppendLine("=== ВАШИ УВЕДОМЛЕНИЯ ===");
            notificationsText.AppendLine();

            foreach (string notification in notifications)
            {
                notificationsText.AppendLine(notification);
                notificationsText.AppendLine();
            }

            textBox20.Text = notificationsText.ToString();
        }
        private void LoadEstimateDataWithDetails()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                e.`Estimate_id`,
                ev.`Наименование` as 'Мероприятие',
                s.`Наименование` as 'Поставщик',
                e.`Количество`,
                e.`Цена за единицу`,
                sv.`Наименование` as 'Услуга'
                FROM `estimate` e
                LEFT JOIN `event` ev ON e.`Мероприятие` = ev.`Event_id`
                LEFT JOIN `supplier_of_services` s ON e.`Поставщик` = s.`Supplier_id`
                LEFT JOIN `services` sv ON s.`Услуга` = sv.`Service_id`";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridView5.DataSource = table;
                if (dataGridView5.Columns.Contains("Estimate_id"))
                {
                    dataGridView5.Columns["Estimate_id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки смет: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadData()
        {
            getdata getdata = new getdata();
            getdata.setData("client", dataGridView1);
            getdata.setData("event", dataGridView2);
            getdata.setData("supplier_of_services", dataGridView3);
            getdata.setData("event", dataGridView4);
            getdata.setData("pay", dataGridView6);
            LoadEstimateDataWithDetails();
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
            dataGridView3.ClearSelection();
            dataGridView4.ClearSelection();
            dataGridView5.ClearSelection();
            dataGridView6.ClearSelection();
            LoadPaymentData();
            LoadNearestEvents();
            CheckAndShowNotifications();
            LoadEstimateDataWithDetails();
            CheckAndShowNotifications();
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                textBox1.Text = selectedRow.Cells["Фамилия"].Value?.ToString() ?? "";
                textBox2.Text = selectedRow.Cells["Имя"].Value?.ToString() ?? "";
                textBox3.Text = selectedRow.Cells["Отчество"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Дата рождения"].Value != null &&
                    selectedRow.Cells["Дата рождения"].Value != DBNull.Value)
                {
                    dateTimePicker1.Value = Convert.ToDateTime(selectedRow.Cells["Дата рождения"].Value);
                }
                else
                {
                    dateTimePicker1.Value = DateTime.Now;
                }
                textBox5.Text = selectedRow.Cells["Счет банка"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Client_id"].Value != null &&
                    selectedRow.Cells["Client_id"].Value != DBNull.Value)
                {
                    selectedClientId = Convert.ToInt32(selectedRow.Cells["Client_id"].Value);
                }
            }
        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            ClearClientFields();
            textBox1.Focus();
        }
        public void ClearClientFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            textBox5.Text = "";
            selectedClientId = -1;
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            string lastName = textBox1.Text.Trim();
            string firstName = textBox2.Text.Trim();
            string middleName = textBox3.Text.Trim();
            string bankAccount = textBox5.Text.Trim();
            DateTime birthDate = dateTimePicker1.Value;

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
            {
                MessageBox.Show("Заполните обязательные поля: Фамилия и Имя!");
                return;
            }

            DBEvent db = new DBEvent();

            try
            {
                db.openConnection();

                if (selectedClientId == -1)
                {
                    string insertQuery = @"INSERT INTO `client` (`Фамилия`, `Имя`, `Отчество`, `Дата рождения`, `Счет банка`) 
                                VALUES (@ln, @fn, @mn, @bd, @ba)";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@ln", lastName);
                    command.Parameters.AddWithValue("@fn", firstName);
                    command.Parameters.AddWithValue("@mn", middleName);
                    command.Parameters.AddWithValue("@bd", birthDate);
                    command.Parameters.AddWithValue("@ba", bankAccount);

                    command.ExecuteNonQuery();
                    MessageBox.Show("Клиент успешно создан!");
                }
                else
                {
                    string updateQuery = @"UPDATE `client`
                                SET `Фамилия` = @ln, `Имя` = @fn, `Отчество` = @mn,
                                    `Дата рождения` = @bd, `Счет банка` = @ba
                                WHERE `Client_id` = @ClientID";
                    MySqlCommand command = new MySqlCommand(updateQuery, db.getConnection());
                    command.Parameters.AddWithValue("@ln", lastName);
                    command.Parameters.AddWithValue("@fn", firstName);
                    command.Parameters.AddWithValue("@mn", middleName);
                    command.Parameters.AddWithValue("@bd", birthDate);
                    command.Parameters.AddWithValue("@ba", bankAccount);
                    command.Parameters.AddWithValue("@ClientID", selectedClientId);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения клиента сохранены!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные клиента.");
                    }
                }
                SaveEvent();
                LoadData();
                ClearClientFields();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении клиента: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton5_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
            "Введите данные для поиска клиента:\n(Фамилия, Имя, Отчество или Счет банка)", "Поиск клиента", "", -1, -1);
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            FindAndSelectClient(searchText);
        }
        private void FindAndSelectClient(string searchText)
        {
            DBEvent db = new DBEvent();

            try
            {
                db.openConnection();
                string query = @"SELECT * FROM `client` 
                        WHERE `Фамилия` LIKE @search 
                           OR `Имя` LIKE @search 
                           OR `Отчество` LIKE @search 
                           OR `Счет банка` LIKE @search
                        LIMIT 1";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Клиент по запросу '{searchText}' не найден.", "Результат поиска");
                    return;
                }
                DataRow foundClient = table.Rows[0];
                SelectClientInDataGridView(foundClient);

                MessageBox.Show($"Клиент найден: {foundClient["Фамилия"]} {foundClient["Имя"]}", "Результат поиска");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска клиента: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void SelectClientInDataGridView(DataRow clientRow)
        {
            int clientId = Convert.ToInt32(clientRow["Client_id"]);
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Client_id"].Value != null &&
                    Convert.ToInt32(row.Cells["Client_id"].Value) == clientId)
                {
                    dataGridView1.ClearSelection();
                    row.Selected = true;
                    dataGridView1.CurrentCell = row.Cells[1];
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void materialButton6_Click(object sender, EventArgs e)
        {
            if (idEvent >= 1)
            {
                string myvalue = $"Мероприятие: {dataGridView2.CurrentRow.Cells[0].Value} {dataGridView2.CurrentRow.Cells[2].Value}";
                GuestsCoordinator gc = new GuestsCoordinator();
                gc.eventid = idEvent;
                gc.eventname = myvalue;
                gc.ShowDialog();
            }
            else
            {
                MessageBox.Show("Сначала выберите или создайте мероприятие и связь меджду гостями");
            }

        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                textBox4.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
                textBox6.Text = selectedRow.Cells["Бюджет"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Дата мероприятия"].Value != null &&
                    selectedRow.Cells["Дата мероприятия"].Value != DBNull.Value)
                {
                    dateTimePicker2.Value = Convert.ToDateTime(selectedRow.Cells["Дата мероприятия"].Value);
                }
                else
                {
                    dateTimePicker2.Value = DateTime.Now;
                }
                textBox8.Text = selectedRow.Cells["Основные пожелания"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Event_id"].Value != null &&
                    selectedRow.Cells["Event_id"].Value != DBNull.Value)
                {
                    idEvent = Convert.ToInt32(selectedRow.Cells["Event_id"].Value);
                }
            }
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
            {
                MessageBox.Show("Перед созданием мероприятия выберите или создайте клиента если у вас есть клиент который заказал мероприятие. в противном случае мероприятие создастся без клиента. вы уверены что хотите продолжить?", "Внимание", MessageBoxButtons.YesNo);
                dataGridView2.ClearSelection();
                ClearEventFields();
                textBox4.Focus();
            }
            else
            {
                dataGridView2.ClearSelection();
                ClearEventFields();
                textBox4.Focus();
                Selected = true;
            }
        }
        public void ClearEventFields()
        {
            textBox4.Text = "";
            textBox6.Text = "";
            dateTimePicker2.Value = DateTime.Now;
            textBox8.Text = "";
            idEvent = -1;
        }
        private void SaveEvent()
        {
            string name = textBox4.Text.Trim();
            string budget = textBox6.Text.Trim();
            DateTime Event_date = dateTimePicker2.Value;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(budget))
            {
                MessageBox.Show("Заполните обязательные поля: имя и бюджет мероприятия!");
                return;
            }
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                if (idEvent == -1)
                {
                    if (Selected)
                    {
                        string insertQuery = @"INSERT INTO `event` (`Наименование`, `Бюджет`, `Дата мероприятия`, `Клиент`) 
                                VALUES (@name, @budget, @de, @cl)";
                        MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@budget", budget);
                        command.Parameters.AddWithValue("@de", Event_date);
                        command.Parameters.AddWithValue("@cl", selectedClientId);
                        command.ExecuteNonQuery();
                        MessageBox.Show("Мероприятие успешно добавлено!");
                    }
                    else
                    {
                        string insertQuery = @"INSERT INTO `event` (`Наименование`, `Бюджет`, `Дата мероприятия`) 
                                VALUES (@name, @budget, @de)";
                        MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@budget", budget);
                        command.Parameters.AddWithValue("@de", Event_date);
                        command.ExecuteNonQuery();
                        MessageBox.Show("Мероприятие успешно добавлено!");
                    }
                }
                else
                {
                    MySqlCommand command = new MySqlCommand();
                    if (Selected)
                    {
                        string updateQuery = @"UPDATE `event`
                                SET `Наименование` = @name, `Бюджет` = @budget, `Дата мероприятия` = @de, `Клиент` = @cl
                                WHERE `id_event` = @EventID";
                        command = new MySqlCommand(updateQuery, db.getConnection());
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@budget", budget);
                        command.Parameters.AddWithValue("@de", Event_date);
                        command.Parameters.AddWithValue("@cl", selectedClientId);
                        command.Parameters.AddWithValue("@EventID", idEvent);
                    }
                    else
                    {
                        string updateQuery = @"UPDATE `event`
                                SET `Наименование` = @name, `Бюджет` = @budget, `Дата мероприятия` = @de,
                                WHERE `id_event` = @EventID";
                        command = new MySqlCommand(updateQuery, db.getConnection());
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@budget", budget);
                        command.Parameters.AddWithValue("@de", Event_date);
                        command.Parameters.AddWithValue("@EventID", idEvent);

                    }
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения мероприятия сохранены!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные мероприятия.");
                    }
                }
                ClearEventFields();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении мероприятия: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
            "Введите данные для поиска мероприятия:\n(Наименование, бюджет, основные пожелания или клиент)", "Поиск мероприятия", "", -1, -1);
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            FindAndSelectEvent(searchText);
        }
        private void FindAndSelectEvent(string searchText)
        {
            DBEvent db = new DBEvent();

            try
            {
                db.openConnection();
                string query = @"SELECT * FROM `event` 
                        WHERE `Наименование` LIKE @search 
                           OR `Бюджет` LIKE @search 
                           OR `Основные пожелания` LIKE @search 
                           OR `Клиент` LIKE @search
                        LIMIT 1";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Мероприятие по запросу '{searchText}' не найдено.", "Результат поиска");
                    return;
                }
                DataRow foundEvent = table.Rows[0];
                SelectEventInDataGridView(foundEvent);

                MessageBox.Show($"Мероприятие найдено: {foundEvent["Наименование"]} {foundEvent["Дата Мероприятия"]}", "Результат поиска");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска мероприятия: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void SelectEventInDataGridView(DataRow eventRow)
        {
            int eventId = Convert.ToInt32(eventRow["Event_id"]);
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["Event_id"].Value != null &&
                    Convert.ToInt32(row.Cells["Event_id"].Value) == eventId)
                {
                    dataGridView2.ClearSelection();
                    row.Selected = true;
                    dataGridView2.CurrentCell = row.Cells[1];
                    dataGridView2.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }
        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView3.SelectedRows[0];
                textBox7.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
                textBox9.Text = selectedRow.Cells["Контактные данные"].Value?.ToString() ?? "";
                textBox10.Text = selectedRow.Cells["Реквизиты"].Value?.ToString() ?? "";
                idService = Convert.ToInt32(selectedRow.Cells["Услуга"].Value);
                if (selectedRow.Cells["Supplier_id"].Value != null &&
                    selectedRow.Cells["Supplier_id"].Value != DBNull.Value)
                {
                    idService = Convert.ToInt32(selectedRow.Cells["Supplier_id"].Value);
                }
                getdata gt = new getdata();
                DataTable dt = new DataTable();
                gt.getEventToTable("services", dt);
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Service_id"].ToString() == idService.ToString())
                    {
                        textBox11.Text = row["Наименование"].ToString();
                        textBox12.Text = row["Единица измерения"].ToString();
                        break;
                    }
                }
                EstimateSupplierName = textBox7.Text + " " + textBox9.Text;
            }
        }
        public string EstimateEventName;
        public string EstimateSupplierName;
        private void dataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView4.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView4.SelectedRows[0];
                textBox15.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
                textBox14.Text = selectedRow.Cells["Бюджет"].Value?.ToString() ?? "";
                textBox16.Text = selectedRow.Cells["Дата мероприятия"].Value?.ToString() ?? "";
                textBox13.Text = selectedRow.Cells["Основные пожелания"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Event_id"].Value != null &&
                    selectedRow.Cells["Event_id"].Value != DBNull.Value)
                {
                    estimateEvent = Convert.ToInt32(selectedRow.Cells["Event_id"].Value);
                }
                EstimateEventName = textBox15.Text + " " + textBox16.Text;
            }
        }

        private void materialButton8_Click(object sender, EventArgs e)
        {
            estimateSupplier = idService;
            dataGridView3.ClearSelection();
            label27.Text = EstimateSupplierName;
            textBox7.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";
            textBox11.Text = "";
            textBox12.Text = "";
        }

        private void materialButton9_Click(object sender, EventArgs e)
        {
            dataGridView4.ClearSelection();
            label29.Text = EstimateEventName;
            textBox15.Text = "";
            textBox14.Text = "";
            textBox16.Text = "";
            textBox13.Text = "";
        }

        private void materialButton7_Click(object sender, EventArgs e)
        {
            int quantity = Convert.ToInt32(textBox18.Text);
            int price = 0;
            if (estimateSupplier > 0 && estimateEvent > 0)
            {
                if (quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество!");
                    return;
                }
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();

                    if (idEstimate == -1) 
                    {
                        string insertQuery = @"INSERT INTO `estimate` (`Мероприятие`, `Поставщик`, `Количество`, `Цена за единицу`) 
                            VALUES (@event, @supplier, @quantity, @price)";
                        MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                        command.Parameters.AddWithValue("@event", estimateEvent);
                        command.Parameters.AddWithValue("@supplier", estimateSupplier);
                        command.Parameters.AddWithValue("@quantity", quantity);
                        command.Parameters.AddWithValue("@price", price);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Смета успешно создана!");
                    }
                    else 
                    {
                        string updateQuery = @"UPDATE `estimate`
                            SET `Мероприятие` = @event, `Поставщик` = @supplier, 
                                `Количество` = @quantity, `Цена за единицу` = @price
                            WHERE `Estimate_id` = @estimateId";
                        MySqlCommand command = new MySqlCommand(updateQuery, db.getConnection());
                        command.Parameters.AddWithValue("@event", estimateEvent);
                        command.Parameters.AddWithValue("@supplier", estimateSupplier);
                        command.Parameters.AddWithValue("@quantity", quantity);
                        command.Parameters.AddWithValue("@estimateId", idEstimate);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Изменения сметы сохранены!");
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить данные сметы.");
                        }
                    }
                    ClearEstimateFields();
                    LoadData();
                    CalculateTotal();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении сметы: {ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите поставщика и мероприятие!");
            }
        }
        public void ClearEstimateFields()
        {
            textBox17.Text = "";
            textBox18.Text = "";
            idEstimate = -1;
        }
        private void dataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView5.SelectedRows.Count > 0)
            {
                if (dataGridView5.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = dataGridView5.SelectedRows[0];
                    textBox18.Text = selectedRow.Cells["Количество"].Value?.ToString() ?? "";
                    textBox17.Text = selectedRow.Cells["Цена за единицу"].Value?.ToString() ?? "";
                    if (selectedRow.Cells["Estimate_id"].Value != null &&
                        selectedRow.Cells["Estimate_id"].Value != DBNull.Value)
                    {
                        idEstimate = Convert.ToInt32(selectedRow.Cells["Estimate_id"].Value);
                    }
                    string eventName = selectedRow.Cells["Мероприятие"].Value?.ToString() ?? "";
                    string supplierName = selectedRow.Cells["Поставщик"].Value?.ToString() ?? "";
                    estimateEvent = GetEventIdByName(eventName);
                    estimateSupplier = GetSupplierIdByName(supplierName);

                    UpdateEventName(estimateEvent);
                    UpdateSupplierName(estimateSupplier);

                    CalculateTotal();
                }
                else
                {
                    ClearEstimateFields();
                    label27.Text = "Поставщик: не выбран";
                    label29.Text = "Мероприятие: не выбрано";
                }

                if (idEstimate > 0)
                {
                    label30.Text = "Смета создана";
                }
                else
                {
                    label30.Text = "Смета не создана";
                }
            }
        }
        private int GetEventIdByName(string eventName)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = "SELECT `Event_id` FROM `event` WHERE `Наименование` = @name";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@name", eventName);

                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения ID мероприятия: {ex.Message}");
                return 0;
            }
            finally
            {
                db.closeConnection();
            }
        }

        private int GetSupplierIdByName(string supplierName)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = "SELECT `Supplier_id` FROM `supplier_of_services` WHERE `Наименование` = @name";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@name", supplierName);

                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения ID поставщика: {ex.Message}");
                return 0;
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void UpdateEventName(int eventId)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = "SELECT `Наименование`, `Дата мероприятия` FROM `event` WHERE `Event_id` = @eventId";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@eventId", eventId);

                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    EstimateEventName = $"{reader["Наименование"]} {reader["Дата мероприятия"]}";
                    label29.Text = EstimateEventName;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных мероприятия: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void UpdateSupplierName(int supplierId)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = "SELECT `Наименование`, `Контактные данные` FROM `supplier_of_services` WHERE `Supplier_id` = @supplierId";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@supplierId", supplierId);

                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    EstimateSupplierName = $"{reader["Наименование"]} {reader["Контактные данные"]}";
                    label27.Text = EstimateSupplierName;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных поставщика: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void CalculateTotal()
        {
            if (decimal.TryParse(textBox17.Text, out decimal quantity) &&
                decimal.TryParse(textBox18.Text, out decimal price))
            {
                decimal total = quantity * price;
                label31.Text = $"Всего смета составляет: {total} руб.";
            }
        }

        private void textBox17_TextChanged(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        private void textBox18_TextChanged(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        private void materialButton11_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
        "Введите данные для поиска сметы:\n(Название мероприятия, поставщика, услуги или дата)",
        "Поиск сметы", "", -1, -1);

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            FindAndSelectEstimate(searchText);
        }

        private void FindAndSelectEstimate(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            foreach (DataGridViewRow row in dataGridView5.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null &&
                        cell.Value.ToString().ToLower().Contains(searchText.ToLower()))
                    {
                        dataGridView5.ClearSelection();
                        row.Selected = true;
                        dataGridView5.CurrentCell = cell;
                        dataGridView5.FirstDisplayedScrollingRowIndex = row.Index;

                        MessageBox.Show("Смета найдена!", "Результат поиска");
                        return;
                    }
                }
            }
            MessageBox.Show($"Смета по запросу '{searchText}' не найдена.", "Результат поиска");
        }

        private void materialTabSelector1_Click(object sender, EventArgs e)
        {

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
            SearchPaymentByClient();
        }

        private void materialButton10_Click(object sender, EventArgs e)
        {
            dataGridView5.ClearSelection();
            ClearEstimateFields();
            textBox18.Focus();
            label31.Text = "Всего смета составляет: ";
            label27.Text = "не назначено";
            label29.Text = "не назначено";
            label30.Text = "Смета не создана";
        }
    }
}