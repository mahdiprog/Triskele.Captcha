using System;
using Tirskele.Captcha.Models;

namespace Tirskele.Captcha.Repositories
{
    public interface ICaptchaRepository
    {
        void Add(SimpleCaptchaResult captcha);

        void Remove(Guid id);

        void CleanUp();

        bool Validate(Guid id, string text);
    }
}
