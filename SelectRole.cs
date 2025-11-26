using EventManagmentSystem.Sipplier;
using MaterialSkin;
using MaterialSkin.Controls;
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
    public partial class SelectRole : MaterialForm
    {
        public SelectRole()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            Koordinator kf = new Koordinator();
            this.Hide();
            kf.ShowDialog();
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            this.Hide();
            Supplier sf = new Supplier(f.login);
            sf.ShowDialog();
        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            this.Hide();
            Client.Client cf = new Client.Client(f.login);
            cf.ShowDialog();
        }

        private void materialButton5_Click(object sender, EventArgs e)
        {
            this.Hide();
            Admin.Admin a = new Admin.Admin(); 
            a.ShowDialog();
        }
    }
}
