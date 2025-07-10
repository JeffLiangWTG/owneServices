using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.ServiceTasks.Testing
{
	[TestedType(typeof(HVLVReadyLogSubscriber))]
	class HVLVReadyLogSubscriberTest : LogSubscriberTest<HVLVReadyLogSubscriber>
	{
		public void TestProcessShipment_AddLogForShipmentIfNotAllowedExceptionThrown()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertNull("Precondition : shipment doesn't has any log", shipment.Logs.MostRecentLog);

			shipment.Logs.AddNew(AutoEvents.HVLVReady, "|RES=Outturn");
			Factory.Save();

			RunLogWalkerCycleForTest();
			shipment = new BusinessObjectFactory().Load<HVLVForwardingShipment>(shipment.PK);

			string expectedLog = string.Format("[HVLVReadyLogSubscriber] Item data is missing for {0}", consignment.HumanReadableName);

			CombineAssertions("log has added", () =>
			{
				AssertCollectionContains(expectedLog, NotifiedEventList);
				AssertEquals(AutoEvents.DataExportFailureCode, shipment.Logs.MostRecentLog.SL_SE_NKEvent);
				AssertEquals(string.Format("|RES=Item data is missing for {0}", consignment.HumanReadableName), shipment.Logs.MostRecentLog.SL_Reference);
			});
		}

		public void TestSendUniversalToCustoms_Air()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event queued for processing for Customs.";

			var shipment = HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air);
			shipment.GetOrCreateHVLVConsignmentHeader();

			Factory.Save();

			RunLogWalkerCycleForTest();
			RunUMIServiceTask();

			AssertCollectionContains(expectedLog, NotifiedEventList);
			AssertUniversalShipmentSentToRecipient(RecipientRoleType.HCA);

			var cusMawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "12345678"));
			AssertNotNull("CusMAWB created", cusMawb);
			AssertEquals("CusHAWB attached", 1, cusMawb.ChildBills.Count);
		}

		public void TestSendUniversalToCustoms_Sea()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event queued for processing for Customs.";

			var shipment = HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Sea);
			shipment.GetOrCreateHVLVConsignmentHeader();

			Factory.Save();

			RunLogWalkerCycleForTest();
			RunUMIServiceTask();

			AssertCollectionContains(expectedLog, NotifiedEventList);
			AssertUniversalShipmentSentToRecipient(RecipientRoleType.HSA);

			var cusOceanBill = Factory.LoadTop1<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "12345678"));
			AssertNotNull("Ocean Bill created", cusOceanBill);
			AssertEquals("House Bill attached", 1, cusOceanBill.HouseBills.Count);
		}

		public void TestSendUniversalToCustoms_UnsupportedTransportMode()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event ignored. Transport Mode is 'RAI', only 'AIR' and 'SEA' Transport Modes are supported.";

			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Rail);
			RunLogWalkerCycleForTest();
			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestSendUniversalToCustoms_UnsupportedShipmentType()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event ignored. Shipment Type is 'HLS', only 'HVL' Shipment Type will be processed.";

			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValueLegacy, TransportModes.Air);
			RunLogWalkerCycleForTest();
			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestCargoReportingEventsAreProcessed()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event queued for processing for Customs.";

			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air);
			RunLogWalkerCycleForTest();
			RunUMIServiceTask();

			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestOutturnEventsSentToSpecifiedRecipientRole()
		{
			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, "Outturn");
			RunLogWalkerCycleForTest();

			AssertUniversalShipmentSentToRecipient(RecipientRoleType.COA);
		}

		public void TestEmptyReasonParameterEventsAreNotProcessed()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event ignored. No reason specified.";

			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, "");
			RunLogWalkerCycleForTest();
			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestNoReasonParameterEventsAreNotProcessed()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event ignored. No reason specified.";

			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, null);
			RunLogWalkerCycleForTest();
			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestOtherReasonParameterEventsAreNotProcessed()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event ignored. Reason 'Other Reason' not recognized.";

			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, "Other Reason");
			RunLogWalkerCycleForTest();
			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestGivenAirShipmentWithHLRLogAndResOutturn_WhenHVLVReadyLogSubscriberProcessesShipment_ThenUXMLHasUnderBondDataContext()
		{
			HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, "Outturn");
			RunLogWalkerCycleForTest();
			RunUMIServiceTask();

			AssertUniversalShipmentDataTargetCollectionHasDataContextType(DataContextType.UnderBond);
		}

		public void TestHLRIsProcessedInSameContextAsCreation()
		{
			const string expectedLog = "[HVLVReadyLogSubscriber] Shipment SHIP123 (House Bill='HBL123') HLR Event queued for processing for Customs.";

			var lwkBranch = Factory.New<GlbCompany>().Branches.AddNew();
			lwkBranch.GB_Code = "LWK";

			var shipment = HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air);

			AssertEquals(Env.CurrentBranch.Code, shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single().SL_GB_NKBranch);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, lwkBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				RunLogWalkerCycleForTest();
				RunUMIServiceTask();
			}

			AssertCollectionContains(expectedLog, NotifiedEventList);

			var cusMawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "12345678"));
			AssertEquals(Env.CurrentBranchPK, cusMawb.CM_GB);
		}

		public void TestSetUserContextBeforeProcessEachLog()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "TestCompany";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AAA";

			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			var queuedLog = new QueuedLogForTesting(new BusinessObjectFactoryForHVLVReadyLogSubscriberUserContextTest());
			queuedLog.SJ_GB_NKBranch = branch.GB_Code;
			queuedLog.SJ_ParentID = shipment.PK;

			new HVLVReadyLogSubscriberForUserContextTest().Process(new IQueuedLog[] { queuedLog });
		}

		[Serializable]
		class HVLVReadyLogSubscriberForUserContextTest : HVLVReadyLogSubscriber
		{
			public void Process(IQueuedLog[] queuedLogs)
			{
				ProcessLogQueueItems(queuedLogs);
			}
		}

		class BusinessObjectFactoryForHVLVReadyLogSubscriberUserContextTest : BusinessObjectFactory
		{
			public override BusinessObject Load(Type bizOType, ZGuid pK)
			{
				if (bizOType == typeof(ForwardingShipment))
				{
					AssertEquals("TestCompany", Env.CurrentCompany.Name);
				}

				return base.Load(bizOType, pK);
			}
		}

		#region TestOutturnEventsSentToSpecifiedRecipientRole when not existing branch/department/staff

		public void TestOutturnEventsSentToSpecifiedRecipientRole_BranchNotExisting()
		{
			var branch1 = SetupBranch();
			var department1 = SetupDepartment();
			var staff = SetupStaff();
			Factory.Save();

			var branchInDB = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BR9"));
			AssertNotNull("Pre-condition: Branch BR9 should exist", branchInDB);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, "Outturn");
			}

			branch1.Delete();
			Factory.Save();

			branchInDB = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BR9"));
			AssertNull("Branch BR9 should not exist", branchInDB);

			var messages = RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks();
			Assert(!messages.Contains("[HVLVReadyLogSubscriber] failed to process logs."));

			AssertUniversalShipmentSentToRecipient(RecipientRoleType.COA);
		}

		public void TestOutturnEventsSentToSpecifiedRecipientRole_DepartmentNotExisting()
		{
			var branch1 = SetupBranch();
			var department1 = SetupDepartment();
			var staff = SetupStaff();
			Factory.Save();

			var departmentInDB = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DP9"));
			AssertNotNull("Pre-condition: Department DP9 should exist", departmentInDB);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, "Outturn");
			}

			department1.Delete();
			Factory.Save();

			departmentInDB = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DP9"));
			AssertNull("Department DP9 should not exist", departmentInDB);

			var messages = RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks();
			Assert(!messages.Contains("[HVLVReadyLogSubscriber] failed to process logs."));

			AssertUniversalShipmentSentToRecipient(RecipientRoleType.COA);
		}

		public void TestOutturnEventsSentToSpecifiedRecipientRole_UserNotExisting()
		{
			var branch1 = SetupBranch();
			var department1 = SetupDepartment();
			var staff = SetupStaff();
			Factory.Save();

			var staffInDB = Factory.LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, "ABCUser"));
			AssertNotNull("Pre-condition: Staff ABCUser should exist", staffInDB);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HVLVTestHelper.CreateShipmentWithHLREvent(Factory, ShipmentTypes.HighVolumeLowValue, TransportModes.Air, "Outturn");
			}

			staff.Delete();
			Factory.Save();

			staffInDB = Factory.LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, "ABCUser"));
			AssertNull("Staff ABCUser should not exist", staffInDB);

			var messages = RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks();
			Assert(!messages.Contains("[HVLVReadyLogSubscriber] failed to process logs."));

			AssertUniversalShipmentSentToRecipient(RecipientRoleType.COA);
		}

		GlbBranch SetupBranch()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CM9";

			var orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			AssertNotNull(orgProxy);
			company1.GC_OH_OrgProxy = orgProxy.PK;

			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR9";

			return branch1;
		}

		GlbDepartment SetupDepartment()
		{
			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "DP9";

			return department1;
		}

		IGlbStaff SetupStaff()
		{
			var staff = Factory.New<IGlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABCUser";
			staff.GS_IsController = true;

			return staff;
		}

		#endregion

		void RunUMIServiceTask()
		{
			var serviceTask = new UniversalDataBuss.ServiceTasks.UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;
			AssertEquals("Precondition: ", 0, logger.Count);
			serviceTask.RunTask();
		}

		void AssertUniversalShipmentSentToRecipient(RecipientRoleType recipientType)
		{
			var xml = Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery())
					.OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc])
					.First().EM_MessageText;
			var xmlDocument = XDocument.Parse(xml);
			var ns = xmlDocument.Root.GetDefaultNamespace();

			var shipmentDataSource = xmlDocument.Root.Element(ns + "Shipment").Element(ns + "DataContext").Element(ns + "DataSourceCollection").Elements().Single(x => x.Element(ns + "Type").Value == "ForwardingShipment");
			AssertEquals("SHIP123", shipmentDataSource.Element(ns + "Key").Value);

			var recipientRole = xmlDocument.Root.Element(ns + "Shipment").Element(ns + "DataContext").Element(ns + "RecipientRoleCollection").Elements().Single();
			AssertEquals(recipientType.ToString(), recipientRole.Element(ns + "Code").Value);
		}

		void AssertUniversalShipmentDataTargetCollectionHasDataContextType(DataContextType expectedDataTargetType)
		{
			var xml = Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery())
					.OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc])
					.First().EM_MessageText;
			var xmlDocument = XDocument.Parse(xml);
			var ns = xmlDocument.Root.GetDefaultNamespace();

			var shipmentDataSource = xmlDocument.Root.Element(ns + "Shipment").Element(ns + "DataContext").Element(ns + "DataSourceCollection").Elements().Single(x => x.Element(ns + "Type").Value == "ForwardingShipment");
			AssertEquals("SHIP123", shipmentDataSource.Element(ns + "Key").Value);

			var dataTarget = xmlDocument.Root.Element(ns + "Shipment").Element(ns + "DataContext").Element(ns + "DataTargetCollection").Elements().Single();
			AssertEquals(expectedDataTargetType.ToString(), dataTarget.Element(ns + "Type").Value);
		}
	}
}
