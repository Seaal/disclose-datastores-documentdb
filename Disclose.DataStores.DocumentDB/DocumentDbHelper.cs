﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Disclose.DataStores.DocumentDB
{
    internal class DocumentDbHelper
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

        public async Task<Document> GetServerDocument(DiscloseServer server)
        {
            DocumentCollection collection = await GetOrCreateCollection();

            return _client.CreateDocumentQuery<Document>(collection.SelfLink).Where(doc => doc.Id == GetServerId(server)).ToList().FirstOrDefault();
        }

        public async Task<Document> GetOrInitServerDocument(DiscloseServer server)
        {
            Document serverDocument = await GetServerDocument(server);

            if (serverDocument == null)
            {
                serverDocument = new Document()
                {
                    Id = GetServerId(server)
                };
            }

            return serverDocument;
        }

        public async Task<Document> GetUserDocument(DiscloseUser user)
        {
            DocumentCollection collection = await GetOrCreateCollection();

            return _client.CreateDocumentQuery(collection.SelfLink).Where(doc => doc.Id == GetUserId(user)).ToList().FirstOrDefault();
        }

        public async Task<Document> GetOrInitUserDocument(DiscloseUser user)
        {
            Document userDocument = await GetUserDocument(user);

            if (userDocument == null)
            {
                userDocument = new Document()
                {
                    Id = GetUserId(user)
                };
            }

            return userDocument;
        }

        private string GetServerId(DiscloseServer server)
        {
            return "server-" + server.Id;
        }

        private string GetUserId(DiscloseUser user)
        {
            return "user-" + user.Id;
        }
    }
}
