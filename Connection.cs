using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace VndbClient
{
    internal class Connection
    {
        private static readonly string LoginBufferString = 
            "login {\"protocol\":1,\"client\":\"FTB\",\"clientver\":0.2}";
        private static readonly string UsernamePasswordLoginPrefix =
            "login {\"protocol\":1,\"client\":\"FTB\",\"clientver\":0.2,";
        private static readonly string UsernameFormatString = "\"username\":\"{0}\"";
        private static readonly string PasswordFormatString = "\"password\":\"{0}\"";
        private static readonly string UsernamePasswordLoginSuffix = "}";

        private TcpClient tcpClient;
        private Stream stream;

        private const string VNDBHost = "api.vndb.org";
        private const ushort VNDBPort = 19535; // always use TLS, because why not.

        public Connection()
        {
        }

        public async Task Open()
        {
            this.tcpClient = new TcpClient();
            await this.tcpClient.ConnectAsync(Connection.VNDBHost, Connection.VNDBPort);
            SslStream sslStream = new SslStream(this.tcpClient.GetStream());
            await sslStream.AuthenticateAsClientAsync("api.vndb.org");
            this.stream = sslStream;
        }

        public async Task Login(string username, string password)
        {
            string loginBuffer = null;

            // ugly, but I don't feel like importing a JSON library just for this...
            if(username != null && password != null)
            {
                string userField = String.Format(CultureInfo.InvariantCulture, Connection.UsernameFormatString, username);
                string passwordField = String.Format(CultureInfo.InvariantCulture, Connection.PasswordFormatString, password);

                loginBuffer = Connection.UsernamePasswordLoginPrefix + userField + "," + passwordField + Connection.UsernamePasswordLoginSuffix;
            }
            else
            {
                loginBuffer = Connection.LoginBufferString;
            }

            Response loginResponse = await this.IssueCommandReadResponse(loginBuffer);

            if(loginResponse.responseType != VndbProtocol.ResponseType.Ok)
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "Connection failed: {0}", loginResponse.jsonPayload));
            }
        }

        public async Task Query(string query)
        {
            Response response = await IssueCommandReadResponse(query);
            
            if(response.responseType == VndbProtocol.ResponseType.Error)
            {
                Console.WriteLine("Error occurred:");
                Console.WriteLine(response.jsonPayload);
            }
            else
            {
                Console.WriteLine("Response:");
                Console.WriteLine(response.jsonPayload);
            }
        }

        private async Task<Response> IssueCommandReadResponse(string command)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(command);
            byte[] requestBuffer = new byte[encoded.Length + 1];
            Buffer.BlockCopy(encoded, 0, requestBuffer, 0, encoded.Length);
            requestBuffer[encoded.Length] = VndbProtocol.EndOfStreamByte;
            await this.stream.WriteAsync(requestBuffer, 0, requestBuffer.Length);
            byte[] responseBuffer = new byte[4096];
            int totalRead = 0;
            while (true)
            {
                int currentRead = await this.stream.ReadAsync(responseBuffer, totalRead, responseBuffer.Length - totalRead);

                if (currentRead == 0)
                {
                    throw new Exception("Connection closed while reading login response");
                }

                totalRead += currentRead;

                if (VndbProtocol.IsCompleteMessage(responseBuffer, totalRead))
                {
                    break;
                }

                // This should probably never happen, but just in case...
                if (totalRead == responseBuffer.Length)
                {
                    byte[] biggerBadderBuffer = new byte[responseBuffer.Length * 2];
                    Buffer.BlockCopy(responseBuffer, 0, biggerBadderBuffer, 0, responseBuffer.Length);
                    responseBuffer = biggerBadderBuffer;
                }
            }

            return VndbProtocol.Parse(responseBuffer, totalRead);
        }
    }
}
