using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace WebApplication12.ImageCompression
{
    public class ImageCompressor
    {
        static readonly string[] _supported = { ".png", ".jpg", ".jpeg", ".gif" };
        string _cwd;

        public ImageCompressor()
        {
            _cwd = System.Web.HttpContext.Current.Server.MapPath("~/ImageCompression/Tools/");
        }

        public ImageCompressionResult CompressFile(string fileName, bool lossy)
        {
            string targetFile = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(fileName));

            var start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = _cwd,
                Arguments = GetArguments(fileName, targetFile, lossy),
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var stopwatch = Stopwatch.StartNew();

            using (var process = Process.Start(start))
            {
                process.WaitForExit();
            }

            stopwatch.Stop();

            var result = new ImageCompressionResult(fileName, targetFile, stopwatch.Elapsed);

            HandleResult(result, 1);

            return result;
        }

        private static string GetArguments(string sourceFile, string targetFile, bool lossy)
        {
            if (!Uri.IsWellFormedUriString(sourceFile, UriKind.RelativeOrAbsolute) && !File.Exists(sourceFile))
                return null;

            string ext;

            try
            {
                ext = Path.GetExtension(sourceFile).ToLowerInvariant();
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return null;
            }

            switch (ext)
            {
                case ".png":
                    File.Copy(sourceFile, targetFile);

                    if (lossy)
                        return string.Format(CultureInfo.CurrentCulture, "/c pingo -s8 -pngpalette=79 -q \"{0}\"", targetFile);
                    else
                        return string.Format(CultureInfo.CurrentCulture, "/c pingo -auto=100 -s8 -q \"{0}\"", targetFile);

                case ".jpg":
                case ".jpeg":
                    if (lossy)
                        return string.Format(CultureInfo.CurrentCulture, "/c cjpeg -quality 80,60 -dct float -smooth 5 -outfile \"{1}\" \"{0}\"", sourceFile, targetFile);
                    else
                        return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy none -optimize -progressive -outfile \"{1}\" \"{0}\"", sourceFile, targetFile);
                //return string.Format(CultureInfo.CurrentCulture, "/c guetzli_windows_x86-64 \"{0}\" \"{1}\"", sourceFile, targetFile);

                case ".gif":
                    return string.Format(CultureInfo.CurrentCulture, "/c gifsicle -O3 \"{0}\" --output=\"{1}\"", sourceFile, targetFile);
            }

            return null;
        }

        public static bool IsFileSupported(string fileName)
        {
            string ext = Path.GetExtension(fileName);

            return _supported.Any(s => s.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        void HandleResult(ImageCompressionResult result, int count)
        {
            string name = Path.GetFileName(result.OriginalFileName);

            if (result.Saving > 0 && result.ResultFileSize > 0 && File.Exists(result.ResultFileName))
            {
                File.Copy(result.ResultFileName, result.OriginalFileName, true);
                File.Delete(result.ResultFileName);
            }
        }
    }
}