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
using System.Windows.Media.Effects;

namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for AddReservations.xaml
    /// </summary>
    public partial class AddReservations : Window
    {
        public AddReservations()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        BlurEffect blurEffect = new BlurEffect
        {
            Radius = 5
        };
        private void AddClientBtn_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            AddClients aC = new AddClients();
            Blur.workTable.Effect = blurEffect;
            Blur.workTable.IsEnabled = false;
            Blur.workTable.Opacity = 0.5;
            aC.ShowDialog();
            Visibility = Visibility.Visible;
            UpdateClientLoadedDate();
        }

        private void UpdateClientLoadedDate()
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT concat_ws(' ', c.first_name,last_name) ,phone FROM restaurant.customers c;",con);

                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    qClient.Items.Add($"{dr.GetValue(0)}_{dr.GetValue(1)}");
                }
            }
        }
        private void AddReservation_Click(object sender, RoutedEventArgs e)
        {
            if (qClient.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string idClient = ClientsSelectedID(qClient.Text.Split('_')[1]);
            string date = SafeData.dateReservation;
            string idTable = idTables.Text;

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand($@"Insert into reservations 
                (customer_id,table_id,reservation_time) Values('{idClient}','{idTable}','{date}')", con))
                {
                    cmd.ExecuteNonQuery();
                }


                using (MySqlCommand cmdUpdateTable = new MySqlCommand($@"Update tables set  status='резерв' where table_id= '{idTable}'",con))
                {
                    cmdUpdateTable.ExecuteNonQuery();
                }
                MessageBox.Show("Столик забронирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }

       public string ClientsSelectedID(string phone)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand($@"SELECT customer_id FROM restaurant.customers  where phone ='{phone}';", con);

                string id = cmd.ExecuteScalar().ToString();
                return id;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateClientLoadedDate();
            idTables.Text = SafeData.tablesId;
        }


    }
}
