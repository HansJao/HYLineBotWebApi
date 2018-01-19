using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using HYLineBotWebApi.Common;
using HYLineBotWebApi.DataClass;

namespace HYLineBotWebApi.Adapter
{
    public class DataAdapter
    {
        // public List<DataObject> Get(int TableNameId)
        // {
        //     var commandText = "select * from TableName Where TableNameId=@TableNameId";
        //     SqlParameter[] parameters = new SqlParameter[]
        //     {
        //         new SqlParameter("@TableNameId",SqlDbType.Int){Value = TableNameId}
        //     };
        //     var result = DapperHelper.QueryCollection<DataObject>(ConfigProvider.ConnectionString, CommandType.Text, commandText, parameters);

        //     return result.ToList();
        // }

        public int Insert(string area, string name, string color, string position, int quantity, string userName, string memo)
        {
            var commandText = @"INSERT INTO dbo.TextileStore
                                (Area,Name,Color,Position,Quantity,ModifyUser,ModifyDate,Memo)
                                VALUES
                                (@Area,@Name,@Color,@Position,@Quantity,@ModifyUser,@ModifyDate,@Memo)
                                SELECT @@ROWCOUNT AS InsertRow;";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Area",SqlDbType.NVarChar){Value = area},
                new SqlParameter("@Name",SqlDbType.NVarChar){Value = name},
                new SqlParameter("@Color",SqlDbType.NVarChar){Value = color},
                new SqlParameter("@Position",SqlDbType.NVarChar){Value = position},
                new SqlParameter("@Quantity",SqlDbType.Int){Value = quantity},
                new SqlParameter("@ModifyUser",SqlDbType.NVarChar){Value = userName},
                new SqlParameter("@ModifyDate",SqlDbType.DateTime){Value = DateTime.Now},
                new SqlParameter("@Memo",SqlDbType.NVarChar){Value = memo},
            };
            var result = DapperHelper.QueryCollection<int>(ConfigProvider.ConnectionString, CommandType.Text, commandText, parameters).FirstOrDefault();

            return result;
        }

        public IEnumerable<TextileStore> SearchArea(string area)
        {
            var commandText = @"SELECT * FROM TextileStore WHERE Area LIKE @Area";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Area",SqlDbType.NVarChar){Value = string.Concat("%",area,"%")}
            };
            var result = DapperHelper.QueryCollection<TextileStore>(ConfigProvider.ConnectionString, CommandType.Text, commandText, parameters);

            return result;
        }

        public IEnumerable<TextileStore> SearchName(string name)
        {
            var commandText = @"SELECT * FROM TextileStore WHERE Name LIKE @Name";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Name",SqlDbType.NVarChar){Value = string.Concat("%",name,"%")}
            };
            var result = DapperHelper.QueryCollection<TextileStore>(ConfigProvider.ConnectionString, CommandType.Text, commandText, parameters);

            return result;
        }

        public int DelectByID(int id)
        {
            var commandText = @"DELETE FROM TextileStore WHERE Id = @Id
                                SELECT @@ROWCOUNT AS DeleteRow;";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id",SqlDbType.Int){Value = id}
            };
            var result = DapperHelper.QueryCollection<int>(ConfigProvider.ConnectionString, CommandType.Text, commandText, parameters).FirstOrDefault();

            return result;
        }

        public int UpdateQuantityByID(int id, int quantity)
        {
            var commandText = @"UPDATE TextileStore
                                SET Quantity = @Quantity
                                 WHERE Id = @Id
                                SELECT @@ROWCOUNT AS UpdateRow;";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id",SqlDbType.Int){Value = id},
                new SqlParameter("@Quantity",SqlDbType.Int){Value = quantity}
            };
            var result = DapperHelper.QueryCollection<int>(ConfigProvider.ConnectionString, CommandType.Text, commandText, parameters).FirstOrDefault();

            return result;
        }
    }
}