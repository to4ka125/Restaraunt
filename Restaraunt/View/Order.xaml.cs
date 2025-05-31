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
using Restaraunt.Model;
using Restaraunt.Utilits;
using MySql.Data.MySqlClient;
using System.Data;
using Restaraunt.Forms;
using System.Windows.Media.Effects;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for Order.xaml
    /// </summary>
    public partial class Order : UserControl
    {
        private string statusTables = "0";

        BlurEffect blurEffect = new BlurEffect
        {
            Radius = 5
        };
        public Order()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Метод для окраски шагов
        /// </summary>
        public void PhaseElips()
        {
            int step = SafeData.step;

            step2.Fill = Brushes.Transparent;
            step3.Fill = Brushes.Transparent;
            step4.Fill = Brushes.Transparent;
            step5.Fill = Brushes.Transparent;

            SolidColorBrush completedColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F85D5D")); // Цвет для завершенных шагов
            SolidColorBrush currentColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FDC5C5")); // Цвет для текущего шага
            SolidColorBrush colorText = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF")); // Цвет для текущего шага

            var steps = new[] { step1, step2, step3, step4, step5 };
            var stepsText = new[] { step1Text, step2Text, step3Text, step4Text, step5Text };

            for (int i = 0; i < steps.Length; i++)
            {
                if (i <= step)
                {
                    steps[i].Fill = completedColor;
                    stepsText[i].Foreground = colorText;
                }
                else if (i == step + 1)
                {
                    steps[i].Fill = currentColor;
                    stepsText[i].Foreground = colorText;
                }
            }
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
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                table.MouseDoubleClick += Tables_MouseDoubleClick;
                Grid.SetRow(table, i / columnCount);
                Grid.SetColumn(table, i % columnCount);
                TablesConteiner.Children.Add(table);
            }
        }
        public void UpdateTableReservationStatus()
        {
          string  updateQuery = @"UPDATE restaurant.tables t
                                SET status = 'резерв'
                                WHERE EXISTS (
                                    SELECT 1 
                                    FROM restaurant.reservations r
                                    WHERE r.table_id = t.table_number
                                    AND r.status = 'Активна'
                                    AND DATE(r.reservation_time) = CURRENT_DATE
                                );
            
             
                          UPDATE restaurant.tables t
	                    JOIN restaurant.reservations r ON r.table_id = t.table_number
	                    SET t.status = 'свободно'
	                    WHERE r.status = 'Завершена'
	                    AND DATE(r.reservation_time) = CURRENT_DATE
	                    AND t.status = 'резерв';

                               UPDATE restaurant.tables t
                               SET t.status = 'свободно'
                               WHERE t.table_id > 0 
                               AND NOT EXISTS (
                                   SELECT 1 
                                   FROM restaurant.reservations r
                                   WHERE r.table_id = t.table_id
                                   AND DATE(r.reservation_time) = CURRENT_DATE
                                   )AND t.status != 'занят';
          
                        UPDATE restaurant.tables t
                    SET status = CASE 
                        WHEN EXISTS (
                            SELECT 1 
                            FROM restaurant.reservations r
                            WHERE r.table_id = t.table_number
                            AND r.status = 'Активна'
                            AND DATE(r.reservation_time) = CURRENT_DATE
                        ) THEN t.status  -- Не меняем статус, если есть активная резервация
    
                        WHEN EXISTS (
                            SELECT 1 
                            FROM restaurant.orders o
                            WHERE o.table_number = t.table_number
                            AND o.status IN ('Завершен', 'Отменен')
                            AND DATE(o.order_time) = CURRENT_DATE
                        ) THEN 'свободно'
    
                        WHEN EXISTS (
                            SELECT 1 
                            FROM restaurant.orders o
                            WHERE o.table_number = t.table_number
                            AND o.status = 'В обработке'
                            AND DATE(o.order_time) = CURRENT_DATE
                        ) THEN 'занят'
    
                        WHEN NOT EXISTS (
                            SELECT 1
                            FROM restaurant.orders o
                            WHERE o.table_number = t.table_number
                            AND DATE(o.order_time) = CURRENT_DATE
                        ) THEN 'свободно'
    
                        ELSE status
                    END
                    WHERE t.table_number > 0;";
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(updateQuery, con);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при обновлении статусов столов: " + ex.Message);
                }
            }
        }

        public string GetNameReservationCustomer()
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                MySqlDataAdapter da = new MySqlDataAdapter($@"SELECT Concat_ws(' ',first_name, last_name) FROM restaurant.reservations r
                                                            inner join customers c on 
                                                            c.customer_id = r.customer_id 
                                                            where reservation_time = current_date() and table_id = '{SafeData.tablesId}'",con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt.Rows[0][0].ToString();
            }
        }
        private void Tables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Tables selectedTable)
            {
                if (selectedTable.Status == "свободно" || selectedTable.Status == "резерв")
                {
                   
                    SafeData.step = 1;
                    SafeData.tablesId = selectedTable.Title.Split(' ')[1].ToString();
                    if (selectedTable.Status == "резерв")
                    {
                        SafeData.isReservTable = true;
                        qPhoneNumber.Text = GetNameReservationCustomer();
                        qPhoneNumber.Visibility = Visibility.Visible;
                        addClients.Visibility = Visibility.Collapsed;
                        containerRadioBtnSearchClients.Visibility = Visibility.Collapsed;
                        SearchClientsBox.Visibility = Visibility.Collapsed;
                    }
                    AddTables.Visibility = Visibility.Collapsed;
                    ChoosingPaymentMethod.Visibility = Visibility.Visible;
                    PhaseElips();
                }
          
                else
                {
                    MessageBox.Show("Выберите свободный стол", "Сообщение пользователю", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        /// <summary>
        /// Метод для загрузки менюы
        /// </summary>
        private void LoadedMenu()
        {
            DataTable dt = new DataTable();
            string query;
            ImageSource image;

            if (SafeData.categoriesId == "0")
            {
                query = @"SELECT menu_id, name,description, concat_ws(' ', price, 'р.') as 'price', Image FROM Menu 
                 WHERE terminalStatus = 'Показать'";
            }
            else
            {
                query = $@"SELECT menu_id, name, description, price, Image FROM Menu 
                  WHERE category_id = '{SafeData.categoriesId}' 
                  AND terminalStatus = 'Показать'";
            }

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, con);
                    da.Fill(dt);

                    menuContainer.Children.Clear();
                    menuContainer.RowDefinitions.Clear();
                    menuContainer.ColumnDefinitions.Clear();


                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Нет данных о меню", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    int columnCount = 4;
                    int rowCount = (int)Math.Ceiling((double)dt.Rows.Count / columnCount);

                    for (int i = 0; i < columnCount; i++)
                    {
                        menuContainer.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });
                    }

                    for (int i = 0; i < rowCount; i++)
                    {
                        menuContainer.RowDefinitions.Add(new RowDefinition
                        {
                            Height = GridLength.Auto
                        });
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        image = new BitmapImage(new Uri($"/Images/ImagesMenu/{dt.Rows[i]["Image"]}", UriKind.RelativeOrAbsolute));
                        var dishes = new DishesBlock
                        {
                            Source = image,
                            Tytle = dt.Rows[i]["name"].ToString(),
                            ID = dt.Rows[i]["menu_id"].ToString(),
                            Description = dt.Rows[i]["description"].ToString(),
                            Order = dt.Rows[i]["price"].ToString(),
                            Margin = new Thickness(10),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        };

                        Grid.SetRow(dishes, i / columnCount);
                        Grid.SetColumn(dishes, i % columnCount);
                        menuContainer.Children.Add(dishes);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                    return;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SafeData.step = 0;
            TablesPopulateGrid();
        }

        /// <summary>
        /// Метод скрытия формы поиск гостя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;

            radioBtn.IsChecked = true;
            if (radioBtn.Name == "radioSearchBtn")
            {
                SearchClientsBox.Visibility = Visibility.Visible;
                SafeData.isCustomerBoolCheck = true;
            }
            else
            {
                SearchClientsBox.Visibility = Visibility.Hidden;
                SafeData.isCustomerBoolCheck = false;
            }
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SafeData.step = 2;
            if(SafeData.isCustomerBoolCheck)
            {
                if (string.IsNullOrWhiteSpace(qPhoneNumber.Text))
                {
                    MessageBox.Show("Выберете гостя", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                SafeData.customer_id = GetIdCustomer(qPhoneNumber.Text);
            }
            else
            {
                SafeData.customer_id = null;
            }
            Menu.Visibility = Visibility.Visible;
            ChoosingPaymentMethod.Visibility = Visibility.Collapsed;
            PhaseElips();
            LoadedMenu();
        }

        private string GetIdCustomer (string name)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand($@"SELECT customer_id FROM restaurant.customers where concat_ws(' ',first_name,last_name) = '{name}';",con);


                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                string id = dt.Rows[0][0].ToString();

                return id;
            }
        }

        /// <summary>
        /// Метод для сортирови блюд
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;
            SafeData.categoriesId = radioBtn.Uid;
            LoadedMenu();
        }

        /// <summary>
        /// Метод открытия формы чека
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            Blur.workTable.Effect = blurEffect;
            Blur.workTable.IsEnabled = false;
            Blur.workTable.Opacity = 0.5;
            CheckOrder cO = new CheckOrder();
            cO.ShowDialog();
            Blur.workTable.Effect = null;
            Blur.workTable.IsEnabled = true;
            Blur.workTable.Opacity = 1;

            if (SafeData.dishesAddBool)
            {
                TablesPopulateGrid();
                UpdateTableReservationStatus();
                SafeData.step = 0;
                PhaseElips();
                AddTables.Visibility = Visibility.Visible;
                Menu.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Метод для сортировки столов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Click_2(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Метод выбора способа оплаты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Click_3(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;
            SafeData.idpayment_method = radioBtn.Uid;
       
        }

        /// <summary>
        /// Метод для поиска гостя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (phoneNumber.Text == null)
            {
                MessageBox.Show("Введите номер телефона для поиска", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string searchPhone = phoneNumber.Text;

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();
                MySqlDataAdapter da = new MySqlDataAdapter($@"SELECT concat_ws(' ',first_name, last_name) 
                                                              FROM restaurant.customers where phone = '{searchPhone}';", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count != 0)
                {
                    MessageBox.Show("Гостя с данным номером телефона найден", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    qPhoneNumber.Text = dt.Rows[0][0].ToString();
                    qPhoneNumber.Visibility = Visibility.Visible;
                    addClients.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Гостя с данным номером телефона не существует", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    qPhoneNumber.Visibility = Visibility.Collapsed;
                    phoneNumber.Text = null;
                    addClients.Visibility = Visibility.Visible;
                }
            }

        }

        private void addClients_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
