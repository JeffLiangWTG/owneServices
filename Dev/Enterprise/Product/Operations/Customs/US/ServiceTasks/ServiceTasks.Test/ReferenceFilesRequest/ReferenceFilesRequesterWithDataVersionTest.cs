using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	abstract class ReferenceFilesRequesterWithDataVersionTest<T> : ServiceTaskTestCase<T>
			where T : Customs.ServiceTasks.CustomsServiceTask, new()
	{
		[TestDate(2008, 6, 20, 7, 0, 0)]
		public void TestNextRunAfterTwelveHoursElapse()
		{
			var serviceTask = GetServiceTask();
			var log = new TestServiceLogger();
			serviceTask.ServiceLogger = log;
			var requester = serviceTask as ReferenceFilesRequesterWithDataVersion;
			requester.DataVersion.UZ_UpdateTime = new ZDateTime(2008, 6, 19, 2, 30, 0);
			requester.DataVersion.Factory.Save();
			InitialiseTaskSchedule(serviceTask, out BusinessObject taskSchedule);
			RunTaskSchedule(serviceTask);
			taskSchedule.Reload();
			var logged = log.ToString();
			AssertContains(@"Information|Start: ", logged);
			AssertContains(@"Information|End: ", logged);
		}

		public void TestDataVersion()
		{
			var serviceTask = GetServiceTask();
			var log = new TestServiceLogger();
			serviceTask.ServiceLogger = log;
			var requester = serviceTask as ReferenceFilesRequesterWithDataVersion;
			var dataVersion = requester.DataVersion;
			InitialiseTaskSchedule(serviceTask, out BusinessObject taskSchedule);
			RunTaskSchedule(serviceTask);
			taskSchedule.Reload();
			AssertNotEquals("DataVersion", dataVersion, requester.DataVersion);
		}

		[TestDate(2008, 6, 20, 7, 0, 0)]
		public void TestRegistryCheck()
		{
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).DeleteValue(TestBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.ProcessingDistrictPortCode).DeleteValue(Guid.Empty, TestBranch.PK.ToGuid(), Guid.Empty);
			var serviceTask = GetServiceTask();
			var log = new TestServiceLogger();
			serviceTask.ServiceLogger = log;
			var requester = serviceTask as ReferenceFilesRequesterWithDataVersion;
			requester.DataVersion.UZ_UpdateTime = new ZDateTime(2008, 6, 21, 2, 30, 0);
			requester.DataVersion.Factory.Save();
			InitialiseTaskSchedule(serviceTask, out _);
			RunTaskSchedule(serviceTask);
			var logged = log.ToString().Trim();
			AssertEquals(@"Error|" + ReferenceFilesRequester.NoABICertifiedUnitedStatesCompany, logged);
			var filer = new EntryFiler()
			{
				IsABICertified = true,
				EntryFilerCode = "ABC"
			};
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(TestBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, filer);
			log.ClearLog();
			RunTaskSchedule(serviceTask);
			logged = log.ToString().Trim();
			AssertEquals(@"Error|" + string.Format(ReferenceFilesRequester.NoProcessingDistrictPortForCompany, "Z1Z"), logged);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, TestBranch.PK.ToGuid(), Guid.Empty, "2321");
			log.ClearLog();
			RunTaskSchedule(serviceTask);
			logged = log.ToString().Trim();
			AssertEquals(@"Information|The last request was made at '21-Jun-08 02:30'(UTC). Reference file requests will not be sent more frequently than once every 12 hours. This is to avoid clogging up of the customs queue with large responses that may impact system performance and normal operational activities.", logged);
			log.ClearLog();
			requester.DataVersion.UZ_UpdateTime = new ZDateTime(2008, 6, 19, 2, 30, 0);
			requester.DataVersion.Factory.Save();
			RunTaskSchedule(serviceTask);
			logged = log.ToString().Trim();
			AssertContains("Information|Start:", logged);
		}

		[TestDate(2008, 6, 20, 7, 0, 0)]
		public void TestDoNotRunWithinTwelveHours()
		{
			SetupSettingsForTestDoNotRunWithinTwelveHours();
			var serviceTask = GetServiceTask();
			var log = new TestServiceLogger();
			serviceTask.ServiceLogger = log;
			InitialiseTaskSchedule(serviceTask, out BusinessObject taskSchedule);
			RunTaskSchedule(serviceTask);
			taskSchedule.Reload();
			var logged = log.ToString();
			AssertContains(@"Information|Start: ", logged);
			AssertContains(@"Information|End: ", logged);
			log.ClearLog();
			RunTaskSchedule(serviceTask);
			AssertContains("Should not run again", "Information|The last request was made at '20-Jun-08 07:00'(UTC).", log.ToString().Trim());
		}

		public void TestOnlyRunTaskOnce()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_Code = "Z2Z";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "CM3";
			var filer = new EntryFiler()
			{
				IsABICertified = true,
				EntryFilerCode = "123"
			};
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, "2008");
			Factory.Save();
			var serviceTask = GetServiceTask();
			var log = new TestServiceLogger();
			serviceTask.ServiceLogger = log;
			int matchCompanyCount = 0;
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
			{
				var entryFiler = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
				if (entryFiler.EntryFilerCode != "" && entryFiler.IsABICertified)
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						if (!string.IsNullOrEmpty(USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)))
						{
							matchCompanyCount++;
						}
					}
				}
			}

			AssertEquals("There are should more than 2 Companies can meet the conditions", true, matchCompanyCount >= 2);
			InitialiseTaskSchedule(serviceTask, out BusinessObject taskSchedule);
			RunTaskSchedule(serviceTask);
			taskSchedule.Reload();
			var logged = log.ToString();
			AssertContains(@"Information|Start: ", logged);
			AssertContains(@"Information|End: ", logged);
			var regex = new System.Text.RegularExpressions.Regex(@".*Information\|End:.*");
			AssertEquals("The task should only run once", 1, regex.Matches(logged).Count);
		}

		protected abstract void SetupSettingsForTestDoNotRunWithinTwelveHours();

		protected T GetServiceTask() => new T();

		internal GlbBranch TestBranch { get; private set; }

		protected override void SetUpCore()
		{
			base.SetUpCore();
			var filer = new EntryFiler()
			{
				IsABICertified = true,
				EntryFilerCode = "ABC"
			};
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_Code = "Z1Z";
			TestBranch = company.Branches.AddNew();
			TestBranch.GB_Code = "CM2";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, TestBranch.PK.ToGuid(), Guid.Empty, "2008");
			initialUserContext = Env.CurrentUserContext;
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			Factory.Save();
		}

		IUserContext initialUserContext;

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDownCore()
		{
			Env.SetUserContext(initialUserContext);
			base.TearDownCore();
		}
	}
}
