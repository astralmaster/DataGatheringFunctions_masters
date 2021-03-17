using System;
using System.Collections.Generic;
using System.Text;

namespace HandlersAndModels.Models
{
    public class PublishedPostsGraphModels
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Action    {
            public string name { get; set; } 
            public string link { get; set; } 
        }

        public class Datum    {
            public string unshimmed_url { get; set; } 
            public string id { get; set; } 
            public List<Action> actions { get; set; } 
            public DateTime created_time { get; set; } 
            public Attachments attachments { get; set; } 
        }

        public class Attachments    {
            public List<Datum> data { get; set; } 
        }

        public class Cursors    {
            public string before { get; set; } 
            public string after { get; set; } 
        }

        public class Paging    {
            public Cursors cursors { get; set; } 
            public string next { get; set; } 
        }

        public class Root    {
            public List<Datum> data { get; set; } 
            public Paging paging { get; set; } 
        }


    }

    public class MediaManagerPostModels
    {
        public class FBPostResponseModel 
        {
            public List<Datum> data { get; set; } 
            public Paging paging { get; set; } 
        }

        public class Value    {
            public bool is_enable { get; set; } 
        }

        public class AttributesWithMetadata    {
            public string key { get; set; } 
            public Value value { get; set; } 
        }

        public class Datum    {
            public bool is_gaming_video { get; set; } 
            public List<AttributesWithMetadata> attributes_with_metadata { get; set; } 
            public bool is_dogfooded { get; set; } 
            public bool is_publishing_with_video_list { get; set; } 
            public string owner_thumbnail_src { get; set; } 
            public int publish_timestamp { get; set; } 
            public bool is_profile_plus_owned_video { get; set; } 
            public string created_by_role { get; set; } 
            public string owner_id { get; set; } 
            public int last_edit_timestamp { get; set; } 
            public int last_added_timestamp { get; set; } 
            public string created_by { get; set; } 
            public string thumbnail_src { get; set; } 
            public string post_status { get; set; } 
            public string page_id_coerced { get; set; } 
            public List<string> eligible_actions { get; set; } 
            public string owner_name { get; set; } 
            public bool show_ad_break_block_list_settings { get; set; } 
            public int scheduled_or_last_added_timestamp { get; set; } 
            public bool is_crosspost_to_ig_eligible_photos { get; set; } 
            public string title { get; set; } 
            public bool can_edit { get; set; } 
            public string post_type { get; set; } 
            public string mature_content_rating { get; set; } 
            public string post_token { get; set; } 
            public string id { get; set; } 
            public string permalink_url { get; set; } 
            public string story_graphql_id { get; set; } 
            public string description { get; set; } 
            public bool is_crossposting_eligible { get; set; } 
            public bool ad_break_enabled_for_page { get; set; } 

            public string data_link { get; set; }
        }

        public class Cursors    {
            public string before { get; set; } 
            public string after { get; set; } 
        }

        public class Paging    {
            public Cursors cursors { get; set; } 
            public string next { get; set; } 
            public string previous { get; set; } 
        }
    }
    
}
