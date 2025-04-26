using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        string query = @"SELECT order_id As 'Номер заказа', concat(us.lastName,' ',Left(us.name,1),'.') AS 'ФИО', o.table_number As 'Номер стола',
                         o.status As 'Статус', o.order_time As 'Дата заказа', Concat (total_price, ' ₽') As 'Стоимость заказа' From orders o
                         inner Join users us On us.user_id  = o.user_id";
        string id;

        public OrderView()
        {
            InitializeComponent();
        }

        private void filteringAndSorting()
        {
            query = @"SELECT order_id As 'Номер заказа', us.name As 'Имя официанта', 
                         us.lastName As 'Фамилия официанта', o.table_number As 'Номер стола',
                         o.status As 'Статус', o.order_time As 'Дата заказа', Concat (total_price, ' ₽') As 'Стоимость заказа' From orders o
                         inner Join users us On us.user_id  = o.user_id";

            bool hasWhereClause = false;
/*
            if (Filtering.SelectedItem != null)
            {
                string selectedStatusValue = (Filtering.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (!string.IsNullOrEmpty(selectedStatusValue))
                {
                    if (!hasWhereClause)
                    {
                        query += " WHERE";
                        hasWhereClause = true;
                    }
                    else
                    {
                        query += " AND";
                    }
                    query += $" o.status = '{selectedStatusValue}'";
                }
            }
*/
            if (!string.IsNullOrEmpty(datePicker.Text))
            {
                string selectedTime = datePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                if (!hasWhereClause)
                {
                    query += " WHERE";
                    hasWhereClause = true;
                }
                else
                {
                    query += " AND";
                }
                query += $" (o.order_time  LIKE '%{selectedTime}%')";
            }

            UpdateDataGridView(query);
        }

        private void UpdateDataGridView(string query)
        {
            DataTable dataTable = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(MySqlCon.con))
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, connection);
                connection.Open();
                try
                {
                    dataAdapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при извлечении данных: {ex.Message}");
                }
            }
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataGridView(query);
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
