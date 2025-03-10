using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BasicWebServer.Server
{
    public class HttpServer
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly TcpListener _serverListener;

        private readonly RoutingTable _routingTable;

        public HttpServer(string ipAddress, int port, Action<IRoutingTable> routingTableConfiguration)
        {
            this._ipAddress = IPAddress.Parse(ipAddress);
            this._port = port;

            this._serverListener = new TcpListener(this._ipAddress, port);

            routingTableConfiguration(this._routingTable = new RoutingTable());
        }
        public HttpServer(int port, Action<IRoutingTable> routingTable)
            : this("127.0.0.1", port, routingTable)
        {

        }
        public HttpServer(Action<IRoutingTable> routingTable)
            : this(8080, routingTable)
        {

        }

        public async Task StartAsync()
        {
            this._serverListener.Start();

            Console.WriteLine($"Server started on port {this._port}.");
            Console.WriteLine("Listening for requests...");

            while (true)
            {
                var connection = await _serverListener.AcceptTcpClientAsync();

                _ = Task.Run(async () =>
                {
                    var networkStream = connection.GetStream();

                    var requestText = await this.ReadRequestAsync(networkStream);

                    Console.WriteLine(requestText);

                    Request request = Request.Parse(requestText);

                    Response response = this._routingTable.MatchRequest(request);

                    AddSession(response, request);

                    await WriteResponseAsync(networkStream, response);

                    connection.Close();
                });
            }
        }

        private void AddSession(Response response, Request request)
        {
            var sessionExists = request.Session
                .ContainsKey(Session.SESSION_CURRENT_DATE_KEY);

            if (!sessionExists)
            {
                request.Session[Session.SESSION_CURRENT_DATE_KEY] = DateTime.Now.ToString();

                response.Cookies.Add(Session.SESSION_COOKIE_NAME, request.Session.Id);
            }
        }
        private async Task WriteResponseAsync(NetworkStream networkStream, Response response)
        {
            var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
            await networkStream.WriteAsync(responseBytes);
        }
        private async Task<string> ReadRequestAsync(NetworkStream networkStream)
        {
            var bufferLength = 1024;
            var buffer = new byte[bufferLength];

            var totalBytes = 0;

            var requestBuilder = new StringBuilder();

            do
            {
                var bytesRead = await networkStream.ReadAsync(buffer, 0, bufferLength);

                totalBytes =+ bytesRead;

                if (totalBytes > 1024 * 10)
                {
                    throw new InvalidOperationException("Request is too large.");
                }

                requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            // May not run ocrretly over Internet
            while (networkStream.DataAvailable);

            return requestBuilder.ToString();
        }
    }
}
