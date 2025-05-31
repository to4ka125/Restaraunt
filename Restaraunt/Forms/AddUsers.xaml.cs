using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for AddUsers.xaml
    /// </summary>
    public partial class AddUsers : Window
    {
        public AddUsers()
        {
            InitializeComponent();
        }
        private bool UserExists(string login)
        {
            MySqlConnection con = new MySqlConnection(MySqlCon.con);
            con.Open();
            MySqlCommand cmd = new MySqlCommand($@"SELECT COUNT(*) FROM users WHERE login = '{login}'", con);

            int count = Convert.ToInt32(cmd.ExecuteScalar());

            con.Close();

            return count > 0;
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(qName.Text) ||
                string.IsNullOrWhiteSpace(qLastName.Text) ||
                string.IsNullOrWhiteSpace(qLogin.Text) ||
                string.IsNullOrWhiteSpace(qPassword.Password)||
                qRole.SelectedItem == null)

            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string login = qLogin.Text;
            if (UserExists(login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();

                    string name = qName.Text;
                    string lastName = qLastName.Text;

                    string password = GetHashPass(qPassword.Password);
                    string role = qRole.Text;
                    string email = qEmail.Text;
                    string phone = qPhone.Text;

                    MySqlCommand cmd = new MySqlCommand($@"Insert into restaurant.users (name, lastName, login, password, role, email, phone)
                                                          Values ('{name}','{lastName}', '{login}','{password}','{role}','{email}','{phone}')", con);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Пользователь добавлен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);


                    qName.Clear();
                    qLastName.Clear();
                    qLogin.Clear();
                    qPassword.Clear();
                    qRole.Text = "";
                    qEmail.Clear();
                    qPhone.Clear();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка-{ex.Message}"); ;
                }
            }
        }

        public static string GetHashPass(string password)
        {

            byte[] bytesPass = Encoding.UTF8.GetBytes(password);

            SHA256Managed hashstring = new SHA256Managed();

            byte[] hash = hashstring.ComputeHash(bytesPass);

            string hashPasswd = string.Empty;

            foreach (byte x in hash)
            {
                hashPasswd += String.Format("{0:x2}", x);
            }

            hashstring.Dispose();

            return hashPasswd;
        }
        private void ClearDishes_Click(object sender, RoutedEventArgs e)
        {
            qName.Clear();
            qLastName.Clear();
            qLogin.Clear();
            qPassword.Clear();
            qRole.Text = "";
            qEmail.Clear();
            qPhone.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void qName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9\W]$")) { e.Handled = true; }
            if (Regex.IsMatch(e.Text, @"^[A-Za-z]$")) { e.Handled = true; }
            if (Regex.IsMatch(e.Text, @"^[_]$")) { e.Handled = true; }
        }

        private void qEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[а-яА-Я \W]$")) { e.Handled = true; }
            if (Regex.IsMatch(e.Text, @"^[_]$")) { e.Handled = false; }
            if (Regex.IsMatch(e.Text, @"^[@]$")) { e.Handled = false; }
        }

        private void qPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[а-яА-ЯA-Za-z \W]$")) { e.Handled = true; }
            if (Regex.IsMatch(e.Text, @"^[_]$")) { e.Handled = true; }
        }

        private void qLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[а-яА-Я \W]$")) { e.Handled = true; }
            if (Regex.IsMatch(e.Text, @"^[_]$")) { e.Handled = false; }
        }

        private void qPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)qPassword.Template.FindName("textBlock", qPassword);

            if (qPassword.Password.Length > 0)
            {
                textBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                textBlock.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            string Passwd = CreatePassword(10);
            MessageBox.Show($"Сгенерированный пароль: {Passwd}",
               "Пароль сгенерирован",
               MessageBoxButton.OK,
               MessageBoxImage.Information);
            qPassword.Password = Passwd;
        }
    }
}
