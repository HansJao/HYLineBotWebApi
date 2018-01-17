using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using HYLineBotWebApi.DataClass;

namespace HYLineBotWebApi.Common
{
    public class DapperHelper
    {
        public static IEnumerable<T> QueryCollection<T>(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            IEnumerable<T> result;
            using (var con = new SqlConnection(ConfigProvider.ConnectionString))
            {
                DynamicParameters parameters = parseParameters(commandParameters);
                result = con.Query<T>(commandText, parameters,commandType: commandType,commandTimeout:30);
            }
            return result;
        }

        private static DynamicParameters parseParameters(SqlParameter[] commandParameters)
        {
            DynamicParameters result = null;
            if (commandParameters != null)
            {
                result = new DynamicParameters();
                foreach (SqlParameter p in commandParameters)
                {
                    if (p == null) continue;
                    if ((p.Direction == ParameterDirection.Input || p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }
                    result.Add(p.ParameterName, p.Value, p.DbType, p.Direction);
                }
            }
            return result;
        }
    }
}