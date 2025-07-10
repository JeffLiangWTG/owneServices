using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.TransactionalBillingReporting.Testing
{
	[TestedType(typeof(TransactionalBillingTask))]
	sealed class TransactionalBillingTaskTest : ServiceTaskTestCase<TransactionalBillingTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attr = typeof(TransactionalBillingTask).Assembly.GetCustomAttributes<HostedServiceAttribute>().Single(x => x.Code == TransactionalBillingTask.Code);
			CombineAssertions(() =>
			{
				AssertEquals("Code", TransactionalBillingTask.Code, attr.Code);
				AssertEquals("Description", TransactionalBillingTask.Description, attr.Description);
				AssertEquals("Category", "SYS", attr.Category);
				AssertEquals("Type", typeof(TransactionalBillingTask).FullName, attr.TypeName);
				AssertEquals("Mandatory", expected: true, attr.IsMandatory);
				AssertEquals("MinimumPeriod", "1day", attr.MinimumPeriod);
				AssertEquals("MaximumPeriod", "1month", attr.MaximumPeriod);
				AssertEquals("CanRunInAnyBranch", expected: true, attr.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleRunEvery", "1day", attr.DefaultScheduleRunEvery);
			});
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestDailyRunnerWontExplodeWithReallyLongInterchangeNumber()
		{
			CustomsDataRegistry.Instance.CustomsTransactionalBillingTaskNextRunDate_EmailReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(1986, 3, 12, 3, 0, 0).ToDateTime());
			CustomsDataRegistry.Instance.CustomsTransactionalBillingTaskLastRunDate_BillingAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(1986, 2, 1, 3, 0, 0).ToDateTime());
			EnvProxy.SetHostedLocationForTest("Milton Keynes");
			using (var env = Environment.DisposableEnvironment.ForBranch(GlbBranch.CurrentBranch.PK.ToGuid()))
			{
				Business.TransactionalBillingReporting.Testing.TransactionalBillingStatementGeneratorBillingAPITest.CreateGenralTextMessages(GlbBranch.CurrentBranch.PK.ToGuid(), Factory);
				Factory.Save();
				_ = InitialiseAndRunTaskSchedule(new TransactionalBillingTask());
				AssertEquals("Should have 4 rows in stmUsageData, without puking", "4", Db.Connection.ExecuteScalar("select count(*) from dbo.stmUsageData").ToString());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
