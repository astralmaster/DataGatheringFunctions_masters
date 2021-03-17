using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HandlersAndModels.Models;
using MongoDB.Bson;

namespace HandlersAndModels.Handlers
{
    public static class MongoDBHandler
    {
        private class MongoConnection
        {
            private MongoClient dbClient;
            private MongoConnection()
            {
                dbClient = new MongoClient($"mongodb://{StaticGlobals.MongoUsername}:{StaticGlobals.MongoPassword}@{StaticGlobals.MongoSRVIP}/?authSource=admin");
            }

            

            private static MongoConnection _instance = null;
            public static MongoConnection Instance()
            {
                if (_instance == null)
                    _instance = new MongoConnection();
                return _instance;
            }

            public MongoClient DBClient => dbClient;
        }

        public static async Task<List<Word>> GetWordCollection()
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("wordData");

            return await db.GetCollection<Word>("words").AsQueryable().ToListAsync();
        }

        
        public static async Task SaveWordCollection(List<Word> words)
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("wordData");

            var wordCollection = db.GetCollection<Word>("words");
            await wordCollection.DeleteManyAsync(new BsonDocument());
            await wordCollection.InsertManyAsync(words);
        }

        public static async Task InsertArticleWordOccurences(List<ArticleWordOccurence> data)
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("wordData");
            var articleWordOccurenceCollection = db.GetCollection<ArticleWordOccurence>("articleWordOccurence");

            await articleWordOccurenceCollection.InsertManyAsync(data);

        }

        public static async Task ClearArticleWordOccurences()
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("wordData");
            var articleWordOccurenceCollection = db.GetCollection<ArticleWordOccurence>("articleWordOccurence");

            await articleWordOccurenceCollection.DeleteManyAsync(new BsonDocument());
           
        }


        public static async Task InsertArticleWordOccurences100Words(List<ArticleWordOccurence> data)
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("wordData");
            var articleWordOccurenceCollection = db.GetCollection<ArticleWordOccurence>("articleWordOccurence100Words");

            await articleWordOccurenceCollection.DeleteManyAsync(new BsonDocument());
            await articleWordOccurenceCollection.InsertManyAsync(data);

        }


        public static async Task InsertFbGraphArticles(List<MediaManagerPostModels.Datum> data)
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("articleDataProcessed");

            var articleCollection = db.GetCollection<MediaManagerPostModels.Datum>("articles");

            await articleCollection.DeleteManyAsync(new BsonDocument());
            await articleCollection.InsertManyAsync(data);

        }

        public static async Task<List<MediaManagerPostModels.Datum>> GetFbGraphArticles()
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("articleDataProcessed");
            
            return await db.GetCollection<MediaManagerPostModels.Datum>("articles").AsQueryable().ToListAsync();
            
        }

        public static async Task InsertFinalArticleData(List<Article> data)
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("articleDataFinal");

            var articleCollection = db.GetCollection<Article>("articles");
           
            await articleCollection.InsertManyAsync(data);

        }

     

        public static async Task<List<ArticleWordOccurence>> GetArticleWordOccurenceData()
        {
            var instance = MongoConnection.Instance();
            var db = instance.DBClient.GetDatabase("wordData");
            
            return await db.GetCollection<ArticleWordOccurence>("articleWordOccurence").AsQueryable().ToListAsync();
            
        }
    }
}
