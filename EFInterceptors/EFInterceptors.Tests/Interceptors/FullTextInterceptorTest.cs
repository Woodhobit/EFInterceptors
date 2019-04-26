using EFInterceptors.Interceptors;
using System.Data.Common;
using System.Data.SqlClient;
using Xunit;

namespace EFInterceptors.Tests.Interceptors
{
    public class FullTextInterceptorTest
    {
        [Fact(DisplayName = "The LIKE statement should be replaced by CONTAINS statement")]
        public void ReplaceLikeStatement()
        {
            var fullTextSearchInterceptor = new FullTextSearchInterceptor();
            var dbCommand = GetDbCommand();

            Assert.Contains("LIKE", dbCommand.CommandText);
            Assert.DoesNotContain("CONTAINS", dbCommand.CommandText);

            fullTextSearchInterceptor.ReaderExecuting(dbCommand, null);

            Assert.DoesNotContain("LIKE", dbCommand.CommandText);
            Assert.Contains("CONTAINS", dbCommand.CommandText);
        }

        private DbCommand GetDbCommand()
        {
            var sqlCommand = new SqlCommand(@"SELECT 
                    [Extent1].[Id] AS [Id], 
                    [Extent1].[FirstName] AS [FirstName], 
                    [Extent1].[SecondName] AS [SecondName]
                    FROM [dbo].[Users] AS [Extent1]
                    WHERE [Extent1].[FirstName] LIKE @p__linq__0 ESCAPE N'~'");

            sqlCommand.Parameters.AddWithValue("p__linq__0", FullTextSearchInterceptor.FullTextSearch("test"));

            return sqlCommand;
        }
    }
}
