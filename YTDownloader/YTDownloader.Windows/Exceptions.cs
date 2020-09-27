using System;

/// <summary>
/// Contains custom exception classes used by the program.
/// </summary>
namespace YTDownloader.Exceptions
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