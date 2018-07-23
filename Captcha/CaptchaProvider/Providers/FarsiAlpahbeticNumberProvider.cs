using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NumberToWordsLib;
using Tirskele.Captcha.CaptchaProvider.Factory;
using Tirskele.Captcha.Models;

namespace Tirskele.Captcha.CaptchaProvider.Providers
{
    public class FarsiAlpahbeticNumberProvider : ICaptchaGenerator
    {
        public int NumberOfDigits { get; set; } = 4;
        public Color BackgroundColor { get; set; }
        public Font CaptchaFont { get; set; }
        public int ImageHeight { get; set; } = 60;
        public int ImageWidth { get; set; } = 230;

        public FarsiAlpahbeticNumberProvider(IConfigurationSection configuration)
        {
            if (!string.IsNullOrEmpty(configuration["ImageWidth"]))
                ImageWidth = Convert.ToInt32(configuration["ImageWidth"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(configuration["ImageHeight"]))
                ImageHeight = Convert.ToInt32(configuration["ImageHeight"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(configuration["CaptchaCharCount"]))
                NumberOfDigits = Convert.ToInt32(configuration["CaptchaCharCount"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(configuration["BackgroundColor"]))
                BackgroundColor = Color.FromName(configuration["BackgroundColor"]);
            if (!string.IsNullOrEmpty(configuration["Font"]))
                CaptchaFont = new Font(configuration["Font"], Convert.ToInt32(ImageHeight * 0.7), FontStyle.Bold);
        }
        public async Task<CaptchaResult> GetCaptchaResult()
        {

            //Color.FromName("SlateBlue");

            BackgroundColor = BackgroundColor.IsEmpty ? Color.FromArgb(255, 239, 239, 239) : BackgroundColor;
            CaptchaFont = CaptchaFont ?? new Font("Tahoma", 12f, FontStyle.Regular);
            Bitmap image = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(image);
            graphics.PageUnit = GraphicsUnit.Pixel;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.Clear(BackgroundColor);
            Random random = new Random();
            int number = random.Next((int)Math.Pow(10,NumberOfDigits-1), (int)Math.Pow(10, NumberOfDigits)-1);
            string s = number.NumberToText(Language.Persian);
            StringFormat format = new StringFormat();
            int lcid = new CultureInfo("fa-IR").LCID;
            format.SetDigitSubstitution(lcid, StringDigitSubstitute.National);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.FormatFlags = StringFormatFlags.DirectionRightToLeft;
            GraphicsPath path = new GraphicsPath();
            path.AddString(s, CaptchaFont.FontFamily, (int)CaptchaFont.Style, (graphics.DpiY * CaptchaFont.SizeInPoints) / 72f, new Rectangle(0, 0, ImageWidth, ImageHeight), format);
            Graphics graphics2 = Graphics.FromImage(image);
            HatchStyle[] styleArray = { HatchStyle.BackwardDiagonal };
            RectangleF rect = new RectangleF(0f, 0f, ImageWidth, ImageHeight);
            Brush brush = new HatchBrush(styleArray[random.Next(styleArray.Length - 1)], Color.FromArgb(random.Next(100, 255), random.Next(100, 255), random.Next(100, 255)), Color.White);
            graphics2.FillRectangle(brush, rect);
            Pen pen = new Pen(Color.FromArgb(random.Next(0, 100), random.Next(0, 100), random.Next(0, 100)));
            graphics.DrawPath(pen, path);
            int num3 = random.Next(-10, 10);
            using (Bitmap bitmap2 = (Bitmap) image.Clone())
            {
                for (int j = 0; j < ImageHeight; j++)
                {
                    for (int k = 0; k < ImageWidth; k++)
                    {
                        int x = k + ((int) (num3 * Math.Sin((3.1415926535897931 * j) / 64.0)));
                        int y = j + ((int) (num3 * Math.Cos((3.1415926535897931 * k) / 64.0)));
                        if ((x < 0) || (x >= ImageWidth))
                        {
                            x = 0;
                        }
                        if ((y < 0) || (y >= ImageHeight))
                        {
                            y = 0;
                        }
                        image.SetPixel(k, j, bitmap2.GetPixel(x, y));
                    }
                }
            }
            for (int i = 1; i <= 10; i++)
            {
                pen.Color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                int width = random.Next(0, ImageWidth / 3);
                int num6 = random.Next(0, ImageWidth);
                int num7 = random.Next(0, ImageHeight);
                int num8 = num6 - width;
                int num9 = num7 - width;
                graphics.DrawEllipse(pen, num8, num9, width, width);
            }
            await Task.Run(() => { graphics.DrawImage(image, new Point(0, 0)); }).ConfigureAwait(false);
            graphics.Flush();
            return new CaptchaResult { 
                Image = image,
                CreateDate = DateTime.Now,
                Id = Guid.NewGuid(),
                Text = number.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
    
}
