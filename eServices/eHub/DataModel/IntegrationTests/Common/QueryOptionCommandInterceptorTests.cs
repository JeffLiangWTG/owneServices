using System;
using System.Linq;
using System.Text;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions;
using NUnit.Framework;

namespace CargoWise.eHub.DataModel.IntegrationTests.Common
{
	[TestFixture]
	public class QueryOptionCommandInterceptorTests : eHubTransactionsTestBase
	{
		[Test]
		public void QueryOptionCommandInterceptor_SetOption()
		{
			using (var context = ContextFactory())
			using (var queryOption = new QueryOptionCommandInterceptor(context, "USE HINT('ENABLE_PARALLEL_PLAN_PREFERENCE')"))
			{
				var commandLog = new StringBuilder();
				context.Log = s =>
				{
					TestContext.WriteLine(s);
					if (s.Length > 0 && s != Environment.NewLine && !s.StartsWith("--") && !s.StartsWith("Opened connection") && !s.StartsWith("Closed connection"))
						commandLog.AppendLine(s);
				};

				var client = context.eHubClients.Select(c => c.CC_ID).FirstOrDefault(c => c == "TSTCLIENT");
				Assert.That(commandLog.ToString(), Is.EqualTo(@"SELECT TOP (1) 
    [Extent1].[CC_ID] AS [CC_ID]
    FROM [dbo].[eHubClient] AS [Extent1]
    WHERE N'TSTCLIENT' = [Extent1].[CC_ID] OPTION(USE HINT('ENABLE_PARALLEL_PLAN_PREFERENCE'))
"));
			}
		}
	}
}
