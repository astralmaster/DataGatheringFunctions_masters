using HandlersAndModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HandlersAndModels;
using HandlersAndModels.Handlers;
using RestSharp;

namespace FBGraphApiDataRetrieval
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
               if (string.IsNullOrEmpty(textBox1.Text)) return;

            var accessToken = textBox1.Text;

            var client = new RestClient("https://graph.facebook.com/v7.0/");

            var unixUntilTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

            var url =$"media_manager/media_manager_posts?limit=250&access_token={accessToken}&__cppo=1&_reqName=objectByName%3Amedia_manager%2Fmedia_manager_posts&_reqSrc=MediaManagerContentLibraryStore&fields=%5B%22is_gaming_video%22%2C%22attributes_with_metadata%22%2C%22download_hd_url%22%2C%22is_dogfooded%22%2C%22aspect_ratio%22%2C%22is_publishing_with_video_list%22%2C%22season_number%22%2C%22embeddable%22%2C%22owner_thumbnail_src%22%2C%22publish_timestamp%22%2C%22duration_sec%22%2C%22is_profile_plus_owned_video%22%2C%22created_by_role%22%2C%22owner_id%22%2C%22monetization_status%22%2C%22profile_plus_id%22%2C%22last_edit_timestamp%22%2C%22video_asset_id%22%2C%22last_added_timestamp%22%2C%22created_by%22%2C%22is_clips_enabled%22%2C%22thumbnail_src%22%2C%22post_status%22%2C%22page_id_coerced%22%2C%22eligible_actions%22%2C%22post_alerts%22%2C%22owner_name%22%2C%22attributes%22%2C%22show_ad_break_block_list_settings%22%2C%22scheduled_or_last_added_timestamp%22%2C%22video_container_post_id%22%2C%22is_crosspost_to_ig_eligible_photos%22%2C%22download_sd_url%22%2C%22story_id_for_profile_plus_post_insight%22%2C%22title%22%2C%22schedule_expire_or_expired_timestamp%22%2C%22can_edit%22%2C%22post_type%22%2C%22mature_content_rating%22%2C%22post_token%22%2C%22demonetization_reasons%22%2C%22id%22%2C%22permalink_url%22%2C%22story_graphql_id%22%2C%22description%22%2C%22backdated_time%22%2C%22schedule_publish_timestamp%22%2C%22is_crossposting_eligible%22%2C%22ad_break_enabled_for_page%22%5D&filtering=%5B%5D&locale=en_US&method=get&page_list=%5B%221479379595698489%22%5D&post_type=ALL&pretty=0&search_terms=&since=1230840000&sort_term=creation_time_desc&suppress_http_code=1&until={unixUntilTimestamp}&view_mode=media_manager&xref=f6d3a708e3ad9";
            
        
            var data = new List<MediaManagerPostModels.Datum>();

            await GetDataMediaManager(client, url, data);

            var str = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            File.WriteAllText(@"D:\wamp\tmp\media_manager_posts.json", str);

        }

        private async Task GetDataMediaManager(RestClient client, string url, List<MediaManagerPostModels.Datum> data)
        {
            var request = new RestRequest(url);
            var ret = await client.GetAsync<MediaManagerPostModels.FBPostResponseModel>(request);

            if (ret?.data != null)
                data.AddRange(ret.data);

            if (ret?.paging != null && !string.IsNullOrEmpty(ret.paging.next))
            {
                await Task.Delay(20);
                var newUrl = ret.paging.next;
                await GetDataMediaManager(client, newUrl, data);
            }
           
            
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text)) return;

            var accessToken = textBox2.Text;

            var client = new RestClient("https://graph.facebook.com/v9.0/");
        
            var unixUntilTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

            var url = "https://graph.facebook.com/v9.0/"+StaticGlobals.FacebookGroupId+"/published_posts?limit=100&fields=promotion_status,promotable_id,id,actions,created_time,attachments{unshimmed_url}&since=1230840000&sort_term=creation_time_desc&suppress_http_code=1&until="+unixUntilTimestamp+"&access_token="+accessToken;
         
            var data = new List<PublishedPostsGraphModels.Datum>();

            await GetDataPublishedPosts(client, url, data);

            var str = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            File.WriteAllText(@"D:\wamp\tmp\published_posts_graphAPI.json", str);
        }

        private async Task GetDataPublishedPosts(RestClient client, string url, List<PublishedPostsGraphModels.Datum> data)
        {
            var request = new RestRequest(url);
            var ret = await client.GetAsync<PublishedPostsGraphModels.Root>(request);
            
            if (ret?.data != null)
                data.AddRange(ret.data);

            if (ret?.paging != null && !string.IsNullOrEmpty(ret.paging.next))
            {
                await Task.Delay(20);
                var newUrl = ret.paging.next;
                await GetDataPublishedPosts(client, newUrl, data);
            }
           
            
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var MediaManagerDataJSON = File.ReadAllText(@"D:\wamp\tmp\media_manager_posts.json");
            var PublishedPostsDataJSON = File.ReadAllText(@"D:\wamp\tmp\published_posts_graphAPI.json");

            var MediaManagerData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MediaManagerPostModels.Datum>>(MediaManagerDataJSON);
            var PublishedPostsData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PublishedPostsGraphModels.Datum>>(PublishedPostsDataJSON);

            // match each article from media manager list with a data_link from published posts list.
           
            foreach (var article in MediaManagerData)
            {
                // PublishedPosts data list contains IDs with prefix, so split them first and get the actual id
                var ppEntry = PublishedPostsData.FirstOrDefault(a => a.id.Split('_')[1].Equals(article.id));
                if (ppEntry != null && ppEntry.attachments != null)
                {
                    article.data_link = ppEntry.attachments.data[0].unshimmed_url;
                    
                }
                    
            }

            await MongoDBHandler.InsertFbGraphArticles(MediaManagerData);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
