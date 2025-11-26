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

namespace EventManagmentSystem
{
    public partial class Form2 : MaterialForm
    {
        public Form2()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            String loginuser = textBox1.Text;
            String secret = textBox2.Text;
            DBUser db = new DBUser();
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `Login` = @ul AND `secret` = @se", db.getConnection());
            command.Parameters.Add("@ul", MySqlDbType.VarChar).Value = loginuser;
            command.Parameters.Add("@se", MySqlDbType.VarChar).Value = secret;
            adapter.SelectCommand = command;
            adapter.Fill(table);
            if (table.Rows.Count > 0)
            {
                String password = table.Rows[0]["Password"].ToString();
                MessageBox.Show($"Ваш пароль: {password}. Если вам необходимо поменять пароль обратитесь к администратору. Это можно сделать через координаторов.");
                this.Hide();
            }
            else
            {
                MessageBox.Show("Данного секрета не найдено. Пожалуйста обратитесь к одному из сотрудников чтобы решить свою проблему.");
            }
        }
    }
}
