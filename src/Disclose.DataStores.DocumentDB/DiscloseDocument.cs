using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Disclose.DataStores.DocumentDB
{
    public enum DocumentType
    {
        Server,
        User
    }

    public class DiscloseDocument : Document
    {
        public DocumentType Type { get; set; }
    }
}
