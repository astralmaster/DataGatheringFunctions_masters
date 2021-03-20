using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HandlersAndModels.Handlers;
using HandlersAndModels.Models;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace WordExtractionApp
{
    public partial class Form1 : Form
    {
        
        private string _intermediateTextContainer = string.Empty;
        private List<Word> _intermediateWordContainer;

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            /*
                Abandoned...

                Fastenshtein.Levenshtein lev = new Fastenshtein.Levenshtein("ფეხბურთელი");
                int levenshteinDistance = lev.DistanceFrom("ფეხბურთელმა");
            */

            var data = await MySQLHandler.GetAllArticlesTextCombinedAsync();
            data = data.Replace("<dmz>", " ");

            var text = data;

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(text);
         
            string res = htmlDoc.DocumentNode.InnerText;
            res = WebUtility.HtmlDecode(res);

            res = res.Replace("\\", " ");

            _intermediateTextContainer = res;

            File.WriteAllText(@"D:\wamp\tmp\articleTextData.txt", res);

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var res = _intermediateTextContainer;

            // define and remove 'operator' words

            var operator_words = new[]
            {
                "და",
                "რომ",
                "არ",
                "წლის",
                "შემდეგ",
                "კი",
                "თმცა",
                "ამ",
                "ეს",
                "მისი",
                "რომელიც",
                "უნდა",
                "რაც",
                "მაგრამ",
                "მას",
                "ის",
                "როგორც",
                "ვერ",
                "თუ",
                "მხოლოდ",
                "თუმცა",
                "იყო",
                "ძალიან",
                "აქვს",
                "რა",
                "ამის",
                "უფრო",
                "იმ",
                "ასე",
                "ასეთი",
                "ან"

            };

            foreach (var item in operator_words)
            {
                string pattern = @"\b"+item+@"\b"; // replace whole word only
                res = Regex.Replace(res, pattern, string.Empty); 
            }

            var punctuation = new[]
            {
                ".",
                ",",
                ":",
                "!",
                "?",
                "%",
                "(",
                ")",
                "=",
                "-",
            };

            foreach (var item in punctuation)
            {
                res = res.Replace(item, string.Empty); 
            }
                

            var result  = res.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(r => r)
                .Select(grp => new Word
                {
                    WordName = grp.Key,
                    Occurence = grp.Count()
                }).OrderByDescending(b => b.Occurence).ToList();

            // remove those words that have occurence = 1,  since they cannot influence the ML algorithm in a positive way.
            //TODO:  for the rest of the words, make sure they are distributed in at least 2 articles. 

            result = result.Where(a => a.Occurence > 1).ToList();
            

            // remove newline chars
     
            result = result.Where(a => !a.WordName.Equals("\n") && !a.WordName.Equals(" ")).ToList();

            result = result.Skip(1).ToList();

            dataGridView1.DataSource = result;

            _intermediateWordContainer = result;

            await MongoDBHandler.SaveWordCollection(result);

        }

        private async void button3_Click(object sender, EventArgs e)
        {
             
            //var strData =  File.ReadAllText(@"D:\wamp\tmp\3.txt");
            //var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Word>>(strData);


            //var wordsWithOccurences = await MongoDBHandler.GetWordCollection();
            var wordsWithOccurences = _intermediateWordContainer;


            var articles = await MySQLHandler.GetAllArticlesAsync();
            //articles.Reverse();
            //articles = articles.Take(110).ToList();
            // take top 1000 words 

            var wordsToProcess = wordsWithOccurences.Take(1000).ToList();

            // get occurence of each word by individual article

            var articleWordOccurenceList = new List<ArticleWordOccurence>();

            int counter = 0;

            await MongoDBHandler.ClearArticleWordOccurences();

            var thread = new Thread(async () =>
            {
                foreach (var article in articles)
                {
                    foreach (var word in wordsToProcess)
                    {
                        var count = GeneralHandlers.CountStringOccurrences(article.Text, word.WordName);
                        articleWordOccurenceList.Add(new ArticleWordOccurence
                        {
                            ArticleId = article.Id,
                            Word = word.WordName,
                            Occurence = count
                        });

                    }


                    if (counter > 1000)
                    {
                        counter = 0;
                        
                        await MongoDBHandler.InsertArticleWordOccurences(articleWordOccurenceList);
                        articleWordOccurenceList.Clear();


                    }

                    counter++;


                }

                await MongoDBHandler.InsertArticleWordOccurences(articleWordOccurenceList);
              

            });

            thread.Start();

           

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            var articleWordOccurenceData = await MongoDBHandler.GetArticleWordOccurenceData();

            var words = await MongoDBHandler.GetWordCollection();

            // exclude numbers 0-9 and article banner placeholder and some other irrelevant characters
           
            var toExclude = new[]
            {
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "\n[ArticleBannerPlaceholder]\n", "10","20","\t", "\n—", "„"
            };

            words = words.Where(a => !toExclude.Contains(a.WordName)).ToList();

            // get first 100 words by occurence (DESC) and extract articleWordOccurence data according to it

            var wordsArray = words.OrderByDescending(a => a.Occurence).Take(100).Select(a => a.WordName).ToList();

            var aWODExtracted = articleWordOccurenceData.Where(a => wordsArray.Contains(a.Word)).ToList();

            await MongoDBHandler.InsertArticleWordOccurences100Words(aWODExtracted);
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            var titlesData = await MySQLHandler.GetAllArticlesTitlesCombinedAsync();
            var file = @"D:\wamp\tmp\titlesData.txt";

            if(File.Exists(file))
            {
                File.Delete(file);
            }
            File.WriteAllText(file, titlesData);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
