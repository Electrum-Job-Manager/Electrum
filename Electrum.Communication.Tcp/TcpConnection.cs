using System.Net.Sockets;

namespace Electrum.Communication.Tcp
{
    public class TcpConnection
    {

        private TcpClient Client { get; }
        private IServiceProvider ServiceProvider { get; }

        public TcpConnection(TcpClient client, IServiceProvider serviceProvider)
        {
            Client = client;
            ServiceProvider = serviceProvider;
        }

    }
}