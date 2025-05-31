using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for EditSupliers.xaml
    /// </summary>
    public partial class EditSupliers : Window
    {
        public EditSupliers()
        {
            InitializeComponent();
        }

        private void AddClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(qName.Text) ||
                string.IsNullOrWhiteSpace(qLastName.Text) ||
                string.IsNullOrWhiteSpace(qPhone.Text) ||
                string.IsNullOrWhiteSpace(qAdress.Text) ||
                string.IsNullOrWhiteSpace(qTytle.Text)) { 
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
                string adress = qAdress.Text;
                string email = qEmail.Text;


                MySqlCommand cmd = new MySqlCommand($@"Update supplier set name = '{name}', first_name ='{firstName}',
                phone='{phone}', email ='{email}', address='{adress}', last_name='{lastName}'", con);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Данные поставщика усрешно измененны!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MySqlDataAdapter da = new MySqlDataAdapter($@"SELECT * FROM restaurant.supplier where supplier_id ='{SafeData.supliers_id}'" ,con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                qTytle.Text = dt.Rows[0][1].ToString();
                qName.Text = dt.Rows[0][2].ToString();
                qPhone.Text = dt.Rows[0][3].ToString();
                qEmail.Text = dt.Rows[0][4].ToString();
                qAdress.Text = dt.Rows[0][5].ToString();
                qLastName.Text = dt.Rows[0][6].ToString();
            }
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
    }
}
