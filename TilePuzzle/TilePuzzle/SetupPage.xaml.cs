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
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
                //todo: pass image object to next page
                this.Frame.Navigate(typeof(GamePage));
            } else {
                MessageDialog messageDialog = new MessageDialog("Cannot open image.");
                messageDialog.Commands.Add(new UICommand("Ok"));
                messageDialog.CancelCommandIndex = 0;
                await messageDialog.ShowAsync();
            }   
        }

    }

}
