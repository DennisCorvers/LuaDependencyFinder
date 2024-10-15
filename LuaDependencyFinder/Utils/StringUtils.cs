namespace LuaDependencyFinder.Utils
{
    internal static class StringUtils
    {
        public delegate void ReadOnlySpanAction<T>(ReadOnlySpan<T> span, int lineNumber);

        public static void SpanToLines(string text, ReadOnlySpanAction<char> processedLine)
        {
            var span = text.AsSpan();
            int start = 0;
            int line = 0;
            for (int i = 0; i < text.Length; i++)
            {
                // Check for line break
                if (text[i] == '\n')
                {
                    processedLine(span[start..i], line);

                    // Update start for the next line
                    start = i + 1;
                    line += 1;
                }
            }

            if (start < text.Length)
            {
                processedLine(span.Slice(start), line);
            }
        }

        public static bool IsValidUrl(string url)
        {
            try
            {
                Uri uri = new(url);
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static string GetFileRoot(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            return Path.GetDirectoryName(path)!;
        }
    }
}
