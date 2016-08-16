using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disclose.DataStores.DocumentDB
{
    public class DataStoreSettings
    {
        public string EndPointUri { get; set; }
        public string Key { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
    }
}
