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
    /// Interaction logic for Supplier.xaml
    /// </summary>
    public partial class Supplier : UserControl
    {
        public Supplier()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Blur.workTable.Effect = blurEffect;
            Blur.workTable.IsEnabled = false;
            Blur.workTable.Opacity = 0.5;
            AddSupliers aS = new AddSupliers();
            Timer.idleTimer.Stop();
            aS.ShowDialog();
            Timer.idleTimer.Start();
            UpdateDataGrid();
            Blur.workTable.Effect = null;
            Blur.workTable.IsEnabled = true;
            Blur.workTable.Opacity = 1;
        }
        BlurEffect blurEffect = new BlurEffect
        {
            Radius = 5
        };
        private void EditBtnClick_Click(object sender, RoutedEventArgs e)
        {

            if (dataGrid.SelectedItem != null)
            {
                var selectedRow = dataGrid.SelectedItem as DataRowView;

                if (selectedRow!=null)
                {
                    SafeData.supliers_id = selectedRow[0].ToString();

                    Blur.workTable.Effect = blurEffect;
                    Blur.workTable.IsEnabled = false;
                    Blur.workTable.Opacity = 0.5;
                    EditSupliers eS = new EditSupliers();
                    Timer.idleTimer.Stop();
                    eS.ShowDialog();
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

                    MySqlCommand cmd = new MySqlCommand(@"SELECT supplier_id, name As 'Наименование', concat(last_name,' ', left(first_name,1),'.') As 'Фио', 
                                                          phone As 'Телефон', email As 'Почта', address As 'Адресс'
                                                          FROM restaurant.supplier;", con);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);


                    foreach (DataRow row in dt.Rows)
                    {
                        string phone = row["Телефон"].ToString();
                        if (phone.Length > 6)
                        {
                            string lastFiveDigits = phone.Substring(phone.Length - 5);
                            string maskedPhone = "+7" + new string('*', phone.Length - 6) + lastFiveDigits;

                            row["Телефон"] = maskedPhone;
                        }

                        else
                        {
                            row["Телефон"] = new string('*', phone.Length);
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
    }
}
