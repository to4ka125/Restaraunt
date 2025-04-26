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
using Restaraunt.View;

namespace Restaraunt.Forms
{
    /// <summary>
    /// Interaction logic for WorkTable.xaml
    /// </summary>
    public partial class WorkTable : Window
    {
        public WorkTable()
        {
            InitializeComponent();
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

                case "OrderAdd":
                    Order oA = new Order();
                    container.Children.Add(oA);
                    break;
            }
        }
    }
}
