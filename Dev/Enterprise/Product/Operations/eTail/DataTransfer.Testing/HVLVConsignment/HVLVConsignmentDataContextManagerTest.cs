using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using EventReferenceConstants = CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVConsignmentDataContextManager))]
	class HVLVConsignmentDataContextManagerTest : ShipmentDataContextManagerTestCase<HVLVConsignmentDataContextManager, HVLVConsignment>
	{
		#region Overrides

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, Shipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.HVL)
			{
				shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.HVLVConsignment, null);
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new RecipientRoleType[] { RecipientRoleType.HVL };

		protected override string ValidPopulatedUniversalShipmentXML => string.Empty;

		protected override bool ManagerChecksDataTargetToImport => true;

		protected override bool SaveAfterUseIncomingShipment => false;

		protected override HVLVConsignment GetNewBusinessObjectForTesting()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();
			return consignment;
		}

		#endregion

		public void TestMatchingByDataContextKey()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "HVC00000001";
			consignment.HVC_WaybillNumber = "AAAAA";

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "HVC00000001");
			shipment.WayBillNumber = "BBBBB";

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated HVLV Consignment HVC00000001 from UniversalShipment.
Successfully saved HVLV Consignment HVC00000001.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Message Log", @"
Successfully loaded matching HVLVConsignment.
Populating HVLVConsignment...
Updated HVLV Consignment HVC00000001 from UniversalShipment.
Successfully saved HVLV Consignment HVC00000001.
".Trim(), logNoteText);

				var consignments = new BusinessObjectFactory().Load<HVLVConsignment>(new ZQuery());
				AssertEquals(1, consignments.Length);
				AssertEquals("BBBBB", consignments[0].HVC_WaybillNumber);
			});
		}

		public void TestRecipientRoleTargettedToThisModule()
		{
			var manager = new HVLVConsignmentDataContextManager() as IShipmentDataContextManager;
			var dataContext = DataContextFactory.New();
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.HVL } } });
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, ""); // We don't care about the incoming data context type

			manager.DefaultDataTargetFromRecipientRole(dataContext, new TestErrorLogger());
			AssertEquals("HVL Recipient Role -> HVLVConsignment should have been added to DataTarget Collection", nameof(DataContextType.HVLVConsignment), dataContext.DataTargetCollection?.SingleOrDefault()?.Type);
		}

		public void TestOnUniversalEventAddedCore_WhenExistConsignmentRequireACK_CreateSHLEvent()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var shipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentOrigin.RL_RN_NKCountryCode = "AU";
			shipment.JS_RL_NKOrigin = shipmentOrigin.RL_Code;

			var shipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentDestination.RL_RN_NKCountryCode = "US";
			shipment.JS_RL_NKDestination = shipmentDestination.RL_Code;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASStatus = string.Empty;
			consignment.HVC_ACASMessageStatus = "OST";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEventForNotKRQMessageStatus = eventDeserializer.Parse(GetXUEForACASStatusTest("SHL", "7I", ("HAWBNumber", "TEST001")));

			var manifestedShipment = Factory.Load<ForwardingShipment>(consignment.HVC_JS_ManifestedOnShipment);
			var logsFromManifestedShipment = manifestedShipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HeldCode);

			AssertEquals("Precondition : There should be no shipment SHL event log", 0, logsFromManifestedShipment.Count());

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEventForNotKRQMessageStatus);

			CombineAssertions("No shipment SHL event log should be created", () =>
			{
				AssertNotEquals("Precondition : Acknowledgement message is not required to be sent to CBP", "KRQ", consignment.HVC_ACASMessageStatus);
				AssertEquals("There should be no shipment SHL event log", 0, logsFromManifestedShipment.Count());
			});

			var xmlEventForKRQMessageStatus = eventDeserializer.Parse(GetXUEForACASStatusTest("SHL", "7H", ("HAWBNumber", "TEST001")));
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEventForKRQMessageStatus);

			CombineAssertions("One shipment SHL event log should be created", () =>
			{
				AssertEquals("Precondition : Acknowledgement message is required to be sent to CBP", "KRQ", consignment.HVC_ACASMessageStatus);
				AssertEquals("There should be one shipment SHL event log", 1, logsFromManifestedShipment.Count());
			});

			var logFromManifestedShipment = logsFromManifestedShipment.First();
			Assert("Shipment log should not be saved in database", !logFromManifestedShipment.IsInDatabase);

			Factory.SaveForTesting();
			Assert("Shipment log should be saved in database", logFromManifestedShipment.IsInDatabase);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEventForKRQMessageStatus);
			Factory.SaveForTesting();
			AssertEquals("There should still be one shipment SHL event log after OnUniversalEventAdded was called twice", 1, logsFromManifestedShipment.Count());

			logFromManifestedShipment.Parameters.TryGetValue(EventReferenceConstants.EventReferenceParameters.Codes.Department, out var logFromManifestedShipmentDepartment);
			logFromManifestedShipment.Parameters.TryGetValue(EventReferenceConstants.EventReferenceParameters.Codes.MessageType, out var logFromManifestedShipmentMessageType);
			CombineAssertions("Check SHL event log reference", () =>
			{
				AssertEquals("Customs", logFromManifestedShipmentDepartment);
				AssertEquals("ACAS Hold Acknowledgement", logFromManifestedShipmentMessageType);
			});
		}

		public void TestOnUniversalEventAddedCore_WhenInvalidCustomStatus()
		{
			var manifestedShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = manifestedShipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();

			var manifestedShipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			manifestedShipmentOrigin.RL_RN_NKCountryCode = "AU";
			manifestedShipment.JS_RL_NKOrigin = manifestedShipmentOrigin.RL_Code;

			var manifestedShipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			manifestedShipmentDestination.RL_RN_NKCountryCode = "US";
			manifestedShipment.JS_RL_NKDestination = manifestedShipmentDestination.RL_Code;

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEventForInvalidCustomsStatusMessageStatus = eventDeserializer.Parse(GetXUEForStatusTest("MSC", "7I", null, ("ComplianceStatus", "%%%")));

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEventForInvalidCustomsStatusMessageStatus);
			AssertContains("Should report an error",
					$"Current Customs Status Codes is [%%%]\r\nOf type: [CSTEX]\r\nSearched code list: []",
					ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestOnUniversalEventAddedCore_FilterCESMSCEventByDataSourceForImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Latvia))
			{
				var factory = new BusinessObjectFactory();
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "&&&", "CSTI - COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.CustomsStatusForInterface, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "%%%", "CSTI - Another Code", "CLR", RefCusCodeListTypes.Codes.CustomsStatusForInterface, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "&&&", "CSTA - COVID-19 go away!", "CLR", RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "%%%", "CSTA - Another Code", "HLD", RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var manifestedShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				var consignmentHeader = manifestedShipment.GetOrCreateHVLVConsignmentHeader();
				var consignment = consignmentHeader.Consignments.AddNew();

				var manifestedShipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
				manifestedShipmentOrigin.RL_RN_NKCountryCode = CountryCodes.UnitedStates;
				manifestedShipment.JS_RL_NKOrigin = manifestedShipmentOrigin.RL_Code;

				var manifestedShipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
				manifestedShipmentDestination.RL_RN_NKCountryCode = CountryCodes.Latvia;
				manifestedShipment.JS_RL_NKDestination = manifestedShipmentDestination.RL_Code;

				var latviaCompany = Factory.New<GlbCompany>();
				latviaCompany.GC_RN_NKCountryCode = CountryCodes.Latvia;
				var testErrorLogger = new TestErrorLogger();
				var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
				var eventDeserializer = new XmlEventDeserializer();

				var xmlEventForCESWithCustomsDeclaration =
					eventDeserializer.Parse(GetXUEForStatusTest("CES", "TEST", "CustomsDeclaration", ("ComplianceStatus", "%%%")));
				xmlEventForCESWithCustomsDeclaration.DataContext.SetCompanyAndDataProviderDetails(latviaCompany);

				var xmlEventForCESWithoutCustomsDeclaration =
					eventDeserializer.Parse(GetXUEForStatusTest("CES", "TEST", null, ("ComplianceStatus", "&&&")));
				xmlEventForCESWithoutCustomsDeclaration.DataContext.SetCompanyAndDataProviderDetails(latviaCompany);

				var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				importDeclaration.IsCancelled = false;
				consignment.HVC_JE_ImportDeclaration = importDeclaration.PK;

				if (dataContextManager.CanUpdateLogParentFromEvent(consignment, xmlEventForCESWithCustomsDeclaration, out var failureReason))
				{
					dataContextManager.OnUniversalEventAdded(testErrorLogger, xmlEventForCESWithCustomsDeclaration);
				}
				CombineAssertions("Should process CES event if consignment has SAD and XUE DataSource is CustomsDeclaration", () =>
				{
					AssertEquals("Should set to target status code in the CES event", "%%%", consignment.HVC_ImportCustomsClearanceStatus);
					AssertEquals("Should be CSTI code's release status", "CLR", consignment.HVC_ImportReleaseStatus);
					AssertEquals("Should be CSTI code's release status description", "CSTI - Another Code", consignment.ImportCustomsClearanceStatusDescription);
				});

				if (dataContextManager.CanUpdateLogParentFromEvent(consignment, xmlEventForCESWithoutCustomsDeclaration, out failureReason))
				{
					dataContextManager.OnUniversalEventAdded(testErrorLogger, xmlEventForCESWithoutCustomsDeclaration);
				}

				CombineAssertions("Should not process CES event if consignment has SAD and XUE DataSource is not CustomsDeclaration", () =>
				{
					AssertEquals("Consignment has a standalone declaration and only accept Customs Status Update Event from Data Source of Declaration", failureReason);
					AssertEquals("Should keep the same status code in the CES event", "%%%", consignment.HVC_ImportCustomsClearanceStatus);
					AssertEquals("Should be CSTI code's release status", "CLR", consignment.HVC_ImportReleaseStatus);
					AssertEquals("Should be CSTI code's release status description", "CSTI - Another Code", consignment.ImportCustomsClearanceStatusDescription);
				});
			}
		}

		public void TestOnUniversalEventAddedCore_FilterCESMSCEventByDataSourceForExport()
		{
			var factory = new BusinessObjectFactory();
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "&&&", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "%%%", "Another Code", "CLR", RefCusCodeListTypes.Codes.ExportCustomsStatus);

			var manifestedShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = manifestedShipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();

			var manifestedShipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			manifestedShipmentOrigin.RL_RN_NKCountryCode = "AU";
			manifestedShipment.JS_RL_NKOrigin = manifestedShipmentOrigin.RL_Code;

			var manifestedShipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			manifestedShipmentDestination.RL_RN_NKCountryCode = "US";
			manifestedShipment.JS_RL_NKDestination = manifestedShipmentDestination.RL_Code;

			var testErrorLogger = new TestErrorLogger();
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEventForCESWithCustomsDeclaration =
				eventDeserializer.Parse(GetXUEForStatusTest("CES", "TEST", "CustomsDeclaration", ("ComplianceStatus", "%%%")));
			var xmlEventForCESWithoutCustomsDeclaration =
				eventDeserializer.Parse(GetXUEForStatusTest("CES", "TEST", null, ("ComplianceStatus", "&&&")));

			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			exportDeclaration.IsCancelled = false;
			consignment.HVC_JE_ExportDeclaration = exportDeclaration.PK;

			if (dataContextManager.CanUpdateLogParentFromEvent(consignment, xmlEventForCESWithCustomsDeclaration, out var failureReason))
			{
				dataContextManager.OnUniversalEventAdded(testErrorLogger, xmlEventForCESWithCustomsDeclaration);
			}
			AssertEquals("Should process CES event if consignment has SAD and XUE DataSource is CustomsDeclaration",
				"%%%", consignment.HVC_ExportCustomsClearanceStatus);

			if (dataContextManager.CanUpdateLogParentFromEvent(consignment, xmlEventForCESWithoutCustomsDeclaration, out failureReason))
			{
				dataContextManager.OnUniversalEventAdded(testErrorLogger, xmlEventForCESWithoutCustomsDeclaration);
			}
			AssertEquals("Consignment has a standalone declaration and only accept Customs Status Update Event from Data Source of Declaration", failureReason);
			AssertEquals("Should not process CES event if consignment has SAD and XUE DataSource is not CustomsDeclaration",
				"%%%", consignment.HVC_ExportCustomsClearanceStatus);
		}

		public void TestUpdateCustomsClearanceStatusByXUEComplianceStatusDataContext()
		{
			var factory = new BusinessObjectFactory();
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "&&&", "COVID-19 go away!", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "%%%", "Another Code", "CLR");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "&&&", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "%%%", "Another Code", "CLR", RefCusCodeListTypes.Codes.ExportCustomsStatus);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var shipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentOrigin.RL_RN_NKCountryCode = "AU";
			shipment.JS_RL_NKOrigin = shipmentOrigin.RL_Code;

			var shipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentDestination.RL_RN_NKCountryCode = "US";
			shipment.JS_RL_NKDestination = shipmentDestination.RL_Code;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			CombineAssertions("Update Export Customs Status", () =>
			{
				AssertUpdateCustomsClearanceStatusByComplianceStatus("not update export customs clearance status from ARV event", "ARV", Directions.Export, ZString.Empty);
				AssertUpdateCustomsClearanceStatusByComplianceStatus("update export customs clearance status from CES event", "CES", Directions.Export, "&&&");
				AssertUpdateCustomsClearanceStatusByComplianceStatus("update export customs clearance status from MSC event", "MSC", Directions.Export, "%%%");
			});

			shipmentOrigin.RL_RN_NKCountryCode = "US";
			shipmentDestination.RL_RN_NKCountryCode = "AU";

			CombineAssertions("Update Import Customs Status", () =>
			{
				AssertUpdateCustomsClearanceStatusByComplianceStatus("not update import customs clearance status from ARV event", "ARV", Directions.Import, ZString.Empty);
				AssertUpdateCustomsClearanceStatusByComplianceStatus("update import customs clearance status from CES event", "CES", Directions.Import, "&&&");
				AssertUpdateCustomsClearanceStatusByComplianceStatus("update import customs clearance status from ARV event", "MSC", Directions.Import, "%%%");
			});

			void AssertUpdateCustomsClearanceStatusByComplianceStatus(string message, string eventType, Directions directions, string expectStatus)
			{
				var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest(eventType, "|TYP=HLD", null, ("ComplianceStatus", expectStatus)));

				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

				switch (directions)
				{
					case Directions.Import:
						AssertEquals(message, expectStatus, consignment.HVC_ImportCustomsClearanceStatus);
						break;
					case Directions.Export:
						AssertEquals(message, expectStatus, consignment.HVC_ExportCustomsClearanceStatus);
						break;
				}
			}
		}

		public void TestUpdateReleaseStatusByXUEWarehouseReleaseStatusDataContext_ExportEventDirection()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var shipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentOrigin.RL_RN_NKCountryCode = "AU";
			shipment.JS_RL_NKOrigin = shipmentOrigin.RL_Code;

			var shipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentDestination.RL_RN_NKCountryCode = "US";
			shipment.JS_RL_NKDestination = shipmentDestination.RL_Code;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertReleaseStatusByWarehouseReleaseStatus(consignment, Directions.Export);
		}

		public void TestUpdateReleaseStatusByXUEWarehouseReleaseStatusDataContext_ImportEventDirection()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var shipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentOrigin.RL_RN_NKCountryCode = "US";
			shipment.JS_RL_NKOrigin = shipmentOrigin.RL_Code;

			var shipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentDestination.RL_RN_NKCountryCode = "AU";
			shipment.JS_RL_NKDestination = shipmentDestination.RL_Code;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertReleaseStatusByWarehouseReleaseStatus(consignment, Directions.Import);
		}

		void AssertReleaseStatusByWarehouseReleaseStatus(HVLVConsignment consignment, Directions direction)
		{
			var testCases = new[]
			{
				(initialReleaseStatus: "NON", eventType: "CES", warehouseReleaseStatus: "CLR", eventReference: "|TYP=HLD", expectedReleaseStatus: "CLR", message: "EventType CES"),
				(initialReleaseStatus: "NON", eventType: "MSC", warehouseReleaseStatus: "CLR", eventReference: "|TYP=HLD", expectedReleaseStatus: "CLR", message: "EventType MSC"),
				(initialReleaseStatus: "NON", eventType: "ARV", warehouseReleaseStatus: "CLR", eventReference: "|TYP=HLD", expectedReleaseStatus: "NON", message: "EventType ARV - only CES and MSC update releaseStatus"),

				(initialReleaseStatus: "NON", eventType: "CES", warehouseReleaseStatus: "HLD", eventReference: "", expectedReleaseStatus: "HLD", message: "WarehouseReleaseStatus HLD"),
				(initialReleaseStatus: "CLR", eventType: "CES", warehouseReleaseStatus: "NON", eventReference: "", expectedReleaseStatus: "NON", message: "WarehouseReleaseStatus NON"),
				(initialReleaseStatus: "NON", eventType: "CES", warehouseReleaseStatus: "INVALID", eventReference: "", expectedReleaseStatus: "NON", message: "WarehouseReleaseStatus invalid does not update releaseStatus"),
				(initialReleaseStatus: "NON", eventType: "CES", warehouseReleaseStatus: "INVALID", eventReference: "|TYP=HLD", expectedReleaseStatus: "HLD", message: "WarehouseReleaseStatus invalid and eventReference |TYP exists, update according to TYP"),
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					consignment.HVC_ImportReleaseStatus = testCase.initialReleaseStatus;
					consignment.HVC_ExportReleaseStatus = testCase.initialReleaseStatus;
					AddEventWithWarehouseReleaseStatus(consignment, testCase.eventType, testCase.eventReference, testCase.warehouseReleaseStatus);
					if (direction == Directions.Export)
					{
						AssertEquals($"{testCase.message}: ExportReleaseStatus", testCase.expectedReleaseStatus, consignment.HVC_ExportReleaseStatus);
						AssertEquals($"{testCase.message}: ImportReleaseStatus should not change", testCase.initialReleaseStatus, consignment.HVC_ImportReleaseStatus);
					}
					else if (direction == Directions.Import)
					{
						AssertEquals($"{testCase.message}: ImportReleaseStatus", testCase.expectedReleaseStatus, consignment.HVC_ImportReleaseStatus);
						AssertEquals($"{testCase.message}: ExportReleaseStatus should not change", testCase.initialReleaseStatus, consignment.HVC_ExportReleaseStatus);
					}
				}
			});
		}

		void AddEventWithWarehouseReleaseStatus(HVLVConsignment consignment, string eventType, string eventReference, string warehouseReleaseStatus)
		{
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest(eventType, eventReference, null, ("WarehouseReleaseStatus", warehouseReleaseStatus)));

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
		}

		public void TestPopulateReleaseStatus_PreferenceOrder()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var shipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentOrigin.RL_RN_NKCountryCode = "US";
			shipment.JS_RL_NKOrigin = shipmentOrigin.RL_Code;

			var shipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentDestination.RL_RN_NKCountryCode = "AU";
			shipment.JS_RL_NKDestination = shipmentDestination.RL_Code;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var factory = new BusinessObjectFactory();
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "%%%", "Cleared Code", "CLR");

			CombineAssertions(() =>
			{
				var xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest("CES", "|TYP=HLD", null, ("ComplianceStatus", "%%%"), ("WarehouseReleaseStatus", "HLD")));
				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
				AssertEquals("ReleaseStatus calculated from ComplianceStatus has highest preference", "CLR", consignment.HVC_ImportReleaseStatus);

				consignment.HVC_ImportReleaseStatus = "NON";
				xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest("CES", "|TYP=HLD", null, ("WarehouseReleaseStatus", "CLR")));
				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
				AssertEquals("valid WarehouseReleaseStatus has next highest priority", "CLR", consignment.HVC_ImportReleaseStatus);

				consignment.HVC_ImportReleaseStatus = "NON";
				xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest("CES", "|TYP=HLD", null, ("WarehouseReleaseStatus", "INV")));
				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
				AssertEquals("EventReference is used if WarehouseReleaseStatus is invalid", "HLD", consignment.HVC_ImportReleaseStatus);

				consignment.HVC_ImportReleaseStatus = "NON";
				xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest("CES", "|TYP=HLD", null));
				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
				AssertEquals("EventReference is used if WarehouseReleaseStatus is missing", "HLD", consignment.HVC_ImportReleaseStatus);
			});
		}

		public void TestPopulateReleaseStatusByXUEWhenComplianceStatusAndWarehouseReleaseStatusIsEmpty_DependsOnShipmentDirection()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var shipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentOrigin.RL_RN_NKCountryCode = "AU";
			shipment.JS_RL_NKOrigin = shipmentOrigin.RL_Code;

			var shipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			shipmentDestination.RL_RN_NKCountryCode = "US";
			shipment.JS_RL_NKDestination = shipmentDestination.RL_Code;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			CombineAssertions("Update Export Release Status", () =>
			{
				AssertUpdateReleaseStatus("not update export release status from ARV event", "ARV", Directions.Export, "NON");
				AssertUpdateReleaseStatus("update export release status from CES event", "CES", Directions.Export, "HLD");
				AssertUpdateReleaseStatus("update export release status from MSC event", "MSC", Directions.Export, "HLD");
			});

			shipmentOrigin.RL_RN_NKCountryCode = "US";
			shipmentDestination.RL_RN_NKCountryCode = "AU";

			CombineAssertions("Update Import Release Status", () =>
			{
				AssertUpdateReleaseStatus("not update import release status from ARV event", "ARV", Directions.Import, "NON");
				AssertUpdateReleaseStatus("update import release status from CES event", "CES", Directions.Import, "HLD");
				AssertUpdateReleaseStatus("update import release status from ARV event", "MSC", Directions.Import, "HLD");
			});

			void AssertUpdateReleaseStatus(string message, string eventType, Directions directions, string expectStatus)
			{
				consignment.HVC_ExportReleaseStatus = "NON";
				consignment.HVC_ImportReleaseStatus = "NON";
				var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest(eventType, "|TYP=HLD"));

				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

				switch (directions)
				{
					case Directions.Import:
						AssertEquals(message, expectStatus, consignment.HVC_ImportReleaseStatus);
						break;
					case Directions.Export:
						AssertEquals(message, expectStatus, consignment.HVC_ExportReleaseStatus);
						break;
				}
			}
		}

		public void TestGivenNewUniversalEventAdded_WhenStatusCodeIsSTU_ThenUpdateConsignmentStatus()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest("STU", "|NEW=" + HVLVConsignmentStatus.Codes.Delivered));

			var referenceParameters = StmALog.GetParametersFromReference(xmlEvent.EventReference);
			referenceParameters.TryGetValue(EventReferenceConstants.EventReferenceParameters.Codes.New, out var newStatus);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: NEW in EventReference expected to be DLV", HVLVConsignmentStatus.Codes.Delivered, newStatus);
				AssertEquals("Precondition: original HVC_Status of newly created consignment is BKD", HVLVConsignmentStatus.Codes.Booked, consignment.HVC_Status);
			});

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
			AssertEquals("Expected consignment status to be populated with DLV", HVLVConsignmentStatus.Codes.Delivered, consignment.HVC_Status);
		}

		public void TestWhenSTUEventOccurs_ThenDoNotLogSTUEvent()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_ItemId = "ITEM1";
			item.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item.HVI_HVC_Consignment = consignment.PK;

			AssertEquals(0, consignment.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count());
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.HVLVItem, "ITEM1");
			universalEvent.EventType = AutoEvents.StatusUpdatedCode;
			universalEvent.EventTime = new ZDateTimeOffset(2017, 6, 1);
			universalEvent.EventReference = "|NEW=DLV";
			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);
			Factory.SaveForTesting();

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, item.PK);
			logQuery.AddToFilter(StmALogSchema.SL_Table, HVLVItemSchema.Constants.TableName);
			var logs = Factory.Load<StmALog>(logQuery);

			AssertEquals(1, logs.Length);
		}

		public void TestHVLVACASStatuses_WhenEventTypeIsNotInACASEventsList()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("MRJ", "some reason"));

			AssertEquals("Precondition: ACAS status is empty", string.Empty, consignment.HVC_ACASStatus);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", string.Empty, consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status did not update", string.Empty, consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status did not update", string.Empty, consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenACASEventTypeIsMPP()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("MPP", "SR", ("HAWBNumber", "TEST001")));

			AssertEquals("Precondition: ACAS status is empty", string.Empty, consignment.HVC_ACASStatus);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status updated", "SR", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status did not update", string.Empty, consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status did not update", string.Empty, consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenACASEventTypeIsSCM()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("SCM", "SF", ("HAWBNumber", "TEST001")));

			AssertEquals("Precondition: ACAS status is empty", string.Empty, consignment.HVC_ACASStatus);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status updated", "SF", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status did not update", string.Empty, consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status did not update", string.Empty, consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenACASEventTypeIsSCH()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("SCH", "6I", ("HAWBNumber", "TEST001")));

			AssertEquals("Precondition: ACAS status is empty", string.Empty, consignment.HVC_ACASStatus);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status updated", "6I", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status did not update", string.Empty, consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status did not update", string.Empty, consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenACASEventTypeIsSHL()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("SHL", "6H", ("HAWBNumber", "TEST001")));

			AssertEquals("Precondition: ACAS status is empty", string.Empty, consignment.HVC_ACASStatus);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status updated", "6H", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status updated to KRQ", "KRQ", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status did not update", string.Empty, consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenLastCBPResponseIsOnSelecteeDataIssueHold_ACKSent_ISNEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "KST";
			consignment.HVC_ACASStatus = "7H";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("ISN"));

			Assert("Pre condition: On Selectee Data Issue Hold", consignment.IsLastCBPResponseOnSelecteeDataIssueHold);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", "7H", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status updated to ARQ", "ARQ", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to KIS", "KIS", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenLastCBPResponseIsOnSelecteeDataIssueHold_ACKSent_IRJEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "KST";
			consignment.HVC_ACASStatus = "7H";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("IRJ"));

			Assert("Pre condition: On Selectee Data Issue Hold", consignment.IsLastCBPResponseOnSelecteeDataIssueHold);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", "7H", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status reverted to KRQ", "KRQ", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to KIJ", "KIJ", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenLastCBPResponseIsOnSelecteeDataIssueHold_AmendmentSent_ISNEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "AST";
			consignment.HVC_ACASStatus = "7H";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("ISN"));

			Assert("Pre condition: On Selectee Data Issue Hold", consignment.IsLastCBPResponseOnSelecteeDataIssueHold);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", "7H", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status did not update", "AST", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to AIS", "AIS", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenLastCBPResponseIsOnSelecteeDataIssueHold_AmendmentSent_IRJEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "AST";
			consignment.HVC_ACASStatus = "7H";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("IRJ"));

			Assert("Pre condition: On Selectee Data Issue Hold", consignment.IsLastCBPResponseOnSelecteeDataIssueHold);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", "7H", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status reverted to ARQ", "ARQ", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to AIJ", "AIJ", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenLastCBPResponseIsOnHold_ACKSent_ISNEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "KST";
			consignment.HVC_ACASStatus = "6H";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("ISN"));

			Assert("Pre condition: CBP response is On Hold", consignment.IsLastCBPResponseOnHold);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", "6H", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status did not update", "KST", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to KIS", "KIS", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenLastCBPResponseIsOnHold_ACKSent_IRJEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "KST";
			consignment.HVC_ACASStatus = "6H";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("IRJ"));

			Assert("Pre condition: CBP response is On Hold", consignment.IsLastCBPResponseOnHold);

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", "6H", consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status reverted to KRQ", "KRQ", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to KIJ", "KIJ", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenOriginalMessageIsSent_ISNEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "OST";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("ISN"));

			CombineAssertions("Pre condition: No CBP response", () =>
			{
				AssertNullOrEmpty(consignment.HVC_ACASStatus);
				Assert(!consignment.IsLastCBPResponseOnHold);
				Assert(!consignment.IsLastCBPResponseOnSelecteeDataIssueHold);
			});

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", string.Empty, consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status did not update", "OST", consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to OIS", "OIS", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestHVLVACASStatuses_WhenOriginalMessageIsSent_IRJEventReceived()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASMessageStatus = "OST"; // Original message is sent

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("IRJ"));

			CombineAssertions("Pre condition: No CBP response", () =>
			{
				AssertNullOrEmpty(consignment.HVC_ACASStatus);
				Assert(!consignment.IsLastCBPResponseOnHold);
				Assert(!consignment.IsLastCBPResponseOnSelecteeDataIssueHold);
			});

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("ACAS status did not update", string.Empty, consignment.HVC_ACASStatus);
			AssertEquals("ACAS Message status reverted to empty", string.Empty, consignment.HVC_ACASMessageStatus);
			AssertEquals("ACAS Interchange Status updated to OIJ", "OIJ", consignment.HVC_ACASInterchangeStatus);
		}

		public void TestMatchesEventByConsignmentId()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = header.Consignments.AddNew();
				consignment.HVC_WaybillNumber = "CONSIGN1";

				Factory.SaveForTesting();

				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN1", consignment.HVC_ConsignmentId);

				var universalEvent = new UniversalEvent();
				universalEvent.DataContext = DataContextFactory.New();
				universalEvent.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN2");
				universalEvent.EventType = AutoEvents.BookingConfirmedCode;
				universalEvent.EventTime = new ZDateTimeOffset(2017, 6, 1);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalEventMessage(universalEvent);
				manager.Process(message);

				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				universalEvent.DataContext.DataTargetCollection.Single().Key = "CONSIGN1";
				serviceTaskLog = new ServiceTaskLogForTesting();
				manager = new UniversalMessageProcessingManager(serviceTaskLog);
				message = GetQueuedUniversalEventMessage(universalEvent);
				manager.Process(message);
				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertEquals("Service Task Log", "Linked Event to HVLV Consignment CONSIGN1.", serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertEquals("Message Log", "Linked Event to HVLV Consignment CONSIGN1.", logNoteText);

					var consignmentInNewFactory = new BusinessObjectFactory().Load<HVLVConsignment>(consignment.PK);
					var importedEvent = consignmentInNewFactory.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingConfirmedCode).Single();
					AssertEquals(new ZDateTime(2017, 6, 1), importedEvent.SL_EventTime);
				});
			}
		}

		public void TestEventContextValues_Default()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "WAYBILL123";
			consignment.HVC_ShipperReference = "SHIPREF123";

			var expectedContextValues = new[]
			{
				"HAWBNumber - WAYBILL123",
				"ShippersReference - SHIPREF123",
				"WarehouseReleaseStatus - NON"
			};
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("WaybillNumber exported as HAWBNumber", expectedContextValues, actualContextValues);
		}

		public void TestEventContextValues_WhenManifestedOnNonAIRShipment()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "WAYBILL123";
			consignment.HVC_ShipperReference = "SHIPREF123";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var expectedContextValues = new[]
			{
				"HBOLNumber - WAYBILL123",
				"ShippersReference - SHIPREF123",
				"WarehouseReleaseStatus - NON"
			};
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("WaybillNumber exported as HBOLNumber", expectedContextValues, actualContextValues);
		}

		[TestDate(2020, 4, 20)]
		public void TestEventContextValues_WithConsolAndAIRShipment()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "WAYBILL123";
			consignment.HVC_ShipperReference = "SHIPREF123";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_MasterBillNum = "12345678";

			var leg = consol.Transports[0];
			leg.JW_ETA = ZDateTime.Today;
			leg.JW_ETD = ZDateTime.Today.AddDays(-1);
			leg.JW_VoyageFlight = "QF01";

			var expectedContextValues = new[]
			{
				"MAWBNumber - 123-45678",
				"HAWBNumber - WAYBILL123",
				"ShippersReference - SHIPREF123",
				"TransportMode - AIR",
				"FlightNumber - QF01",
				"MBOLOriginUNLOCO - AUSYD",
				"MAWBOriginIATAAirportCode - SYD",
				"MBOLDestinationUNLOCO - AUMEL",
				"MAWBDestinationIATAAirportCode - MEL",
				"EstimatedTimeOfArrival - 20-Apr-20 00:00:00",
				"EstimatedTimeOfDeparture - 19-Apr-20 00:00:00",
				"WarehouseReleaseStatus - NON",
			};
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("Air transport details are exported", expectedContextValues, actualContextValues);
		}

		[TestDate(2020, 4, 20)]
		public void TestEventContextValues_WithConsolAndNonAIRShipment()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "WAYBILL123";
			consignment.HVC_ShipperReference = "SHIPREF123";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_MasterBillNum = "12345678";

			var leg = consol.Transports[0];
			leg.JW_ETA = ZDateTime.Today;
			leg.JW_ETD = ZDateTime.Today.AddDays(-1);
			leg.JW_Vessel = "Vessel1";
			leg.JW_VoyageFlight = "ABC123";

			var expectedContextValues = new[]
			{
				"MBOLNumber - 12345678",
				"HBOLNumber - WAYBILL123",
				"ShippersReference - SHIPREF123",
				"TransportMode - SEA",
				"VesselName - Vessel1",
				"VoyageNumber - ABC123",
				"MBOLOriginUNLOCO - AUSYD",
				"MBOLDestinationUNLOCO - AUMEL",
				"EstimatedTimeOfArrival - 20-Apr-20 00:00:00",
				"EstimatedTimeOfDeparture - 19-Apr-20 00:00:00",
				"WarehouseReleaseStatus - NON",
			};
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("Non air transport details are exported", expectedContextValues, actualContextValues);
		}

		public void TestEventContextValues_ImportCustomsStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(new BusinessObjectFactory(), "TES", "COVID-19 go away!");
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();

			consignment.HVC_ImportCustomsClearanceStatus = "TES";

			var expectedContextValues = new[]
			{
				"ComplianceStatus - TES",
				"WarehouseReleaseStatus - HLD",
			};
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("ImportCustomsClearenceStatus exported as ComplianceStatus", expectedContextValues, actualContextValues);
		}

		public void TestEventContextValues_ImportCustomsStatus_TakesPriorityEvenIfExportStatusIsNotEmpty()
		{
			var factory = new BusinessObjectFactory();
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "IMP", "DESC", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(factory, "EXP", "DESC", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();

			consignment.HVC_ImportCustomsClearanceStatus = "IMP";
			consignment.HVC_ExportCustomsClearanceStatus = "EXP";

			var expectedContextValues = new[]
			{
				"ComplianceStatus - IMP",
				"WarehouseReleaseStatus - HLD",
			};
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("ImportCustomsClearenceStatus exported as ComplianceStatus even when ExportStatus is not empty", expectedContextValues, actualContextValues);
		}

		public void TestEventContextValues_ExportCustomsStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(new BusinessObjectFactory(), "TES", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();

			consignment.HVC_ExportCustomsClearanceStatus = "TES";

			var expectedContextValues = new[]
			{
				"ComplianceStatus - TES",
				"WarehouseReleaseStatus - HLD",
			};
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("ExportCustomsClearenceStatus exported as ComplianceStatus", expectedContextValues, actualContextValues);
		}

		public void TestEventContextValues_WarehouseReleaseStatus()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();

			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;

			var expectedContextValues = new[] { "WarehouseReleaseStatus - CLR" };
			var actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("HVC_ImportReleaseStatus exported as WarehouseReleaseStatus", expectedContextValues, actualContextValues);

			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;

			expectedContextValues = new[] { "WarehouseReleaseStatus - HLD" };
			actualContextValues = ((IEventDataContextManager)consignment.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("HVC_ExportReleaseStatus exported as WarehouseReleaseStatus", expectedContextValues, actualContextValues);
		}

		public void TestACASMessageStatus_WhenResponseFromCBPIsOnHold_IsKRQ()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASStatus = string.Empty;
			consignment.HVC_ACASMessageStatus = "OST";

			var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForACASStatusTest("SHL", "7H", ("HAWBNumber", "TEST001")));

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("Acknowledgement message is required to be sent to CBP", "KRQ", consignment.HVC_ACASMessageStatus);
		}

		public void TestCanUpdateLogParentFromEvent_ReturnTrue_XUEIsNotFromDeclarationAndConsignmentIsAUImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var manifestedShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				var consignmentHeader = manifestedShipment.GetOrCreateHVLVConsignmentHeader();
				var consignment = consignmentHeader.Consignments.AddNew();

				var manifestedShipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
				manifestedShipmentOrigin.RL_RN_NKCountryCode = "US";
				manifestedShipment.JS_RL_NKOrigin = manifestedShipmentOrigin.RL_Code;

				var manifestedShipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
				manifestedShipmentDestination.RL_RN_NKCountryCode = "AU";
				manifestedShipment.JS_RL_NKDestination = manifestedShipmentDestination.RL_Code;

				consignment.HVC_JE_ImportDeclaration = declaration.PK;

				var dataContextManager = (IEventDataContextManager)consignment.GetUniversalDataContextManager();

				var eventDeserializer = new XmlEventDeserializer();
				var xmlEventForCESWithoutCustomsDeclaration =
					eventDeserializer.Parse(GetXUEForStatusTest("CES", "TEST", null, ("ComplianceStatus", "&&&")));

				Assert("Still allow AU import consignment process low value XUE when it has declaration",
					dataContextManager.CanUpdateLogParentFromEvent(consignment, xmlEventForCESWithoutCustomsDeclaration, out var failureReason));
			}
		}

		#region Event Context Matching

		public void TestEventContextMatching_UsingHAWBNumber()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contextCollectionValues = @"
				<Context>
					<Type>HAWBNumber</Type>
					<Value>WAYBILL456</Value>
				</Context>";

				AssertEventContextMatching(contextCollectionValues, shouldBeMatched: true);
			}
		}

		public void TestEventContextMatching_UsingHBOLNumber()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contextCollectionValues = @"
				<Context>
					<Type>HBOLNumber</Type>
					<Value>WAYBILL456</Value>
				</Context>";

				AssertEventContextMatching(contextCollectionValues, shouldBeMatched: true);
			}
		}

		public void TestEventContextMatching_DiscardUnmatched()
		{
			var contextCollectionValues = @"
			<Context>
				<Type>HBOLNumber</Type>
				<Value>WAYBILLXXX</Value>
			</Context>";

			AssertEventContextMatching(contextCollectionValues, shouldBeMatched: false);
		}

		public void TestEventContextMatching_HAWBNumberIsDifferentToHBOLNumber_Discard()
		{
			var contextCollectionValues = @"
			<Context>
				<Type>HAWBNumber</Type>
				<Value>WAYBILL123</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>WAYBILL456</Value>
			</Context>";

			AssertEventContextMatching(contextCollectionValues, shouldBeMatched: false);
		}

		void AssertEventContextMatching(string contextCollectionValues, bool shouldBeMatched)
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_WaybillNumber = "WAYBILL123";
			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_WaybillNumber = "WAYBILL456";

			Factory.SaveForTesting();

			var eventXml = string.Format(@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>HVLVConsignment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-12-02T08:40:00</EventTime>
		<EventType>ARV</EventType>
		<EventReference>Goods are here</EventReference>
		<ContextCollection>
			{0}
		</ContextCollection>
	</Event>
</UniversalEvent>", contextCollectionValues);

			var message = GetQueuedUniversalEventMessage(eventXml);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			void AssertLogsCount(HVLVConsignment consignment, int expectedCount)
			{
				consignment.Reload();
				var logs = consignment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode));
				AssertEquals(expectedCount, logs.Length);
			}

			if (shouldBeMatched)
			{
				CombineAssertions(() =>
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertEquals("Linked Event to HVLV Consignment WAYBILL456.", serviceTaskLog.ToString());

					AssertLogsCount(consignment1, 0);
					AssertLogsCount(consignment2, 1);
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
					AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());

					AssertLogsCount(consignment1, 0);
					AssertLogsCount(consignment2, 0);
				});
			}
		}

		public void TestConsignmentEventMatchingByWaybillAndMasterBill()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				Factory.SaveForTesting();

				var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
				var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
				var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();

				consol1.MasterBillMAWB = "MAWB1";
				consol2.MasterBillMAWB = "MAWB2";
				consol3.MasterBillMAWB = "MAWB3";

				var shipmentA = Factory.NewWithValidTestData<HVLVForwardingShipment>();

				var consignA = Factory.NewWithValidTestData<HVLVConsignment>();
				consignA.HVC_ConsignmentId = string.Empty;
				consignA.HVC_WaybillNumber = "CONSIGNA";
				consignA.HVC_JS_ManifestedOnShipment = shipmentA.PK;

				var shipmentB = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				var consignB = Factory.NewWithValidTestData<HVLVConsignment>();
				consignB.HVC_ConsignmentId = string.Empty;
				consignB.HVC_WaybillNumber = "CONSIGNB";
				consignB.HVC_JS_ManifestedOnShipment = shipmentB.PK;

				var consignC = Factory.NewWithValidTestData<HVLVConsignment>();
				consignC.HVC_ConsignmentId = string.Empty;
				consignC.HVC_WaybillNumber = "CONSIGNB";
				bookingHeader.Consignments.Add(consignC);

				consol1.Shipments.Add(shipmentA);
				consol2.Shipments.Add(shipmentA);
				consol2.Shipments.Add(shipmentB);

				Factory.SaveForTesting();

				AssertConsignmentMatching(consignmentToMatch: consignA, ("HAWBNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: null, ("HAWBNumber", "CONSIGNB"));
				AssertConsignmentMatching(consignmentToMatch: null, ("MAWBNumber", "MAWB3"), ("HAWBNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: null, ("MAWBNumber", "MAWB1"), ("HAWBNumber", "CONSIGNB"));
				AssertConsignmentMatching(consignmentToMatch: null, ("MAWBNumber", "MAWB1"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MAWBNumber", "MAWB1"), ("HAWBNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MAWBNumber", "MAWB2"), ("HAWBNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MAWBNumber", "MAWB-2"), ("HAWBNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MAWBNumber", "MA-WB2"), ("HAWBNumber", "CONSIGNA"));

				consol1.JK_MasterBillNum = "THISISMBOL1";
				consol2.JK_MasterBillNum = "THISISMBOL2";
				consol3.JK_MasterBillNum = "THISISMBOL3";

				Factory.SaveForTesting();

				AssertConsignmentMatching(consignmentToMatch: consignA, ("HBOLNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: null, ("HBOLNumber", "CONSIGNB"));
				AssertConsignmentMatching(consignmentToMatch: null, ("MBOLNumber", "THISISMBOL1"), ("HBOLNumber", "CONSIGNB"));
				AssertConsignmentMatching(consignmentToMatch: null, ("MBOLNumber", "THISISMBOL1"), ("HBOLNumber", "CONSIGNB"));
				AssertConsignmentMatching(consignmentToMatch: null, ("MBOLNumber", "THISISMBOL3"), ("HBOLNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MBOLNumber", "THISISMBOL1"), ("HBOLNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MBOLNumber", "THISISMBOL2"), ("HBOLNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MBOLNumber", "THISISMBOL-2"), ("HBOLNumber", "CONSIGNA"));
				AssertConsignmentMatching(consignmentToMatch: consignA, ("MBOLNumber", "THISISMB-OL2"), ("HBOLNumber", "CONSIGNA"));
			}
		}

		public void TestConsignmentEventMatchingByWaybillAndMasterBill_WhenBookingHeaderHasNoShipment()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
				Factory.SaveForTesting();

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.MasterBillMAWB = "MAWB1";

				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

				var consignment = header.Consignments.AddNew();
				consignment.HVC_WaybillNumber = "CONSIGNA";
				consignment.HVC_HVH_BookingHeader = header.PK;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				consol.Shipments.Add(shipment);

				Factory.SaveForTesting();

				AssertConsignmentMatching(consignmentToMatch: consignment, ("MAWBNumber", "MAWB1"), ("HAWBNumber", "CONSIGNA"));
			}
		}

		public void TestConsignmentEventMatching_MatchesAIREventByShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_MasterBillNum = "CONSOLMAWB";

			var shipmentA = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipmentA.JS_HouseBill = "SHB11111";

			var consignA = Factory.NewWithValidTestData<HVLVConsignment>();
			consignA.HVC_WaybillNumber = "CONSIGNHAWB";
			consignA.HVC_JS_ManifestedOnShipment = shipmentA.PK;

			var shipmentB = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipmentB.JS_HouseBill = "SHB22222";

			var consignB = Factory.NewWithValidTestData<HVLVConsignment>();
			consignB.HVC_WaybillNumber = "CONSIGNHAWB";
			consignB.HVC_JS_ManifestedOnShipment = shipmentB.PK;

			consol.Shipments.Add(shipmentA);
			consol.Shipments.Add(shipmentB);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(GetXUE(("MAWBNumber", "CONSOLMAWB"), ("HAWBNumber", "CONSIGNHAWB"), ("MasterHouseBill", "SHB22222")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var consignALogs = consignA.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode));
			var consignBLogs = consignB.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode));

			CombineAssertions("Should have only matched Consignment from Shipment with matching MasterHouseBill", () =>
			{
				AssertEquals("EDI Message should have been processed successfully", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("Service Task Log should indicate a link to only one consignment", 1, Regex.Matches(serviceTaskLog.ToString(), $"Linked Event to HVLV Consignment").Count);
				AssertEquals("Should not match Consignment from Shipment A", 0, consignALogs.Length);
				AssertEquals("Should match Consignment from Shipment B", 1, consignBLogs.Length);
			});
		}

		public void TestConsignmentEventMatching_MatchesSEAEventByShipmentHouseBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "CONSOLMBOL";

			var shipmentA = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			consol.Shipments.Add(shipmentA);
			shipmentA.JS_HouseBill = "SHB11111";

			var consignA = Factory.NewWithValidTestData<HVLVConsignment>();
			consignA.HVC_WaybillNumber = "CONSIGNHBOL";
			consignA.HVC_JS_ManifestedOnShipment = shipmentA.PK;

			var shipmentB = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			consol.Shipments.Add(shipmentB);
			shipmentB.JS_HouseBill = "SHB22222";

			var consignB = Factory.NewWithValidTestData<HVLVConsignment>();
			consignB.HVC_WaybillNumber = "CONSIGNHBOL";
			consignB.HVC_JS_ManifestedOnShipment = shipmentB.PK;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(GetXUE(("MBOLNumber", "CONSOLMBOL"), ("HBOLNumber", "CONSIGNHBOL"), ("MasterHouseBill", "SHB11111")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var consignALogs = consignA.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode));
			var consignBLogs = consignB.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode));

			CombineAssertions("Should have only matched Consignment from Shipment with matching MasterHouseBill", () =>
			{
				AssertEquals("EDI Message should have been processed successfully", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("Service Task Log should indicate a link to only one consignment", 1, Regex.Matches(serviceTaskLog.ToString(), $"Linked Event to HVLV Consignment").Count);
				AssertEquals("Should not match Consignment from Shipment A", 1, consignALogs.Length);
				AssertEquals("Should match Consignment from Shipment B", 0, consignBLogs.Length);
			});
		}

		void AssertConsignmentMatching(HVLVConsignment consignmentToMatch, params (string Type, string Value)[] contextValues)
		{
			var message = GetQueuedUniversalEventMessage(GetXUE(contextValues));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			void AssertLogsCount(HVLVConsignment consign, int expectedCount)
			{
				consign.Reload();
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode)
					.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsCancelled, false);
				var logs = consign.Logs.Find(query);
				AssertEquals(expectedCount, logs.Length);

				consign.Logs.CancelAll();
			}

			if (consignmentToMatch != null)
			{
				CombineAssertions(() =>
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertEquals($"Linked Event to HVLV Consignment {consignmentToMatch.HVC_WaybillNumber}.", serviceTaskLog.ToString());
					AssertLogsCount(consignmentToMatch, 1);
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
					AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
				});
			}
		}

		#endregion

		#region GetXUEs

		string BuildContextCollectionXML(IEnumerable<(string Type, string Value)> contextValues)
		{
			var root = new XElement("ContextCollection",
				contextValues.Select(context => new XElement("Context",
					new XElement("Type", context.Type),
					new XElement("Value", context.Value))));
			return root.ToString();
		}

		string GetXUEForACASStatusTest(string eventType, string reason = "SF", params (string Type, string Value)[] contextValues) => $@"
