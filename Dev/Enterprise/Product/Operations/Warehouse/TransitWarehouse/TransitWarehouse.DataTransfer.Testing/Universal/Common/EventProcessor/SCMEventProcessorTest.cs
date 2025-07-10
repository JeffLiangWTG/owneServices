using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class SCMEventProcessorTest : TransitUniversalTestCase
	{
		#region TestConstructor_EventObjectNotNull

		public void TestConstructor_EventObjectNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new SCMEventProcessor(null, Logger));
		}

		#endregion

		#region TestProcessDispatchConsignment

		public void TestProcessDispatchConsignment_AddsNewPENReference_XMLVersion2012()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			dispatchConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			AssertEquals(0, dispatchConsignment.PortReferences.Count);
			AssertEquals(false, dispatchConsignment.WDC_CompleteTime.IsValid);

			var processor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);
			AssertEquals(false, dispatchConsignment.WDC_CompleteTime.IsValid);

			var portAuthorityReference = dispatchConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", portAuthorityReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, portAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portAuthorityReference.CE_Category);
			});
		}

		public void TestProcessDispatchConsignment_AddsNewPENReference_WarehouseIsPortAuthorityControlled()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			dispatchConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Data.Warehouse.WW_IsPortAuthorityControlled = true;
			Data.Warehouse.WW_IsCustomsControlled = true;

			AssertEquals(0, dispatchConsignment.PortReferences.Count);
			AssertEquals(false, dispatchConsignment.WDC_CompleteTime.IsValid);

			var processor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);
			AssertEquals(true, dispatchConsignment.WDC_CompleteTime.IsValid);

			var portAuthorityReference = dispatchConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", portAuthorityReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, portAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portAuthorityReference.CE_Category);
			});
		}

		public void TestProcessDispatchConsignment_AddsNewPENReference_XMLVersion2011()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "|CRF=CR123|DEP=Customs|LOC=AUBNE"));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			dispatchConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var processor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);

			var portAuthorityReference = dispatchConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", portAuthorityReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, portAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portAuthorityReference.CE_Category);
			});
		}

		public void TestProcessDispatchConsignment_UpdatesPENReference_EntryNumberExists()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			dispatchConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var clearanceHoldProcessor = new SHLEventProcessor((UniversalEvent)xmlEvent, Logger);
			clearanceHoldProcessor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);

			var existingPortAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, dispatchConsignment.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("CR123", existingPortAuthorityReference.CE_EntryNum);
				AssertEquals("", existingPortAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, existingPortAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, existingPortAuthorityReference.CE_Category);
			});

			var clearanceCompletedProcessor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			clearanceCompletedProcessor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);

			var portAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, dispatchConsignment.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals(existingPortAuthorityReference.PK, portAuthorityReference.PK);
				AssertEquals("CR123", portAuthorityReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, portAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, portAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portAuthorityReference.CE_Category);
			});
		}

		public void TestProcessDispatchConsignment_DoesNotAddNewPENReference_CustomsReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			dispatchConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var processor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(0, dispatchConsignment.PortReferences.Count);
		}

		#endregion

		#region TestProcessReceiveConsignmentAndPackage

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPortReference_XMLVersion2012_Export() => TestProcessReceiveConsignmentAndPackage_AddsNewPortReferenceCore(false, true);

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPortReference_XMLVersion2012_Import() => TestProcessReceiveConsignmentAndPackage_AddsNewPortReferenceCore(false, false);

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPortReference_XMLVersion2011_Export() => TestProcessReceiveConsignmentAndPackage_AddsNewPortReferenceCore(true, true);

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPortReference_XMLVersion2011_Import() => TestProcessReceiveConsignmentAndPackage_AddsNewPortReferenceCore(true, false);

		void TestProcessReceiveConsignmentAndPackage_AddsNewPortReferenceCore(bool isXMLVersion2011, bool isExport)
		{
			var documentName = isExport ? "Goods Received(CRESA)" : "Goods Received";
			var customsReferenceNumber = "BBE123";
			var xmlEvent = isXMLVersion2011 ? EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", string.Format("|CRF={0}|DEP=Customs|LOC=AUBNE", customsReferenceNumber.ToLower()), documentName: documentName))
				: EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", customsReferenceNumber, "AUBNE", "", documentName: documentName));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);
			AssertEquals(false, receiveConsignment.WRC_CompleteTime.IsValid);

			var processor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.PortReferences.Count);
			AssertEquals(1, package.PortReferences.Count);
			AssertEquals(false, receiveConsignment.WRC_CompleteTime.IsValid);

			var expectedPortReferenceType = isExport ? TransitWarehousePortReferenceTypes.Codes.PortExport : TransitWarehousePortReferenceTypes.Codes.PortAuthority;
			var rcnPortReference = receiveConsignment.PortReferences.First(expectedPortReferenceType);
			var packagePortReference = package.PortReferences.First(expectedPortReferenceType);
			CombineAssertions(() =>
			{
				AssertEquals(customsReferenceNumber, rcnPortReference.CE_EntryNum.ToUpper());
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, rcnPortReference.CE_EntryStatus);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, rcnPortReference.CE_Category);

				AssertEquals(customsReferenceNumber, packagePortReference.CE_EntryNum.ToUpper());
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, packagePortReference.CE_EntryStatus);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, packagePortReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignment_AddsNewPANReference_WarehouseIsPortAuthorityControlled()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", "", documentName: "Goods Received"));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Data.Warehouse.WW_IsPortAuthorityControlled = true;
			Data.Warehouse.WW_IsCustomsControlled = true;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(false, receiveConsignment.WRC_CompleteTime.IsValid);

			var processor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(receiveConsignment);

			AssertEquals(1, receiveConsignment.PortReferences.Count);
			AssertEquals(true, receiveConsignment.WRC_CompleteTime.IsValid);

			var portAuthorityReference = receiveConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", portAuthorityReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, portAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portAuthorityReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_EntryNumberExists_Export() => TestProcessReceiveConsignmentAndPackage_EntryNumberExistsCore(true);

		public void TestProcessReceiveConsignmentAndPackage_EntryNumberExists_Import() => TestProcessReceiveConsignmentAndPackage_EntryNumberExistsCore(false);

		void TestProcessReceiveConsignmentAndPackage_EntryNumberExistsCore(bool isExport)
		{
			var documentName = isExport ? "Goods Received(CRESA)" : "Goods Received";
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", "", "RFN123", documentName: documentName));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);
			AssertEquals(false, receiveConsignment.WRC_CompleteTime.IsValid);

			var maaProcessor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			maaProcessor.Process(receiveConsignment);
			maaProcessor.Process(package);

			AssertEquals(1, receiveConsignment.PortReferences.Count);
			AssertEquals(1, package.PortReferences.Count);
			AssertEquals(false, receiveConsignment.WRC_CompleteTime.IsValid);

			var expectedPortReferenceType = isExport ? TransitWarehousePortReferenceTypes.Codes.PortExport : TransitWarehousePortReferenceTypes.Codes.PortAuthority;
			var existingRCNPortReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var existingPackagePortReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("CR123", existingRCNPortReference.CE_EntryNum);
				AssertEquals(string.Empty, existingRCNPortReference.CE_EntryStatus);
				AssertEquals(expectedPortReferenceType, existingRCNPortReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, existingRCNPortReference.CE_Category);

				AssertEquals("CR123", existingPackagePortReference.CE_EntryNum);
				AssertEquals(string.Empty, existingPackagePortReference.CE_EntryStatus);
				AssertEquals(expectedPortReferenceType, existingPackagePortReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, existingPackagePortReference.CE_Category);
			});

			var clearanceCompletedProcessor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			clearanceCompletedProcessor.Process(receiveConsignment);
			clearanceCompletedProcessor.Process(package);

			AssertEquals(1, receiveConsignment.PortReferences.Count);
			AssertEquals(1, package.PortReferences.Count);

			var rcnPortReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var packagePortReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals(existingRCNPortReference.PK, rcnPortReference.PK);
				AssertEquals("CR123", rcnPortReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, rcnPortReference.CE_EntryStatus);
				AssertEquals(expectedPortReferenceType, rcnPortReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, rcnPortReference.CE_Category);

				AssertEquals(existingPackagePortReference.PK, packagePortReference.PK);
				AssertEquals("CR123", packagePortReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, packagePortReference.CE_EntryStatus);
				AssertEquals(expectedPortReferenceType, packagePortReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, packagePortReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_DoesNotAddNewPANReference_CustomsReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);

			var processor = new SCMEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);
		}

		#endregion

		#region Implementation

		XmlEventDeserializer EventDeserializer => eventDeserializer ?? (eventDeserializer = new XmlEventDeserializer());
		XmlEventDeserializer eventDeserializer;

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(factory));
		WhsTransitTestHelper helper;

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(factory);

		BusinessObjectFactory factory => Factory.BOFactory;

		#endregion
	}
}
