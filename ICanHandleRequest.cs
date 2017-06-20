using HttpServer;

namespace AjaxLife.Http
{
    public interface ICanHandleRequest
    {
        bool CanHandleRequest(IHttpRequest request);
    }
}
