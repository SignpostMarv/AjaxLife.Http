using HttpServer;
using System.Collections.Generic;

namespace AjaxLife.Http.Rules
{
    public abstract class DynamicStringRule : AbstractFilesystemRule
    {
        private List<string> PathsToFileOnServer = new List<string>();

        public string ContentType { get; private set; }

        public DynamicStringRule(
            string pathToFileOnServer,
            string contentType
        ) : this(new List<string> { pathToFileOnServer }, contentType)
        {
        }

        public DynamicStringRule(
            List<string> pathsToFileOnServer,
            string contentType
        )
        {
            PathsToFileOnServer = pathsToFileOnServer;
            ContentType = contentType;
        }

        public override bool CanHandleRequest(IHttpRequest request)
        {
            return PathsToFileOnServer.Contains(request.UriPath);
        }
    }
}
