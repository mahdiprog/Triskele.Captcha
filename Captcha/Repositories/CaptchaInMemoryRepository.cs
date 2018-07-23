using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Tirskele.Captcha.Models;

namespace Tirskele.Captcha.Repositories
{
    public class CaptchaInMemoryRepository : ICaptchaRepository
    {

        readonly IConfigurationSection _configuration;

        static List<SimpleCaptchaResult> CaptchaList { get; } = new List<SimpleCaptchaResult>();

        public CaptchaInMemoryRepository(IConfigurationSection configuration)
        {
            _configuration = configuration;
        }
        public void Add(SimpleCaptchaResult captcha)
        {
            lock (CaptchaList) { 
            CaptchaList.Add(captcha);
        }}

        public void Remove(Guid id)
        {
            lock (CaptchaList)
            {
                SimpleCaptchaResult captcha = CaptchaList.SingleOrDefault(c => c.Id == id);
                if (captcha != null)
                    CaptchaList.Remove(captcha);
            }
        }

        public void CleanUp()
        {
            lock (CaptchaList)
            {
                List<SimpleCaptchaResult> expired = CaptchaList.Where(c => c.CreateDate < DateTimeOffset.Now.AddMinutes(Convert.ToInt32(_configuration["RepositoryTTLMinutes"], CultureInfo.InvariantCulture)*-1)).ToList();
                for (int i = 0; i < expired.Count; i++)
                    CaptchaList.Remove(expired[i]);
            }
        }

        public bool Validate(Guid id, string text)
        {
            SimpleCaptchaResult captcha = CaptchaList.SingleOrDefault(c => c.Id == id && c.Text.ToUpperInvariant() == text.ToUpperInvariant());
            return captcha != null;
        }
    }
}
