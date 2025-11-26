using EventManagmentSystem.Classes;
using EventManagmentSystem.Sipplier;
using MaterialSkin;
using MaterialSkin.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventManagmentSystem
{
    public partial class Form1 : MaterialForm
    {
        public string login { get; set; }
        public int idUser { get; set; }
        public Form1()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            String loginuser = textBox1.Text;
            login = loginuser;
            String passuser = textBox2.Text;
            DBUser db = new DBUser();
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `Логин` = @ul AND `Пароль` = @up", db.getConnection());
            command.Parameters.Add("@ul", MySqlDbType.VarChar).Value = loginuser;
            command.Parameters.Add("@up", MySqlDbType.VarChar).Value = passuser;
            adapter.SelectCommand = command;
            adapter.Fill(table);
            idUser = Convert.ToInt32(table.Rows[0]["id_user"]);
            if(table.Rows.Count > 0)
            {
                db.openConnection();
                MessageBox.Show($"Добро пожаловать {loginuser}");
                MySqlCommand command1 = new MySqlCommand("SELECT `Роль` FROM `users` WHERE `Логин` = @ul AND `Пароль` = @up", db.getConnection());
                command1.Parameters.Add("@ul", MySqlDbType.VarChar).Value = loginuser;
                command1.Parameters.Add("@up", MySqlDbType.VarChar).Value = passuser;
                adapter.SelectCommand = command1;
                object result = command1.ExecuteScalar();
                int roleID;
                if (result != null)
                {
                    roleID = Convert.ToInt32(result);
                    if (roleID == 1)
                    {
                        Admin.Admin af = new Admin.Admin();
                        this.Hide();
                        af.ShowDialog();
                    }
                    else if (roleID == 2)
                    {
                        Koordinator kf = new Koordinator();
                        this.Hide();
                        kf.ShowDialog();
                    }
                    else if (roleID == 3)
                    {
                        Client.Client cf = new Client.Client(login);
                        this.Hide();
                        cf.ShowDialog();
                    }
                    else if (roleID == 4)
                    {
                        Supplier sf = new Supplier(login);
                        this.Hide();
                        sf.ShowDialog();
                    }
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
        }
    }
}
