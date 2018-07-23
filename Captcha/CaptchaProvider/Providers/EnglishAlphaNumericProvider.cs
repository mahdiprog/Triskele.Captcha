using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Tirskele.Captcha.CaptchaProvider.Factory;
using Tirskele.Captcha.Models;

namespace Tirskele.Captcha.CaptchaProvider.Providers
{
    public class EnglishAlphaNumericProvider : ICaptchaGenerator
    {
        private readonly Random _rand = new Random((int) DateTime.Now.Ticks);
        public int TextLength { get; set; } = 6;
        public Color BackgroundColor { get; set; }
        public Font CaptchaFont { get; set; }
        public int ImageHeight { get; set; } = 60;
        public int ImageWidth { get; set; } = 230;

        public EnglishAlphaNumericProvider(IConfigurationSection configuration)
        {
            if (!string.IsNullOrEmpty(configuration["ImageWidth"]))
                ImageWidth = Convert.ToInt32(configuration["ImageWidth"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(configuration["ImageHeight"]))
                ImageHeight = Convert.ToInt32(configuration["ImageHeight"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(configuration["CaptchaCharCount"]))
                TextLength = Convert.ToInt32(configuration["CaptchaCharCount"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(configuration["BackgroundColor"]))
                BackgroundColor = Color.FromName(configuration["BackgroundColor"]);
            if (!string.IsNullOrEmpty(configuration["Font"]))
                CaptchaFont = new Font(configuration["Font"], Convert.ToInt32(ImageHeight * 0.7), FontStyle.Bold);
        }
        private void AddLine(Graphics graphics, Rectangle rect)
        {
            var num = 4;
            float width = Convert.ToSingle(ImageHeight / 31.25);
            int num3 = 1;
            PointF[] points = new PointF[num + 1];
            using (Pen pen = new Pen(Color.DimGray, width))
            {
                for (int i = 1; i <= num3; i++)
                {
                    for (int j = 0; j <= num; j++)
                    {
                        points[j] = RandomPoint(rect);
                    }
                    graphics.DrawCurve(pen, points, 1.75f);
                }
            }
        }

        private void AddNoise(Graphics graphics1, Rectangle rect)
        {
            int num = 30;
            int num2 = 40;
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                int maxValue = Convert.ToInt32(Math.Max(rect.Width, rect.Height) / num2);
                for (int i = 0; i <= Convert.ToInt32((rect.Width * rect.Height) / num); i++)
                {
                    graphics1.FillEllipse(brush, _rand.Next(rect.Width), _rand.Next(rect.Height), _rand.Next(maxValue), _rand.Next(maxValue));
                }
            }
        }

        private string GenerateRandomText()
        {
            string str = "ACDEFGHJKLNPQRTUVXYZ2346789";
            StringBuilder builder = new StringBuilder(TextLength);
            int length = str.Length;
            for (int i = 0; i <= (TextLength - 1); i++)
            {
                builder.Append(str[_rand.Next(length)]);
            }
            return builder.ToString();
        }

        public async Task<CaptchaResult> GetCaptchaResult()
        {
            TextLength = (TextLength <= 0) ? 5 : TextLength;
            BackgroundColor = BackgroundColor.IsEmpty ? Color.FromArgb(255, 239, 239, 239) : BackgroundColor;
            Bitmap image = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);
            string str = GenerateRandomText();
            using (Graphics graphics = Graphics.FromImage(image))
            {
                Brush brush;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, ImageWidth, ImageHeight);
                using (brush = new SolidBrush(BackgroundColor))
                {
                    graphics.FillRectangle(brush, rect);
                }
                int num = 0;
                double num2 = (double) ImageWidth / TextLength;
                using (brush = new SolidBrush(Color.FromArgb(_rand.Next(0, 100), _rand.Next(0, 100), _rand.Next(0, 100))))
                {
                    foreach (char ch in str)
                    {
                        if (CaptchaFont == null)
                        {
                            CaptchaFont = new Font(RandomFontFamily(), Convert.ToInt32(ImageHeight * 0.7), FontStyle.Bold);
                        }
                        Rectangle layoutRect = new Rectangle(Convert.ToInt32(num * num2), 0, Convert.ToInt32(num2), ImageHeight);
                        StringFormat format = new StringFormat {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Near
                        };
                        GraphicsPath textPath = new GraphicsPath();
                        textPath.AddString(ch.ToString(CultureInfo.InvariantCulture), CaptchaFont.FontFamily, (int) CaptchaFont.Style, CaptchaFont.Size, layoutRect, format);
                        WarpText(textPath, layoutRect);
                        graphics.FillPath(brush, textPath);
                        num++;
                    }
                }
                AddNoise(graphics, rect);
                AddLine(graphics, rect);
                await Task.Run(() => { graphics.DrawImage(image, new Point(0, 0)); }).ConfigureAwait(false);
            }
            return new CaptchaResult { 
                Image = image,
                CreateDate = DateTime.Now,
                Id = Guid.NewGuid(),
                Text = str
            };
        }

        private string RandomFontFamily()
        {
            string str = "arial;arial black;comic sans ms;courier new;estrangelo edessa;franklin gothic medium;georgia;lucida console;lucida sans unicode;mangal;microsoft sans serif;palatino linotype;sylfaen;tahoma;times new roman;trebuchet ms;verdana";
            char[] separator = { ';' };
            string[] strArray = str.Split(separator);
            return strArray[_rand.Next(0, strArray.Length)];
        }

        private PointF RandomPoint(Rectangle rect) => 
            RandomPoint(rect.Left, rect.Width, rect.Top, rect.Bottom);

        private PointF RandomPoint(int xmin, int xmax, int ymin, int ymax) => 
            new PointF(_rand.Next(xmin, xmax), _rand.Next(ymin, ymax));

        private void WarpText(GraphicsPath textPath, Rectangle rect)
        {
            float num = 6f;
            float num2 = 1f;
            RectangleF srcRect = new RectangleF(Convert.ToSingle(rect.Left), 0f, Convert.ToSingle(rect.Width), rect.Height);
            int num3 = Convert.ToInt32(rect.Height / num);
            int num4 = Convert.ToInt32(rect.Width / num);
            int xmin = rect.Left - Convert.ToInt32(num4 * num2);
            int ymin = rect.Top - Convert.ToInt32(num3 * num2);
            int xmax = (rect.Left + rect.Width) + Convert.ToInt32(num4 * num2);
            int ymax = (rect.Top + rect.Height) + Convert.ToInt32(num3 * num2);
            if (xmin < 0)
            {
                xmin = 0;
            }
            if (ymin < 0)
            {
                ymin = 0;
            }
            if (xmax > ImageWidth)
            {
                xmax = ImageWidth;
            }
            if (ymax > ImageHeight)
            {
                ymax = ImageHeight;
            }
            PointF tf = RandomPoint(xmin, xmin + num4, ymin, ymin + num3);
            PointF tf2 = RandomPoint(xmax - num4, xmax, ymin, ymin + num3);
            PointF tf3 = RandomPoint(xmin, xmin + num4, ymax - num3, ymax);
            PointF tf4 = RandomPoint(xmax - num4, xmax, ymax - num3, ymax);
            PointF[] destPoints = { tf, tf2, tf3, tf4 };
            Matrix matrix = new Matrix();
            matrix.Translate(0f, 0f);
            textPath.Warp(destPoints, srcRect, matrix, WarpMode.Perspective, 0f);
        }
    }
    
}
