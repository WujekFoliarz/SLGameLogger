namespace SLGameLogger
{
    using System.Collections.Generic;

    internal static class MimeMap
    {
        private static readonly Dictionary<string, string> Types = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            [".zip"] = "application/zip",
            [".rar"] = "application/vnd.rar",
            [".7z"] = "application/x-7z-compressed",
            [".tar"] = "application/x-tar",
            [".gz"] = "application/gzip",
            [".json"] = "application/json",
            [".txt"] = "text/plain",
            [".log"] = "text/plain",
            [".csv"] = "text/csv",
            [".xml"] = "application/xml",
            [".pdf"] = "application/pdf",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp",
            [".mp3"] = "audio/mpeg",
            [".wav"] = "audio/wav",
            [".mp4"] = "video/mp4",
            [".mkv"] = "video/x-matroska",
            [".exe"] = "application/vnd.microsoft.portable-executable",
            [".dll"] = "application/octet-stream",
            [".bin"] = "application/octet-stream",
        };

        internal static string GetContentType(string extension) =>
            Types.TryGetValue(extension, out var contentType) ? contentType : "application/octet-stream";
    }
}