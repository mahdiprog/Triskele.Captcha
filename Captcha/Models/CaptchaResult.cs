using System.Drawing;

namespace Tirskele.Captcha.Models
{
    public class CaptchaResult:SimpleCaptchaResult
    {
        public Bitmap Image { get; set; }
    }
}
