using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SocketLibrary
{


    public class ServerClient : WebSocketBehavior
    {
        Server server;

        public ServerClient(Server server)
        {
            this.server = server;
        }

        public void Send(string data)
        {
            this.SendAsync(data, null);
        }

        public void Send(byte[] data)
        {
            this.SendAsync(data, null);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            this.server.MessageCallback(this, e.RawData);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            this.server.ConnectionCallback(this);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            server.CloseCallback(this);
        }

        public override string ToString()
        {
            return this.Context.Origin;
        }
    }
}
