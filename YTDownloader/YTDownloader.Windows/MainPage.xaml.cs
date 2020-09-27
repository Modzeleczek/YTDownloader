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

namespace YTDownloader
{
    /// <summary>
    /// Main and only program's page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Downloader DownloaderInstance = new Downloader();
        public MainPage()
        {
            this.InitializeComponent();
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
            ex is FormatException)
            {
                FeedbackTextBox.Text += ex.ToString();
                DownloadButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await (BitratesListBox.SelectedItem as MP3).DownloadAsync(KnownFolders.MusicLibrary);
                FeedbackTextBox.Text += "MP3 downloaded.";
            }
            catch (Exception ex) when (
            ex is ArgumentException ||
            ex is UnknownNameException ||
            ex is IOException ||
            ex is FileNotFoundException)
            {
                FeedbackTextBox.Text += ex.ToString();
            }
        }
    }
}
