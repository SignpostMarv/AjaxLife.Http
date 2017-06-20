using HttpServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace AjaxLife.Http.Rules
{
    public class FileRule : AbstractFilesystemRule
    {
        private string PathToFileOnDisk;

        private string PathToFileOnServer;

        private string CacheControl { get; set; }

        private bool SendCookies { get; set; }

        public FileRule(string pathToFileOnDisk, string pathToFileOnServer, string cacheControl="max-age=3600, public, must-revalidate", bool sendCookies=false)
        {
            if (pathToFileOnDisk.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException("Path to file on disk contained invalid characters before being converted to full path!");
            }

            pathToFileOnDisk = Path.GetFullPath(pathToFileOnDisk);
            
            if (pathToFileOnDisk.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException("Path to file on disk contained invalid characters after being converted to full path!");
            }

            PathToFileOnDisk = pathToFileOnDisk;
            PathToFileOnServer = pathToFileOnServer;
            CacheControl = cacheControl;
            SendCookies = sendCookies;
        }

        public override bool CanHandleRequest(IHttpRequest request)
        {
            if (File.Exists(PathToFileOnDisk) == false) {
                return false;
            }

            return request.UriPath == PathToFileOnServer;
        }

        protected override bool HandleRequest(IHttpRequest request, IHttpResponse response)
        {
            if (File.Exists(PathToFileOnDisk) == false)
            {
                throw new Exception("File does not exist on disk!");
            }

            response.ContentType = ContentTypeFromExtension(
                Path.GetExtension(PathToFileOnDisk)
            );
            FileInfo info = new FileInfo(PathToFileOnDisk);
            response.ContentLength = info.Length;
            Stream stream = new FileStream(PathToFileOnDisk, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);

            string ETag = BitConverter.ToString(SHA512.Create().ComputeHash(stream)).Replace("-", "").ToLower();

            if (request.Headers["If-None-Match"] != null && (new List<string>(request.Headers["If-None-Match"].Split(','))).Contains(ETag))
            {
                response.Status = HttpStatusCode.NotModified;
                return true;
            }

            response.AddHeader("ETag", ETag);
            stream.Position = 0;

            response.Body = stream;
            response.AddHeader("Cache-Control", CacheControl);

            if (SendCookies == false)
            {
                response.Cookies.Clear();
            }

            return true;
        }
    }
}
