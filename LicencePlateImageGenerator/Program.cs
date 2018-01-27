using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicencePlateImageGenerator
{
    //Take all images dir in one array
    //Foreach image:
    //Get image from dir
    //Make grayscale
    //Save in other dir
    class Program
    {
        static void Main(string[] args)
        {
            var worker = new Worker();
            var imagesDir = worker.TakeImagesDir(@"D:\testing");
            foreach (FileInfo file in new DirectoryInfo(@"D:\testing_new").GetFiles())
            {
                file.Delete();
            }
            var tasks = new List<Task>();
            var clock = new Stopwatch();
            clock.Start();
            foreach (var dir in imagesDir)
            {
                var task = new Task(() => { worker.DoConversion(dir); });
                task.Start();
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            clock.Stop();
            Console.WriteLine(clock.Elapsed);
            //Console.ReadKey();
        }

        
    }

    public class Worker
    {
        public void DoConversion(string dir)
        {
            var width = 0;
            var heigth = 0;
            var image = GetImageFromDir(dir);
            var resizedImage = ResizeImage(image, new Size(128, 64));
            var plate = GeneratePlate();
            var withPlate = PutPlateOnImage(resizedImage, plate, ref width, ref heigth);
            var grayscaleImage = MakeGrayScale(withPlate, 30);
            grayscaleImage = AddTestRectangle(grayscaleImage, width, heigth);
            var label = $"{width}-{heigth}_{width+65}-{heigth}_{width}-{heigth+15}_{width+65}-{heigth+15}.jpg";
            SaveImage(grayscaleImage, string.Join("", dir.Split('\\').Reverse().Skip(1).Reverse()) + "\\" + label);
        }

        private Bitmap AddTestRectangle(Bitmap grayscaleImage, int width, int heigth)
        {
            Bitmap newBitmap = new Bitmap(grayscaleImage);
            Graphics g = Graphics.FromImage(newBitmap);
            g.DrawRectangle(Pens.Red, width, heigth, 65, 15);

            return newBitmap;
        }

        private Bitmap GeneratePlate()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var width = 65;//random.Next(40, 120);
            var heigth = 15;// random.Next(6, 20);
            var letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "V", "Z" };
            var randomColor = Color.FromArgb(255, 255, random.Next(0, 255));
            var b = new Bitmap(width, heigth);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < heigth; j++)
                {
                    b.SetPixel(i, j, randomColor);
                }
            }
            using (var g = Graphics.FromImage(b))
            {
                g.DrawString($"{letters[random.Next(0, letters.Count() - 1)]}{letters[random.Next(0, letters.Count() - 1)]}{letters[random.Next(0, letters.Count() - 1)]} {random.Next(0, 9)}{random.Next(0, 9)}{random.Next(0, 9)}", new Font(new FontFamily("Arial"), 10), Brushes.Black, new System.Drawing.Point(5, 0));
            }
            return b;
        }

        public Bitmap PutPlateOnImage(Bitmap image, Bitmap plate, ref int width, ref int heigth)
        {
            Bitmap newBitmap = new Bitmap(image);
            var random = new Random(Guid.NewGuid().GetHashCode());
            Graphics g = Graphics.FromImage(newBitmap);

            // Trigonometry: Tangent = Opposite / Adjacent
            double tangent = (double)newBitmap.Height /
                             (double)newBitmap.Width;

            // convert arctangent to degrees
            double angle = Math.Atan(tangent) * (180 / Math.PI);

            // a^2 = b^2 + c^2 ; a = sqrt(b^2 + c^2)
            double halfHypotenuse = (Math.Sqrt((newBitmap.Height
                                   * newBitmap.Height) +
                                   (newBitmap.Width *
                                   newBitmap.Width))) / 2;

            //g.RotateTransform(random.Next(-15, 15));
            width = random.Next(0, 128);
            heigth = random.Next(0, 64);
            g.DrawImage(plate, width, heigth);
            g.DrawRectangle(Pens.Red, width, heigth, 65, 15);
            
            return newBitmap;
        }

        public Bitmap GetImageFromDir(string path)
        {
            Bitmap bitmap;
            using (Stream bmpStream = File.Open(path, FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);
                bitmap = new Bitmap(image);
            }
            return bitmap;
        }

        public void SaveImage(Bitmap grayscaleImage, string path)
        {
            grayscaleImage.Save($@"D:\testing_new\{path.Split('\\').Last().Split('.').First()}.jpg", ImageFormat.Bmp);
        }

        public Bitmap MakeGrayScale(Bitmap image, int noiseAmount)
        {
            Bitmap d = new Bitmap(image.Width, image.Height);
            var random = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < image.Width; i++)
            {
                for (int x = 0; x < image.Height; x++)
                {
                    Color oc = image.GetPixel(i, x);
                    int grayScale = 0;
                    if (random.Next(0, 255) < noiseAmount)
                        grayScale = (int)((random.Next(0, 255) * 0.3) + (random.Next(0, 255) * 0.59) + (random.Next(0, 255) * 0.11));
                    else
                        grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                    Color nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
                    d.SetPixel(i, x, nc);
                }
            }

            return d;
        }

        public string[] TakeImagesDir(string dir)
        {
            var paths = Directory.GetFiles(dir, "*.jpg", SearchOption.AllDirectories);
            return paths;
        }

        public Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            return new Bitmap(imgToResize, size);
        }
    }
}
