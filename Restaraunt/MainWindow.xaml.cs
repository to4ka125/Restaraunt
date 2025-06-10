using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using MySql.Data.MySqlClient;
using Restaraunt.Forms;
using Restaraunt.Utilits;
using System.IO;
using Path = System.IO.Path;
using System.Diagnostics;

namespace Restaraunt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (qLogin.Text == Properties.Settings.Default.LocalAdminLogin)
            {
                if (qPassword.Password == Properties.Settings.Default.LocalAdminPwd)
                {
                    MessageBox.Show("Вы успешно авторизованы!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    this.Visibility = Visibility.Collapsed;
                    SafeData.role = "Системный администратор";
                    SafeData.userName = "super user";
                    WorkTable wT = new WorkTable();
                    wT.ShowDialog();
                    this.Visibility = Visibility.Visible;
                    qLogin.Clear();
                    qPassword.Clear();
                    return;
                }
                else
                {
                    MessageBox.Show("К сожалению, введённый пароль неверен. Пожалуйста, попробуйте снова.",
                                         "Ошибка входа",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Error);

                    MessageBox.Show("Пройдите капчу");
                    Visibility=Visibility.Collapsed;
                    CaptchaMain cm = new CaptchaMain();
                
                    cm.ShowDialog();
                    Visibility = Visibility.Visible;
                    CaptchaCheck(SafeData.captchaCheck);
                    qPassword.Clear();
                    return;
                }
            }

            using (MySqlConnection con = new MySqlConnection(MySqlCon.con))
            {
                try
                {
                    con.Open();
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка с подключением", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string login = qLogin.Text;
                string password = qPassword.Password;


                MySqlCommand cmd = new MySqlCommand($"Select * From Users where login = '{login}'", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    string passwordBd = dt.Rows[0].ItemArray[4].ToString();

                    if (passwordBd == GetHashPass(password))
                    {
                        SafeData.role = dt.Rows[0].ItemArray[5].ToString();
                        SafeData.userName = $"{dt.Rows[0].ItemArray[1]} {dt.Rows[0].ItemArray[2]}";
                        SafeData.userId = dt.Rows[0].ItemArray[0].ToString();
                        MessageBox.Show("Вы успешно авторизованы!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                        Blur.workTable = new WorkTable();
                        this.Visibility = Visibility.Collapsed;
                        

                        Blur.workTable.ShowDialog();
        
                        this.Visibility = Visibility.Visible;
                        qPassword.Clear();
                    }
                    else
                    {
                        if (passwordBd != GetHashPass(password))
                        {
                            MessageBox.Show("К сожалению, введённый пароль неверен. Пожалуйста, попробуйте снова.",
                                            "Ошибка входа",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                            MessageBox.Show("Пройдите капчу");

                            Visibility = Visibility.Collapsed;
                            CaptchaMain cm = new CaptchaMain();
                            cm.ShowDialog();
                            Visibility = Visibility.Visible;
                            CaptchaCheck(SafeData.captchaCheck);
                            qPassword.Clear();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Пользователь не найден.\nПожалуйста, проверьте введенные данные и попробуйте снова.",
                     "Ошибка",
                     MessageBoxButton.OK,
                     MessageBoxImage.Warning);
                }
            }
        }

        public static void CaptchaCheck(bool check)
        {
            if (!check)
            {
                MessageBox.Show("Капча не пройдена"); 
                MessageBox.Show("Форма заблокирован на 10 секунд");
                Thread.Sleep(10000);
      
            }
            else
            {
                MessageBox.Show("Капча пройдена");
            }
        }
        public static string GetHashPass(string password)
        {

            byte[] bytesPass = Encoding.UTF8.GetBytes(password);

            SHA256Managed hashstring = new SHA256Managed();

            byte[] hash = hashstring.ComputeHash(bytesPass);

            string hashPasswd = string.Empty;

            foreach (byte x in hash)
            {
                hashPasswd += String.Format("{0:x2}", x);
            }

            hashstring.Dispose();

            return hashPasswd;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти из приложения?",
                   "Подтверждение выхода",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                CreateSqlDump();
                Application.Current.Shutdown();
            }
        }
        private void CreateSqlDump()
        {
            string server = "localhost";
            string database = "restaurant";
            string userId = "root";
            string password = "";

            string mysqldumpPath = @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe";

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string backupDir = Path.Combine(documentsPath, "reservCopy");
            string backupFile = Path.Combine(backupDir, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");

            try
            {
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    Console.WriteLine($"Создана папка: {backupDir}");
                }

                string args = $"--host={server} --user={userId} --password={password} " +
                             $"--databases {database} --result-file=\"{backupFile}\" " +
                             "--skip-lock-tables --single-transaction --routines --triggers";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = mysqldumpPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Ошибка mysqldump: {error}");
                    }
                }

                MessageBox.Show($"Резервная копия сохранена в:\n{backupFile}", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка при создании резервной копии:\n{ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void qPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            passwordCheckPsaholder(qPassword);
        }
        private void passwordCheckPsaholder(PasswordBox passwordBox)
        {
            TextBlock textBlock = (TextBlock)passwordBox.Template.FindName("textBlock", passwordBox);
            if (passwordBox.Password.Length > 0)
            {
                textBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                textBlock.Visibility = Visibility.Visible;
            }
        }
        private void qLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[\W]$")) { e.Handled = true; }
            if (Regex.IsMatch(e.Text, @"^[а-яА-Я]$")) { e.Handled = true; }
        }
    }
}
