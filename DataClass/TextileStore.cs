using System;

namespace HYLineBotWebApi.DataClass
{
   public class TextileStore
    {
        public int ID { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Position { get; set; }
        public int Quantity { get; set; }
        public string ModifyUser { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Memo { get; set; }
    }
}