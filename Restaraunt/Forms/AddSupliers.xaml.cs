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
using MySql.Data.MySqlClient;
using Restaraunt.Utilits;

namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for AddSupliers.xaml
    /// </summary>
    public partial class AddSupliers : Window
    {
        public AddSupliers()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddClientBtn_Click(object sender, RoutedEventArgs e)
        {

            if (qName.Text == null || qLastName.Text == null || qPhone.Text == null || qAdress.Text == null ||qTytle.Text == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                string name = qTytle.Text;
                string firstName = qName.Text;
                string lastName = qLastName.Text;
                string phone = qPhone.Text;
                string adress =qAdress.Text;
                string email = qEmail.Text;


                MySqlCommand cmd = new MySqlCommand($@"Insert into supplier (name,first_name,phone,email,address,last_name)
                                                       Values ('{name}','{firstName}', '{phone}','{email}','{adress}','{lastName}')",con);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Поставщик успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Clear();
            }
        }


        private void Clear()
        {
            qAdress.Clear();
            qEmail.Clear();
            qLastName.Clear();
            qName.Clear();
            qPhone.Clear();
            qTytle.Clear();
        }
        private void ClearClientBtn_Click(object sender, RoutedEventArgs e)
        {
            Clear();

        }

        private void qTytle_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9\W]$")) { e.Handled = true; }
        }

        private void qName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9a-zA-Z\W]$")) { e.Handled = true; }
        }

        private void qLastName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[0-9a-zA-Z\W]$")) { e.Handled = true; }
        }

        private void qEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
