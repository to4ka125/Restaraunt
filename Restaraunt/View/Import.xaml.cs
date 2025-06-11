using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Restaraunt.Utilits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : UserControl
    {
        string fileName;
        public Import()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы csv| *.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                FileName.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
                fileName = fileInfo.ToString();
            }
            else
            {
                fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (MySqlCommand cmd = new MySqlCommand($@"SHOW TABLES", con))
                {
                    MySqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        tablesName.Items.Add(dr.GetValue(0));
                    }
                }
            }
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string[] readText = File.ReadAllLines(fileName);
            string[] valField;
            string[] titleField = readText[0].Split(';');

            bool errorOccurred = false; // Логическая переменная для отслеживания ошибок

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                con.Open();

                foreach (string str in readText.Skip(1).ToArray())
                {
                    valField = str.Split(';');
                    // Создаем команду с параметрами
                    string strCmd = $"INSERT INTO {tablesName.SelectedItem} ({String.Join(",", titleField)}) VALUES (";
                    List<MySqlParameter> parameters = new List<MySqlParameter>();

                    for (int i = 0; i < titleField.Length; i++)
                    {
                        string paramName = $"@param{i}";
                        object value = valField[i];

                        // Проверяем на пустое значение для целочисленных полей
                        if (string.IsNullOrEmpty((string)value))
                        {
                            parameters.Add(new MySqlParameter(paramName, DBNull.Value));
                        }
                        else
                        {
                            parameters.Add(new MySqlParameter(paramName, value));
                        }

                        strCmd += paramName;
                        if (i != titleField.Length - 1)
                        {
                            strCmd += ",";
                        }
                    }

                    strCmd += ");";

                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand(strCmd, con))
                        {
                            cmd.Parameters.AddRange(parameters.ToArray());
                            int result = cmd.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        // Если ошибка еще не была выведена, выводим сообщение об ошибке
                        if (!errorOccurred)
                        {
                            MessageBox.Show($"Ошибка при вставке данных: {ex.Message}");
                            errorOccurred = true; // Устанавливаем флаг, что ошибка была выведена
                        }
                    }
                }
            }

            MessageBox.Show($"Импортировано {readText.Length - 1} записей.{tablesName.SelectedItem}");
            //   ImportDataFromCsv(fileName);
            /*
            string[] readText = File.ReadAllLines(fileName, Encoding.GetEncoding(1251));
            string[] titleField = readText[0].Split(',').Select(field => field.Trim().Trim('"')).ToArray();
            string tableName = tablesName.Text;
            string[] valField;
            string errorMsg = string.Empty;
            string query = string.Empty;
            int count = 0;
            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {

                con.Open();

                string[] dbHeaders = GetDataBaseHeaderCheck(con, tableName);
                con.Close();

                if (titleField.SequenceEqual(dbHeaders))
                {
                    con.Open();
                    foreach (string str in readText.Skip(1).ToArray())
                    {
                        valField = str.Split(',').Select(field => field.Trim().Trim('"')).ToArray();
                        switch (tableName)
                        {
                            case "users":
                                query = $@"Insert into `users`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}','{valField[3]}','{valField[4]}','{valField[5]}','{valField[6]}','{valField[7]}')";
                                break;

                            case "tables":
                                query = $@"Insert into `tables`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}')";
                                break;

                            case "supplier":
                                query = $@"Insert into `supplier` ({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}','{valField[3]}','{valField[4]}','{valField[5]}')";
                                break;

                            case "reservations":
                                query = $@"Insert into `reservations`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}','{valField[3]}','{valField[4]}')";
                                break;

                            case "products":
                                var supplierkQuery = $"SELECT COUNT(*) FROM supplier WHERE supplier_id = '{valField[4]}'";

                                using (MySqlCommand supplierCheck = new MySqlCommand(supplierkQuery, con))
                                {
                                    count = int.Parse(supplierCheck.ExecuteScalar().ToString());
                                    if (count == 0)
                                    {
                                        MessageBox.Show("Сначала заполните таблицу поставщики");
                                        return;
                                    }
                                }

                                query = $@"Insert into `products`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}','{valField[3]}','{valField[4]}')";
                                break;

                            case "orders":
                                var ordersCheckQuery = $"SELECT COUNT(*) FROM users WHERE user_id = '{valField[1]}'";

                                using (MySqlCommand orderCheck = new MySqlCommand(ordersCheckQuery, con))
                                {
                                    count = int.Parse(orderCheck.ExecuteScalar().ToString());
                                    if (count == 0)
                                    {
                                        MessageBox.Show("Сначала заполните таблицу пользователи");
                                        return;
                                    }
                                }

                                query = $@"INSERT INTO orders ({string.Join(",", titleField)}) VALUES 
                                        ('{valField[0]}', '{valField[1]}', '{valField[2]}', '{valField[3]}', '{valField[4]}', '{valField[5]}')";
                                break;

                            case "order_items":
                                var menuOrderCheckQuery = $"SELECT COUNT(*) FROM menu WHERE menu_id = '{valField[1]}'";
                                var orderCheckQuery = $"SELECT COUNT(*) FROM orders WHERE order_id = '{valField[0]}'";

                                using (MySqlCommand menuCheck = new MySqlCommand(menuOrderCheckQuery, con))
                                {
                                    count = int.Parse(menuCheck.ExecuteScalar().ToString());
                                    if (count == 0)
                                    {
                                        MessageBox.Show("Сначала заполните таблицу меню");
                                        return;
                                    }
                                }
                                var orderCount = 0;
                                using (MySqlCommand orderCheck = new MySqlCommand(orderCheckQuery, con))
                                {
                                    orderCount = int.Parse(orderCheck.ExecuteScalar().ToString());
                                    if (orderCount == 0)
                                    {
                                        MessageBox.Show("Сначала заполните таблицу заказы");
                                        return;
                                    }
                                }

                                query = $@"Insert into `order_items`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}')";
                                break;

                            case "menu_ingredients":

                                var menuCheckQuery = $"SELECT COUNT(*) FROM menu WHERE menu_id = '{valField[0]}'";
                                var productCheckQuery = $"SELECT COUNT(*) FROM products WHERE product_id = '{valField[1]}'";

                                using (MySqlCommand menuCheck = new MySqlCommand(menuCheckQuery, con))
                                {
                                    count = int.Parse(menuCheck.ExecuteScalar().ToString());
                                    if (count == 0)
                                    {
                                        MessageBox.Show("Сначала заполните таблицу меню");
                                        return;
                                    }
                                }

                                var countProduct = 0;
                                using (MySqlCommand productCheck = new MySqlCommand(productCheckQuery, con))
                                {
                                    countProduct = int.Parse(productCheck.ExecuteScalar().ToString());
                                    if (countProduct == 0)
                                    {
                                        MessageBox.Show("Сначала заполните таблицу продукты");
                                        return;
                                    }
                                }
                                query = $@"Insert into `orders`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}')";
                                break;


                            case "menu":
                                var categoriesCheckQuery = $"SELECT COUNT(*) FROM categories ";

                                using (MySqlCommand categoriesCheck = new MySqlCommand(categoriesCheckQuery, con))
                                {
                                    count = int.Parse(categoriesCheck.ExecuteScalar().ToString());
                                    if (count == 0)
                                    {
                                        MessageBox.Show("Сначала заполните таблицу категории");
                                        return;
                                    }
                                }

                                query = $@"Insert into `menu`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}','{valField[3]}','{valField[4]}','{valField[5]}','{valField[6]}')";
                                break;

                            case "customers":
                                query = $@"Insert into `customers`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}',
                                            '{valField[2]}','{valField[3]}','{valField[4]}','{valField[5]}')";

                                break;
                            case "categories":
                                query = $@"Insert into `categories`({string.Join(",", titleField)}) Values ('{valField[0]}','{valField[1]}'
                                          )";
                                break;
                        }

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                errorMsg = ex.Message;
                            }
                        }
                    }

                    MessageBox.Show("Данные импортированны");
                    tablesName.SelectedItem = null;

                    if (errorMsg.Length > 0) MessageBox.Show(errorMsg);


                }
            }
            */
        }
        public void ImportDataFromCsv(string fileName)
        {
            try
            {
                string[] readText = File.ReadAllLines(fileName, Encoding.GetEncoding(1251));
                if (readText.Length == 0)
                {
                    MessageBox.Show("Файл пуст", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] titleField = readText[0].Split(',').Select(field => field.Trim().Trim('"')).ToArray();
                string tableName = tablesName.Text;

                if (string.IsNullOrEmpty(tableName))
                {
                    MessageBox.Show("Не выбрана таблица для импорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StringBuilder errorMessages = new StringBuilder();
                int successCount = 0;
                int totalRows = readText.Length - 1;

                using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
                {
                    con.Open();

                    // Получаем информацию о типах столбцов
                    Dictionary<string, string> columnTypes = GetColumnDataTypes(con, tableName);

                    string[] dbHeaders = GetDataBaseHeaderCheck(con, tableName);
                    if (!titleField.SequenceEqual(dbHeaders))
                    {
                        MessageBox.Show("Заголовки в файле не соответствуют структуре таблицы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    using (MySqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            foreach (string str in readText.Skip(1))
                            {
                                if (string.IsNullOrWhiteSpace(str)) continue;

                                string[] valField = str.Split(',').Select(field => field.Trim().Trim('"')).ToArray();
                                string query = string.Empty;
                                bool canInsert = true;

                                // Обработка значений перед вставкой
                                for (int i = 0; i < valField.Length && i < titleField.Length; i++)
                                {
                                    string columnName = titleField[i];
                                    if (columnTypes.ContainsKey(columnName) &&
                                        (columnTypes[columnName].Contains("date") || columnTypes[columnName].Contains("datetime")))
                                    {
                                        if (DateTime.TryParse(valField[i], out DateTime dateValue))
                                        {
                                            // Форматируем дату для MySQL
                                            valField[i] = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                        else
                                        {
                                            errorMessages.AppendLine($"Неверный формат даты в строке {successCount + 2}, столбец {columnName}: '{valField[i]}'");
                                            canInsert = false;
                                            break;
                                        }
                                    }
                                }

                                if (!canInsert) continue;

                                // Проверки внешних ключей и подготовка запросов
                                switch (tableName.ToLower())
                                {
                                    case "users":
                                        query = BuildInsertQuery(tableName, titleField, valField, 8);
                                        break;

                                    case "tables":
                                        query = BuildInsertQuery(tableName, titleField, valField, 3);
                                        break;

                                    case "supplier":
                                        query = BuildInsertQuery(tableName, titleField, valField, 6);
                                        break;

                                    case "reservations":
                                        query = BuildInsertQuery(tableName, titleField, valField, 5);
                                        break;

                                    case "products":
                                        canInsert = CheckForeignKeyExists(con, "supplier", "supplier_id", valField[4]);
                                        query = canInsert ? BuildInsertQuery(tableName, titleField, valField, 5) : string.Empty;
                                        break;

                                    case "orders":
                                        canInsert = CheckForeignKeyExists(con, "users", "user_id", valField[1]);
                                        query = canInsert ? BuildInsertQuery(tableName, titleField, valField, 6) : string.Empty;
                                        break;

                                    case "order_items":
                                        canInsert = CheckForeignKeyExists(con, "menu", "menu_id", valField[1]) &&
                                                    CheckForeignKeyExists(con, "orders", "order_id", valField[0]);
                                        query = canInsert ? BuildInsertQuery(tableName, titleField, valField, 3) : string.Empty;
                                        break;

                                    case "menu_ingredients":
                                        canInsert = CheckForeignKeyExists(con, "menu", "menu_id", valField[0]) &&
                                                   CheckForeignKeyExists(con, "products", "product_id", valField[1]);
                                        query = canInsert ? BuildInsertQuery(tableName, titleField, valField, 3) : string.Empty;
                                        break;

                                    case "menu":
                                        canInsert = CheckForeignKeyExists(con, "categories", "category_id", valField[4]);
                                        query = canInsert ? BuildInsertQuery(tableName, titleField, valField, 7) : string.Empty;
                                        break;

                                    case "customers":
                                        query = BuildInsertQuery(tableName, titleField, valField, 6);
                                        break;

                                    case "categories":
                                        query = BuildInsertQuery(tableName, titleField, valField, 2);
                                        break;

                                    default:
                                        errorMessages.AppendLine($"Неизвестная таблица: {tableName}");
                                        canInsert = false;
                                        break;
                                }

                                if (canInsert && !string.IsNullOrEmpty(query))
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(query, con, transaction))
                                    {
                                        try
                                        {
                                            cmd.ExecuteNonQuery();
                                            successCount++;
                                        }
                                        catch (MySqlException ex)
                                        {
                                            errorMessages.AppendLine($"Ошибка при вставке строки {successCount + 1}: {ex.Message}");
                                        }
                                    }
                                }
                            }

                            if (errorMessages.Length > 0)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Ошибки при импорте:\n{errorMessages}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                transaction.Commit();
                                MessageBox.Show($"Успешно импортировано {successCount} из {totalRows} строк", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Критическая ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    con.Close();
                }
                tablesName.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Dictionary<string, string> GetColumnDataTypes(MySqlConnection connection, string tableName)
        {
            var columnTypes = new Dictionary<string, string>();
            string query = $"SHOW COLUMNS FROM `{tableName}`";

            // Создаем новый command для этого запроса
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                // Важно использовать using для reader
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader.GetString(0);
                        string dataType = reader.GetString(1);
                        columnTypes.Add(columnName, dataType.ToLower());
                    }
                } // Reader будет закрыт здесь автоматически
            }
            return columnTypes;
        }

        private string[] GetDataBaseHeaderCheck(MySqlConnection connection, string tableName)
        {
            List<string> columns = new List<string>();
            string query = $"SHOW COLUMNS FROM `{tableName}`";

            // Создаем новый command для этого запроса
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                // Важно использовать using для reader
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(reader.GetString(0));
                    }
                } // Reader будет закрыт здесь автоматически
            }

            return columns.ToArray();
        }

        private bool CheckForeignKeyExists(MySqlConnection connection, string tableName, string columnName, string value)
        {
            string query = $"SELECT COUNT(*) FROM `{tableName}` WHERE `{columnName}` = @value";

            // Создаем новый command для этого запроса
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                // Используем параметризованный запрос для безопасности
                cmd.Parameters.AddWithValue("@value", value);

                // ExecuteScalar не требует reader, поэтому проблем быть не должно
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

    
        private string BuildInsertQuery(string tableName, string[] columns, string[] values, int expectedFieldCount)
        {
            if (values.Length != expectedFieldCount)
            {
                throw new ArgumentException($"Ожидалось {expectedFieldCount} значений, получено {values.Length}");
            }

            string columnsPart = string.Join(",", columns.Select(c => $"`{c}`"));
            string valuesPart = string.Join(",", values.Select(v => $"'{MySqlHelper.EscapeString(v)}'"));

            return $"INSERT INTO `{tableName}` ({columnsPart}) VALUES ({valuesPart})";
        }
        // Новый метод для получения типов столбцов
    
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backup");
            string backupPath = Path.Combine(backupDirectory, "restaraunt.sql");
            string databaseName = "restaurant";

            string conString = "host=127.0.0.1; uid=root;pwd=;";
            using (MySqlConnection con = new MySqlConnection(conString))
            {
                con.Open();
                MySqlCommand cmdCheckExists = new MySqlCommand($"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '{databaseName}';", con);
                int dbExists = Convert.ToInt32(cmdCheckExists.ExecuteScalar());

                if (dbExists > 0)
                {
                    MySqlCommand cmdDrop = new MySqlCommand($"DROP DATABASE IF EXISTS `{databaseName}`;", con);
                    cmdDrop.ExecuteNonQuery();
                }
                MySqlCommand cmdCreate = new MySqlCommand($"CREATE DATABASE `{databaseName}`;", con);
                cmdCreate.ExecuteNonQuery();

                MySqlCommand cmdUse = new MySqlCommand($"USE `{databaseName}`;", con);
                cmdUse.ExecuteNonQuery();

                string script = File.ReadAllText(backupPath);
                MySqlScript sqlScript = new MySqlScript(con, script);
                sqlScript.Execute();

                con.Close();
            }
            MessageBox.Show("Востановление структуры прошло успешно");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку для сохранения CSV файлов",
                ShowNewFolderButton = true
            };

            if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show("Экспорт отменен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string outputDirectory = folderBrowserDialog.SelectedPath;
          try
            {
                using (MySqlConnection con =new MySqlConnection(MySqlCon.con))
                {
                    con.Open();

                    DataTable dt = con.GetSchema("Tables");

                    int exportedTables = 0;

                    foreach(DataRow table in dt.Rows)
                    {
                        string tableName = table["TABLE_NAME"].ToString();
                        if (tableName.StartsWith("sys") || tableName.StartsWith("MS_")) continue;

                        ExportTableToCsv(con, tableName, outputDirectory);
                        exportedTables++;
                    }

                    MessageBox.Show($"Успешно экспортировано {exportedTables} таблиц в папку:\n{outputDirectory}",
                            "Экспорт завершен",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    // Открываем папку с результатами
                    System.Diagnostics.Process.Start(outputDirectory);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
            }

        /*
        private void ExportTableToCsv(MySqlConnection connection, string tableName, string outputDirectory)
        {
            string query = $"SELECT * FROM {tableName}";
            string filePath = Path.Combine(outputDirectory, $"{tableName}.csv");

            using (var command = new MySqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Заголовки столбцов
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (i > 0) writer.Write(";");
                    writer.Write(EscapeCsvValue(reader.GetName(i)));
                }
                writer.WriteLine();

                // Данные
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (i > 0) writer.Write(";");

                        if (!reader.IsDBNull(i))
                        {
                            writer.Write(EscapeCsvValue(reader.GetValue(i).ToString()));
                        }
                    }
                    writer.WriteLine();
                }
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
        */

        private void ExportTableToCsv(MySqlConnection connection, string tableName, string outputDirectory)
        {
            string query = $"SELECT * FROM {tableName}";
            string filePath = Path.Combine(outputDirectory, $"{tableName}.csv");

            using (var command = new MySqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Запись заголовков
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (i > 0) writer.Write(";");
                    writer.Write(EscapeCsvValue(reader.GetName(i)));
                }
                writer.WriteLine();

                // Запись данных
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (i > 0) writer.Write(";");
                        if (!reader.IsDBNull(i))
                        {
                            object value = reader.GetValue(i);
                            // Обработка форматов для чисел и дат
                            if (value is DateTime dateValue)
                            {
                                // Форматируем дату в нужный формат
                                writer.Write(dateValue.ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else if (value is decimal || value is double || value is float)
                            {
                                // Форматируем число с точкой как десятичным разделителем
                                writer.Write(Convert.ToDouble(value).ToString("G", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                writer.Write(EscapeCsvValue(value.ToString()));
                            }
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            string conStr = "host=localhost;uid=root;pwd=;";

            string defaultBackupPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
      "reservCopy");

            // Создаем диалог выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Directory.Exists(defaultBackupPath)
                    ? defaultBackupPath
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "SQL файлы (*.sql)|*.sql|Все файлы (*.*)|*.*",
                Title = "Выберите файл резервной копии",
                DefaultExt = ".sql",
                CheckFileExists = true,
                CheckPathExists = true
            };

            // Показываем диалог
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                // Здесь можно добавить обработку выбранного файла
                MessageBox.Show($"Выбран файл: {selectedFilePath}",
                              "Файл выбран",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // Пример: прочитать содержимое файла
                try
                {
                    using (MySqlConnection con = new MySqlConnection(conStr))
                    {
                        con.Open();
                        string script = File.ReadAllText(selectedFilePath);
                        MySqlScript sqlScript = new MySqlScript(con, script);
                        sqlScript.Execute();
                        MessageBox.Show("Востановление структуры прошло успешно");
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка чтения файла: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }
    }
}
