using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TilePuzzle {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page {

        private const int gridSize = 1000;
        private const int tileSize = 250;
        private const int numRowsAndCols = 4;

        private StorageFile file;

        private ImageBrush imgBrush;
        private Image img;
        
        private List<Rectangle> tiles;
        private List<Rectangle> randomTiles;

        private DispatcherTimer timer = null;

        private int secondsElapsed;

        public GamePage() {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {

            //If image was passed create image puzzle, else create number puzzle
            if (e.Parameter != null) {
                file = e.Parameter as StorageFile;

                //list of tiles
                tiles = new List<Rectangle>();
                loadImageGame(file);
            } else { 
                loadNumberGame();
            }
        }

        private async void loadImageGame(StorageFile file) {

            //split image into 16 smaller images for puzzle
            int index = 0;
            for (int i = 0; i < numRowsAndCols; i++) {
                for (int j = 0; j < numRowsAndCols; j++) {

                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                    InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
                    BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);

                    //make each image the size of the grid
                    encoder.BitmapTransform.ScaledHeight = gridSize;
                    encoder.BitmapTransform.ScaledWidth = gridSize;

                    //Transform bitmap and cut out tile size image
                    BitmapBounds bounds = new BitmapBounds();
                    bounds.Height = tileSize;
                    bounds.Width = tileSize;
                    bounds.X = (uint)i * bounds.Height;
                    bounds.Y = (uint)j * bounds.Width;
                    encoder.BitmapTransform.Bounds = bounds;

                    try {
                        await encoder.FlushAsync();
                    } catch (Exception ex)  {
                        string s = ex.ToString();
                    }

                    BitmapImage tile = new BitmapImage();
                    tile.SetSource(ras);

                    img = new Image();
                    imgBrush = new ImageBrush();
                    img.Source = tile;
                    img.CanDrag = true;
                    imgBrush.ImageSource = img.Source;

                    Rectangle rect = new Rectangle();
                    rect.Fill = imgBrush;
                    rect.CanDrag = true;

                    tiles.Add(rect);

                    //Set grid position of each image
                    tiles[index].SetValue(Grid.RowProperty, j);
                    tiles[index].SetValue(Grid.ColumnProperty, i);
                    index++;
                }
            }
       
            //Add tiles to grid
            for (int i = 0; i < tiles.Count - 1; i++)  {

                puzzleGrid.Children.Add(tiles[i]);
            }
            
            //game timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            timer.Tick += GameTimerTick;
        }

        private void loadNumberGame() {
            //todo
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e) {
            //clear grid
            puzzleGrid.Children.Clear();

            //Create copy of original tile list so we can randomize
            randomTiles = new List<Rectangle>();
            List<Rectangle> tilesRemaining = new List<Rectangle>(tiles);
            //Remove last image for the discarded corner of puzzle
            tilesRemaining.RemoveAt(15);

            Random random = new Random();
            int tilesPlaced = 0;
            while (tilesPlaced != 15) {
                int index = 0;
                if (tilesRemaining.Count > 1) {
                    index = (int)(random.NextDouble() * tilesRemaining.Count());
                }

                randomTiles.Add(tilesRemaining[index]);
                tilesRemaining.RemoveAt(index);
                tilesPlaced++;
            }

            //Add placeholder rectangle to list for the discarded corner of puzzle
            //(has to be 16 images in list to set positions in 4x4 grid)
            Rectangle pieceToDiscard = new Rectangle();
            randomTiles.Add(pieceToDiscard);


            //Set positions of each image in grid
            int randomTileIndex = 0;
            for (int i = 0; i < numRowsAndCols; i++) {
                for (int j = 0; j < numRowsAndCols; j++) {

                    randomTiles[randomTileIndex].SetValue(Grid.RowProperty, j);
                    randomTiles[randomTileIndex].SetValue(Grid.ColumnProperty, i);
                    randomTileIndex++;
                }
            }

            //Place images in grid
            for (int i = 0; i < randomTiles.Count - 1; i++) {
                puzzleGrid.Children.Add(randomTiles[i]);
            }
        }

        private void puzzleGrid_DragOver(object sender, DragEventArgs e) {
            //todo: anything?
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void puzzleGrid_Drop(object sender, DragEventArgs e) {                
            //todo: somehow check for empty spot, drop image being dragged into empty spot
        }

        private void gamePageGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            double boundsHeight = gamePageGrid.ActualHeight - (puzzleGrid.Margin.Top + puzzleGrid.Margin.Bottom);
            double boundsWidth = gamePageGrid.ActualWidth - (puzzleGrid.Margin.Left + puzzleGrid.Margin.Right);

            if(boundsHeight > boundsWidth) {
                puzzleGrid.Height = boundsWidth;
                puzzleGrid.Width = boundsWidth;
            } else {
                puzzleGrid.Height = boundsHeight;
                puzzleGrid.Width = boundsHeight;
            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
        }

        private async void ChangePuzzleButton_Click(object sender, RoutedEventArgs e){

            //Make sure user wants to change puzzle
            MessageDialog messageDialog = new MessageDialog("Are you sure you want to abandon the current puzzle?");

            messageDialog.Commands.Add(new UICommand("Yes",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand( "Cancel",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));
            //Set default command to Yes
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler(IUICommand command)  {
            if (command.Label == "Yes") {
                this.Frame.Navigate(typeof(SetupPage));
            }
        }

        private void GameTimerTick(object sender, object e)  {
            secondsElapsed++;
            timeText.Text = "Time Elapsed: " + secondsElapsed;
        }
    }
}
