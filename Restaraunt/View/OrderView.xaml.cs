using Microsoft.Office.Interop.Word;
using MySql.Data.MySqlClient;
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
using Word= Microsoft.Office.Interop.Word;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
      //  string query = @"SELECT order_id As 'Номер заказа', concat(us.lastName,' ',Left(us.name,1),'.') AS 'ФИО', o.table_number As 'Номер стола',
      //                   o.status As 'Статус', o.order_time As 'Дата заказа', Concat (total_price, ' ₽') As 'Стоимость заказа' From orders o
       //                  inner Join users us On us.user_id  = o.user_id";
        string id;

        string status="all";


        private readonly string FileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", "check.docx");
        public OrderView()
        {
            InitializeComponent();
        }

        private void filteringAndSorting()
        {
            string query = @"SELECT order_id As 'Номер заказа', us.name As 'Имя официанта', 
                         us.lastName As 'Фамилия официанта', o.table_number As 'Номер стола',
                         o.status As 'Статус', o.order_time As 'Дата заказа', Concat (total_price, ' ₽') As 'Стоимость заказа' From orders o
                         inner Join users us On us.user_id  = o.user_id";

            bool hasWhereClause = false;
/*
          
*/
            if (!string.IsNullOrEmpty(datePicker.Text))
            {
                string selectedTime = datePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                if (!hasWhereClause)
                {
                    query += " WHERE";
                    hasWhereClause = true;
                }
                else
                {
                    query += " AND";
                }
                query += $" (o.order_time  LIKE '%{selectedTime}%')";
            }

            UpdateDataGridView();
        }

        private void UpdateDataGridView()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            string baseQuery = @"SELECT order_id As 'Номер заказа', 
                    concat(us.lastName, ' ', Left(us.name, 1), '.') AS 'ФИО', 
                    o.table_number As 'Номер стола',
                    o.status As 'Статус', 
                    o.order_time As 'Дата заказа', 
                    Concat(total_price, ' ₽') As 'Стоимость заказа' 
                    FROM orders o
                    INNER JOIN users us ON us.user_id = o.user_id";

            List<string> whereConditions = new List<string>();

            // Фильтр по статусу
            if (status != "all" && !string.IsNullOrEmpty(status))
            {
                whereConditions.Add($"o.status = '{MySqlHelper.EscapeString(status)}'");
            }

            // Фильтр по дате
            if (datePicker.SelectedDate != null)
            {
                string selectedDate = datePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                whereConditions.Add($"DATE(o.order_time) = '{MySqlHelper.EscapeString(selectedDate)}'");
            }
            
            // Собираем окончательный запрос
            string finalQuery = baseQuery;
            if (whereConditions.Count > 0)
            {
                finalQuery += " WHERE " + string.Join(" AND ", whereConditions);
            }

            using (MySqlConnection connection = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    connection.Open();
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter(finalQuery, connection);
                    dataAdapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при извлечении данных: {ex.Message}");
                }
            }

            dataGrid.ItemsSource = dt.DefaultView;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            cancelBtn.IsEnabled = false;
            completeBtn.IsEnabled = false;

            switch (SafeData.role)
            {
                case "Шеф":
                    cancelBtn.Visibility = Visibility.Collapsed;
                    completeBtn.Visibility = Visibility.Collapsed;
                    break;
                case "Официант":
                    cancelBtn.Visibility = Visibility.Collapsed;
                    break;
            }
            
            UpdateDataGridView();
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataGridView();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;

            switch (radioBtn.Uid)
            {
                case "all":
                    status = "all";
                    break;
                case "Processing":
                    status = "В обработке";
                    break;
                case "Cancelled":
                    status = "Отменен";
                    break;
                case "Completed":
                    status = "Завершен";
                    break;
            }
            UpdateDataGridView();
        }

        private void completeBtn_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                using (MySqlCommand checkCmd = new MySqlCommand(
                $"SELECT status FROM restaurant.orders where order_id='{id}';", con))
                {
                    //Ошбика
                    string currentStatus = checkCmd.ExecuteScalar()?.ToString();


                    if (currentStatus != "В обработке")
                    {
                        MessageBox.Show($"Нельзя завершить заказ со статусом: {currentStatus}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                        return;
                    }
                }

                if (MessageBox.Show($"Заказ №{id} готов к выдаче?", "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand($"Update orders Set status = 'Завершен' where order_id ='{id}' ", con))
                    {
                        cmd.ExecuteNonQuery();
                        UpdateDataGridView();
                        MessageBox.Show("Данные о заказе обновленны");
                    }
                }
            }
            GenerateCheck();
        }

        public void GenerateCheck()
        {
            if (MessageBox.Show("Распечатать чек", "Чек", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                // Загрузка данных
                using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
                {
                    con.Open();
                    string query = @"SELECT 
                        r.order_id, 
                        m.name, 
                        oi.quantity, 
                        r.order_time, 
                        r.total_price,
                        m.price
                    FROM 
                        restaurant.orders r
                        INNER JOIN restaurant.order_items oi ON oi.order_id = r.order_id
                        INNER JOIN restaurant.menu m ON m.menu_id = oi.menu_id
                    WHERE 
                        r.order_id = @orderId;";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@orderId", SafeData.orderId);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    System.Data.DataTable dataTable = new System.Data.DataTable();
                    da.Fill(dataTable);

                    // Работа с Word
                    var wordApp = new Microsoft.Office.Interop.Word.Application();
                    wordApp.Visible = true; // Делаем Word видимым
                    Microsoft.Office.Interop.Word.Document wordDocument = null;
                    string localCopyPath = null;

                    try
                    {
                        // Создаем копию шаблона
                        localCopyPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"check_copy_{Guid.NewGuid()}.docx");

                        int attempts = 0;
                        bool fileCopied = false;
                        while (attempts < 5 && !fileCopied)
                        {
                            try
                            {
                                File.Copy(FileName, localCopyPath, true);
                                fileCopied = true;
                            }
                            catch (IOException)
                            {
                                attempts++;
                                System.Threading.Thread.Sleep(500);
                            }
                        }

                        if (!fileCopied)
                            throw new Exception("Не удалось создать копию файла шаблона. Файл занят другим процессом.");

                        // Открываем документ
                        wordDocument = wordApp.Documents.Open(localCopyPath);

                        if (wordDocument.Tables.Count < 1)
                            throw new InvalidOperationException("Документ не содержит таблиц");

                        // Заполнение таблицы
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            string name = dataTable.Rows[i]["name"].ToString();
                            string quantity = dataTable.Rows[i]["quantity"].ToString();
                            string price = dataTable.Rows[i]["price"].ToString();

                            Word.Table table = wordDocument.Tables[1];
                            Word.Row newRow = table.Rows.Add();

                            newRow.Cells[1].Range.Text = name;
                            newRow.Cells[2].Range.Text = quantity;
                            newRow.Cells[3].Range.Text = price;
                        }

                        // Замена меток
                        ReplaceWordStub("{dataTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), wordDocument);
                        ReplaceWordStub("{totalPrice}", dataTable.Rows[0]["total_price"].ToString(), wordDocument);

                        // Сохраняем изменения
                        wordDocument.Save();

                        // Активируем окно Word
                        wordApp.Activate();

                        // Оставляем документ открытым для пользователя
                        // Теперь не закрываем документ и Word в finally блоке
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при генерации чека: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                        // В случае ошибки все равно нужно освободить ресурсы
                        if (wordDocument != null)
                        {
                            wordDocument.Close(false);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDocument);
                        }
                        if (wordApp != null)
                        {
                            wordApp.Quit();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
                        }
                    }
                    finally
                    {
                        // Удаление временного файла шаблона
                        try
                        {
                            if (localCopyPath != null && File.Exists(localCopyPath))
                                File.Delete(localCopyPath);
                        }
                        catch { /* Игнорируем ошибки удаления */ }

                        // Освобождаем только COM-объекты документа (Word остается открытым)
                        if (wordDocument != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDocument);
                        }
                    }
                }
            }
        }
        private void ReplaceWordStub(string stubToReplace, string text, Microsoft.Office.Interop.Word.Document wordDocument)
        {
            var range = wordDocument.Content;
            range.Find.ClearFormatting();
            range.Find.Execute(FindText: stubToReplace, ReplaceWith: text);
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var selectedRow = dataGrid.SelectedItem as DataRowView;

                if (selectedRow != null)
                {
                    cancelBtn.IsEnabled = true;
                    completeBtn.IsEnabled = true;
                    SafeData.orderId = selectedRow[0].ToString();
                    id = selectedRow[0].ToString();
                }
            }
        }
        BlurEffect blurEffect = new BlurEffect
        {
            Radius = 5
        };

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var selectedRow = dataGrid.SelectedItem as DataRowView;

                if (selectedRow != null)
                {
                    SafeData.orderId = selectedRow[0].ToString();
                    Blur.workTable.Effect = blurEffect;
                    Blur.workTable.IsEnabled = false;
                    Blur.workTable.Opacity = 0.5;
                    ViewOrderStructure VmI = new ViewOrderStructure();
                    Timer.idleTimer.Stop();
                    VmI.ShowDialog();
                    Timer.idleTimer.Start();
                    Blur.workTable.Effect = null;
                    Blur.workTable.IsEnabled = true;
                    Blur.workTable.Opacity = 1;
                }
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                using (MySqlCommand checkCmd = new MySqlCommand(
                   $"SELECT status FROM restaurant.orders where order_id='{id}';", con))
                {
                    string currentStatus = checkCmd.ExecuteScalar()?.ToString();


                    if (currentStatus != "В обработке")
                    {
                        MessageBox.Show($"Нельзя завершить заказ со статусом: {currentStatus}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                        return;
                    }
                }
                if (MessageBox.Show($"Вы уверенны что хотите отменить заказ №{id}?", "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand($"Update orders Set status = 'Отменен' where order_id ='{id}' ", con))
                    {
                        cmd.ExecuteNonQuery();
                        UpdateDataGridView();
                        MessageBox.Show("Данные о заказе обновленны");
                    }
                }

            }
        }
    }
}
