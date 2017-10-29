using System;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Disclose.DataStores.DocumentDB
{
    public class DocumentDbDataStore : IDataStore
    {
        private readonly DocumentClient _client;
        private readonly DocumentDbHelper _helper;

        public DocumentDbDataStore(Action<DataStoreSettings> setSettings)
        {
            DataStoreSettings settings = new DataStoreSettings();

            setSettings(settings);

            _client = new DocumentClient(new Uri(settings.EndPointUri), settings.Key);

            _helper = new DocumentDbHelper(_client, settings);
        }

        public async Task<TData> GetServerDataAsync<TData>(DiscloseServer server, string key)
        {
            Document serverDocument = await _helper.GetServerDocument(server);

            if (serverDocument == null)
            {
                return default(TData);
            }

            return serverDocument.GetPropertyValue<TData>(key);
        }

        public async Task SetServerDataAsync<TData>(DiscloseServer server, string key, TData data)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            Document serverDocument = await _helper.GetOrInitServerDocument(server);

            serverDocument.SetPropertyValue(key, data);

            await _client.UpsertDocumentAsync(collection.SelfLink, serverDocument);
        }

        public async Task<TData> GetUserDataAsync<TData>(DiscloseUser user, string key)
        {
            Document userDocument = await _helper.GetUserDocument(user);

            if (userDocument == null)
            {
                return default(TData);
            }

            return userDocument.GetPropertyValue<TData>(key);
        }

        public async Task SetUserDataAsync<TData>(DiscloseUser user, string key, TData data)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            Document userDocument = await _helper.GetOrInitUserDocument(user);

            userDocument.SetPropertyValue(key, data);

            await _client.UpsertDocumentAsync(collection.SelfLink, userDocument);
        }
    }
}
