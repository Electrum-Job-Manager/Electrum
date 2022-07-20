using System.Net.Sockets;

namespace Electrum.Communication.Tcp
{
    public class TcpConnection
    {

        private TcpClient Client { get; }
        private IServiceProvider ServiceProvider { get; }
        private CancellationToken cancellationToken;
        private NetworkStream stream;

        public TcpConnection(TcpClient client, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            Client = client;
            ServiceProvider = serviceProvider;
            this.cancellationToken = cancellationToken;
            this.stream = Client.GetStream();
        }

        public void StartReadingThread()
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                if (!stream.CanRead) continue;
                byte[] packetIdBuf = new byte[5];
                int bytesRead = stream.Read(packetIdBuf, 0, 5);
                if(bytesRead == 0) continue;
                if(packetIdBuf[0] == 1) // Execute call
                {
                    
                } else if (packetIdBuf[0] == 2) // Response from call
                {

                }
            }
        }

        public void HandlePacket(byte[] bytes)
        {

        }

    }
}