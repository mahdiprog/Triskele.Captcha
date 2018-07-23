using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading.Tasks;
using Captcha;
using Microsoft.Extensions.Configuration;
using Tirskele.Captcha.CaptchaProvider.Factory;
using Tirskele.Captcha.Models;

namespace Tirskele.Captcha.CaptchaProvider.Providers
{
    public class MathematicalProvider : ICaptchaGenerator
    {
        enum OperatorType
        {
            Sum,
            Subtract,
            Multiply,
            Devide
        }
       public Font CaptchaFont { get; set; }
        public int ImageHeight { get; set; } = 60;
        public int ImageWidth { get; set; } = 230;
        public Color BackgroundColor { get; set; }

        public MathematicalProvider(IConfigurationSection configuration)
        {
            if (!string.IsNullOrEmpty(configuration["ImageWidth"]))
                ImageWidth = Convert.ToInt32(configuration["ImageWidth"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(configuration["ImageHeight"]))
                ImageHeight = Convert.ToInt32(configuration["ImageHeight"], CultureInfo.InvariantCulture);
          
            if (!string.IsNullOrEmpty(configuration["Font"]))
                CaptchaFont = new Font(configuration["Font"], Convert.ToInt32(ImageHeight * 0.7), FontStyle.Bold);
        }
        public async Task<CaptchaResult> GetCaptchaResult()
        {
            Random random = new Random();
            int numFirst = random.Next(20, 30);
            int numLast = random.Next(1, 9);
            int type = random.Next(0, 2);

            string strCaptcha;
            int result;
            switch ((OperatorType)type)
            {
                    case OperatorType.Sum:
                        strCaptcha = numFirst + " + " + numLast + " =";
                        result = numFirst + numLast;
                    break;
                    case OperatorType.Subtract:
                        strCaptcha = numFirst + " - " + numLast + " =";
                        result = numFirst - numLast;
                    break;
                default:
                    strCaptcha = numFirst + " + " + numLast + " =";
                    result = numFirst + numLast;
                    break;
            }

            //var _assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //var _imageStream = _assembly.GetManifestResourceStream("BG1");
            
            Bitmap image = new Bitmap(new System.IO.MemoryStream(Resource.BG1));
            Graphics graphicImage = Graphics.FromImage(image);

            //Smooth graphics is nice.
            graphicImage.SmoothingMode = SmoothingMode.AntiAlias;

            //Write your text.
            await Task.Run(() => { graphicImage.DrawString(strCaptcha, CaptchaFont ?? new Font("Constantia", 18, FontStyle.Bold), Brushes.Black, new Point(15, -2)); }).ConfigureAwait(false);
            
           return new CaptchaResult { 
                Image = image,
                CreateDate = DateTime.Now,
                Id = Guid.NewGuid(),
                Text = result.ToString(CultureInfo.InvariantCulture)
            };
        }

    }
    
}
