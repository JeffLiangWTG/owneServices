using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(FIRMSCodeRequester))]
	sealed class FIRMSCodeRequesterTest : ReferenceFilesRequesterWithDataVersionTest<FIRMSCodeRequester>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		[ExpectNoExceptions]
		public void TestInitialise()
		{
			var fIRMSCodeTask = new FIRMSCodeRequesterForTest();
			InitialiseTaskSchedule(fIRMSCodeTask, out BusinessObject taskSchedule);
			RunTaskSchedule(fIRMSCodeTask);
			taskSchedule.Reload();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetupSettingsForTestDoNotRunWithinTwelveHours()
		{
			var factory = new BusinessObjectFactory();
			var data = USCDataVersion.GetOrCreate(factory, USCDataVersion.Constant.RefFileRequestFirmsCodeTimeStamp);
			data.UZ_UpdateTime = ZDateTime.Empty;
			factory.Save();
		}

		sealed class FIRMSCodeRequesterForTest : FIRMSCodeRequester
		{
			protected override TimeSpan GetUtcOffset() => new TimeSpan(2, 0, 0);
		}
	}
}
