using HttpServer;
using System;
using System.Collections.Generic;
using System.IO;

namespace AjaxLife.HttpRules
{
    public abstract class DynamicStringRule : AbstractRule
    {
        private List<string> PathsToFileOnServer = new List<string>();

        private string _charset;

        public override string Charset
        {
            get
            {
                return _charset;
            }
        }

        private string _contentType;

        public override string ContentType
        {
            get
            {
                return _contentType;
            }
        }

        public DynamicStringRule(
            string pathToFileOnServer,
            string charset, 
            string contentType
        ) : this(new List<string> { pathToFileOnServer }, charset, contentType)
        {
        }

        public DynamicStringRule(List<string> pathsToFileOnServer, string charset, string contentType)
        {
            PathsToFileOnServer = pathsToFileOnServer;
            _charset = charset;
            _contentType = contentType;
        }

        public override bool CanHandleRequest(IHttpRequest request)
        {
            return PathsToFileOnServer.Contains(request.UriPath);
        }
    }
}
