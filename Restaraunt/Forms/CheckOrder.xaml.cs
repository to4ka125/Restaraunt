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
using System.Windows.Shapes;

namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for CheckOrder.xaml
    /// </summary>
    public partial class CheckOrder : Window
    {
        public CheckOrder()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            if (Basket.basket.Count > 0)
            {
                AddOrderBtn.IsEnabled = true;
            }
            else
            {
                AddOrderBtn.IsEnabled = false;
            }

            DataTable dataTable = new DataTable();
            decimal totalPrice = 0;

            dataTable.Columns.Add("Наименование");
            dataTable.Columns.Add("Кол-во");
            dataTable.Columns.Add("Цена");

            int countOrder = 0;

            foreach (var item in Basket.basket)
            {
                countOrder++;
                string itemId = item.Key;

                int quantity = item.Value;

                using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand($"Select name,price From Menu " +
                        $"Where menu_id ='{itemId}'", con);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader["name"].ToString();
                            decimal price = Convert.ToDecimal(reader["price"]);
                            dataTable.Rows.Add(name, quantity, price);
                        }
                    }
                }
            }

            foreach (DataRow row in dataTable.Rows)
            {
                decimal price = Convert.ToDecimal(row["Цена"]);

                int quantity = Convert.ToInt32(row["Кол-во"]);

                totalPrice += price * quantity;

            }
            TotalPrice.Text = totalPrice.ToString();
            MenuData.ItemsSource = dataTable.DefaultView;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Basket.basket.Clear();
            MessageBox.Show("Корзина отчищенна","Сообщение пользователю",MessageBoxButton.OK,MessageBoxImage.Information);
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            //Дописать код в дальнейшем

        }
    }
}
