using System;

namespace VndbClient
{
    // Not bothering to parse the JSON payload at all right now. Just return the string JSON payload.
    // Can change to strongly-typed responses later if desired. If we want to shred the JSON, just 
    // define objects and take a dependency on JSON.Net, and let them do the hard work.
    internal class Response
    {
        public VndbProtocol.ResponseType responseType;
        public string jsonPayload;

        public Response(VndbProtocol.ResponseType responseType, string jsonPayload)
        {
            this.responseType = responseType;
            this.jsonPayload = jsonPayload;
        }
    }
}
