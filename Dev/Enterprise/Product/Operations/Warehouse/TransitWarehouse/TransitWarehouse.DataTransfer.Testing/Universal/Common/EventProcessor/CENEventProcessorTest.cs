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
	class CENEventProcessorTest : TransitUniversalTestCase
	{
		#region TestConstructor_FactoryNotNull

		public void TestConstructor_FactoryNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CENEventProcessor(new UniversalEvent(), Logger, null));
		}

		#endregion

		#region TestConstructor_LoggerNotNull

		public void TestConstructor_LoggerNotNull()
		{
			var universalEvent = new UniversalEvent();
			AssertExceptionThrown<ArgumentNullException>(() => new CENEventProcessor(universalEvent, null, Factory));
		}

		#endregion

		#region TestConstructor_EventObjectNotNull

		public void TestConstructor_EventObjectNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CENEventProcessor(null, Logger, Factory));
		}

		#endregion

		#region TestProcessReceiveConsignmentAndPackage

		public void TestProcessReceiveConsignmentAndPackage_AddsNewCENReference_XMLVersion2012()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CENEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var rcnCustomsNumberEnteredReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var packageCustomsNumberEnteredReference = package.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", rcnCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsNumberEnteredReference.CE_Category);

				AssertEquals("CR123", packageCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsNumberEnteredReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_AddsNewCENReference_XMLVersion2011()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "|CRF=CR123|DEP=Customs|LOC=AUBNE"));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CENEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var rcnCustomsNumberEnteredReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var packageCustomsNumberEnteredReference = package.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			CombineAssertions(() =>
			{
				AssertEquals("CR123", rcnCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsNumberEnteredReference.CE_Category);

				AssertEquals("CR123", packageCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsNumberEnteredReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_DoesNotAddNewCENReference_EntryNumberExists()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "CR123", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CENEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var existingRCNCustomsNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var existingPackageCustomsNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("CR123", existingRCNCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, existingRCNCustomsNumberEnteredReference.CE_Category);

				AssertEquals("CR123", existingPackageCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, existingPackageCustomsNumberEnteredReference.CE_Category);
			});

			processor.Process(receiveConsignment);
			processor.Process(package);

			AssertEquals(1, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(1, package.CustomsReferenceNumbers.Count);

			var rcnCustomsNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveConsignment.PK)).Single();
			var packageCustomsNumberEnteredReference = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, package.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals(existingRCNCustomsNumberEnteredReference.PK, rcnCustomsNumberEnteredReference.PK);
				AssertEquals("CR123", rcnCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsNumberEnteredReference.CE_Category);

				AssertEquals(existingPackageCustomsNumberEnteredReference.PK, packageCustomsNumberEnteredReference.PK);
				AssertEquals("CR123", packageCustomsNumberEnteredReference.CE_EntryNum);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsNumberEnteredReference.CE_Category);
			});
		}

		public void TestProcessReceiveConsignmentAndPackage_DoesNotAddNewCENReference_CustomsReferenceNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "Shipment123", "Customs", "", "", "AUBNE", ""));

			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var receiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001", forwardingShipment.PK, forwardingShipment.TablePrefix);
			receiveConsignment.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var package = helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked).Package;

			AssertEquals(0, receiveConsignment.CustomsReferenceNumbers.Count);
			AssertEquals(0, package.CustomsReferenceNumbers.Count);

			var processor = new CENEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
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
			var additionalReference = helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var package = helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.PackLine, "", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.MasterAirWayBillEventXML("CEN", "IMP", "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30"));
			var processor = new CENEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
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
			var additionalReference = helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var package = helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.PackLine, "", TransitWarehouseStatuses.Codes.Booked);

			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWOuterPackQty", "INT", "1");
			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWInnerPackQty", "INT", "10");
			Factory.SaveForTesting();

			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have two addOnValue", 2, addOnValues.Length);
			AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "10", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.MasterAirWayBillEventXML("CEN", "IMP", "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30"));
			var processor = new CENEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
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
			var additionalReference = helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var package = helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.PackLine, "", TransitWarehouseStatuses.Codes.Booked);

			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWOuterPackQty", "INT", "1");
			additionalReferencesHelper.UpdateOrCreateAddOnValue(additionalReference, "TWInnerPackQty", "INT", "10");
			Factory.SaveForTesting();

			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have two addOnValue", 2, addOnValues.Length);
			AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "10", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.MasterAirWayBillEventXML("CEN", "IMP", "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=2|IPQ=0"));
			var processor = new CENEventProcessor((UniversalEvent)xmlEvent, Logger, Factory);
			processor.Process(rcn);

			addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference.PK));
			AssertEquals("should have one addOnValue for outer packages", "2", addOnValues.Where(a => a.XV_Name == "TWOuterPackQty").SingleOrDefault().XV_Data);
			AssertEquals("should have one addOnValue for inner packages", "0", addOnValues.Where(a => a.XV_Name == "TWInnerPackQty").SingleOrDefault().XV_Data);
		}

		#endregion

		#region Implementation

		XmlEventDeserializer EventDeserializer => eventDeserializer ?? (eventDeserializer = new XmlEventDeserializer());
		XmlEventDeserializer eventDeserializer;

		WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(factory)); }
		}
		WhsTransitTestHelper helper;

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(factory);

		WhsTransitAdditionalReferencesHelper additionalReferencesHelper => new WhsTransitAdditionalReferencesHelper(Logger, Factory);

		BusinessObjectFactory factory => Factory.BOFactory;

		#endregion
	}
}
