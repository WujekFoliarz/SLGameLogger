namespace SLGameLogger
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Exiled.API.Features;

    public class HttpFileServer : Plugin<Config>
    {
        private const int CopyBufferSize = 81920; // 80 KB
        private readonly HttpListener _listener = new HttpListener();
        private readonly SemaphoreSlim _concurrencyLimiter;
        private CancellationTokenSource? _cts;
        private Task? _acceptLoop;

        public HttpFileServer()
        {
            FilesRoot = Path.GetFullPath(Path.IsPathRooted(Plugin.Instance?.LogDirectory) ? Plugin.Instance?.LogDirectory : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Plugin.Instance?.LogDirectory));

            ListenerPrefix = $"http://{Plugin.Instance?.Config.ListenHost}:{Plugin.Instance?.Config.Port}/";
            _concurrencyLimiter = new SemaphoreSlim(Math.Max(1, Config.MaxConcurrentRequests));
        }

        public string FilesRoot { get; }

        internal string ListenerPrefix { get; }

        internal void Start()
        {
            Directory.CreateDirectory(FilesRoot);

            _listener.Prefixes.Clear();
            _listener.Prefixes.Add(ListenerPrefix);
            _listener.Start();

            _cts = new CancellationTokenSource();
            _acceptLoop = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        internal void Stop()
        {
            _cts?.Cancel();

            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            {

            }

            try
            {
                _acceptLoop?.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {

            }
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                HttpListenerContext context;

                try
                {
                    context = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch when (token.IsCancellationRequested || !_listener.IsListening)
                {
                    return;
                }
                catch (Exception ex)
                {
                    if (Config.Debug)
                        Log.Warn($"[FileServer] GetContext error: {ex.Message}");
                    continue;
                }

                _ = HandleContextAsync(context, token);
            }
        }

        private async Task HandleContextAsync(HttpListenerContext context, CancellationToken token)
        {
            await _concurrencyLimiter.WaitAsync(token).ConfigureAwait(false);

            try
            {
                await RouteAsync(context, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Config.Debug)
                    Log.Warn($"[FileServer] Request error: {ex}");

                TrySetStatus(context.Response, 500, "Internal Server Error");
            }
            finally
            {
                _concurrencyLimiter.Release();

                try
                {
                    context.Response.OutputStream.Close();
                }
                catch
                {

                }
            }
        }

        private async Task RouteAsync(HttpListenerContext context, CancellationToken token)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.HttpMethod != "GET" && request.HttpMethod != "HEAD")
            {
                TrySetStatus(response, 405, "Method Not Allowed");
                return;
            }

            var path = request.Url.AbsolutePath.TrimStart('/');

            if (path.Equals("motd", StringComparison.OrdinalIgnoreCase))
            {
                var motd = Plugin.Instance?.Config?.Motd ?? string.Empty;
                await WriteTextAsync(response, motd).ConfigureAwait(false);
                return;
            }

            if (path.Equals("files", StringComparison.OrdinalIgnoreCase))
            {
                await WriteFileListAsync(response).ConfigureAwait(false);
                return;
            }

            const string filePrefix = "files/";
            if (path.StartsWith(filePrefix, StringComparison.OrdinalIgnoreCase))
            {
                await ServeFileAsync(context, path.Substring(filePrefix.Length), token).ConfigureAwait(false);
                return;
            }

            TrySetStatus(response, 404, "Not Found");
        }

        private Task WriteFileListAsync(HttpListenerResponse response)
        {
            var sb = new StringBuilder();
            sb.Append('[');

            var files = Directory.GetFiles(FilesRoot)
                .OrderByDescending(File.GetLastWriteTime)
                .ToArray();

            bool first = true;

            foreach (var file in files)
            {
                var info = new FileInfo(file);

                if (info.Name == Plugin.Instance?.CurrentLogFileName)
                    continue;

                if (!first)
                    sb.Append(',');

                sb.Append('{')
                  .Append("\"name\":\"").Append(JsonEscape(info.Name)).Append("\",")
                  .Append("\"sizeBytes\":").Append(info.Length).Append(',')
                  .Append("\"lastModifiedUtc\":\"")
                  .Append(info.LastWriteTimeUtc.ToString("o"))
                  .Append('"')
                  .Append('}');

                first = false;
            }

            sb.Append(']');

            response.ContentType = "application/json";
            return WriteTextAsync(response, sb.ToString());
        }


        private async Task ServeFileAsync(HttpListenerContext context, string rawName, CancellationToken token)
        {
            var request = context.Request;
            var response = context.Response;

            var safeName = Path.GetFileName(Uri.UnescapeDataString(rawName));
            if (string.IsNullOrWhiteSpace(safeName))
            {
                TrySetStatus(response, 400, "Bad Request");
                return;
            }

            var fullPath = Path.GetFullPath(Path.Combine(FilesRoot, safeName));
            if (!fullPath.StartsWith(FilesRoot, StringComparison.Ordinal))
            {
                TrySetStatus(response, 400, "Bad Request");
                return;
            }

            var fileInfo = new FileInfo(fullPath);
            if (!fileInfo.Exists)
            {
                TrySetStatus(response, 404, "Not Found");
                return;
            }

            var totalLength = fileInfo.Length;

            response.AddHeader("Accept-Ranges", "bytes");
            response.AddHeader("Last-Modified", fileInfo.LastWriteTimeUtc.ToString("R", CultureInfo.InvariantCulture));
            response.ContentType = MimeMap.GetContentType(fileInfo.Extension);

            var download = request.QueryString["download"];
            if (!string.IsNullOrEmpty(download) &&
                download != "0" &&
                !download.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                response.AddHeader("Content-Disposition", $"attachment; filename=\"{safeName}\"");
            }

            long start = 0, end = totalLength - 1;
            var isRangeRequest = false;

            var rangeHeader = request.Headers["Range"];
            if (!string.IsNullOrEmpty(rangeHeader) && TryParseRange(rangeHeader, totalLength, out start, out end))
            {
                isRangeRequest = true;
                response.StatusCode = 206;
                response.StatusDescription = "Partial Content";
                response.AddHeader("Content-Range", $"bytes {start}-{end}/{totalLength}");
            }
            else
            {
                response.StatusCode = 200;
            }

            var contentLength = end - start + 1;
            response.ContentLength64 = contentLength;

            if (request.HttpMethod == "HEAD")
                return;

            var buffer = new byte[CopyBufferSize];

            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, useAsync: true))
            {
                if (isRangeRequest)
                    fs.Seek(start, SeekOrigin.Begin);

                var remaining = contentLength;

                try
                {
                    while (remaining > 0)
                    {
                        var toRead = (int)Math.Min(CopyBufferSize, remaining);
                        var read = await fs.ReadAsync(buffer, 0, toRead, token).ConfigureAwait(false);
                        if (read <= 0)
                            break;

                        await response.OutputStream.WriteAsync(buffer, 0, read, token).ConfigureAwait(false);
                        remaining -= read;
                    }
                }
                catch (HttpListenerException)
                {
                    
                }
                catch (IOException)
                {

                }
            }
        }

        private static bool TryParseRange(string rangeHeader, long totalLength, out long start, out long end)
        {
            start = 0;
            end = totalLength - 1;

            // Only single-range "bytes=start-end" requests are supported.
            if (!rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                return false;

            var spec = rangeHeader.Substring("bytes=".Length).Split(',')[0].Trim();
            var parts = spec.Split('-');
            if (parts.Length != 2)
                return false;

            if (string.IsNullOrEmpty(parts[0]))
            {
                // Suffix range, e.g. "-500" => last 500 bytes.
                if (!long.TryParse(parts[1], out var suffixLength) || suffixLength <= 0)
                    return false;

                start = Math.Max(0, totalLength - suffixLength);
                end = totalLength - 1;
                return true;
            }

            if (!long.TryParse(parts[0], out start))
                return false;

            if (string.IsNullOrEmpty(parts[1]))
            {
                end = totalLength - 1;
            }
            else if (!long.TryParse(parts[1], out end))
            {
                return false;
            }

            return start >= 0 && end < totalLength && start <= end;
        }

        private static void TrySetStatus(HttpListenerResponse response, int code, string description)
        {
            try
            {
                response.StatusCode = code;
                response.StatusDescription = description;
            }
            catch
            {
                // Response may already be sent/closed.
            }
        }

        private static async Task WriteTextAsync(HttpListenerResponse response, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        private static string JsonEscape(string value) =>
            value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}