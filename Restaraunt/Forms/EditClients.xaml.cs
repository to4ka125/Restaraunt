using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
using System.Data;
using System.Text.RegularExpressions;

namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for EditClients.xaml
    /// </summary>
    public partial class EditClients : Window
    {
        public EditClients()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                MySqlDataAdapter da = new MySqlDataAdapter($@"SELECT * FROM restaurant.customers 
                                                              where customer_id = '{SafeData.customer_id}'",con);
                DataTable dt = new DataTable();

                da.Fill(dt);

                qName.Text = dt.Rows[0][1].ToString();
                qLastName.Text = dt.Rows[0][2].ToString();
                qEmail.Text = dt.Rows[0][3].ToString();
                qPhone.Text = dt.Rows[0][4].ToString();

            }
        }

        private void AddClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if (qName.Text == null || qLastName.Text == null || qPhone.Text == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string name = qName.Text;
            string lastName = qLastName.Text;
            string email = qEmail.Text;
            string phone = qPhone.Text;

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand($@"Update  customers set (first_name ='{name}',
                                                    last_name='{lastName}', email='{email}','phone='{phone}')",con);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Данные о Клиенте успешно измененны!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                qName.Clear();
                qLastName.Clear();
                qEmail.Clear();
                qPhone.Clear();

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClearClientBtn_Click(object sender, RoutedEventArgs e)
        {
            qName.Clear();
            qLastName.Clear();
            qEmail.Clear();
            qPhone.Clear();
        }
        private void qName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9a-zA-Z\W]$")) { e.Handled = true; }

        }

        private void qEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9\W]$")) { e.Handled = true; }
        }
    
    }
}
