using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TilePuzzle {

    public sealed partial class ArrayGrid : Grid{
        private readonly int width, height;
        private List<UIElement> arr;

        public ArrayGrid(int width, int height) : base(){
            if(width < 1 || height < 1) {
                throw new IndexOutOfRangeException("width and height must be greater than 0");
            }

            this.width = width;
            this.height = height;

            for(int i=0; i<width; i++) {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                ColumnDefinitions.Add(cd);
            }

            for(int i = 0; i<height; i++) {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                RowDefinitions.Add(rd);
            }

            arr = new List<UIElement>(width*height);
            for(int i = 0; i<(width*height); i++) {
                arr.Add(null);
            }
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
