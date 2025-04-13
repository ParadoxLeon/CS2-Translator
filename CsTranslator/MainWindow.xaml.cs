using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Controls;
using CsTranslator.Controllers;
using CsTranslator.Enums;
using CsTranslator.EventArgs;
using CsTranslator.Exceptions;
using CsTranslator.Helpers;
using CsTranslator.Models;

namespace CsTranslator
{
    public partial class MainWindow
    {
        private readonly DispatcherTimer _checkTimer = new DispatcherTimer();
        
        public static EventHandler<TranslatorExceptionEventArgs> ErrorEncountered;
        public static EventHandler<TranslatorExceptionEventArgs> Succeeded;

        public MainWindow()
        {
            InitializeComponent();
            CheckForUpdates();

            ErrorEncountered += OnErrorEncountered;
            Succeeded += OnSucceeded;

            LogsController.Logs = new LinkedList<Log>();
            LogsController.Chats = new List<Chat>();

            Loaded += LoadWindow;
        }

        private void OnErrorEncountered(object sender, TranslatorExceptionEventArgs args)
        {
           Dispatcher.Invoke(() => { LblError.Content = args.Exception.Message; }); 
        }
        
        private void OnSucceeded(object sender, TranslatorExceptionEventArgs e)
        { 
            Dispatcher.Invoke(() => {
                if ((string) LblError.Content == e.Exception.Message)
                    LblError.Content = "";
            });
        }

        private void LoadWindow(object sender, RoutedEventArgs e)
        {
            ChatView.ItemsSource = LogsController.Chats;

            _checkTimer.Interval = TimeSpan.FromSeconds(3);
            _checkTimer.Tick += TimerTick;
            _checkTimer.Start();

            OptionsManager.ValidateSettings();
        }

        private async void TimerTick(object sender, System.EventArgs e)
        {
            await LogsController.LoadLogsAsync(30);
            ChatView.Items.Refresh();
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();
            LogsController.Chats.Clear();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ChatView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatView.SelectedItem is Chat selectedChat && selectedChat.Translation != null)
            {
                Clipboard.SetText(selectedChat.Translation.Message);
            }
        }

        private async void CheckForUpdates()
        {
            VersionChecker versionChecker = new VersionChecker();
            bool isNewVersionAvailable = await versionChecker.CheckForUpdates();

            if (isNewVersionAvailable)
            {
                // Display a notification to the user
                updateNotification.Text = "A new version is available. Click here to update.";
                updateNotification.Visibility = Visibility.Visible;
                dismissUpdate.Visibility = Visibility.Visible;
            }
        }

        private void UpdateNotification_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string downloadLink = "https://github.com/ParadoxLeon/CS2-Translator/releases";
            try
            {
                Process.Start(new ProcessStartInfo(downloadLink) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening the download link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dismissUpdate_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            updateNotification.Visibility = Visibility.Collapsed;
            dismissUpdate.Visibility = Visibility.Collapsed;
        }

        private void ListViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is Chat chat)
            {
                if (!string.IsNullOrEmpty(chat.Translation.Message))
                {
                    Clipboard.SetText(chat.Translation.Message);

                    CopyNotification.Visibility = Visibility.Visible;

                    item.IsSelected = false;

                    Task.Delay(1500).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => CopyNotification.Visibility = Visibility.Collapsed);
                    });
                }
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
