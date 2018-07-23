using System.Drawing;
using System.Threading.Tasks;
using Tirskele.Captcha.Models;

namespace Tirskele.Captcha.CaptchaProvider.Factory
{
    public interface ICaptchaGenerator
    {
        Task<CaptchaResult> GetCaptchaResult();

        Color BackgroundColor { get; set; }

        Font CaptchaFont { get; set; }

        int ImageHeight { get; set; }

        int ImageWidth { get; set; }
    }
}
