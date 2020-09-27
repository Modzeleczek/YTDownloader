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
                {
                    FeedbackTextBox.Text += $"{mp3}\r\n";
                    BitratesListBox.Items.Add(mp3);
                }
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

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            (BitratesListBox.SelectedItem as MP3).DownloadAsync(KnownFolders.MusicLibrary);
            FeedbackTextBox.Text += "MP3 downloaded.";
        }
    }

    /// <summary>
    /// Represents a YouTube sound downloader.
    /// </summary>
    public class Downloader
    {
        /// <summary>
        /// Returns an array of read-only MP3 objects.
        /// </summary>
        /// <param name="videoUrl">URL of the YouTube video, whose sound should be downloaded.</param>
        /// <returns>Array of read-only MP3 objects.</returns>
        public async Task<MP3[]> GetMP3sAsync(string videoUrl)
        {
            string id = ExtractVideoID(videoUrl);
            // an ArgumentException is thrown if ID was not extracted

            // download source of the webpage generated for specified video by the API
            // https://github.com/matthew-asuncion/Fast-YouTube-to-MP3-Converter-API
            string source = await DownloadSourceAsync("https://www.yt-download.org/api/button/mp3/" + id);
            // an System.UriFormatException is thrown if the URL is invalid
            // to download videos instead of MP3s: "https://www.yt-download.org/api/button/videos/" + id

            // find download URLs
            var urls = source.SubstringsBeginningAndEnding("https://www.yt-download.org/download/", '"');
            if (urls.Count == 0)
                throw new DownloaderAPIException("No download URLs were generated.");

            // construct an array containing videos
            var videos = new List<MP3>();
            // example download URL: https://www.yt-download.org/download/C4dbd_dynKs/mp3/320/1601086910/ee4995636533712c10fad8d28a95b4966cb17d3e0a08f0e38da6905a86076ef1/0
            for (int i = 0; i < urls.Count; ++i)
            {
                var urlParts = urls[i].Split('/');
                if (urlParts.Length >= 7)
                {
                    videos.Add(new MP3(urls[i], int.Parse(urlParts[6])));
                    // a FormatException is thrown if the bitrate part is not a valid integer
                }
            }
            return videos.ToArray();
        }

        private string ExtractVideoID(string videoURL)
        {
            // standard URL example: https://www.youtube.com/watch?v=x16aMVCbmeg
            // short URL example: youtu.be/x16aMVCbmeg

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
                    throw new ArgumentException("Invalid YouTube URL.", "url");
            }
            return id;
        }

        private async Task<string> DownloadSourceAsync(string url)
        {
            // create a new HTTP web request
            var http = (HttpWebRequest) WebRequest.Create(url);
            // download webpage's response and save a reference to it
            using (var response = await http.GetResponseAsync())
            // save a reference to the data stream associated with the response
            using (var stream = response.GetResponseStream())
            // create a stream reader on response's stream
            using (var sr = new StreamReader(stream))
                // read all response's bytes as string
                return sr.ReadToEnd();
        }
    }

    /// <summary>
    /// Represents an MP3 file before it is downloaded.
    /// </summary>
    public class MP3
    {
        public string URL { get; }
        public int Bitrate { get; }

        public MP3(string url, int bitrate)
        {
            URL = url;
            Bitrate = bitrate;
        }

        public override string ToString() => Bitrate.ToString();

        /// <summary>
        /// Downloads and saves the MP3 in specified folder.
        /// </summary>
        /// <param name="folder">The folder, where the MP3 file will be saved.</param>
        public async void DownloadAsync(StorageFolder folder)
        {
            if (folder == null)
                throw new ArgumentException("Folder cannot be null.", "folder");

            // download the file from its URL
            var http = (HttpWebRequest)WebRequest.Create(URL);
            var response = await http.GetResponseAsync();
            // get the response's header, which contains the name of the file
            // header example: attachment; filename="Trance Allstars - Lost in Love (128 kbps).mp3"
            string header = response.Headers["Content-Disposition"];

            // retrieve the name of the file from the header
            var substrings = header.SubstringsBeginningAndEnding("filename=\"", '"');
            if (substrings.Count == 0)
                throw new UnknownNameException("No MP3 file's name was found in response's " +
                    $"'Content-Disposition' header: '{header}'.");
            var fileName = substrings.FirstOrDefault().Substring("filename=\"".Length);

            // build the file from bytes copied from the response
            using (var responseStream = response.GetResponseStream())
            {
                // create an empty file in the folder
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                if (file == null)
                    throw new IOException("Cannot create a file to save the downloaded bytes.");
                // prepare a stream used to fill the created file
                using (var fileWriteStream = await file.OpenStreamForWriteAsync())
                    // fill the file copying bytes from the response stream
                    responseStream.CopyTo(fileWriteStream);
            }
        }
    }

    /// <summary>
    /// Contains custom exception classes used by the program.
    /// </summary>
    namespace Exceptions
    {
        public class UnknownNameException : Exception
        {
            public UnknownNameException() { }
            public UnknownNameException(string message) : base(message) { }
        }
        public class IncompleteURLException : Exception
        {
            public IncompleteURLException() { }
            public IncompleteURLException(string message) : base(message) { }
        }
        public class DownloaderAPIException : Exception
        {
            public DownloaderAPIException() { }
            public DownloaderAPIException(string message) : base(message) { }
        }
    }

    /// <summary>
    /// Constains extension methods of String class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a list containing indexes of all beginnings of the specified string.
        /// </summary>
        /// <param name="str">String, where to search.</param>
        /// <param name="value">String, whose occurrences' positions should be returned.</param>
        /// <returns>List containing indexes of all beginnings of the specified string.</returns>
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
        /// <returns>List of substrings, which begin with specified string and end with specified character.</returns>
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
