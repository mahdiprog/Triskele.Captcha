using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Triskele.Captcha.Web.Common
{
    public class CaptchaSettings
    {
        public string AllowedCaptchaDomains { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string BackgroundColor { get; set; }
        public string Font { get; set; }
        public string RepositoryType { get; set; }
        public string GeneratorType { get; set; }
        public int RepositoryTTLMinutes { get; set; }
        public string RepositoryRedisConnectionString { get; set; }
        public int CaptchaCharCount { get; set; }
    }
}