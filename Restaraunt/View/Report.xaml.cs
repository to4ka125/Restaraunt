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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Restaraunt.Utilits;
using MySql.Data.MySqlClient;
using System.Data;
using LiveCharts;
using LiveCharts.Wpf;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : UserControl
    {
        public Report()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ProfitFun();
            DiagramFun();
            AvgPrice();
        }

        private void ProfitFun()
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                //Общая прибыль
                decimal netProfit = 0;
                using (var cmd = new MySqlCommand(
                        @"SELECT IFNULL(SUM(m.price * oi.quantity), 0) 
                          FROM order_items oi
                          JOIN menu m ON oi.menu_id = m.menu_id", con))
                {
                    netProfit = Convert.ToDecimal(cmd.ExecuteScalar());
                    qNetProfit.Text = netProfit.ToString("N2")+ " р.";
                }
                //Чистая прибыль
                decimal clearProfit = 0;
                using (var cmd = new MySqlCommand(
                    @"SELECT IFNULL(SUM(m.price * oi.quantity - 
                        (SELECT IFNULL(SUM(p.unit_price * mi.quantity), 0)
                          FROM menu_ingredients mi
                          JOIN products p ON mi.product_id = p.product_id
                          WHERE mi.menu_id = m.menu_id) * oi.quantity), 0)
                          FROM order_items oi
                          JOIN menu m ON oi.menu_id = m.menu_id", con))
                {
                    clearProfit = Convert.ToDecimal(cmd.ExecuteScalar());
                    qClearProfit.Text = clearProfit.ToString("N2")+" р.";
                }
            }
        }

        private void DiagramFun()
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand($@"SELECT m.name AS 'Блюдо', COUNT(*) AS 'Количество заказов'
                                                            FROM restaurant.orders o
                                                            INNER JOIN order_items oi ON oi.order_id = o.order_id
                                                            INNER JOIN menu m ON m.menu_id = oi.menu_id
                                                            GROUP BY m.name
                                                            ORDER BY COUNT(*) DESC
                                                            LIMIT 5;", con);


                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                
                for(int i = 0; i < dt.Rows.Count; i++)
                {
                    var value = Convert.ToDouble(dt.Rows[i][1]);

                    if (i < diagramm.Series.Count)
                    {
                        var ser = diagramm.Series[i] as PieSeries;

                        if(ser != null)
                        {
                            ser.Values = new ChartValues<double> { value };
                            ser.Title = dt.Rows[i][0].ToString();
                        }
                    }
                }

            }
        }

        private void AvgPrice()
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                decimal avgPrice = 0;
                using (var cmd = new MySqlCommand("select avg(total_price) From orders ",con))
                {
                    avgPrice = Convert.ToDecimal(cmd.ExecuteScalar());
                    qAvgPrice.Text = avgPrice.ToString("N2")+" р.";
                }
            }
        }

        
    }
}
