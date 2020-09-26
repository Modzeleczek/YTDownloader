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
using Windows.Storage;

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
            // create a stream reader on response's stream
            using (var sr = new StreamReader(stream))
            {
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

            // find download URLs
            var urls = source.SubstringsBeginningAndEnding("https://www.yt-download.org/download/", '"');
            if (urls.Count == 0)
            {
                FeedbackTextBox.Text = "No download URLs were generated.\r\n";
                return;
            }
            // select the last generated URL (it is the one with 128 kbps MP3)
            var url = urls[urls.Count - 1];

            // download the selected MP3 from its URL
            var http = (HttpWebRequest)WebRequest.Create(url);
            var response = await http.GetResponseAsync();
            // get the response's header, which contains the name of the MP3
            // attachment; filename="Trance Allstars - Lost in Love (128 kbps).mp3"
            string header = response.Headers["Content-Disposition"];

            // retrieve the name of the MP3 from the header
            const string soughtText = "filename=\"";
            int beginning = header.IndexOf(soughtText);
            if (beginning == -1)
            {
                FeedbackTextBox.Text += $"'{soughtText}' was not found in response's 'Content-Disposition' header: {header}.\r\n";
                return;
            }
            beginning += soughtText.Length;
            int end = header.IndexOf('"', beginning);
            if (end == -1)
            {
                FeedbackTextBox.Text += $"No ending '\"' was found in response's 'Content-Disposition' header: {header}.\r\n";
                return;
            }
            string fileName = header.Substring(beginning, end - beginning);
            if(fileName.Length == 0)
            {
                FeedbackTextBox.Text += "File name stored in header is empty.\r\n";
                return;
            }

            // build the MP3 file from bytes copied from the response
            using (var responseStream = response.GetResponseStream())
            {
                // get Music folder
                var folder = KnownFolders.MusicLibrary;
                if (folder == null)
                {
                    FeedbackTextBox.Text += "Music folder is inaccessible.\r\n";
                    return;
                }
                // create an empty .mp3 file in the Music folder
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                if (file == null)
                {
                    FeedbackTextBox.Text += "Cannot create a file to save the downloaded MP3.\r\n";
                    return;
                }
                // prepare a stream used to fill the .mp3 file
                var fileWriteStream = await file.OpenStreamForWriteAsync();
                // fill the file copying bytes from the response stream
                responseStream.CopyTo(fileWriteStream);
                FeedbackTextBox.Text += "Successfully downloaded MP3.\r\n";
            }
        }
    }

    public static class StringExtension
    {
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("The string to find may not be empty.", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        /// <summary>
        /// Returns a list of substrings, which begin with specified string and end with specified character.
        /// </summary>
        /// <param name="str">String, where to search substrings.</param>
        /// <param name="beginning">String, which has to be at the beginning of a returned substring.</param>
        /// <param name="end">Character, which has to be the first character after a returned substring.</param>
        /// <returns>List of substrings, which begin and end with specified strings.</returns>
        public static List<string> SubstringsBeginningAndEnding(this string str, string beginning, char end)
        {
            if (String.IsNullOrEmpty(beginning))
                throw new ArgumentException("The string at the beginning may not be empty.", "beginning");
            List<string> substrings = new List<string>();
            for (int index = 0; ; )
            {
                index = str.IndexOf(beginning, index);
                if (index == -1)
                    return substrings;
                int endIndex = str.IndexOf(end, index + beginning.Length);
                if (endIndex == -1)
                    return substrings;
                substrings.Add(str.Substring(index, endIndex - index));
                index = endIndex;
            }
        }
    }
}
