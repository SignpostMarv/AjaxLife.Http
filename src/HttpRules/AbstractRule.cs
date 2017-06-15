using AjaxLife.Http;
using HttpServer;
using IRule = HttpServer.Rules.IRule;
using System;

namespace AjaxLife.HttpRules
{
    public abstract class AbstractRule : IRule, ICanHandleRequest
    {
        public abstract bool CanHandleRequest(IHttpRequest request);

        public abstract string Charset { get; }

        public abstract string ContentType { get; }

        public bool Process(IHttpRequest request, IHttpResponse response)
        {
            if (CanHandleRequest(request) == false)
            {
                return false;
            }

            Console.WriteLine(request.UriPath);

            return HandleRequest(request, response);
        }

        protected abstract bool HandleRequest(IHttpRequest request, IHttpResponse response);
    }
}
