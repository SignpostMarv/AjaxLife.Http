using HttpServer;
using IpAddress = System.Net.IPAddress;
using System;

namespace AjaxLife.Http
{
    public class AjaxLifeHttpServer
    {
        public HttpListener listener { get; private set; }

        public HttpServer.HttpServer server { get; private set; }

        public IpAddress Address { get; private set; }

        public int Port { get; private set; }

        public virtual bool IsSecure { get { return false; } }

        public AjaxLifeHttpServer(IpAddress addr, int port)
        {
            Address = addr;
            Port = port;
            server = new HttpServer.HttpServer();
        }

        public virtual void Start(int backlog)
        {
            /*
            listener.Start(64);
            return;
            */
            server.Start(Address, Port);
        }

        public void Stop()
        {
            listener.Stop();
            server.Stop();
        }

        public void OnRequest(object source, RequestEventArgs args)
        {
            Console.WriteLine("request recieved");
        }
    }
}
