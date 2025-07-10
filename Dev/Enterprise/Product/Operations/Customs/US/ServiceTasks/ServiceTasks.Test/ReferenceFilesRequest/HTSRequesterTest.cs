using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(HTSRequester))]
	sealed class HTSRequesterTest : ReferenceFilesRequesterWithDataVersionTest<HTSRequester>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestSkipSpecifiedFailedVersion()
		{
			var dateTime = ZDateTime.UtcToday;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("HAVI", "TEST HAVI", Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType.ZZK_CodeType, "2104", codeType.ZZK_Description, dateTime.AddDays(-10), dateTime.AddDays(10));
			Factory.Save();
			var hTSTask = new HTSRequesterForTest();
			InitialiseTaskSchedule(hTSTask);
			USCDataVersion lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			lastHTSAttemptObj.UZ_Version = 2104;
			lastHTSAttemptObj.UZ_Note = ExtractReferenceFilesResult.Failure;
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals(2105, lastHTSAttemptObj.UZ_Version);
		}

		public void TestSkipSpecifiedVersionWhenLastWasSucceed()
		{
			var dateTime = ZDateTime.UtcToday;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("HAVI", "TEST HAVI", Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType.ZZK_CodeType, "2104", codeType.ZZK_Description, dateTime.AddDays(-10), dateTime.AddDays(10));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType.ZZK_CodeType, "2105", codeType.ZZK_Description, dateTime.AddDays(-10), dateTime.AddDays(10));
			Factory.Save();
			var hTSTask = new HTSRequesterForTest();
			InitialiseTaskSchedule(hTSTask);
			var lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			lastHTSAttemptObj.UZ_Version = 2103;
			lastHTSAttemptObj.UZ_Note = ExtractReferenceFilesResult.Success;
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals(2106, lastHTSAttemptObj.UZ_Version);
		}

		[TestDate(2021, 06, 03)]
		public void TestSkipSpecifiedVersionWhenLastNoteIsEmptyInLastYear()
		{
			var dateTime = ZDateTime.UtcToday;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("HAVI", "TEST HAVI", Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType.ZZK_CodeType, "2101", codeType.ZZK_Description, dateTime.AddDays(-10), dateTime.AddDays(10));
			Factory.Save();
			var hTSTask = new HTSRequesterForTest();
			InitialiseTaskSchedule(hTSTask);
			var lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			lastHTSAttemptObj.UZ_Version = 2004;
			lastHTSAttemptObj.UZ_Note = ZString.Empty;
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals(2102, lastHTSAttemptObj.UZ_Version);
		}

		[TestDate(2021, 06, 03)]
		public void TestSkipSpecifiedVersionWhenLastNoteIsEmptyInThisYear()
		{
			var dateTime = ZDateTime.UtcToday;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("HAVI", "TEST HAVI", Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType.ZZK_CodeType, "2104", codeType.ZZK_Description, dateTime.AddDays(-10), dateTime.AddDays(10));
			Factory.Save();
			var hTSTask = new HTSRequesterForTest();
			InitialiseTaskSchedule(hTSTask);
			var lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			lastHTSAttemptObj.UZ_Version = 2104;
			lastHTSAttemptObj.UZ_Note = ZString.Empty;
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals(2105, lastHTSAttemptObj.UZ_Version);
		}

		[TestDate(2008, 6, 20, 14, 30, 0)]
		public void TestInitialise()
		{
			var hTSTask = new HTSRequesterForTest();
			TestServiceLogger log = InitialiseTaskSchedule(hTSTask, out BusinessObject taskSchedule);
			RunTaskSchedule(hTSTask);
			taskSchedule.Reload();
			var lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			lastHTSAttemptObj.UZ_Version = 701;
			lastHTSAttemptObj.UZ_Note = ExtractReferenceFilesResult.Pending;
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals("Log not empty - Message sent", "Information|Harmonized Tariff Schedule Request Message successfully sent.", log[1]);
			lastHTSAttemptObj.UZ_UpdateTime = ZDateTime.Empty;
			lastHTSAttemptObj.UZ_Note = ExtractReferenceFilesResult.Failure;
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals("Log not empty - Message sent", "Information|Harmonized Tariff Schedule Request Message successfully sent.", log[5]);
			lastHTSAttemptObj.UZ_UpdateTime = ZDateTime.Empty;
			lastHTSAttemptObj.UZ_Note = ExtractReferenceFilesResult.Success;
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals("Log not empty - Message sent", "Information|Harmonized Tariff Schedule Request Message successfully sent.", log[8]);
			lastHTSAttemptObj.Delete();
			Factory.Save();
			log.ClearLog();
			hTSTask.RunTask();
			AssertEquals("Log not empty - Message sent", "Information|Harmonized Tariff Schedule Request Message successfully sent.", log[1]);
		}

		public void TestYearCutover()
		{
			var hTSTask = new HTSRequesterForTest();
			var log = InitialiseTaskSchedule(hTSTask);
			var lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			lastHTSAttemptObj.UZ_Note = ZString.Empty;
			AssertEquals("Precondition:UZ_Version ", 901, lastHTSAttemptObj.UZ_Version);
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals("Log not empty - Message sent", "Information|Harmonized Tariff Schedule Request Message successfully sent.", log[1]);
			AssertEquals("Go to Next Attempt For Year Cutover", 1001, lastHTSAttemptObj.UZ_Version);
		}

		public void TestRequestAgainWhenFail()
		{
			var hTSTask = new HTSRequesterForTest();
			var log = InitialiseTaskSchedule(hTSTask);
			var lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			lastHTSAttemptObj.UZ_Note = ExtractReferenceFilesResult.Failure;
			AssertEquals("Precondition:UZ_Version ", 901, lastHTSAttemptObj.UZ_Version);
			Factory.Save();
			hTSTask.RunTask();
			AssertEquals("Log not empty - Message sent", "Information|Harmonized Tariff Schedule Request Message successfully sent.", log[1]);
			AssertEquals("Ensure previous year's updates are requested", 901, lastHTSAttemptObj.UZ_Version);
		}

		public void TestTaskNotRunWhenEnvironmentIsInvalid()
		{
			//successful the first time
			var hTSTask = new HTSRequesterForTest();
			var log = InitialiseTaskSchedule(hTSTask);
			hTSTask.RunTask();
			AssertEquals("Information|Start: Harmonized Tariff Schedule Request Message", log[0]);
			AssertEquals("Information|Harmonized Tariff Schedule Request Message successfully sent.", log[1]);
			AssertEquals("Information|End: Harmonized Tariff Schedule Request Message", log[2]);
			//no processing district port
			var dataVersion = hTSTask.DataVersion;
			dataVersion.UZ_UpdateTime = ZDateTime.Empty;
			dataVersion.Factory.Save();
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, TestBranch.PK.ToGuid(), Guid.Empty, "");
			log.ClearLog();
			hTSTask.RunTask();
			AssertEquals("Error|The reference file request will not be sent because Company 'Z1Z' has no Branch with Processing District Port Code.", log.ToString().Trim());
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, TestBranch.PK.ToGuid(), Guid.Empty, "2008");
			//no filer code
			var filer = new EntryFiler();
			filer.IsABICertified = true;
			filer.EntryFilerCode = "";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(TestBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, filer);
			dataVersion.UZ_UpdateTime = ZDateTime.Empty;
			dataVersion.Factory.Save();
			log.ClearLog();
			hTSTask.RunTask();
			AssertEquals("Error|No company is ABI Certified and system could not send reference file update request messages.", log[0]);
			//isabicertified is false
			filer.IsABICertified = false;
			filer.EntryFilerCode = "ABC";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(TestBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, filer);
			dataVersion.UZ_UpdateTime = ZDateTime.Empty;
			dataVersion.Factory.Save();
			log.ClearLog();
			hTSTask.RunTask();
			AssertEquals("Error|No company is ABI Certified and system could not send reference file update request messages.", log[0]);
			filer.IsABICertified = true;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(TestBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, filer);
			dataVersion.UZ_UpdateTime = ZDateTime.Empty;
			dataVersion.Factory.Save();
			log.ClearLog();
			hTSTask.RunTask();
			AssertEquals("Information|Harmonized Tariff Schedule Request Message successfully sent.", log[1]);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetupSettingsForTestDoNotRunWithinTwelveHours()
		{
			var factory = new BusinessObjectFactory();
			var data = USCDataVersion.GetLastHTSAttempt(factory);
			data.UZ_UpdateTime = ZDateTime.Empty;
			factory.Save();
		}

		sealed class HTSRequesterForTest : HTSRequester
		{
			protected override TimeSpan GetUtcOffset() => new TimeSpan(2, 0, 0);
		}
	}
}
