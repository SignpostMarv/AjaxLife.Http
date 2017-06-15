using HttpServer;
using HttpServer.HttpModules;
using IRule = HttpServer.Rules.IRule;
using IpAddress = System.Net.IPAddress;
using System;

namespace AjaxLife.Http
{
    public class AjaxLifeHttpServer
    {
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
            server.Start(Address, Port);
        }

        public void Stop()
        {
            server.Stop();
        }

        public void AddHttpModule(HttpModule module)
        {
            server.Add(module);
        }

        public void AddRule(IRule rule)
        {
            server.Add(rule);
        }
    }
}
