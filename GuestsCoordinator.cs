using EventManagmentSystem.Classes;
using MaterialSkin;
using MaterialSkin.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using SD = System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventManagmentSystem
{
    public partial class GuestsCoordinator : MaterialForm
    {
        public int eventid { get; set; }
        public string eventname { get; set; }
        public GuestsCoordinator()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GuestsCoordinator_Load(object sender, EventArgs e)
        {
            string request1 = "SELECT * FROM `guest` WHERE `Мероприятие` = @EventID;";
            getdata getdata = new getdata();
            int idevent = eventid;
            textBox1.Text = eventname;
            var parameters = new Dictionary<string, object>
            {
                { "@EventID", idevent }
            };
            getdata.ProcessingRequestEvent(request1, dataGridView1, parameters);
        }
    }
}
