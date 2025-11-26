using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagmentSystem.Classes
{
    public class DBEvent
    {
        MySqlConnection _connection = new MySqlConnection("server=localhost;port=3306;username=root;password=;database=eventmanagement");

        public void openConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
                _connection.Open();
        }
        public void closeConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                _connection.Close();
        }
        public MySqlConnection getConnection()
        {
            return _connection;
        }
    }
}
