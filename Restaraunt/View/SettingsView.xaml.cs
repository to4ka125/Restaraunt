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
                MessageBox.Show("Поле Время бездействия должно быть заполнено.\nПожалуйста, введите значение.",
        "Ошибка ввода",
        MessageBoxButton.OK,
        MessageBoxImage.Warning);
                return;
            }
            int time = int.Parse(TimeBox.Text) * 1000;
            if (MessageBox.Show("Вы хотите сохранить изменения", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.blockingTime = time;
                Properties.Settings.Default.Save();
                MessageBox.Show("Настройки сохраненны");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TimeBox.Text = (Properties.Settings.Default.blockingTime / 1000).ToString();
        }

        private void TimeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
