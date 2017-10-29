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
            ServerDocument serverDocument = await _helper.GetServerDocument(server);

            if (serverDocument == null)
            {
                return default(TData);
            }

            return serverDocument.GetPropertyValue<TData>(key);
        }

        public async Task SetServerDataAsync<TData>(DiscloseServer server, string key, TData data)
        {
            //Users property is already taken to store user data for a server, so this can't be the key
            if (key == "Users")
            {
                throw new ArgumentException("key cannot be Users");
            }

            DocumentCollection collection = await _helper.GetOrCreateCollection();

            ServerDocument serverDocument = await _helper.GetOrInitServerDocument(server);

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

        public async Task<TData> GetUserDataForServerAsync<TData>(DiscloseServer server, DiscloseUser user, string key)
        {
            ServerDocument serverDocument = await _helper.GetServerDocument(server);

            if (serverDocument == null)
            {
                return default(TData);
            }

            if (!serverDocument.Users.ContainsKey(user.Id))
            {
                return default(TData);
            }

            if (!serverDocument.Users[user.Id].ContainsKey(key))
            {
                return default(TData);
            }

            var value = serverDocument.Users[user.Id][key];

            if (value is TData)
            {
                return (TData) value;
            }

            throw new InvalidCastException("Data Exists for this key, but is not of the type you expect");          
        }

        public async Task SetUserDataForServerAsync<TData>(DiscloseServer server, DiscloseUser user, string key, TData data)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            ServerDocument serverDocument = await _helper.GetOrInitServerDocument(server);

            if (!serverDocument.Users.ContainsKey(user.Id))
            {
                serverDocument.Users.Add(user.Id, new ExpandoObject());
            }

            serverDocument.Users[user.Id][key] = data;

            //The DocumentDB Wrapper does not like our Users property for some reason, so call SetPropertyValue explicitly here so it is saved
            serverDocument.SetPropertyValue("Users", serverDocument.Users);

            await _client.UpsertDocumentAsync(collection.SelfLink, serverDocument);
        }
    }
}
