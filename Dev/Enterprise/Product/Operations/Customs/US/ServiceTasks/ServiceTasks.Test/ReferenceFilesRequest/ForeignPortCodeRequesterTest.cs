using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(ForeignPortCodeRequester))]
	sealed class ForeignPortCodeRequesterTest : ReferenceFilesRequesterWithDataVersionTest<ForeignPortCodeRequester>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		[ExpectNoExceptions]
		public void TestInitialise()
		{
			var portCodeTask = new ForeignPortCodeRequesterForTest();
			InitialiseTaskSchedule(portCodeTask, out BusinessObject taskSchedule);
			RunTaskSchedule(portCodeTask);
			taskSchedule.Reload();
		}

		public void TestForeignPortCodeRequestWhenLoggedInAnotherBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_Code = "COM";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";
			var loginBranch = company.Branches.AddNew();
			loginBranch.GB_Code = "BR2";
			var portCodeTask = new ForeignPortCodeRequesterForTest();
			var log = new TestServiceLogger();
			portCodeTask.ServiceLogger = log;
			var schedule = Factory.NewWithValidTestData<StmScheduleTask>();
			schedule.S5_ScheduleType = ServiceTaskApplicationCodeList.Codes.ForeignPort;
			schedule.S5_GB = loginBranch.PK;
			Factory.Save();
			var entryFiler = new EntryFiler();
			entryFiler.IsABICertified = true;
			entryFiler.EntryFilerCode = "EFC";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, loginBranch.PK.ToGuid(), Guid.Empty, "ABCD");
			portCodeTask.RunTask();
			var logged = log.ToString();
			AssertNotContains(string.Format("The reference file request will not be sent because Company '{0}' has no Branch with Processing District Port Code.", company.GC_Code), logged);
			AssertContains(@"Information|Start: ", logged);
			AssertContains(@"Information|End: ", logged);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetupSettingsForTestDoNotRunWithinTwelveHours()
		{
			var factory = new BusinessObjectFactory();
			var data = USCDataVersion.GetOrCreate(factory, USCDataVersion.Constant.RefFileRequestForeignPortCodeTimeStamp);
			data.UZ_UpdateTime = ZDateTime.Empty;
			factory.Save();
		}

		sealed class ForeignPortCodeRequesterForTest : ForeignPortCodeRequester
		{
			protected override TimeSpan GetUtcOffset() => new TimeSpan(2, 0, 0);
		}
	}
}
