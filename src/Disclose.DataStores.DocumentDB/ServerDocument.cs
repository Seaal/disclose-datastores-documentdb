using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disclose.DataStores.DocumentDB
{
    public class ServerDocument : DiscloseDocument
    {
        public Dictionary<ulong, dynamic> Users { get; set; }

        public ServerDocument()
        {
            Type = DocumentType.Server;
        }
    }
}
