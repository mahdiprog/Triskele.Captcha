using System;

namespace Tirskele.Captcha.Models
{
    public class SimpleCaptchaResult:BaseCaptchaResult
    {
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
