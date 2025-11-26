using MySql.Data.MySqlClient;
using Mysqlx.Session;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using SD = System.Data;

namespace EventManagmentSystem.Classes
{
    public class getdata
    {
        public MySqlCommand command { get; set; }
        public void setData(string nameTable, DataGridView dgv)
        {
            DBEvent db = new DBEvent();
            SD.DataSet dataSet = new SD.DataSet();
            String scriptclient = $"SELECT * From {nameTable}";
            SD.DataTable table = new SD.DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter(scriptclient, db.getConnection());
            adapter.Fill(table);
            dgv.DataSource = table;
            dgv.ClearSelection();
        }
        public void setUser(string nameTable, DataGridView dgv)
        {
            DBUser db = new DBUser();
            SD.DataSet dataSet = new SD.DataSet();
            String scriptclient = $"SELECT * From {nameTable}";
            SD.DataTable table = new SD.DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter(scriptclient, db.getConnection());
            adapter.Fill(table);
            dgv.DataSource = table;
            dgv.ClearSelection();
        }
        public void ProcessingRequestEvent(string request, DataGridView dgv, Dictionary<string, object> parameters = null)
        {
            DBEvent db = new DBEvent();
            DataSet dataSet = new DataSet();
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            command = new MySqlCommand(request, db.getConnection());
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }
            adapter.SelectCommand = command;
            adapter.Fill(table);
            dgv.DataSource = table;
            dgv.ClearSelection();
        }
        public void getEventToTable(string nameTable, DataTable dt)
        {
            DBEvent db = new DBEvent();
            SD.DataSet dataSet = new SD.DataSet();
            String scriptclient = $"SELECT * From {nameTable}";
            MySqlDataAdapter adapter = new MySqlDataAdapter(scriptclient, db.getConnection());
            adapter.Fill(dt);
        }
    }
}
