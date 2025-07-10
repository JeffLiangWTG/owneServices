using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterFiles.Business.ServiceTasks.Test
{
	[TestFixture]
	public class QueueProcessingServiceTaskDummyTest
	{
		[Test]
		public void AlwaysPasses()
		{
			Assert.Pass("this dummy test is to satisfy DAT");
		}
	}

	[TestedType(typeof(QueueProcessingServiceTask))]
	class QueueProcessingServiceTaskTest : ServiceTaskTestCase<QueueProcessingServiceTask>
	{
		public void TestRunTaskCallsAggregateAccOrgBalanceChangesIntoAccOrgBalance()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var sql = string.Format("INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta) VALUES ('{0}', '{1}', 'AP', 1, 2, 3)",
				orgHeader.PK, GlbCompany.CurrentCompany.PK);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}

			using (var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.AccOrgBalanceChanges"))
			{
				AssertEquals("Precondition: records in AccOrgBalanceChanges", 1, cmd.ExecuteScalar());
			}

			using (var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.AccOrgBalance"))
			{
				AssertEquals("Precondition: records in AccOrgBalance", 0, cmd.ExecuteScalar());
			}

			QueueProcessingServiceTask task = new QueueProcessingServiceTask();
			TestServiceLogger log = InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals("Log Count", 2, log.Count);
			AssertContains("Information|Started: Processing Queue for Accounting Balances (recognized, unrecognized and balance)", log[0]);
			AssertContains("Information|Completed: Processing Queue for Accounting Balances (recognized, unrecognized and balance)", log[1]);
			AssertContains("seconds", log[1]);

			using (var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.AccOrgBalanceChanges"))
			{
				AssertEquals("records in AccOrgBalanceChanges", 0, cmd.ExecuteScalar());
			}

			using (var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.AccOrgBalance"))
			{
				AssertEquals("records in AccOrgBalance", 1, cmd.ExecuteScalar());
			}
		}

		public void TestRunTaskHandlesMoneySumOverflow()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var sqlString = "INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta) VALUES ('{0}', '{1}', 'AP', 1, 2, 922337203685477.5807)";
			foreach (var i in Enumerable.Range(0, 2))
			{
				var sql = string.Format(sqlString, org.PK, GlbCompany.CurrentCompany.PK);
				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}
			}

			using (var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.AccOrgBalanceChanges"))
			{
				AssertEquals("Precondition: records in AccOrgBalanceChanges", 2, cmd.ExecuteScalar());
			}

			QueueProcessingServiceTask task = new QueueProcessingServiceTask();
			TestServiceLogger log = InitialiseTaskSchedule(task);
			AssertNoExceptionThrown("Overflow should not be unhandled", () => task.RunTask());

			AssertEquals("Log Count", 2, log.Count);
			AssertContains("Information|Started: Processing Queue for Accounting Balances (recognized, unrecognized and balance)", log[0]);
			AssertContains("Error|A Transaction with too high a balance was processed, please find and reverse any transactions with a transaction or outstanding balance close to 922,337,203,685,477", log[1]);
		}

		// No nudging: queue table is present, but processing is done in batches and queue is reported via IHostedServiceQueueProvider.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		[TestDate(2023, 10, 1)]
		public void TestQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode(QueueProcessingServiceTask.Code);
			AssertNotNull("Service task attribute exists and refers to a valid queue provider.", queueProvider);
			var queueCount = Db.Connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.AccOrgBalanceChanges");
			AssertEquals("Precondition: no records in AccOrgBalanceChanges", 0, queueCount);

			var result = queueProvider.QueueResult;
			AssertEquals("Queue count for service task is 0 when AccOrgBalanceChanges is empty", 0, result.QueueSize);
			AssertEquals("Queue age for service task is 0 when AccOrgBalanceChanges is empty", TimeSpan.Zero, result.MaximumItemAge);

			var anyOrgPk = Db.Connection.ExecuteScalar<Guid>("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");
			var itemDate = new DateTime(2023, 11, 1);
			var sqlString = $@"
INSERT INTO dbo.AccOrgBalanceChanges
(Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_SystemLastEditTimeUTC)
VALUES ('{anyOrgPk}', '{GlbCompany.CurrentCompany.PK}', 'AP', 1, 2, 1.23, '{itemDate.ToString("s")}')";

			var expectedAge = DateTime.UtcNow - itemDate;
			Db.Connection.ExecuteNonQuery(sqlString);
			result = queueProvider.QueueResult;
			AssertEquals("Queue for service task matches AccOrgBalanceChanges", 1, result.QueueSize);
			NUnit.Framework.Assert.That((int)result.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));

			Db.Connection.ExecuteNonQuery(sqlString);
			result = queueProvider.QueueResult;
			AssertEquals("Queue for service task matches AccOrgBalanceChanges", 2, result.QueueSize);
			NUnit.Framework.Assert.That((int)result.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));

			var task = new QueueProcessingServiceTask();
			InitialiseTaskSchedule(task);
			task.RunTask();

			result = queueProvider.QueueResult;
			AssertEquals("Queue for service task is reset to zero when service task runs", 0, result.QueueSize);
			AssertEquals("Queue for service task is reset to zero when service task runs", TimeSpan.Zero, result.MaximumItemAge);
		}
	}
}
