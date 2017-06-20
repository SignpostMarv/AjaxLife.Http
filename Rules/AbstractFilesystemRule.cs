using HttpServer;
using IRule = HttpServer.Rules.IRule;
using System;

namespace AjaxLife.Http.Rules
{
    public abstract class AbstractFilesystemRule : IRule, ICanHandleRequest
    {
        public abstract bool CanHandleRequest(IHttpRequest request);

        public static string CharsetFromExtension(string extension)
        {
            string res = "";

            switch (extension)
            {
                case "css":
                case ".css":
                case "txt":
                case ".txt":
                case "js":
                case ".js":
                    res = "; charset=utf-8";
                    break;
            }

            return res;
        }

        public static string ContentTypeFromExtension(string extension)
        {
            string res = "text/plain";

            switch (extension)
            {
                case "css":
                case ".css":
                    res = "text/css";
                    break;
                case "js":
                case ".js":
                    res = "application/javascript";
                    break;
                case "jpg":
                case "jpeg":
                case ".jpg":
                case ".jpeg":
                    res = "image/jpeg";
                    break;
                case "gif":
                case ".gif":
                    res = "image/gif";
                    break;
                case "png":
                case ".png":
                    res = "image/png";
                    break;
                case "wav":
                case ".wav":
                    res = "audio/wav";
                    break;
            }

            return res + CharsetFromExtension(extension);
        }

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
