using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Disclose.DataStores.DocumentDB
{
    internal class ServerDocument : Document
    {
        [JsonProperty]
        public Dictionary<ulong, IDictionary<string, object>> Users { get; set; }

        public ServerDocument()
        {
            Users = new Dictionary<ulong, IDictionary<string, object>>();
        }
    }
}
