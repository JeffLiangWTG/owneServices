using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Business.ServiceTasks.Test
{
	[TestedType(typeof(HourlyFreightNotificationEmailSenderTask))]
	sealed class HourlyFreightNotificationEmailSenderTaskTest : ServiceTaskTestCase<HourlyFreightNotificationEmailSenderTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "HFN", hostedServiceAttribute.Code);
				AssertEquals("Description", "Hourly Freight Notification", hostedServiceAttribute.Description);
				AssertEquals("Category", "FRT", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "15minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("MaximumPeriod", "1day", hostedServiceAttribute.MaximumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestRunTask()
		{
			DateTime prevSent = FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value;

			HourlyFreightNotificationEmailSenderTask task = new HourlyFreightNotificationEmailSenderTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			Assert("Sender was called.", FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value > prevSent);
		}

		public void TestLogging_ForCTF()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CTF";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BRN";
			branch.GB_RL_NKHomePort = "AUMEL";
			Factory.Save();

			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = "CTF";
			ObjectFactory.Get<IProductRegistration>().KeyForTest.ServerCodeForTest = "VAR";
			var bpGuid = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "~BP")).PK.ToGuid();
			using (Env.SetTemporaryUserContext(bpGuid, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var task = new HourlyFreightNotificationEmailSenderTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				Assert("Ensure log is correct", task.ServiceLogger.ToString().StartsWith("Information|HFN service task for CT Freight has successfully run."));
				AssertNotContains("Machine name should not be a dot.", "Current machine name is .", task.ServiceLogger.ToString());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
