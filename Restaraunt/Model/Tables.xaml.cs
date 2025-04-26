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
using Restaraunt.Utilits;

namespace Restaraunt.Model
{
    /// <summary>
    /// Interaction logic for Tables.xaml
    /// </summary>
    public partial class Tables : UserControl
    {
        public Tables()
        {
            InitializeComponent();
        }

        public  string Tytle 
        {
            get => (string)GetValue(TytleProperty);
            set => SetValue(TytleProperty, value);
        }

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty,value);
        }
        public static readonly DependencyProperty TytleProperty = DependencyProperty.Register("tytle", typeof(string), typeof(Tables));
        public static readonly DependencyProperty StatusProperty= DependencyProperty.Register("status",typeof(string),typeof(Tables));

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            SolidColorBrush occupiedColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#F85D5D");
            SolidColorBrush freelyColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#4FCC8B");
            SolidColorBrush reservedColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffdd00");
            SolidColorBrush tytleTextColot = (SolidColorBrush)new BrushConverter().ConvertFrom("#fff");

            switch (qStatus.Text)
            {
                case "занят":
                    TablesBg.Background = occupiedColor;
                    qTytle.Foreground = tytleTextColot;
                    qStatus.Foreground = tytleTextColot;
                    qStatus.Opacity = 0.8;
                    break;

                case "резерв":
                    TablesBg.Background = reservedColor;
                    qTytle.Foreground = tytleTextColot;
                    qStatus.Foreground = tytleTextColot;
                    qStatus.Opacity = 0.8;
                    break; 

                case "свободно":
                    TablesBg.Background = freelyColor;
                    qTytle.Foreground = tytleTextColot;
                    qStatus.Foreground = tytleTextColot;
                    qStatus.Opacity = 0.8;
                    break;
            }
        }
    }
}
