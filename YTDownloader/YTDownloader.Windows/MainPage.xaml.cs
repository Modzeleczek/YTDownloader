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
using System.Net;
using System.Threading.Tasks;

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

        private async Task<string> DownloadSourceAsync(string URL)
        {
            // create a new HTTP web request
            var http = (HttpWebRequest)WebRequest.Create(URL);
            // download webpage's response and save a reference to it
            var response = await http.GetResponseAsync();            
            // save a reference to the data stream associated with the response
            using (var stream = response.GetResponseStream())
            {
                // create a stream reader on response's stream
                var sr = new StreamReader(stream);
                // read all response's bytes as string
                return sr.ReadToEnd();
            }
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            string id = ExtractVideoID(URLTextBox.Text);
            // if ID was not extracted, print an error message and stop this method
            if (id == null)
            {
                FeedbackTextBox.Text = "Invalid YouTube URL.\r\n";
                return;
            }
            // download source of the webpage generated for specified video by the API
            // https://github.com/matthew-asuncion/Fast-YouTube-to-MP3-Converter-API
            string source = await DownloadSourceAsync("https://www.yt-download.org/api/button/mp3/" + id);
            // https://www.yt-download.org/api/button/videos/

            var indexes = source.AllIndexesOf("https://www.yt-download.org/download/");
            FeedbackTextBox.Text = "";
            //string[] downloadURLs = new string[indexes.Count];
            for(int i = 0; i < indexes.Count; ++i)
            {
                //downloadURLs[i] = "";
                int index = indexes[i];
                while (source[index] != '"')
                    FeedbackTextBox.Text += source[index++];
                FeedbackTextBox.Text += "\r\n";
            }
        }
    }

    public static class StringExtension
    {
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
    }
}
