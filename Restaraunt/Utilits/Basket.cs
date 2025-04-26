using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaraunt.Utilits
{
    public class Basket : INotifyPropertyChanged
    {
        private static readonly Basket instance = new Basket();
        public static Basket Instance => instance;

        public static Dictionary<string, int> basket = new Dictionary<string, int>();
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void AddToBasket(string id)
        {
            if (basket.ContainsKey(id))
            {
                basket[id] += 1;
            }
            else
            {
                basket.Add(id, 1);
            }

            OnPropertyChanged(nameof(basket));
        }

        public void DellIngredients(string id)
        {
            if (basket.ContainsKey(id))
            {
                if (basket[id] > 1)
                {
                    basket[id] -= 1;
                }
                else
                {
                    basket.Remove(id);
                }

                OnPropertyChanged(nameof(basket));
            }
        }
    }
}
