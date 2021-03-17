using System;
using System.Collections.Generic;
using System.Text;

namespace HandlersAndModels
{
    public static class StaticGlobals
    {
        public static string FacebookGroupId = "ID_REDACTED";
        public static string MongoUsername = "admin";
        public static string MongoPassword = "Pa$$w0rd";
        public static string MongoSRVIP = "127.0.0.1:27017";
        public static long PointOfMeasure = 1614264035; // Unix epoch timestamp representing the exact point when SQL article data was extracted and therefore is used to calculate TimeSpan variable of articles
        // old unix epoch values:  1611493209
    }
}
