using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HandlersAndModels;
using HandlersAndModels.Handlers;
using HandlersAndModels.Models;
using MongoDB.Bson.IO;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataGatheringFunctions
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            // get articles from MySQL database filled with metadata

            var sqlArticles = await MySQLHandler.GetAllArticlesWithMetadataAsync();

            //var data = Newtonsoft.Json.JsonConvert.SerializeObject(sqlArticles);
            //File.WriteAllText(@"D:\wamp\tmp\sqlData.json", data);

            //var sqlArticles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SqlArticleItem>>(File.ReadAllText(@"D:\wamp\tmp\sqlData.json"));

            // map SQL article data structure to full featured data set

            var articles = new List<Article>();

            foreach (var sqlArticle in sqlArticles)
            {
                var article = new Article
                {
                    Id = sqlArticle.Id,
                    Title = sqlArticle.Title,
                    Text = sqlArticle.Text,
                    Views = sqlArticle.Views,
                    MainArticleCategory = sqlArticle.MainArticleCategory,
                    ArticleType = sqlArticle.NewsType,
                    Tags = string.IsNullOrEmpty(sqlArticle.Tags)
                        ? new List<Tag>()
                        : sqlArticle.Tags.Split(',').Select(b => new Tag {Id = int.Parse(b)}).ToList(),
                  
                    VideoFocusedArticle = !string.IsNullOrEmpty(sqlArticle.VideoId),
                    ActivePeriod = StaticGlobals.PointOfMeasure - sqlArticle.InsertTime
                };

                if (GeneralHandlers.IsValidJson(sqlArticle.AssocNewsMetadata))
                {
                    var p_author = JObject.Parse(sqlArticle.AssocNewsMetadata)["p_author"];
                    article.Author = p_author == null ? string.Empty : p_author.ToString();
                }

                articles.Add(article);
            }

           
            // Retrieve FB Graph article data to fill Facebook related fields for articles

            var fbGraphArticleData = await MongoDBHandler.GetFbGraphArticles();
           

            // process FB Graph article data to only contain articles we are interested in (remove other post types)

            

            var fbGraphArticleDataFiltered = fbGraphArticleData.Where(a => !string.IsNullOrEmpty(a.data_link) &&
                                                                           a.data_link.IndexOf("sportswebsitenameredacted.ge/news/",
                                                                               StringComparison.InvariantCulture) >= 0)
                .ToList();

           

            fbGraphArticleDataFiltered.ForEach(a => a.data_link = a.data_link.Split('/').Last());
            fbGraphArticleDataFiltered.ForEach(a => a.data_link = a.data_link.Split('?').First());

            fbGraphArticleDataFiltered =
                fbGraphArticleDataFiltered.Where(a => Regex.IsMatch(a.data_link, @"^\d+$")).ToList();

            foreach (var article in articles)
            {
                // 1. Get the amount of images in the article

                article.ImageCount = GeneralHandlers.CountStringOccurrences(article.Text, "<img ");

                // 2. Get the amount of videos in the article

                article.VideoCount = GeneralHandlers.CountStringOccurrences(article.Text, "<video ");

                // 3. Get the amount of iframes in the article

                article.IframeCount = GeneralHandlers.CountStringOccurrences(article.Text, "<iframe ");

                // 4. Check if there's a promotional banner within the article's text

                article.BannerInsideArticle = article.Text.Contains("[ArticleBannerPlaceholder]");
                
                // 5. Get corresponding article object in Graph API Data and use it to fill Facebook related fields

                article.PublishedOnSocialMedia =
                    fbGraphArticleDataFiltered.Any(a => a.data_link.Equals(article.Id.ToString()));

                article.BoostedOnSocialMedia =  fbGraphArticleDataFiltered.Any(a => a.data_link.Equals(article.Id.ToString()) && a.attributes_with_metadata[0].value.is_enable == true);
            }

            // Save final processed article data to mongo db

            await MongoDBHandler.InsertFinalArticleData(articles);


        }

        private async void button2_Click(object sender, EventArgs e)
        {
           
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            /*var z1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tag>>(File.ReadAllText(@"D:\wamp\tmp\distinct_1.json"));
            var z2 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tag>>(File.ReadAllText(@"D:\wamp\tmp\distinct_2.json"));
            var k1 = z1.Select(a => a.Id).ToList();
            var k2 = z2.Select(a => a.Id).ToList();

            var m = k1.Except(k2).ToList();*/


        }
    }
}
