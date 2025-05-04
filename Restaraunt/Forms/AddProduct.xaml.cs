using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for AddProduct.xaml
    /// </summary>
    public partial class AddProduct : Window
    {
        public AddProduct()
        {
            InitializeComponent();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (qNameProduct.Text == null || qСountProduct.Text == null || qPrice.Text == null || qSupliers.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string input = qСountProduct.Text.Replace(',', '.');
            if (Double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double quantity))
            {
                if (quantity < 0)
                {
                    MessageBox.Show("Количество продуктов не может быть отрицательным. Пожалуйста, введите корректное значение.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Введите корректное числовое значение для количества продуктов.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            input = qPrice.Text.Replace(',', '.');
            if (Double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double unitPrice))
            {
                if (unitPrice < 0)
                {
                    MessageBox.Show("Стоимость ед продукта не может быть отрицательным. Пожалуйста, введите корректное значение.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Введите корректное числовое значение для стоимости за ед продукта.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string name = qNameProduct.Text;
            string supplierId = qSupliers.Text.Split('-')[0];
       
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand($@"Insert into products (name,quantity,unit_price,supplier_id) 
                                                              Values ('{name}','{quantity}','{unitPrice}','{supplierId}')", con))
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Продукт успешно добавлен в вашу коллекцию!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            qNameProduct.Clear();
            qСountProduct.Clear();
            qPrice.Clear();
            qSupliers.SelectedItem = null;
        }

        private void ClearProduct_Click(object sender, RoutedEventArgs e)
        {
            qNameProduct.Clear();
            qСountProduct.Clear();
            qPrice.Clear();
            qSupliers.SelectedItem = null;
            MessageBox.Show("Поля успешно очищенны!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("Select supplier_id,name From supplier", con))
                {
                    MySqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        qSupliers.Items.Add($"{dr.GetValue(0)}-{dr.GetValue(1)}");
                    }
                }
            }
        }

        private void qNameProduct_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9\W]$")) { e.Handled = true; }
            if (Regex.IsMatch(e.Text, @"^[_]$")) { e.Handled = true; }
        }

        private void qСountProduct_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[^0-9.,]$"))
            {
                e.Handled = true;
            }
        }

        private void qPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[^0-9.,]$"))
            {
                e.Handled = true;
            }
        }
    }
}
