/*
 * File: SetupPage.xaml.cs
 * Project: Windows and Mobile Programming - Final Project
 * Programmers: Adam Currie and Dylan O'Neill
 * First Version: 2015-12-08
 * Description: Contains the SetupPage class and the code behind the setup page of the project.
                The methods in this file allow the user to select the puzzle type (image or number).
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TilePuzzle {

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupPage : Page {

        public SetupPage() {
            this.InitializeComponent();
        }

        //Method      : loadImageButton_Click
        //Description : Handler for image button click event, lets user select image for game
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private async void loadImageButton_Click(object sender, RoutedEventArgs e) {
            if(ApplicationView.Value == ApplicationViewState.Snapped) {
                ApplicationView.TryUnsnap();
            }

            if(ApplicationView.Value == ApplicationViewState.Snapped) {
                MessageDialog messageDialog = new MessageDialog(
                    "Cannot open an image because the app is snapped and could not be unsnapped."
                );
                messageDialog.Commands.Add(new UICommand("Ok"));
                messageDialog.CancelCommandIndex = 0;
                await messageDialog.ShowAsync();
            }

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");//todo: pick supported types

            StorageFile file = await openPicker.PickSingleFileAsync();

            if(file != null) {
                this.Frame.Navigate(typeof(GamePage), file);
            } else {
                MessageDialog messageDialog = new MessageDialog("Cannot open image.");
                messageDialog.Commands.Add(new UICommand("Ok"));
                messageDialog.CancelCommandIndex = 0;
                await messageDialog.ShowAsync();
            }   
        }

        //Method      : numberedTilesButton_Click
        //Description : Handler for numbered tiles button click event, sends user to game page
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void numberedTilesButton_Click(object sender, RoutedEventArgs e){
            this.Frame.Navigate(typeof(GamePage));
        }
    }

}
