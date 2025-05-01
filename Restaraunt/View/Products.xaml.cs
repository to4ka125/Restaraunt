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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for Products.xaml
    /// </summary>
    public partial class Products : UserControl
    {
        string query = $@"SELECT 
                             products.product_id, 
                             products.name AS 'Наименование', 
                             Concat( products.quantity,' кг.') AS 'Остаток на складе', 
                             CONCAT( products.unit_price, ' руб.') AS 'Цена за кг',
                             supplier.name AS 'Поставщик'                                                       
                             FROM 
                               products
                               INNER JOIN 
                               supplier ON products.supplier_id = supplier.supplier_id 
                            ";

        private int currentPage = 1;
        private const int pageSize = 9;
        private int totalRecords;


        public Products()
        {
            InitializeComponent();
        }
        private void filteringAndSorting()
        {
            string filterText = searchBox.Text;
            int totalCount = GetTotalCount(filterText);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            UpdatePaginationButtons();
            query = @"SELECT products.product_id, 
                     products.name AS 'Наименование',  
                     Concat(products.quantity,' кг.') AS 'Остаток на складе', 
                     CONCAT(products.unit_price, ' руб.') AS 'Цена за кг', 
                     supplier.name AS 'Поставщик'                                                       
              FROM products
              INNER JOIN supplier ON products.supplier_id = supplier.supplier_id";

            string sortOrder = null;
            if (Sorting.SelectedItem != null)
            {
                string selectedSortValue = (Sorting.SelectedItem as ComboBoxItem)?.Content.ToString();
                switch (selectedSortValue)
                {
                    case "По возврастанию":
                        sortOrder = "ORDER BY products.name ASC";
                        break;
                    case "По убыванию":
                        sortOrder = "ORDER BY products.name DESC";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(filterText))
            {
                query += $" WHERE (products.name LIKE '%{filterText}%')";
            }

            if (sortOrder != null)
            {
                query += " " + sortOrder;
            }
     
            UpdateDataGridView(query, 1);
       
        }

        private void UpdateDataGridView(string query, int page)
        {
            query += $@" Limit {(page - 1) * pageSize}, {pageSize}";
            DataTable dataTable = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(MySqlCon.con))
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, connection);
                connection.Open();

                string countQuery = "SELECT COUNT(*) FROM products";


                MySqlCommand countCommand = new MySqlCommand(countQuery, connection);
                totalRecords = Convert.ToInt32(countCommand.ExecuteScalar());

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
        }

        private void UpdatePaginationButtons()
        {
            paginationBar.Children.Clear();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            for (int i = 0; i < totalPages; i++)
            {
                var paginationBtn = new Button
                {
                    Width = 30,
                    Height = 30,
                    Style = (Style)FindResource("PaginationButtonStyle"),
                    Content = (i + 1).ToString(),
                    Margin = new Thickness(0, 0, 10, 0)
                };

                paginationBtn.Click += PaginationBtn_Click;
                paginationBar.Children.Add(paginationBtn);
            }
        }
        private Button selectedPaginationButton;
        private void PaginationBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                selectedPaginationButton?.SetResourceReference(Control.StyleProperty, "PaginationButtonStyle");
                clickedButton.Style = (Style)FindResource("ActivePaginationButtonStyle");
                selectedPaginationButton = clickedButton;

                currentPage = int.Parse(clickedButton.Content.ToString());
                UpdateDataGridView(query, currentPage);
            }
        }

        private int GetTotalCount(string filterText)
        {
            string countQuery = @"SELECT COUNT(*) FROM products";

            if (!string.IsNullOrEmpty(filterText))
            {
                countQuery += $" WHERE (name LIKE '%{filterText}%')";
            }

            using (var connection = new MySqlConnection(MySqlCon.con))
            {
                connection.Open();
                using (MySqlCommand countCommand = new MySqlCommand(countQuery, connection))
                {
                    totalRecords = Convert.ToInt32(countCommand.ExecuteScalar());
                    return totalRecords;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataGridView(query, currentPage);
            UpdatePaginationButtons();
            foreach (var child in paginationBar.Children)
            {
                if (child is Button firstButton && firstButton.Content?.ToString() == "1")
                {
                    firstButton.Style = (Style)FindResource("ActivePaginationButtonStyle");
                    selectedPaginationButton = firstButton;
                    break;
                }
            }
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentPage = 1;
            filteringAndSorting();
        }

        private void Sorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentPage = 1;
            filteringAndSorting();
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                if (selectedPaginationButton != null)
                {
                    selectedPaginationButton.Style = (Style)FindResource("PaginationButtonStyle");
                }

                currentPage -= 1;

                // Находим и стилизуем новую активную кнопку
                foreach (UIElement element in paginationBar.Children)
                {
                    if (element is Button button)
                    {
                        if (button.Content.ToString() == currentPage.ToString())
                        {
                            button.Style = (Style)FindResource("ActivePaginationButtonStyle");
                            selectedPaginationButton = button;
                            break;
                        }
                    }
                }

                UpdateDataGridView(query, currentPage);
            }
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            int maxPage = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (currentPage < maxPage)
            {
                // Сбрасываем стиль текущей активной кнопки
                if (selectedPaginationButton != null)
                {
                    selectedPaginationButton.Style = (Style)FindResource("PaginationButtonStyle");
                }

                // Увеличиваем текущую страницу
                currentPage += 1;

                // Находим и стилизуем новую активную кнопку
                foreach (Button button in paginationBar.Children)
                {
                    if (button.Content.ToString() == currentPage.ToString())
                    {
                        button.Style = (Style)FindResource("ActivePaginationButtonStyle");
                        selectedPaginationButton = button;
                        break;
                    }
                }

                UpdateDataGridView(query, currentPage);
            }
        }
    }
}
