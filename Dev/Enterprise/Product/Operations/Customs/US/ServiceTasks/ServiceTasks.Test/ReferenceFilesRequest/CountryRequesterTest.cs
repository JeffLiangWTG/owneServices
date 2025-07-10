using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(CountryRequester))]
	sealed class CountryRequesterTest : ReferenceFilesRequesterWithDataVersionTest<CountryRequester>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		[ExpectNoExceptions]
		public void TestInitialise()
		{
			var countryTask = new CountryRequesterForTest();
			InitialiseTaskSchedule(countryTask, out BusinessObject taskSchedule);
			RunTaskSchedule(countryTask);
			taskSchedule.Reload();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetupSettingsForTestDoNotRunWithinTwelveHours()
		{
			var factory = new BusinessObjectFactory();
			var data = USCDataVersion.GetOrCreate(factory, USCDataVersion.Constant.RefFileRequestCountryCodeTimeStamp);
			data.UZ_UpdateTime = ZDateTime.Empty;
			factory.Save();
		}

		sealed class CountryRequesterForTest : CountryRequester
		{
			protected override TimeSpan GetUtcOffset() => new TimeSpan(2, 0, 0);
		}
	}
}
