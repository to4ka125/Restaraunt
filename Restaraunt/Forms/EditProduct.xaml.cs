using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for EditProduct.xaml
    /// </summary>
    public partial class EditProduct : Window
    {
        public EditProduct()
        {
            InitializeComponent();
        }

        private void AddProductBtn_Click(object sender, RoutedEventArgs e)
        {
            if (qСountProduct.Text == null || qСountProduct.Text.Trim() == "")
            {
                MessageBox.Show("Пожалуйста, укажите количество продуктов. Это поле не может быть пустым.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
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


            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand($"Update products Set quantity = '{qСountProduct.Text.Replace(',', '.')}' Where product_id = '{SafeData.product_id}' ", con))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Количество продуктов успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка при обновлении количества продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        this.Close();
                    }
                }
            }
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
                using (MySqlCommand cmd = new MySqlCommand($@"SELECT 
                                                                p.name AS 'Product Name', 
                                                                p.quantity AS 'Quantity', 
                                                               concat(p.unit_price, ' р.')AS 'Unit Price', 
                                                                CONCAT(p.supplier_id, '-', s.name) AS 'Supplier Info'
                                                                FROM 
                                                                    products p
                                                                INNER JOIN 
                                                                    supplier s ON s.supplier_id = p.supplier_id
                                                                WHERE 
                                                                    p.product_id ='{SafeData.product_id}'", con))
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    qNameProduct.Text = dt.Rows[0].ItemArray[0].ToString();
                    qСountProduct.Text = dt.Rows[0].ItemArray[1].ToString();
                    qPrice.Text = dt.Rows[0].ItemArray[2].ToString();
                    qSupliers.Text = dt.Rows[0].ItemArray[3].ToString();

                    qNameProduct.IsEnabled = false;
                    qPrice.IsEnabled = false;
                    qSupliers.IsEnabled = false;
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

        }

        private void ClearProduct_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
