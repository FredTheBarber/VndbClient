using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbClient
{
    internal static class VndbProtocol
    {
        public const byte EndOfStreamByte = 0x04;

        public enum ResponseType
        {
            Ok,
            Error,
            Unknown,
        }

        private struct ResponseTypeMapEntry
        {
            string responseText;
            ResponseType responseType;
        }

        private static Tuple<string, ResponseType>[] ResponseTypeMap = new Tuple<string, ResponseType>[]
        {
            new Tuple<string, ResponseType>("Ok", ResponseType.Ok),
            new Tuple<string, ResponseType>("Error", ResponseType.Error),
        };

        public static bool IsCompleteMessage(byte[] message, int bytesUsed)
        {
            if(bytesUsed == 0)
            {
                throw new Exception("You have a bug, dummy. You should have at least one byte here.");
            }

            // ASSUMPTION: simple request-response protocol, so we should see at most one message in a given byte array.
            // So, there's no need to walk the whole array looking for validity - just be lazy and check the last byte for EOS.
            if (message[bytesUsed - 1] == VndbProtocol.EndOfStreamByte)
                return true;
            else
                return false;
        }

        public static Response Parse(byte[] message, int bytesUsed)
        {
            if (!IsCompleteMessage(message, bytesUsed))
            {
                throw new Exception("You have a bug, dummy.");
            }

            string stringifiedResponse = Encoding.UTF8.GetString(message, 0, bytesUsed - 1);

            // format is either:
            // "messageType"
            // or "messageType JSONPAYLOAD"
            // So, if there's no space, it's just messageType, and if there's a single space,
            // we split on it and treat the rest as JSON (and there needs to be at least one more character).
            int firstSpace = stringifiedResponse.IndexOf(' ');
            if (firstSpace == bytesUsed - 1)
            {
                // protocol violation!
                throw new Exception("Protocol violation: last character in response is first space");
            }
            else if (firstSpace == -1)
            {
                // whole stringifiedResponse is just messageType
                Tuple<string, ResponseType> responseTypeEntry = ResponseTypeMap.FirstOrDefault(
                    l => string.Compare(l.Item1, stringifiedResponse, StringComparison.OrdinalIgnoreCase) == 0);
                if (responseTypeEntry == null)
                {
                    return new Response(ResponseType.Unknown, string.Empty);
                }
                else
                {
                    return new Response(responseTypeEntry.Item2, string.Empty);
                }
            }
            else
            {
                string responseTypeString = stringifiedResponse.Substring(0, firstSpace);

                Tuple<string, ResponseType> responseTypeEntry = ResponseTypeMap.FirstOrDefault(
                    l => string.Compare(l.Item1, responseTypeString, StringComparison.OrdinalIgnoreCase) == 0);

                if (responseTypeEntry == null)
                {
                    return new Response(ResponseType.Unknown, stringifiedResponse.Substring(firstSpace + 1));
                }
                else
                {
                    return new Response(responseTypeEntry.Item2, stringifiedResponse.Substring(firstSpace + 1));
                }
            }
        }
    }
}
