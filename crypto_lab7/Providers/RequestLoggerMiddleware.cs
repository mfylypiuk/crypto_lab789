using Microsoft.Owin;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace crypto_lab7.Providers
{
    public class RequestLoggerMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public RequestLoggerMiddleware(
            Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OwinContext(environment);

            if (context.Request.Headers.TryGetValue("Authorization", out string[] token))
            {
                logger.Info($"{context.Request.Method} {context.Request.Uri.AbsoluteUri} {token.FirstOrDefault()}");
            }
            else
            {
                logger.Warn($"{context.Request.Method} {context.Request.Uri.AbsoluteUri} Not authenticated");
            }

            var result = _next.Invoke(environment);
            logger.Info($"Response status code: {context.Response.StatusCode}");

            return result;
        }
    }
}