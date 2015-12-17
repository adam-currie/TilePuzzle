using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TilePuzzle {

    public class ArrayGrid : Grid{
        private int width, height;
        private List<UIElement> arr;

        public static ArrayGrid ArrayGridFactory(Grid g){
            ArrayGrid newArrayGrid = (ArrayGrid)g;

            newArrayGrid.width = newArrayGrid.ColumnDefinitions.Count;
            newArrayGrid.height = newArrayGrid.RowDefinitions.Count;

            newArrayGrid.arr = new List<UIElement>(newArrayGrid.width*newArrayGrid.height);
            for(int i = 0; i<(newArrayGrid.width*newArrayGrid.height); i++) {
                newArrayGrid.arr.Add(null);
            }

            return newArrayGrid;
        }

        public UIElement Get(int x, int y) {
            if(x > width || y > height) {
                throw new IndexOutOfRangeException();
            }
            return arr[(y*width)+x];
        }

        public void Set(int x, int y, UIElement value) {
            if(x > width || y > height) {
                throw new IndexOutOfRangeException();
            }

            if(arr[(y*width)+x] != null) {
                Children.Remove(arr[(y*width)+x]);
            }

            if(value != null) {
                value.SetValue(Grid.RowProperty, y);
                value.SetValue(Grid.ColumnProperty, x);
                Children.Add(value);
            }

            arr[(y*width)+x] = value;
        }

    }

}
