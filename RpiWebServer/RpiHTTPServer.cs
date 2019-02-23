﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RpiWebServer
{
    class RpiHTTPServer
    {

        private StreamSocketListener listener; // the socket listner to listen for TCP requests
                                               // Note: this has to stay in scope!

        private const uint BufferSize = 8192; // this is the max size of the buffer in bytes 

        public RpiHTTPServer() { }

        public void Initialise()
        {
            listener = new StreamSocketListener();

            listener.BindServiceNameAsync("80");

            listener.ConnectionReceived += async (sender, args) =>
            {
                HandleRequest(sender, args);
            };
        }

        public async void HandleRequest( StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args )
        {
            StringBuilder request = new StringBuilder();

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

            // Send a response back
            using (IOutputStream output = args.Socket.OutputStream)
            {
                using (Stream response = output.AsStreamForWrite())
                {
                    // For now we are just going to reply to anything with Hello World!
                    byte[] bodyArray = Encoding.UTF8.GetBytes("<html><body>Hi, Honey!</body></html>");

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
    }
}
