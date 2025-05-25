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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using Restaraunt.Utilits;


namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for ViewOrderStructure.xaml
    /// </summary>
    public partial class ViewOrderStructure : Window
    {
        public ViewOrderStructure()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand($@"SELECT order_id, m.name As 'Наименование блюда',
                                                           oi.quantity As 'Количество'
                                                                            FROM restaurant.order_items oi 
                                                                            inner join menu m on m.menu_id = oi.menu_id
                                                                            where order_id = '{SafeData.orderId}'; ", con))
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGrid.ItemsSource = dt.DefaultView;

                    dataGrid.Columns[0].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
