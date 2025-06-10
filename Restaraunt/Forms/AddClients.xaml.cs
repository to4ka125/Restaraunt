using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AddClients.xaml
    /// </summary>
    public partial class AddClients : Window
    {
        public AddClients()
        {
            InitializeComponent();
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
            MessageBox.Show("Поля успешно очищенны!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(qName.Text) ||
                string.IsNullOrWhiteSpace(qLastName.Text) ||
                string.IsNullOrWhiteSpace(qPhone.Text))
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
                try
                {
                    con.Open();

                    using (MySqlCommand checkCmd = new MySqlCommand($@"SELECT COUNT(*) FROM restaurant.customers WHERE phone = '{phone}'", con))
                    {
                        int phoneExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (phoneExists > 0)
                        {
                            MessageBox.Show("Клиент с таким номером телефона уже существует!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }


                    MySqlCommand cmd = new MySqlCommand($@"Insert into restaurant.customers (first_name,last_name,email,phone,registration_date) 
                Values ('{name}','{lastName}','{email}','{phone}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')", con);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Клиент успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    qName.Clear();
                    qLastName.Clear();
                    qEmail.Clear();
                    qPhone.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void qName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9a-zA-Z\W]$")) { e.Handled = true; }
         
        }

        private void qEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[а-я-А-Я]$")) { e.Handled = true; }
        }
    }
}
