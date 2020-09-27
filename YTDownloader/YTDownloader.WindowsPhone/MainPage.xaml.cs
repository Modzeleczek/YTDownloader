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
using Windows.Storage;
using YTDownloader.Exceptions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace YTDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Downloader DownloaderInstance = new Downloader();
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BitratesListBox.Items.Count != 0)
                    BitratesListBox.Items.Clear();

                var mp3s = await DownloaderInstance.GetMP3sAsync(URLTextBox.Text);

                FeedbackTextBox.Text = "";
                foreach (var mp3 in mp3s)
                    BitratesListBox.Items.Add(mp3);
                BitratesListBox.SelectedIndex = 0;
                DownloadButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex) when (
            ex is DownloaderAPIException ||
            ex is ArgumentException ||
            ex is UnknownNameException ||
            ex is IOException)
            {
                FeedbackTextBox.Text += ex.ToString();
                DownloadButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            await (BitratesListBox.SelectedItem as MP3).DownloadAsync(KnownFolders.MusicLibrary);
            FeedbackTextBox.Text += "MP3 downloaded.";
        }
    }
}
