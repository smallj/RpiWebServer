using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;

namespace RpiWebServer
{
    class RpiHTTPServer
    {
        private SenseHat _senseHat { get; set; }
        private StreamSocketListener listener; // the socket listner to listen for TCP requests
                                               // Note: this has to stay in scope!
        private const uint BufferSize = 8192; // this is the max size of the buffer in bytes 
        private string DefaultPage;

        public RpiHTTPServer()
        {
            DefaultPage = File.ReadAllText("Assets\\mainpage.html");
            _senseHat = new SenseHat();
            this.ActivateSenseHat();
        }

        public void Dispose()
        {
            _senseHat.ClearDisplay();
            _senseHat.UpdateDisplay();
            _senseHat.Dispose();
            listener.Dispose();
        }

        public void Initialise()
        {
            listener = new StreamSocketListener();

            listener.BindServiceNameAsync("80");

            listener.ConnectionReceived += async (sender, args) =>
            {
                HandleRequest(sender, args);
            };

        }

        private async void ActivateSenseHat()
        {
            await _senseHat.Activate();
        }

        public async void HandleRequest( StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args )
        {
            StringBuilder request = new StringBuilder();
            string responseHTML = "<html><body>ERROR</body></html>";

            // Handle a incoming request
            // First read the request
            using (IInputStream input = args.Socket.InputStream)
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            responseHTML = PrepareResponse(ParseRequest(request.ToString()));

            // Send a response back
            using (IOutputStream output = args.Socket.OutputStream)
            {
                using (Stream response = output.AsStreamForWrite())
                {
                    // For now we are just going to reply to anything with Hello World!
                    byte[] bodyArray = Encoding.UTF8.GetBytes(responseHTML);

                    var bodyStream = new MemoryStream(bodyArray);

                    // This is a standard HTTP header so the client browser knows the bytes returned are a valid http response
                    var header = "HTTP/1.1 200 OK\r\n" +
                                $"Content-Length: {bodyStream.Length}\r\n" +
                                    "Connection: close\r\n\r\n";

                    byte[] headerArray = Encoding.UTF8.GetBytes(header);

                    // send the header with the body inclded to the client
                    await response.WriteAsync(headerArray, 0, headerArray.Length);
                    await bodyStream.CopyToAsync(response);
                    await response.FlushAsync();
                }
            }     
        }

        private string ParseRequest(string buffer)
        {
            string request = "ERROR";

            string[] tokens = buffer.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if ( tokens[0] == "GET" )
            {
                request = tokens[1].Replace("/", "").ToLower();
            }

            return request;
        }

        private string PrepareResponse(string request)
        {
            string response = "ERROR";

            response = DefaultPage;

            switch (request)
            {
                case "red": // GET /red
                    _senseHat.FillDisplay(Colors.Red);
                    break;
                case "green": // GET /green
                    _senseHat.FillDisplay(Colors.Green);
                    break;
                case "blue": // GET /blue
                    _senseHat.FillDisplay(Colors.Blue);
                    break;
                case "yellow": // GET /yellow
                    _senseHat.FillDisplay(Colors.Yellow);
                    break;
                case "clear": // GET /clear
                    _senseHat.ClearDisplay();
                    break;
                default:
                    break;
            }
            _senseHat.UpdateDisplay();

            return response;
        }
    }
}
