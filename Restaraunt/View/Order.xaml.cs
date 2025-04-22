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
using Restaraunt.Model;
using Restaraunt.Utilits;

namespace Restaraunt.View
{
    /// <summary>
    /// Interaction logic for Order.xaml
    /// </summary>
    public partial class Order : UserControl
    {
        public Order()
        {
            InitializeComponent();
        }

        public void PhaseElips()
        {
            int step = SafeData.step;

            // Сброс цвета всех эллипсов
            step2.Fill = Brushes.Transparent;
            step3.Fill = Brushes.Transparent;
            step4.Fill = Brushes.Transparent;
            step5.Fill = Brushes.Transparent;

            SolidColorBrush completedColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F85D5D")); // Цвет для завершенных шагов
            SolidColorBrush currentColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FDC5C5")); // Цвет для текущего шага

            var steps = new[] { step1, step2, step3, step4, step5 };

            for (int i = 0; i < steps.Length; i++)
            {
                if (i == step)
                {
                    steps[i].Fill = completedColor; 
                }
                else if (i == step+1)
                {
                    steps[i].Fill = currentColor;
                }
            }
        }

        public void TablesPopulateGrid()
        {
            TablesConteiner.Children.Clear();
            TablesConteiner.RowDefinitions.Clear();
            TablesConteiner.ColumnDefinitions.Clear();

            int columnCount = 3;
            int rowCount = (8 + columnCount - 1) / columnCount;

            for (int i = 0; i < columnCount; i++)
            {
                var columnDefinition = new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                };
                TablesConteiner.ColumnDefinitions.Add(columnDefinition);
            }

            for (int i = 0; i < rowCount; i++)
            {
                TablesConteiner.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < 8; i++)
            {
                var tables = new Tables
                {
                    Margin = new Thickness(0, 10, 50, 10),
                };

                tables.MouseDoubleClick += Tables_MouseDoubleClick;
                Grid.SetRow(tables, i / columnCount);
                Grid.SetColumn(tables, i % columnCount);
                TablesConteiner.Children.Add(tables);
            }
        }

        private void Tables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SafeData.step = 1;
            AddTables.Visibility = Visibility.Collapsed;
            ChoosingPaymentMethod.Visibility = Visibility.Visible;
            PhaseElips();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SafeData.step = 0;
            TablesPopulateGrid();
        }
    }
}
