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
using Restaraunt.Forms;

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
            WorkTable wT = new WorkTable();
            wT.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти из приложения?",
                   "Подтверждение выхода",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
