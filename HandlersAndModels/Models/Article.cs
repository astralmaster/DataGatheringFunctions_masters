using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HandlersAndModels.Models
{
    public class Word

    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string WordName { get; set; }
        public int Occurence { get; set; }

        public Word()
        {
            _id = ObjectId.GenerateNewId();
        }
    }

    public class Article
    {
        public int Id { get; set; }
        public long ActivePeriod { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int Views { get; set; }
        public int ImageCount { get; set; }
        public int VideoCount { get; set; }
        public int IframeCount { get; set; }
        public int MainArticleCategory { get; set; }
        public int ArticleType { get; set; }
        public List<Tag> Tags { get; set; }
        public string Author { get; set; }
        public bool VideoFocusedArticle { get; set; }
        public bool BannerInsideArticle { get; set; }
        public bool PublishedOnSocialMedia { get; set; }
        public bool BoostedOnSocialMedia { get; set; }

    }

    public class SqlArticleItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public long InsertTime { get; set; }
        public string Text { get; set; }
        public int Views { get; set; }
        public int MainArticleCategory { get; set; }
        public int NewsType { get; set; }
        public string Tags { get; set; }
        public string AssocNewsMetadata { get; set; }
        public string VideoId { get; set; }
     

    }


    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [BsonIgnoreExtraElements] 
    public class ArticleWordOccurence
    {
        public int ArticleId { get; set; }
        public string Word { get; set; }
        public int Occurence { get; set; }

    }
}
