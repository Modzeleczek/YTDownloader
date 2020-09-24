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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace YTDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private string ExtractVideoID(string videoURL)
        {
            // https://www.youtube.com/watch?v=x16aMVCbmeg  standard URL
            // youtu.be/x16aMVCbmeg shortened URL

            string id;
            // check if the URL is a standard YouTube URL
            string soughtText = "youtube.com/watch?v=";
            int index = videoURL.LastIndexOf(soughtText);
            if (index != -1)
            {
                // get all characters after '='
                id = videoURL.Substring(index + soughtText.Length);
                // get the position of the first ampersand if any exists
                index = id.IndexOf("&");
                // get all characters from start to the ampersand if it exists
                if (index != -1)
                    id = id.Substring(0, index);
            }
            // otherwise, check if the URL is a shortened YouTube URL
            else
            {
                soughtText = "youtu.be/";
                index = videoURL.LastIndexOf(soughtText);
                if (index != -1)
                    id = videoURL.Substring(index + soughtText.Length);
                // otherwise, the URL is not a recognizable YouTube video URL
                else
                    return null;
            }
            return id;
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            string id = ExtractVideoID(URLTextBox.Text);
            // if ID was not extracted, print an error message and stop this method
            if (id == null)
            {
                FeedbackTextBox.Text = "Invalid YouTube URL.\r\n";
                return;
            }
            // generate download buttons in WebView by navigating to API URI (https://github.com/matthew-asuncion/Fast-YouTube-to-MP3-Converter-API)
            Browser.Navigate(new Uri("https://www.yt-download.org/api/button/mp3/" + id));
            // https://www.yt-download.org/api/button/videos/
        }

        private async void Browser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
                // print a success message
                FeedbackTextBox.Text = "Navigation successful.\r\n";
            else
                // print an error message
                FeedbackTextBox.Text = $"Navigation failed.\r\n{args.Uri}\r\n{args.WebErrorStatus}\r\n";

            FeedbackTextBox.Text += await Browser.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" }) + "\r\n";
        }
    }
}
