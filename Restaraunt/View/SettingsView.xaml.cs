using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TimeBox.Text == null)
            {
                MessageBox.Show("Необходимо ввести время бездействия пользователя \nдля сохранения изменений", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int time = int.Parse(TimeBox.Text);
            if (MessageBox.Show("Сохранить изменения?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.blockingTime = time;
                Properties.Settings.Default.Save();
                MessageBox.Show("Измения успешно сохранены?");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TimeBox.Text = (Properties.Settings.Default.blockingTime).ToString();
   
        }

        private void TimeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"^[а-яА-ЯA-Za-z \W]$")) { e.Handled = true; }
        }
    }
}
