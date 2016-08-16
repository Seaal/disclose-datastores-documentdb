using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disclose.DiscordClient;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Disclose.DataStores.DocumentDB
{
    public class DocumentDbHelper
    {
        private readonly DocumentClient _client;
        private readonly DataStoreSettings _settings;

        private DocumentCollection _collection;

        public DocumentDbHelper(DocumentClient client, DataStoreSettings settings)
        {
            _client = client;
            _settings = settings;
        }

        private async Task<Database> GetOrCreateDatabase()
        {
            Database database = _client.CreateDatabaseQuery().Where(db => db.Id == _settings.DatabaseId).ToList().FirstOrDefault();

            if (database != null)
            {
                return database;
            }

            return await _client.CreateDatabaseAsync(new Database() { Id = _settings.DatabaseId });
        }

        public async Task<DocumentCollection> GetOrCreateCollection()
        {
            if (_collection != null)
            {
                return _collection;
            }

            Database database = await GetOrCreateDatabase();

            _collection = _client.CreateDocumentCollectionQuery(database.SelfLink).Where(col => col.Id == _settings.CollectionId).ToList().FirstOrDefault();

            if (_collection != null)
            {
                return _collection;
            }

            _collection = await _client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection() {Id = _settings.CollectionId});

            return _collection;
        }

        public async Task<ServerDocument> GetOrCreateServerDocument(IServer server)
        {
            DocumentCollection collection = await GetOrCreateCollection();

            ServerDocument serverDocument = _client.CreateDocumentQuery<ServerDocument>(collection.SelfLink).Where(doc => doc.Id == server.Id.ToString() && doc.Type == DocumentType.Server).ToList().FirstOrDefault();

            if (serverDocument == null)
            {
                serverDocument = new ServerDocument()
                {
                    Id = server.Id.ToString()
                };
            }

            return serverDocument;
        }
    }
}
