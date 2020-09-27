using System;
using System.Linq;
using Windows.Storage;
using System.Threading.Tasks;
using YTDownloader.Exceptions;
using System.Net;
using System.IO;

namespace YTDownloader
{
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
        public async Task DownloadAsync(StorageFolder folder)
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
}
