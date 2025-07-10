using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EDICommunicationsModeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUniversalXMLFileFormatsAvailableInModulesSupportingUniversalDatatransfer()
		{
			CombineAssertions(delegate
			{
				AssertAllowsUniversal(JobInvoicingConsumerTypes.Consol.Code, SupportsUniversal.EventAndShipment);
				AssertAllowsUniversal(JobInvoicingConsumerTypes.Shipment.Code, SupportsUniversal.EventAndShipment);
				AssertAllowsUniversal(JobInvoicingConsumerTypes.Brokerage.Code, SupportsUniversal.EventAndShipment);
				AssertAllowsUniversal(WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, SupportsUniversal.EventAndShipment);
				AssertAllowsUniversal(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode, SupportsUniversal.EventAndShipment);
				AssertAllowsUniversal(WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode, SupportsUniversal.EventAndShipment);
				AssertAllowsUniversal(WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode, SupportsUniversal.None);
				AssertAllowsUniversal(WorkflowDescriptors.ARInvoiceCode, SupportsUniversal.EventAndTransaction);
				AssertAllowsUniversal(WorkflowDescriptors.APInvoiceCode, SupportsUniversal.EventAndTransaction);
				AssertAllowsUniversal(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, SupportsUniversal.Activity | SupportsUniversal.Event);
				AssertAllowsUniversal(WorkflowDescriptors.SailingScheduleWorkflowDescriptorCode, SupportsUniversal.EventAndShipment | SupportsUniversal.Schedule);
				AssertAllowsUniversal(EDICommunicationsMode.Modules.Netting, SupportsUniversal.Transaction);
				AssertAllowsUniversal(EDICommunicationsMode.Modules.CreditControlledDocumentApproval, SupportsUniversal.Shipment);
			});
		}

		void AssertAllowsUniversal(string moduleCode, SupportsUniversal supports)
		{
			Mode.EK_ParentID = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Mode.EK_Module = moduleCode;
			string description = Mode.Lookups.ModuleList.GetDescriptionFromCode(moduleCode);
			AssertEquals(moduleCode + " (" + description + ") allows XmlUniversalEvent"
				, supports.HasFlag(SupportsUniversal.Event)
				, Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent));
			AssertEquals(moduleCode + " (" + description + ") allows XmlUniversalShipment"
				, supports.HasFlag(SupportsUniversal.Shipment)
				, Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment));
			AssertEquals(moduleCode + " (" + description + ") allows XmlUniversalTransaction"
				, supports.HasFlag(SupportsUniversal.Transaction)
				, Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction));
			AssertEquals(moduleCode + " (" + description + ") allows XmlUniversalSchedule"
				, supports.HasFlag(SupportsUniversal.Schedule)
				, Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule));
			AssertEquals(moduleCode + " (" + description + ") allows XmlUniversalTransactionBatch"
				, supports.HasFlag(SupportsUniversal.TransactionBatch)
				, Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransactionBatch));
			AssertEquals(moduleCode + " (" + description + ") allows XmlUniversalActivity"
				, supports.HasFlag(SupportsUniversal.Activity)
				, Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XmlUniversalActivity));
		}

		[Flags]
		enum SupportsUniversal { None = 0, Event = 1, Shipment = 2, Transaction = 4, EventAndShipment = 3, EventAndTransaction = 5, Schedule = 8, TransactionBatch = 16, Activity = 32 }

		public void TestModuleList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			list.AddRange(ObjectFactory.Get<IWorkflowDescriptorList>());
			list.AddPair(EDICommunicationsMode.Modules.ClientSpecific, "Client");

			var moduleList = Mode.Lookups.ModuleList;
			foreach (CodeDescriptionPair pair in list)
			{
				Assert("Module list should contain " + pair.Code, moduleList.ContainsCode(pair.Code));
			}

			Assert(moduleList.ContainsCode(EDICommunicationsMode.Modules.US_BIRD));
			Assert(moduleList.ContainsCode(EDICommunicationsMode.Modules.ContainerMovements));
			Assert(!moduleList.ContainsCode(EDICommunicationsMode.Modules.Netting));
			Assert(!moduleList.ContainsCode(EDICommunicationsMode.Modules.CreditControlledDocumentApproval));
			Assert(!moduleList.ContainsCode(EDICommunicationsMode.Modules.GlobalElectronicInvoicing));
		}

		public void TestModuleList_WithConditionalModules()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var moduleList = Mode.Lookups.ModuleList;

			Assert(moduleList.ContainsCode(EDICommunicationsMode.Modules.US_BIRD));
			Assert(moduleList.ContainsCode(EDICommunicationsMode.Modules.ContainerMovements));
			Assert(moduleList.ContainsCode(EDICommunicationsMode.Modules.Netting));
			Assert(moduleList.ContainsCode(EDICommunicationsMode.Modules.CreditControlledDocumentApproval));
			Assert(moduleList.ContainsCode(EDICommunicationsMode.Modules.GlobalElectronicInvoicing));
		}

		public void TestPartiesListHasAFixedActiveFilter()
		{
			var collection = Mode.Lookups.Parties;

			AssertEquals(collection.FilterBusinessObjectDefaults.Count, 1);

			AssertEquals(collection.FilterBusinessObjectDefaults["Active Status: Outbound:Property"].FilterName, "Active Status: Outbound");
			AssertEquals(collection.FilterBusinessObjectDefaults["Active Status: Outbound:Property"].Value, "Active");
			AssertEquals(collection.FilterBusinessObjectDefaults["Active Status: Outbound:Property"].IsRemovable, false);
			AssertEquals(collection.FilterBusinessObjectDefaults["Active Status: Outbound:Property"].PropertyName, "Property");
		}

		public void TestFileFormatList_FHLFWBFileFormat()
		{
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL)));
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB)));

			Mode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL)));
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB)));

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL)));
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB)));

			Mode.EK_ParentID = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Mode.EK_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			Mode.Organisation.OH_IsShippingProvider = true;
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL)));
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB)));

			Mode.Organisation.OH_IsAirLine = true;
			Assert(Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL)));
			Assert(Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB)));
		}

		public void TestEAdaptorFormatListInvalidLicence()
		{
			Mode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Mode.EK_ParentID = org.PK;
			Mode.Organisation.OH_IsAirLine = true;
			Mode.Organisation.OH_IsShippingProvider = true;

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL)));
			Assert(!Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB)));
		}

		public void TestEAdaptorFormatListValidLicence()
		{
			Mode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Mode.EK_ParentID = org.PK;
			Mode.Organisation.OH_IsAirLine = true;
			Mode.Organisation.OH_IsShippingProvider = true;

			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			Assert(Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL)));
			Assert(Mode.Lookups.FileFormatList.Contains(new CodeDescriptionPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB)));
			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		public void TestFileFormatList()
		{
			var fileFormatList = Mode.Lookups.FileFormatList;
			AssertEquals("FileFormatList should contain XML", true, fileFormatList.ContainsCode("XML"));
			AssertEquals("FileFormatList should contain SFF", true, fileFormatList.ContainsCode("SFF"));
			AssertEquals("FileFormatList should contain NTF", true, fileFormatList.ContainsCode("NTF"));
			AssertEquals("FileFormatList should contain EXL", true, fileFormatList.ContainsCode("EXL"));
			AssertEquals("FileFormatList should contain ALL", true, fileFormatList.ContainsCode("ALL"));
		}

		public void TestFileFormatList_ForWarehouseOrderModule()
		{
			Mode.EK_Module = WorkflowDescriptors.WhsOrderWorkflowDescriptorCode;
			var fileFormatList = Mode.Lookups.FileFormatList;
			AssertEquals("FileFormatList should contain XML", true, fileFormatList.ContainsCode("XML"));
			AssertEquals("FileFormatList should contain SFF", true, fileFormatList.ContainsCode("SFF"));
			AssertEquals("FileFormatList should contain NTF", true, fileFormatList.ContainsCode("NTF"));
			AssertEquals("FileFormatList should contain ALL", true, fileFormatList.ContainsCode("ALL"));
			AssertEquals("FileFormatList should contain IFS", true, fileFormatList.ContainsCode("XIF"));
		}

		public void TestFileFormatList_ForDeclarationModule()
		{
			Mode.EK_Module = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode;
			var fileFormatList = Mode.Lookups.FileFormatList;
			AssertEquals("FileFormatList should contain XML", true, fileFormatList.ContainsCode("XML"));
			AssertEquals("FileFormatList should contain SFF", true, fileFormatList.ContainsCode("SFF"));
			AssertEquals("FileFormatList should contain NTF", true, fileFormatList.ContainsCode("NTF"));
			AssertEquals("FileFormatList should contain ALL", true, fileFormatList.ContainsCode("ALL"));
			AssertEquals("FileFormatList should contain XMB", true, fileFormatList.ContainsCode("XMB"));
		}

		public void TestFileFormatList_ForOrderModule()
		{
			Mode.EK_Module = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			var fileFormatList = Mode.Lookups.FileFormatList;
			var pos = -1; // position is significant			
			AssertEquals("FileFormatList should contain XML", "XML", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain SFF", "SFF", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain NTF", "NTF", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain NBT", "NBT", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain XML With eDoc (EXL)", "EXL", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain OrderImportChanges (OCI)", "OCI", fileFormatList[++pos].Code);
		}

		public void TestFileFormatList_ForShipmentModule()
		{
			Mode.EK_Module = JobInvoicingConsumerTypes.Shipment.Code;
			var fileFormatList = Mode.Lookups.FileFormatList;
			var pos = -1; // position is significant			
			AssertEquals("FileFormatList should contain XML", "XML", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain SFF", "SFF", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain NTF", "NTF", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain NBT", "NBT", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain EXL", "EXL", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain DXL", "DXL", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain FXL", "FXL", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain XMB", "XMB", fileFormatList[++pos].Code);
		}

		public void TestFileFormatList_ForConsolModule()
		{
			Mode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			var fileFormatList = Mode.Lookups.FileFormatList;
			int pos = -1; // position is significant
			AssertEquals("FileFormatList should contain XML", "XML", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain SFF", "SFF", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain NTF", "NTF", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain NBT", "NBT", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain EXL", "EXL", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain DXL", "DXL", fileFormatList[++pos].Code);
			AssertEquals("FileFormatList should contain FXL", "FXL", fileFormatList[++pos].Code);
		}

		public void TestFileFormatList_ForContainerMovementModule()
		{
			Mode.EK_Module = EDICommunicationsMode.Modules.ContainerMovements;
			var fileFormatList = Mode.Lookups.FileFormatList;
			AssertEquals("Count", 1, fileFormatList.Count);
			AssertEquals("FileFormatList should only contain XUE", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, fileFormatList[0].Code);
		}

		public void TestCommunicationsTransportListContent()
		{
			EDICommunicationsModeLookupForTest lookups = (EDICommunicationsModeLookupForTest)Mode.Lookups;
			var communicationsTransportList = lookups.CommunicationsTransportList;

			lookups.SetAllowInterfaceConnectorForTest(false);

			AssertEquals("CommunicationsTransportList should contain EMA", "EMA", communicationsTransportList[0].Code);
			AssertEquals("CommunicationsTransportList should contain EMT", "EMT", communicationsTransportList[1].Code);
			AssertEquals("CommunicationsTransportList should contain NXC", EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector, communicationsTransportList[2].Code);
			AssertEquals("CommunicationsTransportList should contain NXC", EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, communicationsTransportList[3].Code);

			lookups.SetAllowInterfaceConnectorForTest(true);
			mode.resetLookup();
			communicationsTransportList = lookups.CommunicationsTransportList;

			AssertEquals("CommunicationsTransportList should contain FIL", "FIL", communicationsTransportList[0].Code);
			AssertEquals("CommunicationsTransportList should contain EMA", "EMA", communicationsTransportList[1].Code);
			AssertEquals("CommunicationsTransportList should contain EMT", "EMT", communicationsTransportList[2].Code);
			AssertEquals("CommunicationsTransportList should contain NXC", EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector, communicationsTransportList[3].Code);
			AssertEquals("CommunicationsTransportList should contain NXC", EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, communicationsTransportList[4].Code);
			AssertEquals("CommunicationsTransportList should contain XTT", EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, communicationsTransportList[5].Code);
			AssertEquals("CommunicationsTransportList should contain FTP", "FTP", communicationsTransportList[6].Code);
		}

		public void TestCommunicationsTransportListContentForContainerMovements()
		{
			AssertOnlyEHubAsCommunicationTransport(EDICommunicationsMode.Modules.ContainerMovements);
		}

		public void TestCommunicationsTransportListContentForNettingSystem()
		{
			AssertOnlyEHubAsCommunicationTransport(EDICommunicationsMode.Modules.Netting);
		}

		public void TestCommunicationsTransportListContentForCreditControlledDocumentApproval()
		{
			Mode.EK_ParentID = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Mode.EK_Module = EDICommunicationsMode.Modules.CreditControlledDocumentApproval;
			var communicationsTransportList = Mode.Lookups.CommunicationsTransportList;
			AssertEquals("Count", 2, communicationsTransportList.Count);
			AssertEquals("CMM supports EAdaptorInterface.", EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, communicationsTransportList[0].Code);
			AssertEquals("CMM supports eHub.", EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, communicationsTransportList[1].Code);
		}

		void AssertOnlyEHubAsCommunicationTransport(string moduleCode)
		{
			Mode.EK_ParentID = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Mode.EK_Module = moduleCode;
			var communicationsTransportList = Mode.Lookups.CommunicationsTransportList;
			AssertEquals("Count", 1, communicationsTransportList.Count);
			AssertEquals("CMM supports eHub only.", EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, communicationsTransportList[0].Code);
		}

		public void TestCommunicationsTransportListContent_ForEHub()
		{
			var communicationsTransportList = Mode.Lookups.CommunicationsTransportList;
			Assert("CommunicationsTransportList should contain HUB", communicationsTransportList.ContainsCode("HUB"));
		}

		public void TestCommsDirectionListContent()
		{
			var commsDirectionList = Mode.Lookups.CommsDirectionList;
			AssertEquals("CommsDirectionList should contain TRX", "TRX", commsDirectionList[0].Code);
			AssertEquals("CommsDirectionList should contain RCV", "RCV", commsDirectionList[1].Code);
		}

		public void TestModeForGEIModule()
		{
			Mode.EK_Module = "GEI";
			AssertContainsExactElementsInAnyOrder(new[] { "TRX" }, Mode.Lookups.CommsDirectionList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new[] { "XML" }, Mode.Lookups.FileFormatList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new[] { "HUB", "EDP" }, Mode.Lookups.CommunicationsTransportList.GetAllCodes());
		}

		public void TestEK_TransportMode()
		{
			AssertCollectionContains("AIR", Mode.Lookups.TransportModes.GetAllCodes());
		}

		public void TestEK_RecipientRole()
		{
			AssertCollectionContains("ORP", Mode.Lookups.RecipientRoles.GetAllCodes());
		}

		public void TestEK_EventCode()
		{
			AssertCollectionContains(Events.CustomisableEvent01Code, Mode.Lookups.Events.GetAllCodes());
		}

		public void TestEventReferenceType()
		{
			AssertCollectionContains(EventReferenceConditionList.Codes.EventReference, Mode.Lookups.EventReferenceTypes.GetAllCodes());
			AssertCollectionNotContains(EventReferenceConditionList.Codes.ConditionWithMacros, Mode.Lookups.EventReferenceTypes.GetAllCodes());
			AssertCollectionNotContains(EventReferenceConditionList.Codes.UserDefined, Mode.Lookups.EventReferenceTypes.GetAllCodes());
		}

		EDICommunicationsModeForTest Mode
		{
			get { return mode ?? (mode = Factory.New<EDICommunicationsModeForTest>()); }
		}
		EDICommunicationsModeForTest mode;

		sealed class EDICommunicationsModeForTest : EDICommunicationsMode
		{
			public EDICommunicationsModeForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override EDICommunicationsModeLookups GetNewLookups()
			{
				return new EDICommunicationsModeLookupForTest(this);
			}

			public void resetLookup()
			{
				GetNewLookups();
			}
		}

		sealed class EDICommunicationsModeLookupForTest : EDICommunicationsModeLookups
		{
			public EDICommunicationsModeLookupForTest(AutoEDICommunicationsMode parent)
				: base(parent)
			{ }

			protected override bool AllowInterfaceConnector
			{
				get { return allowInterfaceConnector; }
			}

			bool allowInterfaceConnector;

			public void SetAllowInterfaceConnectorForTest(bool newValue)
			{
				allowInterfaceConnector = newValue;
			}
		}
	}
}
