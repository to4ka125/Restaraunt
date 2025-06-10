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
using Restaraunt.Utilits;

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
            idTables.Text = SafeData.tablesId;
            DataTable dataTable = new DataTable();
            double totalPrice = 0;

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
                            double price = Convert.ToDouble(reader["price"]);
                            dataTable.Rows.Add(name, quantity, price);
                        }
                    }
                }
            }

            foreach (DataRow row in dataTable.Rows)
            {
                double price = Convert.ToDouble(row["Цена"]);

                int quantity = Convert.ToInt32(row["Кол-во"]);

                totalPrice += price * quantity;

            }
            TotalPrice.Text = Discount.DiscountFun(totalPrice).ToString();
            MenuData.ItemsSource = dataTable.DefaultView;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Basket.basket.Clear();
            MessageBox.Show("Корзина отчищенна", "Сообщение пользователю", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
            SafeData.dishesAddBool = false;
        }

        private void AddOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int maxOrder_id;
                using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
                {
                    con.Open();

                    string customerIdValue = string.IsNullOrEmpty(SafeData.customer_id)
                        ? "NULL"
                        : $"'{SafeData.customer_id}'";

                    string query = $@"
                    INSERT INTO Orders (
                        user_id,
                        table_number,
                        idpayment_method,
                        { (string.IsNullOrEmpty(SafeData.customer_id) ? "" : "customer_id,") }
                        status,
                        order_time,
                        total_price
                    ) 
                    VALUES (
                        '{SafeData.userId}',
                        '{SafeData.tablesId}',
                        '{SafeData.idpayment_method}',
                        { (string.IsNullOrEmpty(SafeData.customer_id) ? "" : $"'{SafeData.customer_id}',") }
                        'В обработке',
                        '{DateTime.Now.ToString("yyyy-MM-dd")}',
                        '{TotalPrice.Text.Replace(',', '.')}'
                    )";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.ExecuteNonQuery();
                    }


                    using (MySqlCommand cmd = new MySqlCommand($@"Update tables set  status = 'занят' Where table_id ='{SafeData.tablesId}'", con))
                    {
                        cmd.ExecuteNonQuery();
                    }


                    if (SafeData.isReservTable)
                    {
                        using (MySqlCommand cmd = new MySqlCommand($@"Update reservations set status = 'Завершена' Where table_id='{SafeData.tablesId}' 
                                                                  And reservation_time =CURRENT_DATE", con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        SafeData.isReservTable = false;
                    }



                    using (MySqlCommand cmd = new MySqlCommand("SELECT MAX(order_id) FROM Orders", con))
                    {
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        maxOrder_id = int.Parse(dt.Rows[0].ItemArray[0].ToString());
                    }


                    foreach (var item in Basket.basket)
                    {
                        string menuId = item.Key;
                        int quantity = item.Value;
                        using (MySqlCommand cmd = new MySqlCommand($@"INSERT INTO `restaurant`.`Order_Items` (`order_id`, `menu_id`, `quantity`)
                                                                  Values ('{maxOrder_id}','{menuId}','{quantity}')", con))
                        {
                            cmd.ExecuteNonQuery();
                        }


                        for (int i = 0; i < quantity; i++)
                        {
                            using (MySqlCommand cmd = new MySqlCommand($@"
                        UPDATE Products p
                        JOIN Menu_Ingredients mn ON p.product_id = mn.product_id
                        SET p.quantity = p.quantity - mn.quantity
                        WHERE mn.menu_id = '{menuId}'", con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show("Ваш заказ успешно сформирован!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                    SafeData.dishesAddBool = true;
                    SafeData.idpayment_method = "1";
                    SafeData.tablesId = null;
                    Basket.basket.Clear();
                    InitializeComponent();
                    this.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
      

        }
    }
}
