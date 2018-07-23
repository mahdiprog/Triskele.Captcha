using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.MsgPack;
using Tirskele.Captcha.Models;
using Tirskele.Captcha.Repositories;

namespace Triskele.Captcha.RedisRepository
{
    public class CaptchaRedisRepository : ICaptchaRepository, IDisposable
    {
        readonly IConfigurationSection _configuration;
        private volatile ConnectionMultiplexer _connection;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private StackExchangeRedisCacheClient _client;

        const string RedisPrefix = "Captcha:";
        public CaptchaRedisRepository(IConfigurationSection configuration)
        {
            _configuration = configuration;
        }
        public void Add(SimpleCaptchaResult captcha)
        {
            Connect();
            _client.Add<SimpleCaptchaResult>(captcha.Id.ToString(), captcha, DateTimeOffset.Now.AddMinutes(Convert.ToInt32(_configuration["RepositoryTTLMinutes"], CultureInfo.InvariantCulture)));
        }

        public void Remove(Guid id)
        {
            Connect();
            _client.Remove(id.ToString());
        }

        public void CleanUp()
        {
            
        }

        public bool Validate(Guid id, string text)
        {
            Connect();
            SimpleCaptchaResult captcha = _client.Get<SimpleCaptchaResult>(id.ToString());
            return string.Equals(captcha.Text, text, StringComparison.CurrentCultureIgnoreCase);
        }

        private void Connect()
        {
            if (_connection != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_connection == null)
                {
                    //ApplicationSettings:OutputCacheRedisConnectionString  || ApplicationSettings:RedisConnectionString
                    _connection = ConnectionMultiplexer.Connect(_configuration["RepositoryRedisConnectionString"]);
                    _client = new StackExchangeRedisCacheClient(_connection, new MsgPackObjectSerializer(), _connection.GetDatabase().Database, RedisPrefix);
             
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                _connection.Dispose();
                _client.Dispose();
            }

            disposed = true;
        }
    }
}

