/*
 * File: GamePage.xaml.cs
 * Project: Windows and Mobile Programming - Final Project
 * Programmers: Adam Currie and Dylan O'Neill
 * First Version: 2015-12-08
 * Description: Contains the GamePage class and the code behind the game page of the project.
                The methods in this file hold the main functionality of the game: cutting up the image,
                randomizing the tiles, moving the tiles, and checking the solution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

        private const int numRowsAndCols = 4;

        private StorageFile file;
        private DispatcherTimer timer = null;
        private int secondsElapsed;
        private bool loadingImage = false;
        private List<Rectangle> originalTiles = new List<Rectangle>(numRowsAndCols*numRowsAndCols);

        public GamePage() {
            this.InitializeComponent();
        }

        //Method      : OnNavigatedTo
        //Description : Handler for when this page is navigated to
        //Parameters  : NavigationEventArgs e - event args   
        //Returns     : void         
        protected override void OnNavigatedTo(NavigationEventArgs e) {

            //If image was passed, create image puzzle, else create number puzzle
            if (e.Parameter != null) {
                file = e.Parameter as StorageFile;

                //list of tiles
                loadImageGame(file);
            } else { 
                loadNumberGame();
            }
        }

        //Method      : loadImageGame
        //Description : Loads a new tile puzzle with an image
        //Parameters  : StorageFile file - image
        //Returns     : void              
        private async void loadImageGame(StorageFile file) {
            if(loadingImage) {
                return;
            }

            loadingImage = true;

            //clear old
            originalTiles.Clear();
            puzzleGrid.Children.Clear();

            //split image into smaller images for puzzle
            for (int i = 0; i < numRowsAndCols; i++) {
                for (int j = 0; j < numRowsAndCols; j++) {

                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                    InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
                    BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);

                    //reduce either width or height to make image square
                    uint croppedSize;
                    if(decoder.PixelHeight > decoder.PixelWidth) {
                        croppedSize = decoder.PixelWidth;
                    } else {
                        croppedSize = decoder.PixelHeight;
                    }

                    //Transform bitmap and cut out tile size image
                    BitmapBounds bounds = new BitmapBounds();
                    bounds.Height = croppedSize/4;
                    bounds.Width = croppedSize/4;
                    bounds.X = ((decoder.PixelWidth-croppedSize)/2) + ((uint)i * bounds.Width);
                    bounds.Y = ((decoder.PixelHeight-croppedSize)/2) + ((uint)j * bounds.Height);
                    encoder.BitmapTransform.Bounds = bounds;

                    try {
                        await encoder.FlushAsync();
                    } catch (Exception ex)  {
                        string s = ex.ToString();
                    }

                    BitmapImage tile = new BitmapImage();
                    tile.SetSource(ras);

                    Image img = new Image();
                    ImageBrush imgBrush = new ImageBrush();
                    img.Source = tile;
                    imgBrush.ImageSource = img.Source;

                    Rectangle rect = new Rectangle();
                    rect.Fill = imgBrush;

                    //Set grid position of each image
                    rect.SetValue(Grid.RowProperty, j);
                    rect.SetValue(Grid.ColumnProperty, i);

                    originalTiles.Add(rect);
                    puzzleGrid.Children.Add(rect);
                }
            }

            loadingImage = false;
            
            //game timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            timer.Tick += GameTimerTick;
            secondsElapsed = 0;
        }

        //Method      : loadNumberGame
        //Description : Loads a new tile puzzle with numbers
        //Parameters  : none
        //Returns     : void         
        private void loadNumberGame() {
            //todo
        }

        //Method      : RandomizeButton_Click
        //Description : Handler for randomize button click event, randomizes the tiles in the grid
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void RandomizeButton_Click(object sender, RoutedEventArgs e) {
            if (loadingImage){
                return;
            }

            int emptyX = 0;
            int emptyY = 0;

            //get empty pos
            bool done = false;
            for (int x = 0; x < numRowsAndCols; x++) {
                for (int y = 0; y < numRowsAndCols; y++) {
                    if (GetAtGridPos(puzzleGrid, x, y) == null){
                        emptyX = x;
                        emptyY = y;
                        done = true;
                        break;
                    }
                }
                if (done) {
                    break;
                }
            }

            Random r = new Random();
            for (int i = 0; i < 200 * numRowsAndCols; i++) {

                int dir = r.Next(4);//direction to move empty tile

                switch (dir) {
                    case (0)://move empty up(move tile above down)
                        if (emptyY > 0) {
                            var tile = GetAtGridPos(puzzleGrid, emptyX, emptyY - 1);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);

                            emptyY = emptyY - 1;
                        }
                        break;
                    case 1://move empty down
                        if (emptyY < (numRowsAndCols - 1)) {
                            var tile = GetAtGridPos(puzzleGrid, emptyX, emptyY + 1);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);

                            emptyY = emptyY + 1;
                        }
                        break;
                    case 2://move empty left
                        if (emptyX > 0){
                            var tile = GetAtGridPos(puzzleGrid, emptyX - 1, emptyY);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);

                            emptyX = emptyX - 1;
                        }
                        break;
                    case 3://move empty right
                        if (emptyX < (numRowsAndCols - 1))  {
                            var tile = GetAtGridPos(puzzleGrid, emptyX + 1, emptyY);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);

                            emptyX = emptyX + 1;
                        }
                        break;
                }

            }
        }

        //Method      : checkSolved
        //Description : Checks to see if the puzzle is solved
        //Parameters  : none
        //Returns     : void         
        private void checkSolved(){

            //Check if each tiles is in its orignal postion
            bool solved = true;
            for (int i = 0; i < originalTiles.Count - 1; i++){
                if (puzzleGrid.Children[i] != originalTiles[i]) {
                    solved = false;
                }
            }

            if (solved){
                namePopup.IsOpen = true;
                ((GetNamePopup)(namePopup.Child)).GotInput += (popupSender, name) => {
                    namePopup.IsOpen = false;
                    Frame.Navigate(typeof(LeaderboardPage), new LeaderboardScore(name, secondsElapsed));
                };
            }
        }

        //Method      : gamePageGrid_SizeChanged
        //Description : Loads a new tile puzzle with an image
        //Parameters  : object sender          - sender
        //              SizeChangedEventArgs e - event args
        //Returns     : void         
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

        //Method      : ChangePuzzleButton_Click
        //Description : Sends user back to setup page to change the current puzzle
        //Parameters  : object sender     - sender
        //              RoutedEventArgs e - event args 
        //Returns     : void         
        private async void ChangePuzzleButton_Click(object sender, RoutedEventArgs e){
            if(loadingImage) {
                return;
            }

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

        //Method      : CommandInvokedHandler
        //Description : Handler for when the message dialog receives a command
        //Parameters  : IUICommand command - command
        //Returns     : void         
        private void CommandInvokedHandler(IUICommand command)  {
            if (command.Label == "Yes") {
                this.Frame.Navigate(typeof(SetupPage));
            }
        }

        //Method      : GameTimerTick
        //Description : Handler for game timer tick event, updates timer
        //Parameters  : object sender - sender
        //              object e      - object
        //Returns     : void         
        private void GameTimerTick(object sender, object e)  {
            secondsElapsed++;
            timeText.Text = "Time Elapsed: " + secondsElapsed;
        }

        //Method      : puzzleGrid_Tapped
        //Description : Handler for puzzle grid tapped event, moves tile to empty grid space
        //Parameters  : object sender           - object
        //              TappedRoutedEventArgs e - event args   
        //Returns     : void                  
        private void puzzleGrid_Tapped(object sender, TappedRoutedEventArgs e) {
            if(loadingImage) {
                return;
            }

            Rectangle tile = (Rectangle)e.OriginalSource;
            int oldX = Grid.GetColumn(tile);
            int oldY = Grid.GetRow(tile);

            //try to move down
            try {
                if(GetAtGridPos(puzzleGrid, oldX, oldY+1) == null) {
                    tile.SetValue(Grid.ColumnProperty, oldX);
                    tile.SetValue(Grid.RowProperty, oldY+1);

                    //Check if puzzle is solved
                    checkSolved();
                    return;
                }
            } catch(IndexOutOfRangeException) {
            }

            //try to move up
            try {
                if(GetAtGridPos(puzzleGrid, oldX, oldY-1) == null) {
                    tile.SetValue(Grid.ColumnProperty, oldX);
                    tile.SetValue(Grid.RowProperty, oldY-1);

                    //Check if puzzle is solved
                    checkSolved();
                    return;
                }
            } catch(IndexOutOfRangeException) {
            }

            //try to move left
            try {
                if(GetAtGridPos(puzzleGrid, oldX-1, oldY) == null) {
                    tile.SetValue(Grid.ColumnProperty, oldX-1);
                    tile.SetValue(Grid.RowProperty, oldY);

                    //Check if puzzle is solved
                    checkSolved();
                    return;
                }
            } catch(IndexOutOfRangeException) {
            }

            //try to move right
            try {
                if(GetAtGridPos(puzzleGrid, oldX+1, oldY) == null) {
                    tile.SetValue(Grid.ColumnProperty, oldX+1);
                    tile.SetValue(Grid.RowProperty, oldY);

                    //Check if puzzle is solved
                    checkSolved();
                    return;
                }
            } catch(IndexOutOfRangeException) {
            }

            e.Handled = true;
        }

        //Method      : GetAtGridPos
        //Description : returns the element at a certain grid postion
        //Parameters  : Grid g - grid
        //              int x  - position x
        //              int y  - position y
        //Returns     : FrameworkElement       
        private static FrameworkElement GetAtGridPos(Grid g, int x, int y) {
            if(x >= g.ColumnDefinitions.Count || y >= g.RowDefinitions.Count || x < 0 || y < 0){
                throw new IndexOutOfRangeException("x and y must be withing range of column and row definitions of g");
            }

            FrameworkElement ret = null;
            foreach(FrameworkElement e in g.Children) {
                if(Grid.GetColumn(e) == x && Grid.GetRow(e) == y) {
                    ret = e;
                    break;
                }
            }
            return ret;
        }

    }
}
