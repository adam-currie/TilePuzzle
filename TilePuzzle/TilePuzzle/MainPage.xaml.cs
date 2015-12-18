/*
 * File: MainPage.xaml.cs
 * Project: Windows and Mobile Programming - Final Project
 * Programmers: Adam Currie and Dylan O'Neill
 * First Version: 2015-12-08
 * Description: Contains the MainPage class and the code behind the main page of the project.
                This file contains the methods for the main menu of the game and allow the user to
                navigate to other pages or quit the game.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TilePuzzle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {

        public MainPage() {
            this.InitializeComponent();
        }

        //Method      : newGameButton_Click
        //Description : Handler for new game button click event, sends user to setup page 
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void newGameButton_Click(object sender, RoutedEventArgs e) {
            this.Frame.Navigate(typeof(SetupPage));
        }

        //Method      : leaderboardButton_Click
        //Description : Handler for leaderboard button click event, sends user to the leaderboard page
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void leaderboardButton_Click(object sender, RoutedEventArgs e) {
            this.Frame.Navigate(typeof(LeaderboardPage));
        }

        //Method      : quitButton_Click
        //Description : Handler for quit button click event, exits the application
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void         
        private void quitButton_Click(object sender, RoutedEventArgs e)  {
            Application.Current.Exit();
        }

        //Method      : Page_Loaded
        //Description : This method lets the user continue from the saved game state if their game was terminated or suspended
        //Parameters  : object sender     - object
        //              RoutedEventArgs e - event args   
        //Returns     : void    
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            if(GamePage.CanContinue) {
                Button btn = new Button() { Content = "Continue" };

                btn.Margin = newGameButton.Margin;//copy margin of other buttons

                btn.HorizontalAlignment = HorizontalAlignment.Stretch;

                btn.Click += (s,ev) => { Frame.Navigate(typeof(GamePage), new GameNavigationEventArgs(null, true)); };

                mainPanel.Children.Insert(0, btn);
            }

            mainPanel.Height = (newGameButton.ActualHeight + newGameButton.Margin.Bottom) * mainPanel.Children.Count - newGameButton.Margin.Bottom;
        }
    }
}
