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
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private DispatcherTimer timer = null;
        private int elapsedTime = 0;
        private bool loadingImage = false;
        private Rectangle[,] originalTiles = new Rectangle[numRowsAndCols, numRowsAndCols];

        public static bool CanContinue{
            get{
                try {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    localFolder.GetFileAsync("last_image").AsTask().Wait();
                } catch(Exception) {
                    return false;
                }

                if(localSettings.Values["time"] == null) {
                    return false;
                }

                if(localSettings.Values["positions"] == null) {
                    return false;
                }

                return true;
            }
        }

        public int SecondsElapsed{
            get{
                return elapsedTime;
            }
            set{
                elapsedTime = value;
                timeText.Text = "Time Elapsed: " + elapsedTime;
                localSettings.Values["time"] = elapsedTime;
            }
        }

        public GamePage() {
            this.InitializeComponent();
        }


        //Method      : OnNavigatedTo
        //Description : Handler for when this page is navigated to
        //Parameters  : NavigationEventArgs e - event args   
        //Returns     : void         
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            //If image was passed, create image puzzle, else create number puzzle
            if(e.Parameter != null) {
                GameNavigationEventArgs args = e.Parameter as GameNavigationEventArgs;

                if(args.continuing) {

                    try {

                        //load file
                        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                        StorageFile lastImage = await localFolder.GetFileAsync("last_image");
                        await LoadImageGameAsync(lastImage);

                        //load postions
                        string positionsString = (string)localSettings.Values["positions"];
                        if(positionsString != null) {
                            string[] positions = positionsString.Split('|');

                            for(int i = 0; i<positions.Length && i<puzzleGrid.Children.Count; i++) {
                                string[] pos = positions[i].Split(',');
                                if(pos.Length == 2) {
                                    puzzleGrid.Children[i].SetValue(Grid.ColumnProperty, int.Parse(pos[0]));
                                    puzzleGrid.Children[i].SetValue(Grid.RowProperty, int.Parse(pos[1]));
                                }
                            }

                        }

                        //load time
                        object timeObj = localSettings.Values["time"];
                        if(timeObj != null) {
                            SecondsElapsed = (int)timeObj;
                        } else {
                            SecondsElapsed = 0;
                        }
                        timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Start();
                        timer.Tick += GameTimerTick;
                        SecondsElapsed = 0;

                    } catch(FileNotFoundException) {
                        Uri uri = new Uri("ms-appx:///Assets/numbers.png", UriKind.RelativeOrAbsolute);
                        await LoadImageGameAsync(await StorageFile.GetFileFromApplicationUriAsync(uri));
                        RandomizeTiles(this, new RoutedEventArgs());
                    }

                } else if(args.file != null) {

                    await LoadImageGameAsync(args.file);
                    RandomizeTiles(this, new RoutedEventArgs());

                }
            } else {
                Uri uri = new Uri("ms-appx:///Assets/numbers.png", UriKind.RelativeOrAbsolute);
                await LoadImageGameAsync(await StorageFile.GetFileFromApplicationUriAsync(uri));
                RandomizeTiles(this, new RoutedEventArgs());
            }

            SetLiveTile();
        }

        //Method      : SetLiveTile
        //Description : saves snapshot of current game and uses it as live tile
        //Parameters  : none
        //Returns     : async void        
        private async void SetLiveTile() {
            
            //save tile image
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(puzzleGrid);
            IBuffer buffer = await rtb.GetPixelsAsync();
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "live-tile.png", 
                CreationCollisionOption.ReplaceExisting
            );
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                (uint)rtb.PixelWidth,
                (uint)rtb.PixelHeight, 96d, 96d,
                buffer.ToArray()
            );
            await encoder.FlushAsync();

            //setup tile notification
            TileTemplateType tileTemplate = TileTemplateType.TileSquare150x150Image;
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);
            TileNotification notification = new TileNotification(tileXml);

            //set image
            XmlNodeList nodes = tileXml.GetElementsByTagName("image");
            nodes[0].Attributes[1].NodeValue = "ms-appdata:///local/live-tile.png";

            //update tile
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Update(notification);
        }

        //Method      : loadImageGame
        //Description : Loads a new tile puzzle with an image
        //Parameters  : StorageFile file - image
        //Returns     : void              
        private async Task LoadImageGameAsync(StorageFile file) {
            if(loadingImage) {
                return;
            }

            loadingImage = true;

            //clear old
            puzzleGrid.Children.Clear();

            //split image into smaller images for puzzle
            for (int x = 0; x < numRowsAndCols; x++) {
                for (int y = 0; y < numRowsAndCols; y++) {
                    if(x == numRowsAndCols-1 && y == numRowsAndCols-1) {
                        //ignore last
                        break;
                    }

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
                    bounds.X = ((decoder.PixelWidth-croppedSize)/2) + ((uint)x * bounds.Width);
                    bounds.Y = ((decoder.PixelHeight-croppedSize)/2) + ((uint)y * bounds.Height);
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
                    rect.SetValue(Grid.RowProperty, y);
                    rect.SetValue(Grid.ColumnProperty, x);

                    originalTiles[x, y] = rect;
                }
            }

            for(int x = 0; x < numRowsAndCols; x++) {
                for(int y = 0; y < numRowsAndCols; y++) {
                    if(x == numRowsAndCols-1 && y == numRowsAndCols-1) {
                        //ignore last
                        break;
                    }
                    puzzleGrid.Children.Add(originalTiles[x, y]);
                }
            }

            loadingImage = false;

            //save copy of image
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFile savedCopy = null;
            try {
                savedCopy = await localFolder.GetFileAsync("last_image");
            } catch(Exception) {
                //if file doesnt exist create it
                savedCopy = await localFolder.CreateFileAsync(
                    "last_image",
                    CreationCollisionOption.ReplaceExisting
                );
                
            }
            if(file.Path != savedCopy.Path) {
                await file.CopyAndReplaceAsync(savedCopy);
            }
        }

        //Method      : RandomizeButton_Click
        //Description : Handler for randomize button click event, randomizes the tiles in the grid
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void RandomizeTiles(object sender, RoutedEventArgs e) {
            if(loadingImage) {
                return;
            }

            try {
                timer.Tick -= GameTimerTick;
                SecondsElapsed = 0;
            } catch(ArgumentException) { }//incase timer is not set


            int emptyX = 0;
            int emptyY = 0;

            //get empty pos
            bool done = false;
            for(int x = 0; x<numRowsAndCols; x++) {
                for(int y = 0; y<numRowsAndCols; y++) {
                    if(GetAtGridPos(puzzleGrid, x, y) == null) {
                        emptyX = x;
                        emptyY = y;
                        done = true;
                        break;
                    }
                }
                if(done) {
                    break;
                }
            }

            Random r = new Random();
            for(int i = 0; i<100*numRowsAndCols; i++) {

                int dir = r.Next(4);//direction to move empty tile

                switch(dir) {
                    case (0)://move empty up(move tile above down)
                        if(emptyY > 0) {
                            var tile = GetAtGridPos(puzzleGrid, emptyX, emptyY-1);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);
                            emptyY = emptyY-1;
                        }
                        break;
                    case 1://move empty down
                        if(emptyY < (numRowsAndCols-1)) {
                            var tile = GetAtGridPos(puzzleGrid, emptyX, emptyY+1);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);

                            emptyY = emptyY+1;
                        }
                        break;
                    case 2://move empty left
                        if(emptyX > 0) {
                            var tile = GetAtGridPos(puzzleGrid, emptyX-1, emptyY);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);

                            emptyX = emptyX-1;
                        }
                        break;
                    case 3://move empty right
                        if(emptyX < (numRowsAndCols-1)) {
                            var tile = GetAtGridPos(puzzleGrid, emptyX+1, emptyY);
                            tile.SetValue(Grid.ColumnProperty, emptyX);
                            tile.SetValue(Grid.RowProperty, emptyY);

                            emptyX = emptyX+1;
                        }
                        break;
                }
            }

            //game timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            timer.Tick += GameTimerTick;
            SecondsElapsed = 0;

            //save new positions
            SavePositions();
        }

        //Method      : savePositions
        //Description : saves the position of each tile
        //Parameters  : none
        //Returns     : void         
        private void SavePositions() {
            string posStr = "";
            foreach(FrameworkElement tile in puzzleGrid.Children) {
                posStr += Grid.GetColumn(tile);
                posStr += ",";
                posStr += Grid.GetRow(tile);
                posStr += "|";
            }
            localSettings.Values["positions"] = posStr;
        }

        //Method      : checkSolved
        //Description : Checks to see if the puzzle is solved
        //Parameters  : none
        //Returns     : void         
        private async void checkSolved(){

            //Check if each tiles is in its orignal postion
            bool solved = true;
            for(int x = 0; x<numRowsAndCols; x++) {
                for(int y = 0; y<numRowsAndCols; y++) {
                    if(x == numRowsAndCols-1 && y == numRowsAndCols-1) {
                        //ignore last
                        break;
                    }

                    if(Grid.GetColumn(originalTiles[x,y]) != x || Grid.GetRow(originalTiles[x, y]) != y) {
                        solved = false;
                        break;
                    }
                }
            }

            if (solved)  {
                timer.Stop();

                namePopup.IsOpen = true;
                ((GetNamePopup)(namePopup.Child)).GotInput += (popupSender, name) => {
                    namePopup.IsOpen = false;
                    Frame.Navigate(typeof(LeaderboardPage), new LeaderboardScore(name, SecondsElapsed));
                };

                //delete saved state
                localSettings.Values["time"] = null;
                localSettings.Values["positions"] = null;

                try {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    StorageFile savedCopy = await localFolder.GetFileAsync("last_image");
                    await savedCopy.DeleteAsync();
                } catch(FileNotFoundException) { }
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
            SecondsElapsed++;
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
                    SavePositions();
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
                    SavePositions();
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
                    SavePositions();
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
                    SavePositions();
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
