using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class MAAEventProcessorTest : TransitUniversalTestCase
	{
		#region TestConstructor_LoggerNotNull

		public void TestConstructor_LoggerNotNull()
		{
			var universalEvent = new UniversalEvent();
			AssertExceptionThrown<ArgumentNullException>(() => new MAAEventProcessor(universalEvent, null));
		}

		#endregion

		#region TestConstructor_EventObjectNotNull

		public void TestConstructor_EventObjectNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new MAAEventProcessor(null, Logger));
		}

		#endregion

		#region TestProcessDispatchConsignment

		public void TestProcessDispatchConsignment_AddsNewPENReference_XMLVersion2012()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageAcceptedCode, customsReferenceNumber: "CRF123", referenceNumber: "RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));

			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);

			var portExportReference = dispatchConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertEquals("CRF123", portExportReference.CE_EntryNum);
				AssertEquals("", portExportReference.CE_EntryStatus);
				AssertEquals("RFN123", portExportReference.CE_EntryLineReference);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, portExportReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portExportReference.CE_Category);
			});
		}

		public void TestProcessDispatchConsignment_AddsNewPENReference_XMLVersion2011()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));

			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);

			var portExportReference = dispatchConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertEquals("CRF123", portExportReference.CE_EntryNum);
				AssertEquals("", portExportReference.CE_EntryStatus);
				AssertEquals("RFN123", portExportReference.CE_EntryLineReference);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, portExportReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portExportReference.CE_Category);
			});
		}

		public void TestProcessDispatchConsignment_UpdatesPENReference_EntryNumberExists()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));

			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(1, dispatchConsignment.PortReferences.Count);

			var existingPortAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, dispatchConsignment.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("CRF123", existingPortAuthorityReference.CE_EntryNum);
				AssertEquals("RFN123", existingPortAuthorityReference.CE_EntryLineReference);
				AssertEquals("", existingPortAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, existingPortAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, existingPortAuthorityReference.CE_Category);
			});

			processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			var portAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, dispatchConsignment.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals(existingPortAuthorityReference.PK, portAuthorityReference.PK);
				AssertEquals("CRF123", portAuthorityReference.CE_EntryNum);
				AssertEquals("RFN123", portAuthorityReference.CE_EntryLineReference);
				AssertEquals("", portAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, portAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, portAuthorityReference.CE_Category);
			});
		}

		public void TestProcessDispatchConsignment_DoesNotAddNewPENReference_CustomsReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=|RFN=RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));

			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(0, dispatchConsignment.PortReferences.Count);
		}

		public void TestProcessDispatchConsignment_DoesNotAddNewPENReference_ReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));

			var dispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			AssertEquals(0, dispatchConsignment.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(dispatchConsignment);

			AssertEquals(0, dispatchConsignment.PortReferences.Count);
		}

		#endregion

		#region TestProcessReceiveConsignmentAndPackage

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPANReference_XMLVersion2012_Export() => TestProcessReceiveConsignmentAndPackage_AddsNewPANReferenceCore(false, true);

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPANReference_XMLVersion2011_Export() => TestProcessReceiveConsignmentAndPackage_AddsNewPANReferenceCore(true, true);

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPANReference_XMLVersion2012_Import() => TestProcessReceiveConsignmentAndPackage_AddsNewPANReferenceCore(false, false);

		public void TestProcessReceiveConsignmentAndPackage_AddsNewPANReference_XMLVersion2011_import() => TestProcessReceiveConsignmentAndPackage_AddsNewPANReferenceCore(true, false);

		void TestProcessReceiveConsignmentAndPackage_AddsNewPANReferenceCore(bool isXMLVersion2011, bool isExport)
		{
			var documentName = isExport ? "Goods Received(CRESA)" : "Goods Received";
			var xmlEvent = isXMLVersion2011 ? EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: string.Format("|CRF={0}|RFN=RFN123", "BBE123"), dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", documentName: documentName))
				: EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageAcceptedCode, customsReferenceNumber: "BBE123", referenceNumber: "RFN123", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", documentName: documentName));

			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", null, null);
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.PortReferences.Count);
			AssertEquals(1, package.PortReferences.Count);

			var expectedPortReferenceType = isExport ? TransitWarehousePortReferenceTypes.Codes.PortExport : TransitWarehousePortReferenceTypes.Codes.PortAuthority;
			var rcnPortReference = receiveConsignment.PortReferences.First(expectedPortReferenceType);
			var packagePortReference = package.PortReferences.First(expectedPortReferenceType);
			CombineAssertions(() =>
			{
				AssertEquals("BBE123", rcnPortReference.CE_EntryNum);
				AssertEquals("", rcnPortReference.CE_EntryStatus);
				AssertEquals("RFN123", rcnPortReference.CE_EntryLineReference);
				AssertEquals(expectedPortReferenceType, rcnPortReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, rcnPortReference.CE_Category);

				AssertEquals("BBE123", packagePortReference.CE_EntryNum);
				AssertEquals("", packagePortReference.CE_EntryStatus);
				AssertEquals("RFN123", packagePortReference.CE_EntryLineReference);
				AssertEquals(expectedPortReferenceType, packagePortReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, packagePortReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_UpdatesPANReference_EntryNumberExists()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=RFN123", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001"));

			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", null, null);
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);

			var messageAcceptedProcessor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			messageAcceptedProcessor.Process(receiveConsignment);
			messageAcceptedProcessor.Process(package);

			AssertEquals(1, receiveConsignment.PortReferences.Count);
			AssertEquals(1, package.PortReferences.Count);

			var existingPortAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var existingPackagePortAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("CRF123", existingPortAuthorityReference.CE_EntryNum);
				AssertEquals("RFN123", existingPortAuthorityReference.CE_EntryLineReference);
				AssertEquals("", existingPortAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, existingPortAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, existingPortAuthorityReference.CE_Category);

				AssertEquals("CRF123", existingPackagePortAuthorityReference.CE_EntryNum);
				AssertEquals("RFN123", existingPackagePortAuthorityReference.CE_EntryLineReference);
				AssertEquals("", existingPackagePortAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, existingPackagePortAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, existingPackagePortAuthorityReference.CE_Category);
			});

			messageAcceptedProcessor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			messageAcceptedProcessor.Process(receiveConsignment);
			messageAcceptedProcessor.Process(package);

			var rcnPortAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var packagePortAuthorityReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals(existingPortAuthorityReference.PK, rcnPortAuthorityReference.PK);
				AssertEquals("CRF123", rcnPortAuthorityReference.CE_EntryNum);
				AssertEquals("RFN123", rcnPortAuthorityReference.CE_EntryLineReference);
				AssertEquals("", rcnPortAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, rcnPortAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, rcnPortAuthorityReference.CE_Category);

				AssertEquals(existingPackagePortAuthorityReference.PK, packagePortAuthorityReference.PK);
				AssertEquals("CRF123", packagePortAuthorityReference.CE_EntryNum);
				AssertEquals("RFN123", packagePortAuthorityReference.CE_EntryLineReference);
				AssertEquals("", packagePortAuthorityReference.CE_EntryStatus);
				AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, packagePortAuthorityReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, packagePortAuthorityReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_DoesNotAddNewPANReference_CustomsReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=|RFN=RFN123", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001"));

			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", null, null);
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);
		}

		public void TestProcessReceiveConsignmentAndPackage_DoesNotAddNewPANReference_ReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001"));

			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", null, null);
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.PortReferences.Count);
			AssertEquals(0, package.PortReferences.Count);

			var processor = new MAAEventProcessor((UniversalEvent)xmlEvent, Logger);
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
