namespace HYLineBotWebApi.DataClass.Enumeration
{
    public class PageEnum
    {
        /// <summery>
        /// 倉庫搜尋分頁列舉
        /// </summery>
        public enum SearchPageEnum : int
        {
            /// <summery>
            /// 每筆查詢總資料數
            /// </summery>
            TotalCount = 6,
            /// <summery>
            /// 每一個Column的數量
            /// </summery>
            PerColumn = 3,
        }
    }
}