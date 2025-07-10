using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	[TestedType(typeof(UpdateRelatedJobSubscriber))]
	public class UpdateRelatedJobDeniedPartyScreeningStatusSubscriberTest : LogSubscriberTest<UpdateRelatedJobSubscriber>
	{
		public void TestReportTimeoutException()
		{
			var timeoutSql = @"WAITFOR DELAY '00:00:02'";

			var subscriber = new UpdateRelatedJobSubscriberForTest();
			using (var command = TestConnection.Command(timeoutSql))
			{
				command.CommandTimeout = 1;
				ExceptionReporterTestListener.Instance.Clear();

				subscriber.ExecuteCommandForTest(command, TestConnection);
				AssertContains("Timeout", ExceptionReporterTestListener.Instance[0].Message);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestProcessLogQueueItems()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				Factory.Save();
				((LoggerForTesting)Notifier).AllowDebug = true;
				RunLogWalkerCycleForTest();

				var notifierLogs = ((LoggerForTesting)Notifier).NotifiedEventList.Where(l => l.StartsWith($"[{LogSubscriber.FriendlyName}]"));
				var logs = string.Join(System.Environment.NewLine, notifierLogs);

				AssertContains("[Update Related Job Subscriber] finished processing logs.", logs);
			}
		}

		public void TestUpdateRelatedJobsByLogWalkerServiceTaskIfRegistrySetToTrue()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.New<IForwardingConsol>();
				consol.JK_OA_CreditorAddress = org.MainAddress.PK;
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				org.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				RunLogWalkerCycleForTest();

				((BusinessObject)consol).Reload();
				AssertEquals("Should update related jobs.", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			}
		}

		public void TestDoNotUpdateRelatedJobsByLogWalkerServiceTaskIfRegistrySetToFalse()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.New<IForwardingConsol>();
				consol.JK_OA_CreditorAddress = org.MainAddress.PK;
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				org.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				RunLogWalkerCycleForTest();

				((BusinessObject)consol).Reload();
				AssertEquals("Should not update related jobs.", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
			}
		}

		public void TestProcessLogQueueItems_TypeIsSpecified()
		{
			AssertProcessLogQueueItems_TypeIsSpecified("RSA", "Should update related jobs.", ScreeningStatusesList.Codes.Matched);
			AssertProcessLogQueueItems_TypeIsSpecified("SIL", "Should not update related jobs.", ScreeningStatusesList.Codes.Clear);
		}

		void AssertProcessLogQueueItems_TypeIsSpecified(string type, string message, string expectedStatusAfterLWK)
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.New<IForwardingConsol>();
				consol.JK_OA_CreditorAddress = org.MainAddress.PK;
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP={type}";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				org.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				RunLogWalkerCycleForTest();

				((BusinessObject)consol).Reload();
				AssertEquals(message, expectedStatusAfterLWK, consol.JK_ScreeningStatus);
			}
		}

		public void TestProcessLogQueueItems_IfNoRescreenAdviceTypeAndUpdateRelatedJobsIsNotEnabled_DoNotUpdateRelatedJobs()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.New<IForwardingConsol>();
				consol.JK_OA_CreditorAddress = org.MainAddress.PK;
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				org.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				RunLogWalkerCycleForTest();

				((BusinessObject)consol).Reload();
				AssertEquals("Should not update related jobs.", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
			}
		}

		public void TestProcessLogQueueItems_IfNoRescreenAdviceTypeAndUpdateRelatedJobsIsEnabled_UpdateRelatedJobs()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.New<IForwardingConsol>();
				consol.JK_OA_CreditorAddress = org.MainAddress.PK;
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				org.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				RunLogWalkerCycleForTest();

				((BusinessObject)consol).Reload();
				AssertEquals("Should update related jobs.", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			}
		}

		public void TestProcessLogQueueItems_Consol_Org()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.New<IForwardingConsol>();
				consol.JK_OA_CreditorAddress = org.MainAddress.PK;
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				org.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				((LoggerForTesting)Notifier).AllowDebug = true;
				RunLogWalkerCycleForTest();

				var notifierLogs = ((LoggerForTesting)Notifier).NotifiedEventList.Where(l => l.StartsWith($"[{LogSubscriber.FriendlyName}]"));
				var logs = string.Join(System.Environment.NewLine, notifierLogs);

				AssertContains("[Update Related Job Subscriber] finished processing logs.", logs);
				((BusinessObject)consol).Reload();
				AssertEquals(ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			}
		}

		public void TestProcessLogQueueItems_Consol_Vessel()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				var consol = Factory.New<IForwardingConsol>();
				var transport = consol.Transports_AddNew();
				transport.JW_Vessel = vessel.RV_Code;
				var log = vessel.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();
				vessel.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				((LoggerForTesting)Notifier).AllowDebug = true;
				RunLogWalkerCycleForTest();

				var notifierLogs = ((LoggerForTesting)Notifier).NotifiedEventList.Where(l => l.StartsWith($"[{LogSubscriber.FriendlyName}]"));
				var logs = string.Join(System.Environment.NewLine, notifierLogs);

				AssertContains("[Update Related Job Subscriber] finished processing logs.", logs);
				((BusinessObject)consol).Reload();
				AssertEquals(ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			}
		}

		public new void TestRegisteredInSystemOrClientSpecificLogSubscribersList()
		{
			Assert(true);
		}

		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
		}

		protected override void TearDown()
		{
			base.TearDown();
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}
	}

	#region Implementation

	[Serializable]
	public class UpdateRelatedJobSubscriberForTest : UpdateRelatedJobSubscriber
	{
		public void ExecuteCommandForTest(DbCommand command, DbConnection connection) => ExecuteCommand(command, connection);
	}

	#endregion
}
