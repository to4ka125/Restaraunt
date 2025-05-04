using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for EditUser.xaml
    /// </summary>
    public partial class EditUser : Window
    {
        public EditUser()
        {
            InitializeComponent();
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
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (qName.Text == null || qLastName.Text == null
             || qRole.SelectedItem == null || qLogin.Text == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                string query;

                if (qPassword.Password == "")
                {
                    query = $@"Update 
                               users set name ='{qName.Text}',
                               lastName = '{qLastName.Text}',
                               login = '{qLogin.Text}',
                               role = '{qRole.Text}', 
                               email ='{qEmail.Text}',
                               phone = '{qPhone.Text}'
                               where user_id='{SafeData.userIdEdit}'";
                }
                else
                {
                    query = $@"Update 
                               users set name ='{qName.Text}',
                               lastName = '{qLastName.Text}', 
                               login = '{qLogin.Text}',
                               password ='{GetHashPass(qPassword.Password)}',
                               role = '{qRole.Text}', 
                               email = '{qEmail.Text}', 
                               phone = '{qPhone.Text}' 
                               where user_id='{SafeData.userIdEdit}'";
                }

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {


                    if (MessageBox.Show("Вы хотите изменить пользователя?", "Информация", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        MessageBox.Show($"Пользователь под логином: {qLogin.Text} был успешно изменен.", "Успешное изменение",
                                           MessageBoxButton.OK,
                                             MessageBoxImage.Information);
                        cmd.ExecuteNonQuery();
                        this.Close();
                    }


                }
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand($"Select * From users Where user_id = '{SafeData.userIdEdit}'", con))
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    qName.Text = dt.Rows[0].ItemArray[1].ToString();
                    qLastName.Text = dt.Rows[0].ItemArray[2].ToString();
                    qLogin.Text = dt.Rows[0].ItemArray[3].ToString();
                    qRole.Text = dt.Rows[0].ItemArray[5].ToString();
                    qPhone.Text = dt.Rows[0].ItemArray[7].ToString();
                    qEmail.Text = dt.Rows[0].ItemArray[6].ToString();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void ClearUser_Click(object sender, RoutedEventArgs e)
        {

        }
    }

  
}
