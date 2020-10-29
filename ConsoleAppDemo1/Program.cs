using Csv;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;

namespace ConsoleAppDemo1
{
    class Program
    {
        static void Main(string[] args)
        {
            EdgeDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://www.77file.com/account.php?action=login&ref=/mydisk.php");
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.FindElement(By.Name("username")).SendKeys("jkqcrty");
            driver.FindElement(By.Name("password")).SendKeys("******");
            driver.FindElement(By.Name("user_form")).Submit();

            string csv = File.ReadAllText(@"");
            foreach (var line in CsvReader.ReadFromText(csv))
            {
                Console.WriteLine(line);
                driver.Navigate().GoToUrl(line[1]);
                IWebDriver validate = driver.SwitchTo().Frame(driver.FindElement(By.Id("tcaptcha_transform")));

                Thread.Sleep(2000);
                var slideBkg = validate.FindElement(By.Id("slideBkg"));

                string newUrl = slideBkg.GetAttribute("src");
                string oldUrl = newUrl.Replace("img_index=1", "img_index=0");
                Bitmap oldBmp = (Bitmap)GetImg(oldUrl);
                Bitmap newBmp = (Bitmap)GetImg(newUrl);

                int left = GetArgb(oldBmp, newBmp);
                int leftShift = (int)(left * ((double)slideBkg.Size.Width / (double)newBmp.Width) - 36);

                var slideBlock = validate.FindElement(By.Id("slideBlock"));
                Actions actions = new Actions(driver);
                actions.DragAndDropToOffset(slideBlock, leftShift, 0).Build().Perform();

                validate.FindElement(By.Id("downs10")).Click();
                Console.WriteLine(line + " done");
                Thread.Sleep(20000);
            }

            Console.ReadLine();
        }

        static void CaptureImage(byte[] fromImageByte, int offsetX, int offsetY, string toImagePath, int width, int height)
        {
            MemoryStream ms = new MemoryStream(fromImageByte);
            Image fromImage = Image.FromStream(ms);
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphic = Graphics.FromImage(bitmap);
            graphic.DrawImage(fromImage, 0, 0, new Rectangle(offsetX, offsetY, width, height), GraphicsUnit.Pixel);
            Image saveImage = Image.FromHbitmap(bitmap.GetHbitmap());
            saveImage.Save(toImagePath, ImageFormat.Png);
            saveImage.Dispose();
            graphic.Dispose();
            bitmap.Dispose();
        }

        static int GetArgb(Bitmap oldBmp, Bitmap newBmp)
        {
            for (int i = 0; i < newBmp.Width; i++)
            {
                for (int j = 0; j < newBmp.Height; j++)
                {
                    if ((i >= 0 && i <= 1) && ((j >= 0 && j <= 1) || (j >= (newBmp.Height - 2) && j <= (newBmp.Height - 1))))
                    {
                        continue;
                    }
                    if ((i >= (newBmp.Width - 2) && i <= (newBmp.Width - 1)) && ((j >= 0 && j <= 1) || (j >= (newBmp.Height - 2) && j <= (newBmp.Height - 1))))
                    {
                        continue;
                    }

                    Color oldColor = oldBmp.GetPixel(i, j);
                    Color newColor = newBmp.GetPixel(i, j);
                    if (Math.Abs(oldColor.R - newColor.R) > 60 || Math.Abs(oldColor.G - newColor.G) > 60 || Math.Abs(oldColor.B - newColor.B) > 60)
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        static Image GetImg(string url)
        {
            WebRequest webreq = WebRequest.Create(url);
            WebResponse webres = webreq.GetResponse();
            Image img;
            using (System.IO.Stream stream = webres.GetResponseStream())
            {
                img = Image.FromStream(stream);
            }
            return img;
        }
    }
}
