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
            if (args.Length < 1)
            {
                Console.WriteLine("ImageToXNB <source> [<target>]\n");
                Console.WriteLine("Supported formats BMP, GIF, JPEG, PNG, TIFF");

                return;
            }

            var sourceFilename = args[0];
            var targetFilename = sourceFilename.Split('.').First() + ".xnb";

            if (args.Length == 2)
                targetFilename = args[1];

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
                    w.Write(new char[] { 'X', 'N', 'B', 'w', (char) 4, (char) 0});
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
    }
}
