using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(AntiDumpingRequester))]
	sealed class AntiDumpingRequesterTest : ReferenceFilesRequesterWithDataVersionTest<AntiDumpingRequester>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		[ExpectNoExceptions]
		public void TestInitialise()
		{
			var aDDTask = new AntiDumpingRequesterForTest();
			InitialiseTaskSchedule(aDDTask, out BusinessObject taskSchedule);
			RunTaskSchedule(aDDTask);
			taskSchedule.Reload();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetupSettingsForTestDoNotRunWithinTwelveHours()
		{
			var factory = new BusinessObjectFactory();
			var data = USCDataVersion.GetOrCreate(factory, USCDataVersion.Constant.RefFileRequestAntiDumpingTimeStamp);
			data.UZ_UpdateTime = ZDateTime.Empty;
			factory.Save();
		}

		sealed class AntiDumpingRequesterForTest : AntiDumpingRequester
		{
			protected override TimeSpan GetUtcOffset() => new TimeSpan(2, 0, 0);
		}
	}
}
