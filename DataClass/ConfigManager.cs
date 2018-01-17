using System.Collections.Generic;
namespace HYLineBotWebApi.DataClass
{
    public class ConfigManager
    {
        public string ConnectionString { get; set; }
        public string ChannelAccessToken { get; set; }
        public Dictionary<string, string> UserIDList { get; set; }
    }
    public static class ConfigProvider
    {
        public static bool IsDevelopment { get; set; }
        public static string ConnectionString { get; set; }
        public static string ChannelAccessToken { get; set; }
        public static Dictionary<string, string> UserIDList { get; set; }
    }
}