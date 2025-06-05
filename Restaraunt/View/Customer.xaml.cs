using MySql.Data.MySqlClient;
using Restaraunt.Forms;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for Customer.xaml
    /// </summary>
    public partial class Customer : UserControl
    {
        public Customer()
        {
            InitializeComponent();
        }

        private void EditBtnClick_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var selectedRow = dataGrid.SelectedItem as DataRowView;

                if (selectedRow != null)
                {
                    SafeData.customer_id = selectedRow[0].ToString();
                    Blur.workTable.Effect = blurEffect;
                    Blur.workTable.IsEnabled = false;
                    Blur.workTable.Opacity = 0.5;
                    EditClients eC = new EditClients();
                    Timer.idleTimer.Stop();
                    eC.ShowDialog();
                    Timer.idleTimer.Start();
                    UpdateDataGrid();
                    Blur.workTable.Effect = null;
                    Blur.workTable.IsEnabled = true;
                    Blur.workTable.Opacity = 1;
                }
            }
        }
        private void UpdateDataGrid()
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(@"SELECT customer_id, 
                                                        concat(last_name,' ',Left(first_name,1),'.')
                                                        AS 'ФИО',email AS 'Почта',
                                                        phone As 'Телефон' FROM restaurant.customers;", con);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);


                    foreach (DataRow row in dt.Rows)
                    {
                        string email = row["Почта"].ToString();
                        string phone = row["Телефон"].ToString();

                        if (phone.Length > 6)
                        {
                            string lastFiveDigits = phone.Substring(phone.Length - 5);
                            string maskedPhone = "+7" + new string('*', phone.Length - 6) + lastFiveDigits;

                            row["Телефон"] = maskedPhone;
                        }
                    }
                    dataGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }

                dataGrid.Columns[0].Visibility = Visibility.Collapsed;
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataGrid();
        }
        BlurEffect blurEffect = new BlurEffect
        {
            Radius = 5
        };
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Blur.workTable.Effect = blurEffect;
            Blur.workTable.IsEnabled = false;
            Blur.workTable.Opacity = 0.5;
            AddClients aC = new AddClients();
            Timer.idleTimer.Stop();
            aC.ShowDialog();
            Timer.idleTimer.Start();
            UpdateDataGrid();
            Blur.workTable.Effect = null;
            Blur.workTable.IsEnabled = true;
            Blur.workTable.Opacity = 1;
        }
    }
}
