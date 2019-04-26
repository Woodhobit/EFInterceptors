using EFInterceptors.Extensions;
using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Text.RegularExpressions;

namespace EFInterceptors.Interceptors
{
    public class FullTextSearchInterceptor : DbCommandInterceptor
    {
        private static readonly string FullTextPrefix = Guid.NewGuid().ToString();
        public static string FullTextSearch(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return searchTerm;
            }

            if (searchTerm.IndexOfAny(new char[] { '\"', '\'', ';', '+' }) >= 0)
            {
                return searchTerm;
            }

            return $"({FullTextPrefix}{searchTerm})";
        }

        public static bool IsFullTextApplied(string searchTerm)
        {
            return searchTerm.Contains(FullTextPrefix);
        }

        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            RewriteFullTextQuery(command);
            base.ReaderExecuting(command, interceptionContext);
        }

        public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            RewriteFullTextQuery(command);
            base.ScalarExecuting(command, interceptionContext);
        }

        private static void RewriteFullTextQuery(DbCommand cmd)
        {
            for (var i = 0; i < cmd.Parameters.Count; i++)
            {
                DbParameter parameter = cmd.Parameters[i];
                if (parameter.DbType.In(DbType.String, DbType.AnsiString, DbType.StringFixedLength, DbType.AnsiStringFixedLength))
                {
                    if (parameter.Value == DBNull.Value)
                    {
                        continue;
                    }

                    var value = (string)parameter.Value;
                    if (value.Contains(FullTextPrefix))
                    {
                        parameter.Value = FormatCommandValue(value);
                        cmd.CommandText = FormatCommandText(parameter.ParameterName, cmd.CommandText);
                    }
                }
            }
        }

        private static string FormatCommandValue(string value)
        {
            value = value.Replace(FullTextPrefix, "");
            value = value.Replace("~", string.Empty);
            value = value.Substring(1, value.Length - 2);
            value = $"\"*{value}*\"";

            return value;
        }

        private static string FormatCommandText(string parameterName, string commandText)
        {
            var result = Regex.Replace(commandText,
                $@"\[(\w*)\].\[(\w*)\]\s*LIKE\s*@{parameterName}\s?(?:ESCAPE N?'~')",
                $@"CONTAINS([$1].[$2], @{parameterName})");

            if (result == commandText)
            {
                throw new Exception("Full-text search was not replaced on: " + commandText);
            }

            return result;
        }
    }
}
