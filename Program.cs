using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageToXNB
{
    class Program
    {
        static void Main(string[] args)
        {
            var argsList = new List<string>(args);

            bool recurse = false;

            if (args.Length < 1)
            {
                Console.WriteLine("ImageToXNB [-r] <source> [<target>]\n");
                Console.WriteLine("Supported formats BMP, GIF, JPEG, PNG, TIFF");
                Console.WriteLine("\nSource can be a directory. When source is a directory, target is ignored.");
                Console.WriteLine("\n\t-r\tWhen source is a directory, recurse subdirectories");

                return;
            }

            if (argsList.Any(x => x == "-r"))
            {
                recurse = true;
                argsList.Remove("-r");
            }

            var sourceFilenames = new List<string>();

            var sourcePath = argsList[0];
            var targetPath = argsList[0];
            if (Directory.Exists(sourcePath))
            {
                sourceFilenames = GetFilesInDirectory(new string[] { "png", "jpg", "tiff", "tif", "gif", "bmp" }, sourcePath, recurse);
            } else
            {
                sourceFilenames.Add(sourcePath);
            }

            var targetFilenames = new List<string>();
            if (recurse || !recurse && args.Length == 1)
            {
                if (args.Length == 3) targetPath = argsList[2];

                targetFilenames = GetTargetFilenames(sourceFilenames, sourcePath, targetPath);
            }
            else if (!recurse && args.Length == 2)
            {
                targetFilenames.Add(argsList[1]);
            }

            var sourceTargetPairs = sourceFilenames.Select((x, i) => new { Source = x, Target = targetFilenames[i] });

            Console.WriteLine("Converting " + sourcePath + " to " + targetPath + (recurse ? "\nWill recurse subdirectories" : ""));
            foreach (var pair in sourceTargetPairs)
            {
                if (sourceTargetPairs.Count() > 1) Console.WriteLine(pair.Source + " -> " + pair.Target);
                ConvertFile(pair.Source, pair.Target);
            }
        }

        private static List<string> GetTargetFilenames(List<string> sourceFilenames, string sourcePath, string targetPath)
        {
            return sourceFilenames.Select(x => x.Replace(sourcePath, targetPath).Split('.')).Select(x => string.Join(".", x.Take(x.Length - 1)) + ".xnb").ToList();
        }

        private static void ConvertFile(string sourceFilename, string targetFilename)
        {
            if (!File.Exists(sourceFilename))
            {
                Console.WriteLine($"File '{sourceFilename}' not found.\nExiting...\n");
                Console.Read();

                return;
            }

            var bitmap = new Bitmap(sourceFilename);
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            var argbValues = new byte[bytes];
            Marshal.Copy(ptr, argbValues, 0, bytes);

            int headerSize = 85;
            int totalSize = headerSize + bytes;

            using (var s = new FileStream(targetFilename, FileMode.Create))
            {
                using (var w = new BinaryWriter(s))
                {
                    // Write header (w - windows, version 4, flags - 0)
                    w.Write(new char[] { 'X', 'N', 'B', 'w', (char)4, (char)0 });
                    // Write total file size
                    w.Write(totalSize);
                    // How many typereaders do we have
                    w.Write((char)1);
                    // The type readers and their versions
                    w.Write("Microsoft.Xna.Framework.Content.Texture2DReader");
                    w.Write(0);
                    // Shared resource count (should always be 0?)
                    w.Write((char)0);
                    // First object, what type are we (0 means null, 1 is the first type defined in the typereaders
                    w.Write((char)1);

                    // Write Texture2D
                    // Surface format - 0x01 is Color (ARGB32)
                    w.Write(1);
                    w.Write(bitmap.Width);
                    w.Write(bitmap.Height);
                    // Number of mipmap levels
                    w.Write(1);
                    w.Write(bytes);

                    // The picture data, uncompressed
                    w.Write(argbValues, 0, bytes);
                }
            }
        }

        private static List<string> GetFilesInDirectory(string[] fileExtensions, string sourcePath, bool recurse)
        {
            var files = Directory.EnumerateFiles(sourcePath).Where(x => fileExtensions.Any(y => y == x.Split('.').Last())).ToList();

            if (recurse)
            {
                files.AddRange(Directory.EnumerateDirectories(sourcePath).SelectMany(x => GetFilesInDirectory(fileExtensions, x, recurse)));
            }

            return files;
        }
    }
}
