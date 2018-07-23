using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tirskele.Captcha.CaptchaProvider.Factory;
using Tirskele.Captcha.Models;
using Tirskele.Captcha.Repositories;
using Microsoft.Extensions.Options;
using Triskele.Captcha.Web.Common;
using Microsoft.Extensions.Configuration;

namespace Triskele.Captcha.Web.Controllers
{
    [Route("api/[controller]")]
    public class CaptchaController : Controller
    {
        #region Fields
        private readonly ICaptchaGenerator _generator;
        private readonly ICaptchaRepository _repository;
        #endregion

        #region Methods

        void CleanUpSessionRepository()
        {
            _repository.CleanUp();
        }

        #endregion

        public CaptchaController(ICaptchaGenerator generator, ICaptchaRepository repository)
        {
           _generator = generator;
            _repository = repository;
        }


        [Route("Image")]
        //[NoBrowserCache]
        //[OutputCache(Location = OutputCacheLocation.None, NoStore = true, Duration = 0, VaryByParam = "None")]
        public async Task<ClientCaptchaResult> GetImage([FromQuery]int? width = null, [FromQuery]int? height = null)
        {
            CleanUpSessionRepository();

            if (width.HasValue)
                _generator.ImageWidth = width.Value;
            if (height.HasValue)
                _generator.ImageHeight = height.Value;

            CaptchaResult captchaResult = await _generator.GetCaptchaResult().ConfigureAwait(false);
            MemoryStream stream = new MemoryStream();
            captchaResult.Image.Save(stream, ImageFormat.Png);

            ClientCaptchaResult clientCaptchaResults =
                new ClientCaptchaResult
                {
                    Id = captchaResult.Id,
                    ImageBase64 = Convert.ToBase64String(stream.ToArray())
                };

            _repository.Add(new SimpleCaptchaResult
            {
                Id = captchaResult.Id,
                Text = captchaResult.Text,
                CreateDate = DateTime.Now
            });

            return clientCaptchaResults;
        }

        [Route("ImageFile")]
        public async Task<IActionResult> GetImageFile([FromQuery]int? width = null, [FromQuery]int? height = null)
        {
            CleanUpSessionRepository();

            if (width.HasValue)
                _generator.ImageWidth = width.Value;
            if (height.HasValue)
                _generator.ImageHeight = height.Value;

            CaptchaResult captchaResult = await _generator.GetCaptchaResult().ConfigureAwait(false);
            _repository.Add(new SimpleCaptchaResult
                            {
                                Id = captchaResult.Id,
                                Text = captchaResult.Text,
                                CreateDate = DateTime.Now
                            });
            using (MemoryStream stream = new MemoryStream())
            {
                captchaResult.Image.Save(stream, ImageFormat.Png);
                return File(stream.ToArray(), "image/png", captchaResult.Id.ToString());
            }

        }

        [Route("Validate/{id:guid}/{text}")]
        [HttpGet]
        public IActionResult Validate(Guid id, string text)
        {
            //Logger.LogTrace("id: {0}, text:{1}",id,text);
            if (string.IsNullOrEmpty(text))
                return BadRequest();
            if (_repository.Validate(id, text))
            {
                //Logger.LogTrace("validated");
                _repository.Remove(id);
                CleanUpSessionRepository();
                return Ok();
            }
            //Logger.LogTrace("invalid");
            return BadRequest();
        }
    }
}
