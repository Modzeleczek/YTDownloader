using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTDownloader.Exceptions;
using System.Net;
using System.IO;

namespace YTDownloader
{
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
            var http = (HttpWebRequest)WebRequest.Create(url);
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
}
