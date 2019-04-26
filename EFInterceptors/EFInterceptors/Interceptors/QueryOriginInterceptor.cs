using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text.RegularExpressions;

namespace EFInterceptors.Interceptors
{
    public class QueryOriginInterceptor : DbCommandInterceptor
    {
        private const string sqlCommentOpenTag = "/*";
        private const string sqlCommentCloseTag = "*/";
        private const string stackLoggerStartTag = sqlCommentOpenTag + " Stack:";

        private readonly string newLine = Environment.NewLine;
        private readonly Regex regex = new Regex(@"at (?<namespace>.*)\.(?<class>.*)\.(?<method>.*(.*)) in (?<file>.*):line (?<line>\d*)");

        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            AddStackTraceToSqlCommand(command);
            base.ReaderExecuting(command, interceptionContext);
        }

        public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            AddStackTraceToSqlCommand(command);
            base.NonQueryExecuting(command, interceptionContext);
        }

        public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            AddStackTraceToSqlCommand(command);
            base.ScalarExecuting(command, interceptionContext);
        }

        private void AddStackTraceToSqlCommand(DbCommand command)
        {
            int isStackTraceAdded = command.CommandText.IndexOf(stackLoggerStartTag);
            if (isStackTraceAdded > 0)
            {
                return;
            }

            var comment = GetCommentedStack();
            command.CommandText = $"{newLine}{comment}{newLine}{command.CommandText}";
        }

        private List<string> GetStack()
        {
            var frames = Environment.StackTrace
                .Split(new string[] { newLine }, StringSplitOptions.None).Select(x => x.TrimStart())
                .Where(l => !l.Contains("System.") && !l.Contains(this.GetType().FullName))
                .Select(x => ParseFrame(x))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return frames;
        }

        private string GetCommentedStack()
        {
            var frames = GetStack();
            string comment = $"{stackLoggerStartTag}{newLine} {string.Join(newLine, frames)}{sqlCommentCloseTag}";

            return comment;
        }

        private string ParseFrame(string frame)
        {
            var result = regex.Match(frame);
            var namespaceName = result.Groups["namespace"].Value.ToString();
            var className = result.Groups["class"].Value.ToString();
            var methodName = result.Groups["method"].Value.ToString();
            var line = result.Groups["line"].Value.ToString();

            return $"{namespaceName}.{className}.{methodName} line : {line}";
        }
    }
}