<UniversalEvent>
   <Event>
      <DataContext>
         <DocumentaryOverride>
            <DocumentName>HVLV ACAS Shipment Report</DocumentName>
         </DocumentaryOverride>
         <DataTargetCollection>
            <DataTarget>
               <Type>HVLVConsignment</Type>
            </DataTarget>
         </DataTargetCollection>
      </DataContext>
      <EventTime>2019-02-04T00:57:00</EventTime>
      <EventType>{eventType}</EventType>
      <EventParameters>
         <Department>Customs</Department>
         <MessageType>ACAS Shipment Report</MessageType>
         <Location>US</Location>
         <Reason>{reason}</Reason>
      </EventParameters>
      {BuildContextCollectionXML(contextValues)}
   </Event>
</UniversalEvent>";

		string GetXUEForStatusTest(string eventType, string eventReference, string dataSource = null, params (string Type, string Value)[] contextValues) => $@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>HVLVConsignment</Type>
				</DataTarget>
			</DataTargetCollection>
			<DataSourceCollection>
				<DataSource>
					<Type>{dataSource}</Type>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>DAU</Code>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
			</Company>
		</DataContext>
		<EventTime>2017-12-02T08:40:00</EventTime>
		<EventType>{eventType}</EventType>
		<EventReference>{eventReference}</EventReference>
		{BuildContextCollectionXML(contextValues)}
	</Event>
</UniversalEvent>";

		string GetXUE(params (string Type, string Value)[] contextValues) => $@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>HVLVConsignment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-12-02T08:40:00</EventTime>
		<EventType>ARV</EventType>
		<EventReference>Goods are here</EventReference>
		{BuildContextCollectionXML(contextValues)}
	</Event>
</UniversalEvent>";

		#endregion
	}
}
