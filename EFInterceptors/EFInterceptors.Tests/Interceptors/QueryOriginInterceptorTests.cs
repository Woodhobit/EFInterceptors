using EFInterceptors.Interceptors;
using System.Data.Common;
using System.Data.SqlClient;
using Xunit;

namespace EFInterceptors.Tests.Interceptors
{
    public class QueryOriginInterceptorTests
    {
        [Fact(DisplayName = "Stack trace should be added in the result sql query")]
        public void StackTraceShouldBeAddedInQuery()
        {
            var interceptor = new QueryOriginInterceptor();
            var dbCommand = GetDbCommand();

            var lastMethodInStackTrace = nameof(this.StackTraceShouldBeAddedInQuery);

            interceptor.ReaderExecuting(dbCommand, null);
            Assert.Contains(lastMethodInStackTrace, dbCommand.CommandText);
        }

        private DbCommand GetDbCommand()
        {
            return new SqlCommand(@"SELECT 
                    [Extent1].[Id] AS [Id], 
                    [Extent1].[FirstName] AS [FirstName], 
                    [Extent1].[SecondName] AS [SecondName]
                    FROM [dbo].[Users] AS [Extent1]");
        }
    }
}
