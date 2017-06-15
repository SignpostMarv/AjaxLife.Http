using HttpServer;

namespace AjaxLife.Http
{
    interface ICanHandleRequest
    {
        bool CanHandleRequest(IHttpRequest request);
    }
}
