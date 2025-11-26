using EventManagmentSystem.Classes;
using MaterialSkin;
using MaterialSkin.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EventManagmentSystem.Admin
{
    public partial class Admin : MaterialForm
    {
        private int selectedUserId = -1;
        private string currentUserRole = "";
        private int idEvent = -1;
        private int selectedClientId = -1;
        private int idGuest = -1;
        private int idService = -1;
        private int idSupplier = -1;
        private int idEstimate = -1;
        private int idPayment = -1;
        public Admin()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            LoadData();
            LoadRoles();
            LoadUsers();
            LoadEvent();
            LoadGuests();
            LoadSupplier();
            LoadEstimate();
            LoadPay();
            dataGridView1.ReadOnly = true;
            dataGridView2.ReadOnly = true;
            dataGridView3.ReadOnly = true;
            dataGridView4.ReadOnly = true;
            dataGridView5.ReadOnly = true;
            dataGridView6.ReadOnly = true;
            dataGridView7.ReadOnly = true;
            dataGridView8.ReadOnly = true;
            LoadComboBox(comboBox2, "client", "Client_id", "Фамилия", "Фамилия NOT NULL");
            LoadComboBox(comboBox3, "event", "Event_id", "CONCAT(Наименование, ' - ', `Дата мероприятия`)", "");
            LoadComboBox(comboBox4, "services", "Service_id", "Наименование", "Наименование NOT NULL");
            LoadComboBox(comboBox5, "event", "Event_id", "CONCAT(Наименование, ' - ', `Дата мероприятия`)", "");
            LoadComboBox(comboBox6, "supplier_of_services", "Supplier_id", "Наименование", "Наименование NOT NULL");
            LoadComboBox(comboBox7, "event", "Event_id", "CONCAT(Наименование, ' - ', `Дата мероприятия`)", "");
        }
        private void LoadPay()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                p.pay_id,
                CONCAT(e.Наименование, ' - ', e.`Дата мероприятия`) as Мероприятие, 
                p.Оплата,
                p.`Дата оплаты`
                FROM pay p
                LEFT JOIN event e ON p.Мероприятие = e.Event_id;";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView8.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadEstimate()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                e.Estimate_id,
                CONCAT(ev.Наименование, ' - ', ev.`Дата мероприятия`) as Мероприятие,  
                s.Наименование as Поставщик,     
                e.Количество,
                e.`Цена за единицу`,
                (e.Количество * e.`Цена за единицу`) as Общая_стоимость  
                FROM estimate e
                LEFT JOIN event ev ON e.Мероприятие = ev.Event_id
                LEFT JOIN supplier_of_services s ON e.Поставщик = s.Supplier_id;";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView7.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadSupplier()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                s.Supplier_id,
                s.Наименование,
                s.`Контактные данные`,
                s.Реквизиты,
                st.Наименование as Услуга, 
                s.Готовность
                FROM supplier_of_services s
                LEFT JOIN services st ON s.Услуга = Service_id";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView6.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadGuests()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                g.Guest_id,
                g.Фамилия,
                g.Имя,
                g.Отчество,
                g.`Контактные данные`,
                CONCAT(e.Наименование, ' - ', e.`Дата мероприятия`) as Мероприятие
                FROM guest g
                LEFT JOIN event e ON g.Мероприятие = e.Event_id;";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView3.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadEvent()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                e.Наименование,
                e.Бюджет,
                e.`Дата мероприятия`,
                e.`Основные пожелания`,
                c.Фамилия as Клиент, 
                e.Event_id,
                e.`Подтверждение гостей`,
                e.`Подтверждение сметы`
                FROM event e
                LEFT JOIN client c ON e.Клиент = c.Client_id;";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView2.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void LoadData()
        {
            getdata gd = new getdata();
            gd.setData("client", dataGridView1);
            gd.setData("services", dataGridView4);
        }
        private void LoadUsers()
        {
            DBUser db = new DBUser();
            try
            {
                db.openConnection();
                string query = @"
                    SELECT 
                    u.id_user,
                    u.Фамилия,
                    u.Имя,
                    u.Логин,
                    u.Пароль,
                    r.name as 'Роль',
                    u.Секрет,
                    u.`От кого` as 'От кого (только для поставщиков)'
                    FROM users u
                    LEFT JOIN roles r ON u.Роль = r.id_roles
                    ORDER BY u.Фамилия, u.Имя";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView5.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void LoadRoles()
        {
            DBUser db = new DBUser();
            try
            {
                db.openConnection();
                string query = "SELECT id_roles, name FROM roles ORDER BY name";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                comboBox1.DataSource = null;
                comboBox1.Items.Clear();
                comboBox1.DataSource = table;
                comboBox1.DisplayMember = "name";
                comboBox1.ValueMember = "id_roles";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            string lastName = textBox18.Text.Trim();
            string firstName = textBox17.Text.Trim();
            string username = textBox7.Text.Trim();
            string password = textBox8.Text.Trim();
            string supplierInfo = textBox24.Text.Trim();
            string secret = textBox25.Text.Trim();

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните обязательные поля: Фамилия, Имя, Логин и Пароль!", "Внимание");
                return;
            }

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль пользователя!", "Внимание");
                return;
            }

            int roleId = Convert.ToInt32(comboBox1.SelectedValue);

            DBUser db = new DBUser();
            try
            {
                db.openConnection();
                string checkQuery = "SELECT * FROM users WHERE Логин = @username";
                MySqlCommand checkCommand = new MySqlCommand(checkQuery, db.getConnection());
                checkCommand.Parameters.AddWithValue("@username", username);
                int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (userCount > 0)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка");
                    return;
                }

                string insertQuery = @"INSERT INTO users (Фамилия, Имя, Логин, Пароль, Роль, Секрет, `От кого`) 
                                VALUES (@lastName, @firstName, @username, @password, @roleId, @secret, @supplierInfo)";
                MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@roleId", roleId);
                command.Parameters.AddWithValue("@secret", secret);
                command.Parameters.AddWithValue("@supplierInfo", supplierInfo);

                command.ExecuteNonQuery();

                MessageBox.Show("Пользователь успешно создан!", "Успех");
                ClearUserFields();
                LoadUsers();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании пользователя: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void dataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView5.SelectedRows.Count > 0 && dataGridView5.SelectedRows[0] != null)
            {
                DataGridViewRow selectedRow = dataGridView5.SelectedRows[0];

                try
                {
                    textBox18.Text = selectedRow.Cells["Фамилия"].Value?.ToString() ?? "";
                    textBox17.Text = selectedRow.Cells["Имя"].Value?.ToString() ?? "";
                    textBox7.Text = selectedRow.Cells["Логин"].Value?.ToString() ?? "";
                    textBox8.Text = selectedRow.Cells["Пароль"].Value?.ToString() ?? "";
                    textBox25.Text = selectedRow.Cells["Секрет"].Value?.ToString() ?? "";
                    textBox24.Text = selectedRow.Cells["От кого (только для поставщиков)"].Value?.ToString() ?? "";
                    string roleName = selectedRow.Cells["Роль"].Value?.ToString() ?? "";
                    SetSelectedRole(roleName);
                    if (selectedRow.Cells["id_user"].Value != null &&
                        selectedRow.Cells["id_user"].Value != DBNull.Value)
                    {
                        selectedUserId = Convert.ToInt32(selectedRow.Cells["id_user"].Value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}");
                }
            }
        }
        private void SetSelectedRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName) || comboBox1.Items.Count == 0)
                return;

            try
            {
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    DataRowView item = comboBox1.Items[i] as DataRowView;
                    if (item != null && item["name"].ToString() == roleName)
                    {
                        comboBox1.SelectedIndex = i;
                        currentUserRole = roleName;
                        return;
                    }
                }
                comboBox1.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе роли: {ex.Message}");
                comboBox1.SelectedIndex = -1;
            }
        }
        private void ClearUserFields()
        {
            textBox18.Text = "";
            textBox17.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            textBox24.Text = "";
            selectedUserId = -1;
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            if (selectedUserId == -1)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Внимание");
                return;
            }
            if (IsCurrentUser(selectedUserId))
            {
                MessageBox.Show("Вы не можете удалить своего собственного аккаунта!", "Ошибка");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DBUser db = new DBUser();
                try
                {
                    db.openConnection();
                    string query = "DELETE FROM users WHERE user_id = @userId";
                    MySqlCommand command = new MySqlCommand(query, db.getConnection());
                    command.Parameters.AddWithValue("@userId", selectedUserId);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Пользователь успешно удален!", "Успех");
                        ClearUserFields();
                        LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
            }
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            if (selectedUserId == -1)
            {
                MessageBox.Show("Выберите пользователя для изменения роли!", "Внимание");
                return;
            }

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите новую роль!", "Внимание");
                return;
            }

            int newRoleId = Convert.ToInt32(comboBox1.SelectedValue);
            string supplierInfo = textBox24.Text.Trim();
            string secret = textBox25.Text.Trim();

            DBUser db = new DBUser();
            try
            {
                db.openConnection();
                string query = "UPDATE users SET Роль = @roleId, `От кого` = @supplierInfo, `Секрет` = @secret WHERE id_user = @userId";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@roleId", newRoleId);
                command.Parameters.AddWithValue("@supplierInfo", supplierInfo);
                command.Parameters.AddWithValue("@secret", secret);
                command.Parameters.AddWithValue("@userId", selectedUserId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Роль пользователя успешно изменена!", "Успех");
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении роли: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
            "Введите данные для поиска пользователя:\n(Фамилия, Имя, Логин или Роль)", "Поиск пользователя", "", -1, -1);

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            DBUser db = new DBUser();
            try
            {
                db.openConnection();
                string query = @"
                    SELECT 
                    u.id_user,
                    u.Фамилия,
                    u.Имя,
                    u.Логин,
                    u.Пароль,
                    r.name,
                    u.Секрет,
                    u.`От кого` as 'От кого (только для поставщиков)'
                    FROM users u
                    LEFT JOIN roles r ON u.Роль = r.id_roles
                    WHERE u.Фамилия LIKE @search 
                    OR u.Имя LIKE @search 
                    OR u.Логин LIKE @search
                    OR r.name LIKE @search
                    OR u.Секрет LIKE @search
                    ORDER BY u.Фамилия, u.Имя";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView5.DataSource = table;

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Пользователи по запросу '{searchText}' не найдены.", "Результат поиска");
                }
                else
                {
                    MessageBox.Show($"Найдено {table.Rows.Count} пользователь(ей)", "Результат поиска");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска пользователя: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton34_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из приложения?", "Подтверждение выхода",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private bool IsCurrentUser(int userId)
        {
            Form1 form = new Form1();
            int user = form.idUser;
            if (userId == user)
                return false;
            else
                return true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void materialButton33_Click(object sender, EventArgs e)
        {
            this.Hide();
            SelectRole srf = new SelectRole();
            srf.ShowDialog();
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                textBox9.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
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
                textBox4.Text = selectedRow.Cells["Основные пожелания"].Value?.ToString() ?? "";
                comboBox2.Text = selectedRow.Cells["Клиент"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Event_id"].Value != null &&
                    selectedRow.Cells["Event_id"].Value != DBNull.Value)
                {
                    idEvent = Convert.ToInt32(selectedRow.Cells["Event_id"].Value);
                }
            }
        }
        private void LoadComboBox(System.Windows.Forms.ComboBox comboBox, string tableName, string valueMember, string displayMember, string whereClause = "")
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = $"SELECT {valueMember}, {displayMember} FROM {tableName}";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                comboBox.DataSource = null;
                comboBox.Items.Clear();
                comboBox.DataSource = table;
                comboBox.DisplayMember = displayMember;
                comboBox.ValueMember = valueMember;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void materialButton6_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            ClearClientFields();
            textBox1.Focus();
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

        private void materialButton8_Click(object sender, EventArgs e)
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
        public void ClearClientFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            textBox5.Text = "";
            selectedClientId = -1;
        }

        private void materialButton7_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Удалить выбранного клиента?", "Подтверждение",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string insertQuery = @"DELETE FROM `client` WHERE `Client_id` = @id;";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@id", selectedClientId);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Клиент успешно удален!");
                        LoadData();
                        ClearClientFields();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить клиента.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении клиента:{ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
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

        private void materialButton12_Click(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
            ClearEventFields();
            textBox9.Focus();
        }
        private void ClearEventFields()
        {
            textBox9.Text = "";
            textBox6.Text = "";
            dateTimePicker2.Value = DateTime.Now;
            textBox4.Text = "";
            comboBox2.Text = "";
            idEvent = -1;
        }

        private void materialButton9_Click(object sender, EventArgs e)
        {
            string name = textBox9.Text.Trim();
            string budget = textBox6.Text.Trim();
            DateTime Event_date = dateTimePicker2.Value;
            string Wishes = textBox4.Text.Trim();
            int idClient = 0;
            if (comboBox2.SelectedItem != null)
            {
                var selectedItem = comboBox2.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    idClient = Convert.ToInt32(selectedItem["Client_id"]);
                }
            }
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
                    string insertQuery = @"INSERT INTO `event` (`Наименование`, `Бюджет`, `Дата мероприятия`, `Клиент`) 
                                VALUES (@name, @budget, @de, @cl)";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@budget", budget);
                    command.Parameters.AddWithValue("@de", Event_date);
                    command.Parameters.AddWithValue("@cl", idClient);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Мероприятие успешно добавлено!");
                        ClearEventFields();
                        LoadEvent();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить мероприятия.");
                    }
                }
                else
                {
                    MySqlCommand command = new MySqlCommand();
                    string updateQuery = @"UPDATE `event`
                                SET `Наименование` = @name, `Бюджет` = @budget, `Дата мероприятия` = @de, `Основные пожелания` = @mw `Клиент` = @cl
                                WHERE `id_event` = @EventID";
                    command = new MySqlCommand(updateQuery, db.getConnection());
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@budget", budget);
                    command.Parameters.AddWithValue("@de", Event_date);
                    command.Parameters.AddWithValue("@mw", Wishes);
                    command.Parameters.AddWithValue("@cl", comboBox2.ValueMember);
                    command.Parameters.AddWithValue("@EventID", idEvent);
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
                LoadEvent();
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

        private void materialButton10_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Удалить выбранное мероприятие?", "Подтверждение",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string insertQuery = @"DELETE FROM `event` WHERE `Event_id` = @id;";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@id", idEvent);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Мероприятие успешно удалено!");
                        LoadData();
                        ClearClientFields();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить мероприятие.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении мероприятия:{ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
            }
        }

        private void materialButton11_Click(object sender, EventArgs e)
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

        private void materialButton14_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Удалить выбранного гостя?", "Подтверждение",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string insertQuery = @"DELETE FROM `guest` WHERE `Guest_id` = @id;";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@id", idGuest);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Гость успешно удален!");
                        LoadGuests();
                        ClearGuestFields();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить гостя.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении гостя:{ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
                LoadGuests();
                ClearGuestFields();
            }

        }

        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView3.SelectedRows[0];
                textBox13.Text = selectedRow.Cells["Фамилия"].Value?.ToString() ?? "";
                textBox12.Text = selectedRow.Cells["Имя"].Value?.ToString() ?? "";
                textBox11.Text = selectedRow.Cells["Отчество"].Value?.ToString() ?? "";
                textBox10.Text = selectedRow.Cells["Контактные данные"].Value?.ToString() ?? "";
                comboBox3.Text = selectedRow.Cells["Мероприятие"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Guest_id"].Value != null &&
                    selectedRow.Cells["Guest_id"].Value != DBNull.Value)
                {
                    idGuest = Convert.ToInt32(selectedRow.Cells["Guest_id"].Value);
                }
            }
        }

        private void materialButton16_Click(object sender, EventArgs e)
        {
            dataGridView3.ClearSelection();
            ClearGuestFields();
            textBox13.Focus();
        }
        private void ClearGuestFields()
        {
            textBox13.Text = "";
            textBox12.Text = "";
            textBox11.Text = "";
            textBox10.Text = "";
            comboBox3.Text = "";
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            idGuest = -1;
        }

        private void materialButton13_Click(object sender, EventArgs e)
        {
            string ln = textBox13.Text.Trim();
            string fn = textBox12.Text.Trim();
            string mn = textBox11.Text.Trim();
            string cd = textBox10.Text.Trim();
            int EventId = 0;
            if (comboBox3.SelectedItem != null)
            {
                var selectedItem = comboBox3.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    EventId = Convert.ToInt32(selectedItem["Event_id"]);
                }
            }
            if (string.IsNullOrEmpty(ln) || string.IsNullOrEmpty(cd))
            {
                MessageBox.Show("Заполните обязательные поля: Фамилия и контактные данные!");
                return;
            }
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                if (idGuest == -1)
                {
                    string insertQuery = @"INSERT INTO `guest` (`Фамилия`, `Имя`, `Отчество`, `Контактные данные`, `Мероприятие`) 
                                VALUES (@ln, @fn, @mn, @cd, @ev)";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@ln", ln);
                    command.Parameters.AddWithValue("@fn", fn);
                    command.Parameters.AddWithValue("@mn", mn);
                    command.Parameters.AddWithValue("@cd", cd);
                    command.Parameters.AddWithValue("@ev", EventId);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Гость успешно добавлен!");
                        ClearGuestFields();
                        LoadGuests();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить гостя.");
                    }
                }
                else
                {
                    MySqlCommand command = new MySqlCommand();
                    string updateQuery = @"UPDATE `guest`
                                SET `Фамилия` = @ln, `Имя` = @fn, `Отчество` = @mn, `Контактные данные` = @cd `Мероприятие` = @ev
                                WHERE `Guest_id` = @GuestID";
                    command = new MySqlCommand(updateQuery, db.getConnection());
                    command.Parameters.AddWithValue("@ln", ln);
                    command.Parameters.AddWithValue("@fn", fn);
                    command.Parameters.AddWithValue("@mn", mn);
                    command.Parameters.AddWithValue("@cd", cd);
                    command.Parameters.AddWithValue("@ev", EventId);
                    command.Parameters.AddWithValue("@GuestID", idGuest);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения гостя сохранены!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные гостя.");
                    }
                }
                ClearGuestFields();
                LoadGuests();
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

        private void materialButton15_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
            "Введите данные для поиска гостя:\n(Фамилия, имя, отчество контактные данные или мероприятие)", "Поиск гостя", "", -1, -1);
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            FindAndSelectGuest(searchText);
        }
        private void FindAndSelectGuest(string searchText)
        {
            DBEvent db = new DBEvent();

            try
            {
                db.openConnection();
                string query = @"SELECT * FROM `guest` 
                        WHERE `Фамилия` LIKE @search 
                           OR `Имя` LIKE @search 
                           OR `Отчество` LIKE @search 
                           OR `Контактные данные` LIKE @search
                           OR `Клиент` LIKE @search
                        LIMIT 1";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Гость по запросу '{searchText}' не найден.", "Результат поиска");
                    return;
                }
                DataRow foundGuest = table.Rows[0];
                SelectGuestInDataGridView(foundGuest);

                MessageBox.Show($"Гость найден: {foundGuest["Фамиилия"]} {foundGuest["Имя"]}", "Результат поиска");
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
        private void SelectGuestInDataGridView(DataRow eventRow)
        {
            int eventId = Convert.ToInt32(eventRow["Guest_id"]);
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                if (row.Cells["Guest_id"].Value != null &&
                    Convert.ToInt32(row.Cells["Guest_id"].Value) == eventId)
                {
                    dataGridView3.ClearSelection();
                    row.Selected = true;
                    dataGridView3.CurrentCell = row.Cells[1];
                    dataGridView3.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }
        private void dataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView4.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView4.SelectedRows[0];
                textBox15.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
                textBox14.Text = selectedRow.Cells["Единица измерения"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Service_id"].Value != DBNull.Value)
                {
                    idService = Convert.ToInt32(selectedRow.Cells["Service_id"].Value);
                }
            }
        }
        private void materialButton20_Click(object sender, EventArgs e)
        {
            ClearServiceFields();
        }
        private void ClearServiceFields()
        {
            textBox15.Text = "";
            textBox14.Text = "";
            idService = -1;
        }
        private void materialButton17_Click(object sender, EventArgs e)
        {
            string Name = textBox15.Text.Trim();
            string Unit = textBox14.Text.Trim();

            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Unit))
            {
                MessageBox.Show("Заполните обязательные поля: Наименование и Единицу измерения!");
                return;
            }
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                if (selectedClientId == -1)
                {
                    string insertQuery = @"INSERT INTO `services` (`Наименование`, `Единица измерения`) 
                                VALUES (@name, @unit)";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@unit", Unit);

                    command.ExecuteNonQuery();
                    MessageBox.Show("Услуга успешно создана!");
                }
                else
                {
                    string updateQuery = @"UPDATE `services`
                                SET `Наименование` = @name, `Единица измерения` = @unit
                                WHERE `Service_id` = @ServiceID";
                    MySqlCommand command = new MySqlCommand(updateQuery, db.getConnection());
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@unit", Unit);
                    command.Parameters.AddWithValue("@ServiceID", idService);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения услуги сохранены!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные услуги.");
                    }
                }
                LoadData();
                ClearServiceFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении услугм: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void materialButton19_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
            "Введите данные для поиска услуги:\n(Наименование или единица измерения)", "Поиск гостя", "", -1, -1);
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            FindAndSelectService(searchText);
        }
        private void FindAndSelectService(string searchText)
        {
            DBEvent db = new DBEvent();

            try
            {
                db.openConnection();
                string query = @"SELECT * FROM `services` 
                        WHERE `Наименование` LIKE @search 
                           OR `Единица измерения` LIKE @search 
                        LIMIT 1";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Услуга по запросу '{searchText}' не найдена.", "Результат поиска");
                    return;
                }
                DataRow foundService = table.Rows[0];
                SelectServiceInDataGridView(foundService);

                MessageBox.Show($"Услуга найдена: {foundService["Наименование"]} {foundService["Единица измерения"]}", "Результат поиска");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска услуги: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void SelectServiceInDataGridView(DataRow eventRow)
        {
            int eventId = Convert.ToInt32(eventRow["Service_id"]);
            foreach (DataGridViewRow row in dataGridView4.Rows)
            {
                if (row.Cells["Service_id"].Value != null &&
                    Convert.ToInt32(row.Cells["Service_id"].Value) == eventId)
                {
                    dataGridView4.ClearSelection();
                    row.Selected = true;
                    dataGridView4.CurrentCell = row.Cells[1];
                    dataGridView4.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void materialButton18_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Удалить выбранную услугу?", "Подтверждение",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string insertQuery = @"DELETE FROM `services` WHERE `Service_id` = @id;";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@id", idService);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Услуга успешно удалена!");
                        LoadData();
                        ClearServiceFields();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить услугу.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении услуги:{ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
                LoadData();
                ClearServiceFields();
            }
        }

        private void materialButton24_Click(object sender, EventArgs e)
        {
            dataGridView6.ClearSelection();
            ClearSupplierFields();
            textBox21.Focus();
        }
        private void ClearSupplierFields()
        {
            textBox21.Text = "";
            textBox19.Text = "";
            textBox16.Text = "";
            comboBox4.Text = "";
            idSupplier = -1;
        }

        private void materialButton21_Click(object sender, EventArgs e)
        {
            string name = textBox21.Text.Trim();
            string contact = textBox19.Text.Trim();
            string requisites = textBox16.Text.Trim();
            int ServiceId = 0;
            if (comboBox4.SelectedItem != null)
            {
                var selectedItem = comboBox4.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    ServiceId = Convert.ToInt32(selectedItem["Service_id"]);
                }
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(contact))
            {
                MessageBox.Show("Заполните обязательные поля: наименование и контактные данные!");
                return;
            }

            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                if (idSupplier == -1)
                {
                    string insertQuery = @"INSERT INTO `supplier_of_services` 
                                (`Наименование`, `Контактные данные`, `Реквизиты`, `Услуга`) 
                                VALUES (@name, @contact, @requisites, @service)";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@contact", contact);
                    command.Parameters.AddWithValue("@requisites", requisites);
                    command.Parameters.AddWithValue("@service", ServiceId);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Поставщик успешно добавлен!");
                        ClearSupplierFields();
                        LoadSupplier();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить поставщика.");
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE `supplier_of_services`
                                SET `Наименование` = @name, `Контактные данные` = @contact, 
                                    `Реквизиты` = @requisites, `Услуга` = @service,
                                WHERE `id` = @SupplierID";
                    MySqlCommand command = new MySqlCommand(updateQuery, db.getConnection());
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@contact", contact);
                    command.Parameters.AddWithValue("@requisites", requisites);
                    command.Parameters.AddWithValue("@service", ServiceId);
                    command.Parameters.AddWithValue("@SupplierID", idSupplier);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения поставщика сохранены!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные поставщика.");
                    }
                }
                ClearSupplierFields();
                LoadSupplier();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении поставщика: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton22_Click(object sender, EventArgs e)
        {
            if (idSupplier == -1)
            {
                MessageBox.Show("Выберите поставщика для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранного поставщика?", "Подтверждение",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string deleteQuery = @"DELETE FROM `supplier_of_services` WHERE `Supplier_id` = @id";
                    MySqlCommand command = new MySqlCommand(deleteQuery, db.getConnection());
                    command.Parameters.AddWithValue("@id", idSupplier);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Поставщик успешно удален!");
                        LoadSupplier();
                        ClearSupplierFields();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить поставщика.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении поставщика: {ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
            }
        }

        private void materialButton23_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
    "Введите данные для поиска поставщика:\n(Наименование, контактные данные или реквизиты)",
    "Поиск поставщика", "", -1, -1);

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            FindAndSelectSupplier(searchText);
        }

        private void FindAndSelectSupplier(string searchText)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT * FROM `supplier_of_services` 
                WHERE `Наименование` LIKE @search 
                OR `Контактные данные` LIKE @search 
                OR `Реквизиты` LIKE @search 
                LIMIT 1";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Поставщик по запросу '{searchText}' не найден.", "Результат поиска");
                    return;
                }
                DataRow foundSupplier = table.Rows[0];
                SelectSupplierInDataGridView(foundSupplier);

                MessageBox.Show($"Поставщик найден: {foundSupplier["Наименование"]}", "Результат поиска");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска поставщика: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void SelectSupplierInDataGridView(DataRow supplierRow)
        {
            int supplierId = Convert.ToInt32(supplierRow["Supplier_id"]);
            foreach (DataGridViewRow row in dataGridView6.Rows)
            {
                if (row.Cells["Supplier_id"].Value != null &&
                    Convert.ToInt32(row.Cells["Supplier_id"].Value) == supplierId)
                {
                    dataGridView6.ClearSelection();
                    row.Selected = true;
                    dataGridView6.CurrentCell = row.Cells[1];
                    dataGridView6.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void dataGridView6_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView6.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView6.SelectedRows[0];
                textBox21.Text = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
                textBox19.Text = selectedRow.Cells["Контактные данные"].Value?.ToString() ?? "";
                textBox16.Text = selectedRow.Cells["Реквизиты"].Value?.ToString() ?? "";
                comboBox4.Text = selectedRow.Cells["Услуга"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Supplier_id"].Value != null &&
                    selectedRow.Cells["Supplier_id"].Value != DBNull.Value)
                {
                    idSupplier = Convert.ToInt32(selectedRow.Cells["Supplier_id"].Value);
                }
            }
        }

        private void dataGridView7_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView7.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView7.SelectedRows[0];
                comboBox5.Text = selectedRow.Cells["Мероприятие"].Value?.ToString() ?? "";
                comboBox6.Text = selectedRow.Cells["Поставщик"].Value?.ToString() ?? "";
                textBox22.Text = selectedRow.Cells["Количество"].Value?.ToString() ?? "";
                textBox23.Text = selectedRow.Cells["Цена за единицу"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Estimate_id"].Value != null &&
                    selectedRow.Cells["Estimate_id"].Value != DBNull.Value)
                {
                    idEstimate = Convert.ToInt32(selectedRow.Cells["Estimate_id"].Value);
                }
            }
        }

        private void materialButton28_Click(object sender, EventArgs e)
        {
            dataGridView7.ClearSelection();
            ClearEstimateFields();
            comboBox5.Focus();
        }
        private void ClearEstimateFields()
        {
            comboBox5.Text = "";
            comboBox6.Text = "";
            textBox22.Text = "";
            textBox23.Text = "";
            idEstimate = -1;
        }

        private void materialButton25_Click(object sender, EventArgs e)
        {
            int eventId = 0;
            int supplierId = 0;
            int quantity = 0;
            decimal price = 0;
            if (comboBox5.SelectedItem != null)
            {
                var selectedItem = comboBox5.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    eventId = Convert.ToInt32(selectedItem["Event_id"]);
                }
            }
            if (comboBox6.SelectedItem != null)
            {
                var selectedItem = comboBox6.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    supplierId = Convert.ToInt32(selectedItem["Supplier_id"]);
                }
            }

            if (!int.TryParse(textBox22.Text, out quantity) ||
                !decimal.TryParse(textBox23.Text, out price))
            {
                MessageBox.Show("Проверьте правильность ввода количества и цены!");
                return;
            }

            if (eventId == 0 || supplierId == 0)
            {
                MessageBox.Show("Выберите мероприятие и поставщика!");
                return;
            }

            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();

                if (idEstimate == -1)
                {
                    string insertQuery = @"INSERT INTO `estimate` 
                                (`Мероприятие`, `Поставщик`, `Количество`, `Цена за единицу`) 
                                VALUES (@event, @supplier, @quantity, @price)";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@event", eventId);
                    command.Parameters.AddWithValue("@supplier", supplierId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@price", price);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Позиция сметы успешно добавлена!");
                        ClearEstimateFields();
                        LoadEstimate();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить позицию сметы.");
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE `estimate`
                                SET `Мероприятие` = @event, `Поставщик` = @supplier, 
                                    `Количество` = @quantity, `Цена за единицу` = @price
                                WHERE `Estimate_id` = @EstimateID";
                    MySqlCommand command = new MySqlCommand(updateQuery, db.getConnection());
                    command.Parameters.AddWithValue("@event", eventId);
                    command.Parameters.AddWithValue("@supplier", supplierId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@EstimateID", idEstimate);

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
                LoadEstimate();
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

        private void materialButton26_Click(object sender, EventArgs e)
        {
            if (idEstimate == -1)
            {
                MessageBox.Show("Выберите позицию сметы для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранную позицию сметы?", "Подтверждение",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string deleteQuery = @"DELETE FROM `estimate` WHERE `Estimate_id` = @id";
                    MySqlCommand command = new MySqlCommand(deleteQuery, db.getConnection());
                    command.Parameters.AddWithValue("@id", idEstimate);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Позиция сметы успешно удалена!");
                        LoadEstimate();
                        ClearEstimateFields();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить позицию сметы.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении сметы: {ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
            }
        }

        private void materialButton27_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите данные для поиска сметы:\n(ID мероприятия, ID поставщика или цена)",
                "Поиск сметы", "", -1, -1);

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            FindAndSelectEstimate(searchText);
        }

        private void FindAndSelectEstimate(string searchText)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"SELECT * FROM `estimate` 
                WHERE `Мероприятие` LIKE @search 
                   OR `Поставщик` LIKE @search 
                   OR `Цена за единицу` LIKE @search 
                LIMIT 1";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Смета по запросу '{searchText}' не найдена.", "Результат поиска");
                    return;
                }
                DataRow foundEstimate = table.Rows[0];
                SelectEstimateInDataGridView(foundEstimate);

                MessageBox.Show($"Смета найдена: ID {foundEstimate["Estimate_id"]}", "Результат поиска");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска сметы: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void SelectEstimateInDataGridView(DataRow estimateRow)
        {
            int estimateId = Convert.ToInt32(estimateRow["Estimate_id"]);
            foreach (DataGridViewRow row in dataGridView7.Rows)
            {
                if (row.Cells["Estimate_id"].Value != null &&
                    Convert.ToInt32(row.Cells["Estimate_id"].Value) == estimateId)
                {
                    dataGridView7.ClearSelection();
                    row.Selected = true;
                    dataGridView7.CurrentCell = row.Cells[1];
                    dataGridView7.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void dataGridView8_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView8.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView8.SelectedRows[0];
                comboBox7.Text = selectedRow.Cells["Мероприятие"].Value?.ToString() ?? "";
                textBox32.Text = selectedRow.Cells["Оплата"].Value?.ToString() ?? "";
                if (selectedRow.Cells["Дата оплаты"].Value != null &&
                    selectedRow.Cells["Дата оплаты"].Value != DBNull.Value)
                {
                    dateTimePicker7.Value = Convert.ToDateTime(selectedRow.Cells["Дата оплаты"].Value);
                }
                else
                {
                    dateTimePicker7.Value = DateTime.Now;
                }
                if (selectedRow.Cells["Pay_id"].Value != null &&
                    selectedRow.Cells["Pay_id"].Value != DBNull.Value)
                {
                    idPayment = Convert.ToInt32(selectedRow.Cells["Pay_id"].Value);
                }
            }
        }

        private void materialButton32_Click(object sender, EventArgs e)
        {
            dataGridView8.ClearSelection();
            ClearPaymentFields();
            comboBox7.Focus();
        }
        private void ClearPaymentFields()
        {
            comboBox7.Text = "";
            textBox32.Text = "";
            dateTimePicker7.Value = DateTime.Now;
            idPayment = -1;
        }

        private void materialButton29_Click(object sender, EventArgs e)
        {
            int eventId = 0;
            decimal amount = 0;
            DateTime paymentDate = dateTimePicker7.Value;
            if (comboBox7.SelectedItem != null)
            {
                var selectedItem = comboBox7.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    eventId = Convert.ToInt32(selectedItem["Event_id"]);
                }
            }
            if (!decimal.TryParse(textBox32.Text, out amount))
            {
                MessageBox.Show("Проверьте правильность ввода суммы оплаты!");
                return;
            }
            if (eventId == 0)
            {
                MessageBox.Show("Выберите мероприятие!");
                return;
            }
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                if (idPayment == -1)
                {
                    string insertQuery = @"INSERT INTO `pay` 
                                (`Мероприятие`, `Оплата`, `Дата оплаты`) 
                                VALUES (@event, @amount, @paymentDate)";
                    MySqlCommand command = new MySqlCommand(insertQuery, db.getConnection());
                    command.Parameters.AddWithValue("@event", eventId);
                    command.Parameters.AddWithValue("@amount", amount);
                    command.Parameters.AddWithValue("@paymentDate", paymentDate);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Оплата успешно добавлена!");
                        ClearPaymentFields();
                        LoadPay();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить оплату.");
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE `pay`
                                SET `Мероприятие` = @event, `Оплата` = @amount, 
                                    `Дата оплаты` = @paymentDate
                                WHERE `Pay_id` = @PaymentID";
                    MySqlCommand command = new MySqlCommand(updateQuery, db.getConnection());
                    command.Parameters.AddWithValue("@event", eventId);
                    command.Parameters.AddWithValue("@amount", amount);
                    command.Parameters.AddWithValue("@paymentDate", paymentDate);
                    command.Parameters.AddWithValue("@PaymentID", idPayment);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения оплаты сохранены!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные оплаты.");
                    }
                }
                ClearPaymentFields();
                LoadPay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении оплаты: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton30_Click(object sender, EventArgs e)
        {
            if (idPayment == -1)
            {
                MessageBox.Show("Выберите оплату для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранную оплату?", "Подтверждение",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DBEvent db = new DBEvent();
                try
                {
                    db.openConnection();
                    string deleteQuery = @"DELETE FROM `pay` WHERE `Pay_id` = @id";
                    MySqlCommand command = new MySqlCommand(deleteQuery, db.getConnection());
                    command.Parameters.AddWithValue("@id", idPayment);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Оплата успешно удалена!");
                        LoadPay();
                        ClearPaymentFields();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить оплату.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении оплаты: {ex.Message}");
                }
                finally
                {
                    db.closeConnection();
                }
            }
        }

        private void materialButton31_Click(object sender, EventArgs e)
        {
            string searchText = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите данные для поиска оплаты:\n(ID мероприятия, сумма или дата)",
                "Поиск оплаты", "", -1, -1);

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }
            FindAndSelectPayment(searchText);
        }

        private void FindAndSelectPayment(string searchText)
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"SELECT * FROM `pay` 
                WHERE `Мероприятие` LIKE @search 
                   OR `Оплата` LIKE @search 
                   OR `Дата оплаты` LIKE @search 
                LIMIT 1";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show($"Оплата по запросу '{searchText}' не найдена.", "Результат поиска");
                    return;
                }
                DataRow foundPayment = table.Rows[0];
                SelectPaymentInDataGridView(foundPayment);

                MessageBox.Show($"Оплата найдена: Сумма {foundPayment["Оплата"]}", "Результат поиска");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска оплаты: {ex.Message}");
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void SelectPaymentInDataGridView(DataRow paymentRow)
        {
            int paymentId = Convert.ToInt32(paymentRow["Pay_id"]);
            foreach (DataGridViewRow row in dataGridView8.Rows)
            {
                if (row.Cells["Pay_id"].Value != null &&
                    Convert.ToInt32(row.Cells["Pay_id"].Value) == paymentId)
                {
                    dataGridView8.ClearSelection();
                    row.Selected = true;
                    dataGridView8.CurrentCell = row.Cells[1];
                    dataGridView8.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void materialButton35_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage2;
        }

        private void materialButton38_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage3;
            materialTabControl2.SelectedTab = tabPage6;
        }

        private void materialButton42_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage3;
            materialTabControl2.SelectedTab = tabPage4;
        }

        private void materialButton36_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage3;
            materialTabControl2.SelectedTab = tabPage5;
        }

        private void materialButton39_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage3;
            materialTabControl2.SelectedTab = tabPage7;
        }

        private void materialButton40_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage3;
            materialTabControl2.SelectedTab = tabPage8;
        }

        private void materialButton41_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage3;
            materialTabControl2.SelectedTab = tabPage9;
        }

        private void materialButton37_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = tabPage3;
            materialTabControl2.SelectedTab = tabPage10;
        }
    }
}
