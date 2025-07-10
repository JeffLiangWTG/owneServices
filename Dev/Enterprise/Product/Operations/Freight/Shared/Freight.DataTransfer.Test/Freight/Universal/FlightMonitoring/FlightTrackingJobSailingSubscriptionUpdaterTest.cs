using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring.Testing
{
	[TestedType(typeof(FlightTrackingJobSailingSubscriptionUpdater))]
	sealed class FlightTrackingJobSailingSubscriptionUpdaterTest : LogSubscriberTest<FlightTrackingJobSailingSubscriptionUpdater>
	{
		#region Console

		public void TestProcessLogs_SailingHasLog_ForConsol()
		{
			CreateSailingJobAndCheckSBR(
				sailing => CreateNewConsol(sailing, "08187443521"),
				shouldHaveSBRBeforeRunningLogWalker: false,
				shouldHaveSBRAfterRunningLogWalker: true,
				shouldHaveCancelledSBR: false);
		}

		public void TestProcessLogs_CreateNewSBR_JobSailing_Changed_ForConsol()
		{
			CreateSailingJobAndCheckSBR(
				sailing => CreateNewConsol(sailing, "08187443521"),
				shouldHaveSBRBeforeRunningLogWalker: true,
				shouldHaveSBRAfterRunningLogWalker: true,
				shouldHaveCancelledSBR: true,
				saveJobSailing: true);
		}

		public void TestProcessLogs_SailingHasLog_ForConsol_WithInvalidMAWB()
		{
			CreateSailingJobAndCheckSBR(
				sailing => CreateNewConsol(sailing, ZString.Empty),
				shouldHaveSBRBeforeRunningLogWalker: false,
				shouldHaveSBRAfterRunningLogWalker: false,
				shouldHaveCancelledSBR: false,
				saveJobSailing: true);
		}

		public void TestProcessLogs_SailingHasLog_ConsolTypeChanged()
		{
			CreateSailingJobAndCheckSBR(
				sailing =>
				{
					var console = CreateNewConsol(sailing, "08187443521");

					console.JK_TransportMode = Core.Constants.TransportModes.Sea;
					Factory.Save();

					return console;
				},
				shouldHaveSBRBeforeRunningLogWalker: true,
				shouldHaveSBRAfterRunningLogWalker: true,
				shouldHaveCancelledSBR: false,
				saveJobSailing: true);
		}

		public void TestProcessLogs_ShouldNotCreateSBR_IfConsoleIsNotForwarding()
		{
			CreateSailingJobAndCheckSBR(
				sailing =>
				{
					var console = CreateNewConsol(sailing, "08187443521");

					console.JK_IsCFS = true;
					console.JK_IsForwarding = false;
					Factory.Save();

					return console;
				},
				shouldHaveSBRBeforeRunningLogWalker: false,
				shouldHaveSBRAfterRunningLogWalker: false,
				shouldHaveCancelledSBR: false);
		}

		#endregion

		#region Declaration

		public void TestProcessLogs_SailingHasLog_ForDeclaration()
		{
			CreateSailingJobAndCheckSBR(
				sailing => (EnterpriseBusinessObject)CreateNewDeclaration(sailing, "08187443521"),
				shouldHaveSBRBeforeRunningLogWalker: false,
				shouldHaveSBRAfterRunningLogWalker: true,
				shouldHaveCancelledSBR: false);
		}

		public void TestProcessLogs_CreateNewSBR_JobSailing_Changed_ForDeclaration()
		{
			CreateSailingJobAndCheckSBR(
				sailing => (EnterpriseBusinessObject)CreateNewDeclaration(sailing, "08187443521"),
				shouldHaveSBRBeforeRunningLogWalker: true,
				shouldHaveSBRAfterRunningLogWalker: true,
				shouldHaveCancelledSBR: true,
				saveJobSailing: true);
		}

		public void TestProcessLogs_SailingHasLog_ForDeclaration_WithInvalidMAWB()
		{
			CreateSailingJobAndCheckSBR(
				sailing => (EnterpriseBusinessObject)CreateNewDeclaration(sailing, ZString.Empty),
				shouldHaveSBRBeforeRunningLogWalker: false,
				shouldHaveSBRAfterRunningLogWalker: false,
				shouldHaveCancelledSBR: false,
				saveJobSailing: true);
		}

		public void TestProcessLogs_SailingHasLog_DeclarationTypeChanged()
		{
			CreateSailingJobAndCheckSBR(
				sailing =>
				{
					var declaration = CreateNewDeclaration(sailing, "08187443521");

					declaration["JE_TransportMode"] = Core.Constants.TransportModes.Sea;
					Factory.Save();

					return (EnterpriseBusinessObject)declaration;
				},
				shouldHaveSBRBeforeRunningLogWalker: true,
				shouldHaveSBRAfterRunningLogWalker: true,
				shouldHaveCancelledSBR: false,
				saveJobSailing: true);
		}

		#endregion

		#region Implementations

		void CreateSailingJobAndCheckSBR(
			Func<JobSailing, EnterpriseBusinessObject> getBusinessObject,
			bool shouldHaveSBRBeforeRunningLogWalker,
			bool shouldHaveSBRAfterRunningLogWalker,
			bool shouldHaveCancelledSBR,
			bool saveJobSailing = false)
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			testCompany.CompanyName = "testCompany";
			testCompany.GC_Code = "HMC";

			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_BranchName = "testBranch";
			testBranch.GB_Code = "HMB";
			testBranch.GB_GC = testCompany.PK;

			var testDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			testDepartment.GE_Code = "HMD";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "XYX";
			staff.GS_GB_HomeBranch = testBranch.PK;

			Factory.Save();

			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.AWBTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "FMSEHubId"))
			{
				JobSailing sailing;
				EnterpriseBusinessObject businessObject;

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), testBranch.PK.ToGuid(), testDepartment.PK.ToGuid()))
				{
					sailing = CreateNewSailing();
					if (saveJobSailing)
					{
						Factory.Save();
					}

					businessObject = getBusinessObject(sailing);
				}

				//There should be always one SBR created when jobSailing is saved
				var sailingLogsCount = sailing.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);
				AssertEquals("Should create new SBR event for JobSailing", 1, sailingLogsCount);

				var logsCount = businessObject.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);
				AssertEquals("Before LogWalker, should create SBR event for " + businessObject.GetType().Name, shouldHaveSBRBeforeRunningLogWalker, logsCount == 1);

				RunLogWalkerCycleForTest();

				//Reload the business object from database, as LogWalker using different factory from tests, and Test Factory does not have the changes
				businessObject = (EnterpriseBusinessObject)new BusinessObjectFactory().Load(businessObject.GetType(), businessObject.PK);
				var logs = businessObject.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToArray();
				AssertEquals("Should create new SBR event for " + businessObject.GetType().Name, shouldHaveSBRAfterRunningLogWalker, logs.Length == 1);
				if (shouldHaveSBRAfterRunningLogWalker)
				{
					var log = logs.Single();
					AssertEquals("Log reference is not valid", "|RFN=08187443521|TYP=AWB Automation", log.SL_Reference);
					AssertEquals(staff.GS_Code, log.SL_GS_NKUser);
					AssertEquals(testBranch.GB_Code, log.SL_GB_NKBranch);
					AssertEquals(testDepartment.GE_Code, log.SL_GE_NKDepartment);
				}

				var cancelledLogsCount = businessObject.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && x.IsCancelled);
				AssertEquals("Should cancel existing SBR event for " + businessObject.GetType().Name, shouldHaveCancelledSBR, cancelledLogsCount == 1);
			}
		}

		JobSailing CreateNewSailing()
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var voyage = sailing.Voyage;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF001";

			var origin = sailing.Origin;
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = 3.DaysAgo();
			origin.JA_JV = voyage.PK;

			var destination = sailing.Destination;
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = 1.DaysAgo();
			destination.JB_JV = voyage.PK;

			return sailing;
		}

		CommonConsol CreateNewConsol(JobSailing sailing, ZString mawbNumber)
		{
			var consol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol)));
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_MasterBillNum = mawbNumber;
			consol.JK_UniqueConsignRef = "CS-1234567";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUPER";

			var transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Factory.Save();

			return consol;
		}

		IBaseJobDeclaration CreateNewDeclaration(JobSailing sailing, ZString mawbNumber)
		{
			var declaration = (IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType<IBaseJobDeclaration>());
			declaration["JE_TransportMode"] = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = mawbNumber;

			var transport = ((TransportCollection)declaration["Transports"]).AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Factory.Save();

			return declaration;
		}

		#endregion
	}
}
