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
using System.Windows.Shapes;
using Restaraunt.Utilits;
using Restaraunt.View;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for WorkTable.xaml
    /// </summary>
    public partial class WorkTable : Window
    {
        private DispatcherTimer _inactivityTimer;
        private readonly TimeSpan _inactivityTimeout = TimeSpan.FromSeconds(Properties.Settings.Default.blockingTime);
        public WorkTable()
        {
            InitializeComponent(); 
            InitializeInactivityTimer();
            EventManager.RegisterClassHandler(typeof(Window), Keyboard.KeyDownEvent, new KeyEventHandler(ResetInactivityTimer));
            EventManager.RegisterClassHandler(typeof(Window), Mouse.MouseMoveEvent, new MouseEventHandler(ResetInactivityTimer));
            EventManager.RegisterClassHandler(typeof(Window), Mouse.MouseDownEvent, new MouseButtonEventHandler(ResetInactivityTimer));
        }
        private void InitializeInactivityTimer()
        {
            _inactivityTimer = new DispatcherTimer
            {
                Interval = _inactivityTimeout
            };
            _inactivityTimer.Tick += OnInactivityTimeout;
            _inactivityTimer.Start();
            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;
            this.MouseDown += ResetInactivityTimer;
        }
        private void ResetInactivityTimer(object sender, EventArgs e)
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }
        private void OnInactivityTimeout(object sender, EventArgs e)
        {
            _inactivityTimer.Stop();
            MessageBox.Show("Вы бездействовали слишком долго. Приложение будет закрыто.", "Внимание");
            Close();
        }
   
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;

            container.Children.Clear();

            switch (radioButton.Name)
            {
                case "Users":
                    User u = new User();
                    container.Children.Add(u);
                    break;
                case "Order":
                    OrderView oV = new OrderView();
                    container.Children.Add(oV);
                    break;

                case "Report":
                    Report r = new Report();
                    container.Children.Add(r);
                    break;

                case "Dishes":
                    Dishes d = new Dishes();
                    container.Children.Add(d);
                    break;

                case "TableReserv":
                    TablesReservations tR = new TablesReservations();
                    container.Children.Add(tR);
                    break;
                
                case "Settings":
                    SettingsView sV = new SettingsView();
                    container.Children.Add(sV);
                    break;



                case "OrderAdd":
                    Order oA = new Order();
                    container.Children.Add(oA);
                    break;    
                case "Products":
                    Products p = new Products();
                    container.Children.Add(p);
                    break;

                case "Import":
                    Import i = new Import();
                    container.Children.Add(i);
                    break;

                case "Clients":
                   Customer C = new Customer();
                    container.Children.Add(C);
                    break; 
                case "Suplier":
                   Supplier s = new Supplier();
                    container.Children.Add(s);
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            qRole.Text = SafeData.role;
            qFio.Text = SafeData.userName;

            switch (SafeData.role)
            {
                case "Администратор":
                    icon.Source = new BitmapImage(new Uri("/Images/IconUsers/Admin.png", UriKind.Relative));
                    Dishes.Visibility = Visibility.Collapsed;
                    // Report.Visibility = Visibility.Collapsed;
                    // Order.Visibility = Visibility.Collapsed;
                    Settings.Visibility = Visibility.Collapsed;
                    Products.Visibility = Visibility.Collapsed;
                   // TableReserv.Visibility = Visibility.Collapsed;
                    OrderAdd.Visibility = Visibility.Collapsed;
                    Import.Visibility = Visibility.Collapsed;
                   // Clients.Visibility = Visibility.Collapsed;
                    Suplier.Visibility = Visibility.Collapsed;
                    break;
                case "Менеджер":
                    Settings.Visibility = Visibility.Collapsed;
                    icon.Source = new BitmapImage(new Uri("/Images/IconUsers/Manager.png", UriKind.Relative));
                    Dishes.Visibility = Visibility.Collapsed;
                    Users.Visibility = Visibility.Collapsed;
                    OrderAdd.Visibility = Visibility.Collapsed;
                    Import.Visibility = Visibility.Collapsed;

                    TableReserv.Visibility = Visibility.Collapsed;
                    Order.Visibility = Visibility.Collapsed;
                    Clients.Visibility = Visibility.Collapsed;
                    break;
                case "Шеф":
                    Settings.Visibility = Visibility.Collapsed;
                    icon.Source = new BitmapImage(new Uri("/Images/IconUsers/Chef.png", UriKind.Relative));
                    OrderAdd.Visibility = Visibility.Collapsed;
                    Import.Visibility = Visibility.Collapsed;
                    Report.Visibility = Visibility.Collapsed;
                    Users.Visibility = Visibility.Collapsed;
                    TableReserv.Visibility = Visibility.Collapsed;
                    Clients.Visibility = Visibility.Collapsed;
                    Suplier.Visibility = Visibility.Collapsed;
                    break;
                case "Официант":
                    Settings.Visibility = Visibility.Collapsed;
                    Products.Visibility = Visibility.Collapsed;
                    TableReserv.Visibility = Visibility.Collapsed;
                    Dishes.Visibility = Visibility.Collapsed;
                    Import.Visibility = Visibility.Collapsed;
                    Users.Visibility = Visibility.Collapsed;
                   
                    Report.Visibility = Visibility.Collapsed;
                    Suplier.Visibility = Visibility.Collapsed;
                    Clients.Visibility = Visibility.Collapsed;
                    Products.Visibility = Visibility.Collapsed;
                    icon.Source = new BitmapImage(new Uri("/Images/IconUsers/Waiter.png", UriKind.Relative));
                    break;

                case "Системный администратор":
                    Products.Visibility = Visibility.Collapsed;
                    TableReserv.Visibility = Visibility.Collapsed;
                    Dishes.Visibility = Visibility.Collapsed;
                    Users.Visibility = Visibility.Collapsed;
                    Report.Visibility = Visibility.Collapsed;
                    Suplier.Visibility = Visibility.Collapsed;
                    Clients.Visibility = Visibility.Collapsed;
                    Products.Visibility = Visibility.Collapsed;
                    OrderAdd.Visibility = Visibility.Collapsed;
                    Order.Visibility = Visibility.Collapsed;
                    icon.Source = new BitmapImage(new Uri("/Images/IconUsers/Admin.png", UriKind.Relative));
                    break;
                default:


                    break;
            }
        }
    }
}
