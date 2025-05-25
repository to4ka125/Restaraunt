using Microsoft.Win32;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using Restaraunt.Forms;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;

using System.Data;
using System.IO;
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

        [Obsolete]
        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string query = @"SELECT   products.product_id As 'Номер продукта', 
                             products.name AS 'Наименование', 
                             Concat(products.quantity, ' кг.') AS 'Остаток на складе', 
                             CONCAT(products.unit_price, ' руб.') AS 'Цена за кг',
                             supplier.name AS 'Поставщик'
                             FROM
                               products
                               INNER JOIN
                               supplier ON products.supplier_id = supplier.supplier_id WHERE products.quantity < 5";

            using (MySqlConnection connection = new MySqlConnection(MySqlCon.con))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dataTable = new DataTable();

                try
                {
                    connection.Open();
                    adapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                    return;
                }
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Отчет");
                    string reportTitle = $"Отчет от {DateTime.Now.ToString("dd.MM.yyyy")}, продукты необходимые для закупки";
                    worksheet.Cells[1, 1].Value = reportTitle;
                    worksheet.Cells[1, 1, 1, dataTable.Columns.Count].Merge = true;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    worksheet.Cells[1, 1].Style.Font.Size = 16;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        worksheet.Cells[2, i + 1].Value = dataTable.Columns[i].ColumnName;
                        worksheet.Cells[2, i + 1].Style.Font.Size = 14;
                    }

                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataTable.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 3, j + 1].Value = dataTable.Rows[i][j];
                            worksheet.Cells[i + 3, j + 1].Style.Font.Size = 12;
                        }
                    }
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить отчет";
                    saveFileDialog.FileName = $"Отчет_{DateTime.Now:dd-MM-yyyy}.xlsx";


                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string filePath = saveFileDialog.FileName;
                        FileInfo fileInfo = new FileInfo(filePath);
                        excelPackage.SaveAs(fileInfo);
                        MessageBox.Show("Отчет сохранен: " + filePath);
                    }
                }
            }
        }
        BlurEffect blurEffect = new BlurEffect
        {
            Radius = 5
        };

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Blur.workTable.Effect = blurEffect;
            Blur.workTable.IsEnabled = false;
            Blur.workTable.Opacity = 0.5;
            AddProduct aP = new AddProduct();
            aP.ShowDialog();
            UpdateDataGridView(query, currentPage);
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
                    SafeData.product_id = selectedRow[0].ToString();

                    Blur.workTable.Effect = blurEffect;
                    Blur.workTable.IsEnabled = false;
                    Blur.workTable.Opacity = 0.5;
                    EditProduct eP = new EditProduct();
                    eP.ShowDialog();
                    UpdateDataGridView(query, currentPage);
                    Blur.workTable.Effect = null;
                    Blur.workTable.IsEnabled = true;
                    Blur.workTable.Opacity = 1;
                }
            }
        }
    }
}
