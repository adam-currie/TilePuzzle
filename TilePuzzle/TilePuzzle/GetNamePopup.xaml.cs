/*
 * File: GetNamePopup.xaml.cs
 * Project: Windows and Mobile Programming - Final Project
 * Programmers: Adam Currie and Dylan O'Neill
 * First Version: 2015-12-12
 * Description: Contains the GetNamePopup class and the code behind the NamePopup page of 
                the project. The methods in this file are used to get the users name so that
                we can add it to the leaderboard.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TilePuzzle {

    public sealed partial class GetNamePopup : UserControl {
        public event EventHandler<string> GotInput;

        public GetNamePopup() {
            this.InitializeComponent();

            okButton.Click += OkClicked;
        }

        //Method      : OkClicked
        //Description : Handler for Ok clicked event, get users input
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void OkClicked(object sender, RoutedEventArgs e) {
            if(nameTextBox.Text != null) {
                string name = nameTextBox.Text.Trim();
                if(name != "") {
                    GotInput(sender, name);
                }
            }
        }

        //Method      : nameTextBox_KeyDown
        //Description : Handler if user presses enter in the nameTextbox
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void nameTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
            if(e.Key == Windows.System.VirtualKey.Enter) {
                if(nameTextBox.Text != null) {
                    string name = nameTextBox.Text.Trim();
                    if (name != "") {
                        GotInput(sender, nameTextBox.Text);
                    }
                }
            }
        }
    }
}
