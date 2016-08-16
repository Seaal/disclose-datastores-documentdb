using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Disclose.DiscordClient;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.CSharp.RuntimeBinder;

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

        public async Task<TData> GetServerData<TData>(IServer server, string key)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            ServerDocument serverDocument = _client.CreateDocumentQuery<ServerDocument>(collection.SelfLink).Where(doc => doc.Id == server.Id.ToString() && doc.Type == DocumentType.Server).ToList().FirstOrDefault();

            if (serverDocument == null)
            {
                return default(TData);
            }

            return serverDocument.GetPropertyValue<TData>(key);
        }

        public async Task SetServerData<TData>(IServer server, string key, TData data)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            ServerDocument serverDocument = await _helper.GetOrCreateServerDocument(server);

            serverDocument.SetPropertyValue(key, data);

            await _client.UpsertDocumentAsync(collection.SelfLink, serverDocument);
        }

        public async Task<TData> GetUserData<TData>(IUser user, string key)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            DiscloseDocument userDocument = _client.CreateDocumentQuery<DiscloseDocument>(collection.SelfLink).Where(doc => doc.Id == user.Id.ToString() && doc.Type == DocumentType.User).ToList().FirstOrDefault();

            if (userDocument == null)
            {
                return default(TData);
            }

            return userDocument.GetPropertyValue<TData>(key);
        }

        public async Task SetUserData<TData>(IUser user, string key, TData data)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            DiscloseDocument userDocument = _client.CreateDocumentQuery<DiscloseDocument>(collection.SelfLink).Where(doc => doc.Id == user.Id.ToString() && doc.Type == DocumentType.Server).ToList().FirstOrDefault();

            if (userDocument == null)
            {
                userDocument = new DiscloseDocument()
                {
                    Id = user.Id.ToString(), Type = DocumentType.Server
                };
            }

            userDocument.SetPropertyValue(key, data);

            await _client.UpsertDocumentAsync(collection.SelfLink, userDocument);
        }

        public async Task<TData> GetUserDataForServer<TData>(IServer server, IUser user, string key, TData data)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            ServerDocument serverDocument = _client.CreateDocumentQuery<ServerDocument>(collection.SelfLink).Where(doc => doc.Id == server.Id.ToString() && doc.Type == DocumentType.Server).ToList().FirstOrDefault();

            if (serverDocument == null)
            {
                return default(TData);
            }

            if (!serverDocument.Users.ContainsKey(user.Id))
            {
                return default(TData);
            }

            try
            {
                return serverDocument.Users[user.Id][key];
            }
            catch (RuntimeBinderException)
            {
                return default(TData);
            }
            
        }

        public async Task SetUserData<TData>(IServer server, IUser user, string key, TData data)
        {
            DocumentCollection collection = await _helper.GetOrCreateCollection();

            ServerDocument serverDocument = await _helper.GetOrCreateServerDocument(server);

            if (!serverDocument.Users.ContainsKey(user.Id))
            {
                serverDocument.Users.Add(user.Id, new ExpandoObject());
            }

            serverDocument.Users[user.Id][key] = data;

            await _client.UpsertDocumentAsync(collection.SelfLink, serverDocument);
        }
    }
}
