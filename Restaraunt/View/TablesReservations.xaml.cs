using MySql.Data.MySqlClient;
using Restaraunt.Model;
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
using Restaraunt.Forms;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for TablesReservations.xaml
    /// </summary>
    public partial class TablesReservations : UserControl
    {
        private string statusTables = "0";

        BlurEffect blurEffect = new BlurEffect
        {
            Radius = 5
        };

        public TablesReservations()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Метод для загрузки столов
        /// </summary>
        public void TablesPopulateGrid()
        {
            UpdateTableReservationStatus();

            DataTable dt = new DataTable();
            string query;

            if (statusTables == "0")
            {
                query = @"SELECT CONCAT('Стол ', table_id) AS table_name, 
                                                      status FROM restaurant.tables;";
            }
            else
            {
                query = $@"SELECT CONCAT('Стол ', table_id) AS table_name, 
                                                      status FROM restaurant.tables where  status ='{statusTables}';";
            }

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, con);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                    return;
                }
            }

            TablesConteiner.Children.Clear();
            TablesConteiner.RowDefinitions.Clear();
            TablesConteiner.ColumnDefinitions.Clear();

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных о столах");
                return;
            }

            int columnCount = 3;
            int rowCount = (int)Math.Ceiling((double)dt.Rows.Count / columnCount);

            for (int i = 0; i < columnCount; i++)
            {
                TablesConteiner.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }

            for (int i = 0; i < rowCount; i++)
            {
                TablesConteiner.RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var table = new Tables
                {
                    Title = dt.Rows[i]["table_name"].ToString(),
                    Status = dt.Rows[i]["status"].ToString(),
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    
                };
           
                if (table.Status == "резерв")
                {
                    var contextMenu = new ContextMenu();
                    var menuItem = new MenuItem { Header = "Отменить" };
                    menuItem.Click += (s, e) => ChangeTableStatus(table.Title.Split(' ')[1].ToString());
                    contextMenu.Items.Add(menuItem);
                    table.ContextMenu = contextMenu;
                }
              


                table.MouseDoubleClick += Tables_MouseDoubleClick;
                Grid.SetRow(table, i / columnCount);
                Grid.SetColumn(table, i % columnCount);
                TablesConteiner.Children.Add(table);
            }
        }
        public void ChangeTableStatus(string idTable)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand($@"Update restaurant.tables set status = 'свободно' where table_id ='{idTable}';
                                                       Update restaurant.reservations set status = 'Отменена' where table_id='{idTable}'",con);

                cmd.ExecuteNonQuery();
                MessageBox.Show(
           "Бронирование успешно отменено",
           "Информация",
           MessageBoxButton.OK,
           MessageBoxImage.Information
       );
            }
        }
        public void UpdateTableReservationStatus()
        {
            string updateQuery;
            if (qDate.SelectedDate == null)
            {
                updateQuery = @"UPDATE restaurant.tables t
SET status = 'резерв'
WHERE EXISTS (
    SELECT 1 
    FROM restaurant.reservations r
    WHERE r.table_id = t.table_number
    AND r.status = 'Активна'
    AND DATE(r.reservation_time) = CURRENT_DATE
);

UPDATE restaurant.tables t
JOIN restaurant.reservations r ON r.table_id = t.table_number AND r.status = 'Завершена' AND DATE(r.reservation_time) = CURRENT_DATE
SET t.status = 'свободно'
WHERE t.table_number > 0 AND t.status = 'резерв';

UPDATE restaurant.tables t
SET t.status = 'свободно'
WHERE t.table_number > 0 
AND NOT EXISTS (
    SELECT 1 
    FROM restaurant.reservations r
    WHERE r.table_id = t.table_number
    AND DATE(r.reservation_time) = CURRENT_DATE
)
AND t.status != 'занят';

UPDATE restaurant.tables t
SET status = CASE 
    WHEN EXISTS (
        SELECT 1 
        FROM restaurant.orders o
        WHERE o.table_number = t.table_number
        AND o.status = 'В обработке'
        AND DATE(o.order_time) = CURRENT_DATE
    ) THEN 'занят'
    
    WHEN EXISTS (
        SELECT 1 
        FROM restaurant.reservations r
        WHERE r.table_id = t.table_number
        AND r.status = 'Активна'
        AND DATE(r.reservation_time) = CURRENT_DATE
    ) THEN 'резерв'
    
    WHEN EXISTS (
        SELECT 1 
        FROM restaurant.orders o
        WHERE o.table_number = t.table_number
        AND o.status IN ('Завершен', 'Отменен')
        AND DATE(o.order_time) = CURRENT_DATE
    ) THEN 'свободно'
    
    WHEN NOT EXISTS (
        SELECT 1
        FROM restaurant.orders o
        WHERE o.table_number = t.table_number
        AND DATE(o.order_time) = CURRENT_DATE
    ) AND NOT EXISTS (
        SELECT 1
        FROM restaurant.reservations r
        WHERE r.table_id = t.table_number
        AND DATE(r.reservation_time) = CURRENT_DATE
    ) THEN 'свободно'
    
    ELSE status
END
WHERE t.table_number > 0;";
            }
            else
            {
                var date = qDate.SelectedDate.Value.ToString("yyyy-MM-dd");
                updateQuery = $@"
UPDATE restaurant.tables t
SET status = 'резерв'
WHERE EXISTS (
    SELECT 1 
    FROM restaurant.reservations r
    WHERE r.table_id = t.table_number
    AND r.status = 'Активна'
    AND DATE(r.reservation_time) = '{date}'
);

UPDATE restaurant.tables t
JOIN restaurant.reservations r ON r.table_id = t.table_number AND r.status = 'Завершена' AND DATE(r.reservation_time) = CURRENT_DATE
SET t.status = 'свободно'
WHERE t.table_number > 0 AND t.status = 'резерв';

UPDATE restaurant.tables t
SET t.status = 'свободно'
WHERE t.table_number > 0 
AND NOT EXISTS (
    SELECT 1 
    FROM restaurant.reservations r
    WHERE r.table_id = t.table_number
    AND DATE(r.reservation_time) = '{date}'
)
AND t.status != 'занят';

UPDATE restaurant.tables t
SET status = CASE 
    WHEN EXISTS (
        SELECT 1 
        FROM restaurant.orders o
        WHERE o.table_number = t.table_number
        AND o.status = 'В обработке'
        AND DATE(o.order_time) = '{date}'
    ) THEN 'занят'
    
    WHEN EXISTS (
        SELECT 1 
        FROM restaurant.reservations r
        WHERE r.table_id = t.table_number
        AND r.status = 'Активна'
        AND DATE(r.reservation_time) = '{date}'
    ) THEN 'резерв'
    
    WHEN EXISTS (
        SELECT 1 
        FROM restaurant.orders o
        WHERE o.table_number = t.table_number
        AND o.status IN ('Завершен', 'Отменен')
        AND DATE(o.order_time) = '{date}'
    ) THEN 'свободно'
    
    WHEN NOT EXISTS (
        SELECT 1
        FROM restaurant.orders o
        WHERE o.table_number = t.table_number
        AND DATE(o.order_time) = '{date}'
    ) AND NOT EXISTS (
        SELECT 1
        FROM restaurant.reservations r
        WHERE r.table_id = t.table_number
        AND DATE(r.reservation_time) = '{date}'
    ) THEN 'свободно'
    
    ELSE status
END
WHERE t.table_number > 0;
";

            }


            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(updateQuery, con);
                    cmd.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Ошибка при обновлении статусов столов: " + ex.Message);
                }
            }
        }

        private void Tables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Tables selectedTable)
            {
                if (selectedTable.Status == "свободно")
                {
                    SafeData.tablesId = selectedTable.Title.Split(' ')[1].ToString();
                    Blur.workTable.Effect = blurEffect;
                    Blur.workTable.IsEnabled = false;
                    Blur.workTable.Opacity = 0.5;
                    AddReservations aR = new AddReservations();
                    SafeData.dateReservation = qDate.SelectedDate.Value.ToString("yyyy-MM-dd");
//Timer.idleTimer.Stop();
                    aR.ShowDialog();
                   // Timer.idleTimer.Start();
                    TablesPopulateGrid();
                    Blur.workTable.Effect = null;
                    Blur.workTable.IsEnabled = true;
                    Blur.workTable.Opacity = 1;
                }
                else
                {
                    MessageBox.Show("Выберите свободный стол", "Сообщение пользователю", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;

            switch (radioBtn.Uid)
            {
                case "all":
                    statusTables = "0";
                    break;
                case "Freely":
                    statusTables = "свободно";
                    break;
                case "Reserve":
                    statusTables = "резерв";
                    break;
                case "Occupied":
                    statusTables = "занят";
                    break;
            }
            TablesPopulateGrid();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            qDate.Text = DateTime.Now.ToString();
            SafeData.dateReservation = qDate.SelectedDate.Value.ToString("yyyy-MM-dd");
            TablesPopulateGrid();
        }
        private void qDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TablesPopulateGrid();
        }
    }
}
