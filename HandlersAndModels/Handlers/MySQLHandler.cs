using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using HandlersAndModels.Models;
using MySql.Data.MySqlClient;

namespace HandlersAndModels.Handlers
{
    public class DBConnection
    {
        private DBConnection()
        {
        }

        public string Server = "127.0.0.1";
        public string DatabaseName = "sportnew_sport";
        public string UserName = "root";
        public string Password = "ke324ksdfF8as";

        public MySqlConnection Connection { get; set;}

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }
    
        public async Task<bool> IsConnected()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                    return false;
                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DatabaseName, UserName, Password);
                Connection = new MySqlConnection(connstring);
                await Connection.OpenAsync();
            }
            if (Connection.State == ConnectionState.Closed) await Connection.OpenAsync();
    
            return true;
        }
    
        public void Close()
        {
            Connection.Close();
        }        
    }

    public static class MySQLHandler
    {
        public static async Task<List<Article>> GetAllArticlesAsync()
        {
            var dbCon = DBConnection.Instance();
            var retData = new List<Article>();

            if (await dbCon.IsConnected())
            {

                string query = "SELECT id,text,view FROM red_news";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = await cmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    string text = reader.GetString(1);
                    int views = reader.GetInt32(2);

                    retData.Add(new Article
                    {
                        Id = id,
                        Text = text,
                        Views = views
                    });
                    
                }
                dbCon.Close();
            }

            return retData;
        }

        public static async Task<string> GetAllArticlesTitlesCombinedAsync()
        {
            var dbCon = DBConnection.Instance();
            var retData = string.Empty;

            if (await dbCon.IsConnected())
            {

                string query = "SET SESSION group_concat_max_len = 100000000; SELECT GROUP_CONCAT(CONCAT(name, '<dmz>')) FROM red_news";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = await cmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                    retData = reader.GetString(0);
 
                }
                dbCon.Close();
            }

            return retData;
        }

        public static async Task<string> GetAllArticlesTextCombinedAsync()
        {
            var dbCon = DBConnection.Instance();
            var retData = string.Empty;

            if (await dbCon.IsConnected())
            {

                string query = "SET SESSION group_concat_max_len = 100000000; SELECT GROUP_CONCAT(CONCAT(text, '<dmz>')) FROM red_news";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = await cmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                    retData = reader.GetString(0);
 
                }
                dbCon.Close();
            }

            return retData;
        }

        public static async Task<List<SqlArticleItem>> GetAllArticlesWithMetadataAsync()
        {
            var dbCon = DBConnection.Instance();
            var retData = new List<SqlArticleItem>();

            if (await dbCon.IsConnected())
            {

                string query = @"SELECT 

	                                cn.id, 
	                                cn.text, 
	                                cn.view, 
	                                COALESCE( 
		                                NULLIF( preferred_menu, 0 ), 
		                                (SELECT menu_id FROM red_news_menu WHERE news_id = cn.id ORDER BY id ASC LIMIT 1) 
	                                ) as MainArticleCategory,
	                                cn.news_type,
	                                GROUP_CONCAT(cnm.menu_id) as Tags,
	                                cn.assocnews as Metadata,
	                                cn.video_id, 
	                                cn.insert_time,
                                    cn.name
	                                
                                FROM red_news cn 

                                LEFT JOIN red_news_menu cnm ON cn.id = cnm.news_id

                                LEFT JOIN red_video cv ON cn.video_id = cv.video_url

                                GROUP BY cn.id, cn.text, cn.view, MainArticleCategory, cn.news_type, Metadata, cn.video_id

                                ORDER BY cn.insert_time ASC";

                var cmd = new MySqlCommand(query, dbCon.Connection);
                cmd.CommandTimeout = 86400;
                var reader = await cmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                 

                    retData.Add(new SqlArticleItem
                    {
                        Id = reader.IsDBNull(0) ? -1 : reader.GetInt32(0),
                        Text = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Views = reader.IsDBNull(2) ? -1 : reader.GetInt32(2),
                        MainArticleCategory = reader.IsDBNull(3) ? -1 : reader.GetInt32(3),
                        NewsType = reader.IsDBNull(4) ? -1 : reader.GetInt32(4),
                        Tags =  reader.IsDBNull(5) ?  string.Empty : reader.GetString(5),
                        AssocNewsMetadata = reader.IsDBNull(6) ?  string.Empty : reader.GetString(6),
                        VideoId =  reader.IsDBNull(7) ?  string.Empty : reader.GetString(7),
                        InsertTime =  reader.IsDBNull(8) ?  0 : reader.GetInt64(8),
                        Title =  reader.IsDBNull(9) ?  string.Empty : reader.GetString(9),
                    });
                    
                }
                dbCon.Close();
            }

            return retData;
        }
    }
}
