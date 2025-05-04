using MySql.Data.MySqlClient;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for ViewMenuIngredient.xaml
    /// </summary>
    public partial class ViewMenuIngredient : Window
    {
        public ViewMenuIngredient()
        {
            InitializeComponent();
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

                MySqlCommand cmd = new MySqlCommand($@"SELECT  products.name As 'Наименование' , products.unit_price As 'Цена за единицу', CONCAT(menu_ingredients.quantity, ' кг') As 'Грамовки'  From menu_ingredients
                                                                   Inner Join products
                                                                   On menu_ingredients.product_id = products.product_id where menu_ingredients.menu_id = '{SafeData.menuId}'",con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                dataGrid.ItemsSource = dt.DefaultView;

            }
        }
    }
}
