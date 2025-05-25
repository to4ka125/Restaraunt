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
    /// Interaction logic for User.xaml
    /// </summary>
    public partial class User : UserControl
    {
        public User()
        {
            InitializeComponent();
        }

        private void UpdateDataGrid()
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(@"Select user_id, concat(lastName,' ',Left(name,1), '.') As 'ФИО',login As 'Логин', role As 'Роль', 
                                                           email As 'Почта', phone As 'Телефон' From users ", con);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);


                    foreach (DataRow row in dt.Rows)
                    {
                        string email = row["Почта"].ToString();
                        string phone = row["Телефон"].ToString();
                        string login = row["Логин"].ToString();


                        /*
                        if (name.Length>2)
                        {
                            row["Имя"] = name.Substring(0, 2) + new string('*', name.Length+2);
                        }
                        else
                        {
                            row["Имя"] = new string('*', name.Length+2);
                        }


                        if (lastName.Length>3)
                        {
                            row["Фамилия"] = lastName.Substring(0, 3) + new string('*', lastName.Length + 3);
                        }
                        else
                        {
                            row["Фамилия"] = new string('*', lastName.Length);
                        }


                        */

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


                        if (login.Length > 4)
                        {
                            row["Логин"] = login.Substring(0, 4) + new string('*', login.Length + 4);
                        }
                        else
                        {
                            row["Логин"] = new string('*', login.Length);
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
            AddUsers aU = new AddUsers();
            aU.ShowDialog();
            UpdateDataGrid();
            Blur.workTable.Effect = null;
            Blur.workTable.IsEnabled = true;
            Blur.workTable.Opacity = 1;
        }

        private void EditBtnClick_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var selectedRow = dataGrid.SelectedItem as DataRowView;

                if (selectedRow != null)
                {
                    SafeData.userIdEdit = selectedRow[0].ToString();

                    Blur.workTable.Effect = blurEffect;
                    Blur.workTable.IsEnabled = false;
                    Blur.workTable.Opacity = 0.5;
                    EditUser eU = new EditUser();
                    eU.ShowDialog();
                    UpdateDataGrid();
                    Blur.workTable.Effect = null;
                    Blur.workTable.IsEnabled = true;
                    Blur.workTable.Opacity = 1;
                }
            }
        }
    }
}
