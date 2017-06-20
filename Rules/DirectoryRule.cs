using HttpServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace AjaxLife.Http.Rules
{
    public class DirectoryRule : AbstractFilesystemRule
    {
        private string PathToDirectoryOnDisk;

        private string PathToDirectoryOnServer;

        private string CacheControl { get; set; }

        private bool SendCookies { get; set; }

        public DirectoryRule(
            string pathToDirectoryOnDisk,
            string pathToDirectoryOnServer,
            string cacheControl = "max-age=3600, public, must-revalidate",
            bool sendCookies = false
        )
        {
            if (
                pathToDirectoryOnDisk.IndexOfAny(
                    Path.GetInvalidPathChars()
                ) >= 0
            )
            {
                throw new ArgumentException(
                    "Path to file on disk contained invalid characters before being converted to full path!"
                );
            }

            pathToDirectoryOnDisk = Path.GetFullPath(pathToDirectoryOnDisk);

            if (
                pathToDirectoryOnDisk.IndexOfAny(
                    Path.GetInvalidPathChars()
                ) >= 0
            )
            {
                throw new ArgumentException(
                    "Path to file on disk contained invalid characters after being converted to full path!"
                );
            }

            PathToDirectoryOnDisk = pathToDirectoryOnDisk;
            PathToDirectoryOnServer = pathToDirectoryOnServer;
            CacheControl = cacheControl;
            SendCookies = sendCookies;
        }

        public override bool CanHandleRequest(IHttpRequest request)
        {
            return (
                request.UriPath.IndexOf(PathToDirectoryOnServer) == 0 &&
                (
                    PathToDirectoryOnDisk +
                    request.UriPath.Substring(
                        PathToDirectoryOnServer.Length
                    )
                ).IndexOfAny(Path.GetInvalidPathChars()) < 0 &&
                File.Exists(
                    PathToDirectoryOnDisk +
                    request.UriPath.Substring(
                        PathToDirectoryOnServer.Length
                    )
                ) == true
            );
        }

        protected override bool HandleRequest(
            IHttpRequest request,
            IHttpResponse response
        )
        {
            string PathtoFileOnDisk = (
                PathToDirectoryOnDisk +
                request.UriPath.Substring(
                    PathToDirectoryOnServer.Length
                )
            );
            if (PathtoFileOnDisk.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new Exception("Path contains invalid characters!");
            }
            else if (File.Exists(PathtoFileOnDisk) == false)
            {
                throw new Exception("File does not exist on disk!");
            }

            response.ContentType = ContentTypeFromExtension(
                Path.GetExtension(PathtoFileOnDisk)
            );

            FileInfo info = new FileInfo(PathtoFileOnDisk);
            response.ContentLength = info.Length;
            Stream stream = new FileStream(
                PathtoFileOnDisk,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan
            );

            string ETag = BitConverter.ToString(
                SHA512.Create().ComputeHash(stream)
            ).Replace("-", "").ToLower();

            if (
                request.Headers["If-None-Match"] != null &&
                (
                    new List<string>(
                        request.Headers["If-None-Match"].Split(',')
                    )
                ).Contains(ETag)
            )
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
