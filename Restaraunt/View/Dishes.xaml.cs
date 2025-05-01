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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for Dishes.xaml
    /// </summary>
    public partial class Dishes : UserControl
    {
        string categoriesId;
        string query = @"SELECT 
                         menu.menu_id, 
                         menu.name AS 'Наименование', 
                         menu.description AS 'Описание', 
                         categories.name AS 'Категория',  
                         CONCAT(menu.price, ' ₽') AS 'Цена',
                         menu.terminalStatus

                         FROM 
                             menu
                         INNER JOIN 
                         categories ON menu.category_id = categories.category_id 
                        ";
        public Dishes()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataGridView(query);
        }
        private void filteringAndSorting()
        {
            query = @"SELECT 
                         menu.menu_id, 
                         menu.name AS 'Наименование', 
                         menu.description AS 'Описание', 
                         categories.name AS 'Категория',  
                         CONCAT(menu.price, ' ₽') AS 'Цена',
                         menu.terminalStatus

                         FROM 
                             menu
                         INNER JOIN 
                         categories ON menu.category_id = categories.category_id 
                        ";

            // Определяем сортировку (по умолчанию - по возрастанию)
            string sortOrder = "ORDER BY menu.name ASC";
            if (Sorting.SelectedItem != null)
            {
                string selectedSortValue = (Sorting.SelectedItem as ComboBoxItem)?.Content.ToString();
                switch (selectedSortValue)
                {
                    case "По возврастанию":
                        sortOrder = "ORDER BY menu.name ASC";
                        break;
                    case "По убыванию":
                        sortOrder = "ORDER BY menu.name DESC";
                        break;
                }
            }
            List<string> whereConditions = new List<string>();

            if (categoriesId != null && categoriesId != "0")
            {
                whereConditions.Add($"categories.category_id = '{categoriesId}'");
            }

            string filterText = searchBox.Text;
            if (!string.IsNullOrEmpty(filterText))
            {
                whereConditions.Add($"(menu.name LIKE '%{filterText}%' OR menu.description LIKE '%{filterText}%')");
            }

            // Добавляем условия WHERE если они есть
            if (whereConditions.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", whereConditions);
            }

            // Добавляем сортировку
            query += " " + sortOrder;

            UpdateDataGridView(query);
        }

        private void UpdateDataGridView(string query)
        {
            DataTable dataTable = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(MySqlCon.con))
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, connection);
                connection.Open();
                try
                {
                    dataAdapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при извлечении данных: {ex.Message}");
                }
            }
            dataGrid.ItemsSource = dataTable.DefaultView;
            dataGrid.Columns[0].Visibility = Visibility.Hidden;
            dataGrid.Columns[5].Visibility = Visibility.Hidden;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;
            categoriesId = radioBtn.Uid;
            filteringAndSorting();
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filteringAndSorting();
        }

        private void Sorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filteringAndSorting();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddDishes aD = new AddDishes();
            aD.ShowDialog();
        }
    }
}
