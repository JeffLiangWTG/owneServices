using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class CRNEventProcessorTest : TransitUniversalTestCase
	{
		#region TestConstructor_FactoryNotNull

		public void TestConstructor_FactoryNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CRNEventProcessor(new UniversalEvent(), Logger, null));
		}

		#endregion

		#region TestConstructor_LoggerNotNull

		public void TestConstructor_LoggerNotNull()
		{
			var universalEvent = new UniversalEvent();
			AssertExceptionThrown<ArgumentNullException>(() => new CRNEventProcessor(universalEvent, null, Factory));
		}

		#endregion

		#region TestConstructor_EventObjectNotNull

		public void TestConstructor_EventObjectNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CRNEventProcessor(null, Logger, Factory));
		}

		#endregion

		#region TestProcessReceiveConsignmentAndPackage

		public void TestProcessReceiveConsignmentAndPackage_AddsNewCRNReference_XMLVersion2012()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CRNEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var rcnCustomsReleaseNumberEnteredReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var packageCustomsReleaseNumberEnteredReference = package.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", rcnCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReleaseNumberEnteredReference.CE_Category);

				AssertEquals("CR123", packageCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsReleaseNumberEnteredReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_AddsNewCRNReference__XMLVersion2011()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "|CRF=CR123|DEP=Customs|LOC=AUBNE"));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CRNEventProcessor((UniversalEvent)xmlEvent, Logger,	Factory);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var rcnCustomsReleaseNumberEnteredReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var packageCustomsReleaseNumberEnteredReference = package.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", rcnCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReleaseNumberEnteredReference.CE_Category);

				AssertEquals("CR123", packageCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsReleaseNumberEnteredReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_DoesNotAddNewCRNReference_EntryNumberExists()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CRNEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var existingRcnCustomsReleaseNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var existingPackageCustomsReleaseNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("CR123", existingRcnCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, existingRcnCustomsReleaseNumberEnteredReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, existingRcnCustomsReleaseNumberEnteredReference.CE_Category);

				AssertEquals("CR123", existingPackageCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, existingPackageCustomsReleaseNumberEnteredReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, existingPackageCustomsReleaseNumberEnteredReference.CE_Category);
			});

			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var rcnCustomsReleaseNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var packageCustomsReleaseNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals(existingRcnCustomsReleaseNumberEnteredReference.PK, rcnCustomsReleaseNumberEnteredReference.PK);
				AssertEquals("CR123", rcnCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, rcnCustomsReleaseNumberEnteredReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReleaseNumberEnteredReference.CE_Category);

				AssertEquals(existingPackageCustomsReleaseNumberEnteredReference.PK, packageCustomsReleaseNumberEnteredReference.PK);
				AssertEquals("CR123", packageCustomsReleaseNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, packageCustomsReleaseNumberEnteredReference.CE_EntryType);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsReleaseNumberEnteredReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_DoesNotAddNewCRNReference_CustomsReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CRNEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);
		}

		public void TestMatchesReceiveConsignmentsByWayBillNumber()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRLIO";

			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var additionalReference = helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var packages = helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.MasterAirWayBillEventXML("CRN", "IMP", "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30"));
			var processor = new CRNEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(rcn);

			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have two addOnValue", 2, addOnValues.Length);
			AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "30", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);
		}

		public void TestMatchesReceiveConsignmentsByWayBillNumber_Update()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRLIO";

			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var additionalReference = helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var packages = helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked);

			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWOuterPackQty", "INT", "1");
			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWInnerPackQty", "INT", "10");
			Factory.SaveForTesting();

			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have two addOnValue", 2, addOnValues.Length);
			AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "10", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.MasterAirWayBillEventXML("CRN", "IMP", "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30"));
			var processor = new CRNEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(rcn);

			addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "30", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);
		}

		public void TestMatchesReceiveConsignmentsByWayBillNumber_NoInners_ShouldNotUpdateInner()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRLIO";

			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var additionalReference = helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var packages = helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked);

			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWOuterPackQty", "INT", "1");
			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWInnerPackQty", "INT", "10");
			Factory.SaveForTesting();

			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have two addOnValue", 2, addOnValues.Length);
			AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "10", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.MasterAirWayBillEventXML("CRN", "IMP", "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=2|IPQ=0"));
			var processor = new CRNEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(rcn);

			addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have one addOnValue for outer packages", "2", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "0", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);
		}

		#endregion

		#region Implementation

		XmlEventDeserializer EventDeserializer => eventDeserializer ?? (eventDeserializer = new XmlEventDeserializer());
		XmlEventDeserializer eventDeserializer;

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(factory));
		WhsTransitTestHelper helper;

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(factory);

		WhsTransitAdditionalReferencesHelper additionalReferencesHelper => new WhsTransitAdditionalReferencesHelper(Logger, Factory);

		BusinessObjectFactory factory => Factory.BOFactory;

		#endregion
	}
}
