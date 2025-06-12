using System;
using System.Data;
using System.Linq;
using CargoWise.Billing.API;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.IntegrationTests
{
	[TestFixture]
	[WithBillingWcfService]
	public class BillingDatabaseSPExecutionTests : TestBase
	{
		[Test]
		public void TestBillingDatabaseSPExecution()
		{
			var dtUtc = new DateTime(2024, 7, 15, 0, 0, 0, DateTimeKind.Utc);
			var jobId = "TestProduceWGRMonthlyChargeables";
			var transaction = new BillingTransaction
			{
				BillableCount = 400,
				ClientID = "ENTMELMEL",
				ClientNumber = null,
				Category = "WGR",
				PriceItemCode = "WGR",
				Reference1 = "300",
				Reference2 = "Writer",
				Reference3 = "System",
				Reference4 = "SSQL/INSTANCE1",
				Reference5 = "286958",
				ReportingSource = "MSC",
				ServiceOccuredUTC = dtUtc,
				Version = 0,
			};

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var existing = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				var initialTransactionsCount = existing.Rows.Count;

				using (var client = WithBillingWcfServiceAttribute.Current.CreateClient())
				{
					client.AddTransaction(transaction);
				}

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);

				Assert.That(actual.Rows.Count, Is.EqualTo(initialTransactionsCount + 1));
				var transactionRow = actual.Rows[0];
				Assert.That(transactionRow, Is.Not.Null);
				BillingDataTestHelper.AssertPropertiesAreEqual(transaction, transactionRow);

				RecurringJobManager.Trigger("ProcessBillingDatabase");
				TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabase"), "AwaitProcessBillingDatabaseFinished", TimeSpan.FromSeconds(5));

				var table = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				Assert.That(table.Rows.Count, Is.EqualTo(1), "usage transaction count");
				BillingDataTestHelper.AssertPropertiesAreEqual(transaction, table.Rows.Cast<DataRow>().First(), "US_");

				BillingDataTestHelper.ExecuteNonQuery(con, $@"INSERT INTO [HangFire].[Hash] ([Key], [Field], [Value]) VALUES
('recurring-job:{jobId}', 'CreateAt', '{dtUtc:O}')
,('recurring-job:{jobId}', 'Cron', '*/15 * * * *')
,('recurring-job:{jobId}', 'Job', '{{""Type"":""CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase.BillingDatabaseSPExecution, CargoWise.eServices.Billing.WcfService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350"",""Method"":""StartProcess"",""ParameterTypes"":""[\""System.Threading.CancellationToken, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"",\""Hangfire.Server.PerformContext, Hangfire.Core, Version=1.8.14.0, Culture=neutral, PublicKeyToken=e33b67d3bb5581e4\"",\""System.Object[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\""]"",""Arguments"":""[null,null,\""[{{\\\""Name\\\"":\\\""edi.ProduceWGRMonthlyChargeables\\\"", \\\""TimeoutInSeconds\\\"": 60,\\\""SqlParams\\\"":[{{\\\""Name\\\"":\\\""@period\\\"",\\\""DbType\\\"":8,\\\""Value\\\"":202407}}]}}]\""]""}}')
,('recurring-job:{jobId}', 'LastExecution', NULL)
,('recurring-job:{jobId}', 'LastJobId', NULL)
,('recurring-job:{jobId}', 'NextExecution', '{dtUtc.AddMinutes(15):O}')
,('recurring-job:{jobId}', 'Queue', 'default')
,('recurring-job:{jobId}', 'TimeZoneId', 'UTC')
,('recurring-job:{jobId}', 'V', '2')");

				RecurringJobManager.Trigger(jobId);
				TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted(jobId), $"Await{jobId}Finished", TimeSpan.FromSeconds(5));
				table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				Assert.That(table.Rows.Count, Is.EqualTo(1), "chargeable transaction count");
				var expectedChargeable = new BillingTransaction
				{
					BillableCount = 2,
					ClientID = "ENTMELMEL",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = string.Empty,
					Reference2 = "Writer",
					Reference3 = "System",
					Reference4 = "SSQL/INSTANCE1",
					Reference5 = "286958",
					ReportingSource = "MSC",
					ServiceOccuredUTC = dtUtc,
					Version = 0,
				};
				BillingDataTestHelper.AssertPropertiesAreEqual(expectedChargeable, table.Rows.Cast<DataRow>().First(), "CH_");
			}
		}
	}
}
