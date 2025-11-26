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
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace EventManagmentSystem
{
    public partial class ReportForm : MaterialForm
    {
        private string reportType;
        private DataTable reportData;

        public ReportForm(string reportName)
        {
            InitializeComponent();
            reportType = reportName;
            Text = reportName;
            InitializeMaterialDesign();
            GenerateReport();
        }

        private void InitializeMaterialDesign()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }
        private void GenerateReport()
        {
            try
            {
                switch (reportType)
                {
                    case "ОТЧЕТ ПО МЕРОПРИЯТИЯМ С ФИНАНСОВОЙ АНАЛИТИКОЙ":
                        GenerateEventsFinancialReport();
                        break;
                    case "ОТЧЕТ ПО КЛИЕНТАМ И ИХ МЕРОПРИЯТИЯМ":
                        GenerateClientsEventsReport();
                        break;
                    case "ОТЧЕТ ПО ГОСТЯМ МЕРОПРИЯТИЙ":
                        GenerateGuestsReport();
                        break;
                    case "ФИНАНСОВЫЙ ОТЧЕТ ПО МЕСЯЦАМ":
                        GenerateMonthlyFinancialReport();
                        break;
                    case "ОТЧЕТ ПО ПОСТАВЩИКАМ И УСЛУГАМ":
                        GenerateSuppliersServicesReport();
                        break;
                    case "ДЕТАЛИЗИРОВАННАЯ СМЕТА МЕРОПРИЯТИЙ":
                        GenerateDetailedEstimatesReport();
                        break;
                    case "ОТЧЕТ ПО ПОПУЛЯРНОСТИ УСЛУГ":
                        GenerateServicesPopularityReport();
                        break;
                    case "ОТЧЕТ ПО НЕЗАВЕРШЕННЫМ ОПЛАТАМ":
                        GenerateUnpaidReports();
                        break;
                    case "СТАТИСТИКА ПО МЕРОПРИЯТИЯМ И СЕЗОНАМ":
                        GenerateSeasonalStatistics();
                        break;
                    case "СВОДНЫЙ ОТЧЕТ ПО ЭФФЕКТИВНОСТИ ПОСТАВЩИКОВ":
                        GenerateSuppliersEfficiencyReport();
                        break;
                }

                dataGridView1.DataSource = reportData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}");
            }
        }
        private void GenerateEventsFinancialReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                e.`Наименование` as 'Мероприятие',
                e.`Дата мероприятия` as 'Дата',
                e.`Бюджет` as 'Бюджет',
                c.`Фамилия` as 'Клиент',
                COUNT(g.`Guest_id`) as 'Количество гостей',
                SUM(est.`Количество` * est.`Цена за единицу`) as 'Фактические затраты',
                (e.`Бюджет` - SUM(est.`Количество` * est.`Цена за единицу`)) as 'Прибыль'
                FROM `event` e
                LEFT JOIN `client` c ON e.`Клиент` = c.`Client_id`
                LEFT JOIN `guest` g ON e.`Event_id` = g.`Мероприятие`
                LEFT JOIN `estimate` est ON e.`Event_id` = est.`Мероприятие`
                GROUP BY e.`Event_id`";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateMonthlyFinancialReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                DATE_FORMAT(e.`Дата мероприятия`, '%Y-%m') as 'Месяц',
                COUNT(e.`Event_id`) as 'Количество мероприятий',
                SUM(e.`Бюджет`) as 'Общий бюджет',
                SUM(est.`Количество` * est.`Цена за единицу`) as 'Фактические затраты',
                SUM(e.`Бюджет` - (est.`Количество` * est.`Цена за единицу`)) as 'Прибыль'
                FROM `event` e
                LEFT JOIN `estimate` est ON e.`Event_id` = est.`Мероприятие`
                GROUP BY DATE_FORMAT(e.`Дата мероприятия`, '%Y-%m')
                ORDER BY Месяц DESC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateSuppliersServicesReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                s.`Наименование` as 'Поставщик',
                sv.`Наименование` as 'Услуга',
                COUNT(est.`Estimate_id`) as 'Количество заказов',
                SUM(est.`Количество`) as 'Общее количество',
                SUM(est.`Количество` * est.`Цена за единицу`) as 'Общая сумма',
                AVG(est.`Цена за единицу`) as 'Средняя цена'
                FROM `supplier_of_services` s
                LEFT JOIN `services` sv ON s.`Услуга` = sv.`Service_id`
                LEFT JOIN `estimate` est ON s.`Supplier_id` = est.`Поставщик`
                GROUP BY s.`Supplier_id`, sv.`Service_id`";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateClientsEventsReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                c.`Фамилия` as 'Фамилия',
                c.`Имя` as 'Имя',
                c.`Отчество` as 'Отчество',
                c.`Дата рождения` as 'Дата рождения',
                c.`Счет банка` as 'Счет банка',
                COUNT(e.`Event_id`) as 'Количество мероприятий',
                SUM(e.`Бюджет`) as 'Общая сумма мероприятий',
                MAX(e.`Дата мероприятия`) as 'Последнее мероприятие'
                FROM `client` c
                LEFT JOIN `event` e ON c.`Client_id` = e.`Клиент`
                GROUP BY c.`Client_id`
                ORDER BY `Количество мероприятий` DESC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateGuestsReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                e.`Наименование` as 'Мероприятие',
                e.`Дата мероприятия` as 'Дата',
                COUNT(g.`Guest_id`) as 'Количество гостей',
                GROUP_CONCAT(CONCAT(g.`Фамилия`, ' (', g.`Контактные данные`, ')') SEPARATOR '; ') as 'Список гостей'
                FROM `event` e
                LEFT JOIN `guest` g ON e.`Event_id` = g.`Мероприятие`
                GROUP BY e.`Event_id`, e.`Наименование`, e.`Дата мероприятия`
                ORDER BY e.`Дата мероприятия` DESC";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateDetailedEstimatesReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                ev.`Наименование` as 'Мероприятие',
                ev.`Дата мероприятия` as 'Дата',
                s.`Наименование` as 'Поставщик',
                sv.`Наименование` as 'Услуга',
                est.`Количество` as 'Количество',
                est.`Цена за единицу` as 'Цена за единицу',
                (est.`Количество` * est.`Цена за единицу`) as 'Общая стоимость',
                sv.`Единица измерения` as 'Единица измерения'
                FROM `estimate` est
                LEFT JOIN `event` ev ON est.`Мероприятие` = ev.`Event_id`
                LEFT JOIN `supplier_of_services` s ON est.`Поставщик` = s.`Supplier_id`
                LEFT JOIN `services` sv ON s.`Услуга` = sv.`Service_id`
                ORDER BY ev.`Дата мероприятия` DESC, ev.`Наименование`";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        
        private void GenerateServicesPopularityReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                sv.`Наименование` as 'Услуга',
                sv.`Единица измерения` as 'Единица измерения',
                COUNT(est.`Estimate_id`) as 'Количество заказов',
                SUM(est.`Количество`) as 'Общее количество',
                AVG(est.`Цена за единицу`) as 'Средняя цена',
                MIN(est.`Цена за единицу`) as 'Минимальная цена',
                MAX(est.`Цена за единицу`) as 'Максимальная цена'
                FROM `services` sv
                LEFT JOIN `supplier_of_services` s ON sv.`Service_id` = s.`Услуга`
                LEFT JOIN `estimate` est ON s.`Supplier_id` = est.`Поставщик`
                GROUP BY sv.`Service_id`
                ORDER BY `Количество заказов` DESC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateUnpaidReports()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                e.`Наименование` as 'Мероприятие',
                e.`Дата мероприятия` as 'Дата мероприятия',
                e.`Бюджет` as 'Общий бюджет',
                c.`Фамилия` as 'Фамилия клиента',
                c.`Имя` as 'Имя клиента',
                COALESCE(SUM(p.`Оплата`), 0) as 'Сумма оплаты',
                MAX(p.`Дата оплаты`) as 'Дата последней оплаты',
                CASE 
                    WHEN COALESCE(SUM(p.`Оплата`), 0) >= e.`Бюджет` THEN 'Оплачено'
                    WHEN COALESCE(SUM(p.`Оплата`), 0) = 0 THEN 'Не оплачено'
                    ELSE 'Частично оплачено'
                END as 'Статус оплаты',
                (e.`Бюджет` - COALESCE(SUM(p.`Оплата`), 0)) as 'Остаток к оплате'
                FROM `event` e
                LEFT JOIN `client` c ON e.`Клиент` = c.`Client_id`
                LEFT JOIN `pay` p ON e.`Event_id` = p.`Мероприятие`
                GROUP BY e.`Event_id`, e.`Наименование`, e.`Дата мероприятия`, e.`Бюджет`, c.`Фамилия`, c.`Имя`
                HAVING `Остаток к оплате` > 0
                ORDER BY e.`Дата мероприятия` ASC";
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateSeasonalStatistics()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                CASE 
                    WHEN MONTH(`Дата мероприятия`) IN (12, 1, 2) THEN 'Зима'
                    WHEN MONTH(`Дата мероприятия`) IN (3, 4, 5) THEN 'Весна'
                    WHEN MONTH(`Дата мероприятия`) IN (6, 7, 8) THEN 'Лето'
                    WHEN MONTH(`Дата мероприятия`) IN (9, 10, 11) THEN 'Осень'
                END as 'Сезон',
                COUNT(`Event_id`) as 'Количество мероприятий',
                AVG(`Бюджет`) as 'Средний бюджет',
                SUM(`Бюджет`) as 'Общий бюджет',
                AVG((SELECT COUNT(*) FROM `guest` g WHERE g.`Мероприятие` = e.`Event_id`)) as 'Среднее количество гостей'
                FROM `event` e
                GROUP BY `Сезон`
                ORDER BY 
                CASE `Сезон`
                    WHEN 'Зима' THEN 1
                    WHEN 'Весна' THEN 2
                    WHEN 'Лето' THEN 3
                    WHEN 'Осень' THEN 4
                END";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }
        private void GenerateSuppliersEfficiencyReport()
        {
            DBEvent db = new DBEvent();
            try
            {
                db.openConnection();
                string query = @"
                SELECT 
                s.`Наименование` as 'Поставщик',
                s.`Контактные данные` as 'Контакты',
                COUNT(DISTINCT est.`Мероприятие`) as 'Количество мероприятий',
                COUNT(est.`Estimate_id`) as 'Количество заказов',
                SUM(est.`Количество` * est.`Цена за единицу`) as 'Общий объем продаж',
                AVG(est.`Цена за единицу`) as 'Средняя цена',
                MIN(est.`Цена за единицу`) as 'Минимальная цена',
                DATEDIFF(MAX(e.`Дата мероприятия`), MIN(e.`Дата мероприятия`)) as 'Период сотрудничества (дней)'
                FROM `supplier_of_services` s
                LEFT JOIN `estimate` est ON s.`Supplier_id` = est.`Поставщик`
                LEFT JOIN `event` e ON est.`Мероприятие` = e.`Event_id`
                GROUP BY s.`Supplier_id`
                ORDER BY `Общий объем продаж` DESC";

                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                reportData = new DataTable();
                adapter.Fill(reportData);
            }
            finally
            {
                db.closeConnection();
            }
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV Files|*.csv|Excel Files|*.xlsx";
                saveFileDialog.Title = "Сохранить отчет";
                saveFileDialog.FileName = $"{reportType}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        writer.Write('\uFEFF');
                        var headers = reportData.Columns.Cast<DataColumn>();
                        writer.WriteLine(string.Join(";", headers.Select(column => EscapeCsvField(column.ColumnName))));
                        foreach (DataRow row in reportData.Rows)
                        {
                            var fields = row.ItemArray.Select(field => EscapeCsvField(field?.ToString() ?? ""));
                            writer.WriteLine(string.Join(";", fields));
                        }
                    }

                    MessageBox.Show("Отчет успешно экспортирован!", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";
            if (field.Contains(";") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Функция печати будет реализована в следующей версии", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
