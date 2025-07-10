using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class WhsTransitReceiveConsignmentEventParentFinderTest : WhsTransitConsignmentEventParentFinderTest
	{
		#region TestTransportSelectedPackageShouldReturnMatch

		public void TestTransportReceiveWithARVPackageShouldSelectARVPackage()
		{
			var packageState = CreatePackageState("PKGARV", TransitWarehouseStatuses.Codes.Arrived);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: true);
		}

		public void TestTransportReceiveWithARUPackageShouldSelectARUPackage()
		{
			var packageState = CreatePackageState("PKGARU", TransitWarehouseStatuses.Codes.ArrivedNotProcessed);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: true);
		}

		public void TestTransportReceiveWithCTTPackageShouldSelectCTTPackage()
		{
			var packageState = CreatePackageState("PKGCTT", TransitWarehouseStatuses.Codes.Committed);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: true);
		}

		public void TestTransportReceiveWithPICPackageShouldSelectPICPackage()
		{
			var packageState = CreatePackageState("PKGPIC", TransitWarehouseStatuses.Codes.Picked);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: true);
		}

		public void TestTransportReceiveWithPUTPackageShouldSelectPUTPackage()
		{
			var packageState = CreatePackageState("PKGPUT", TransitWarehouseStatuses.Codes.Putaway);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: true);
		}

		public void TestTransportReceiveWithSTAPackageShouldSelectSTAPackage()
		{
			var packageState = CreatePackageState("PKGSTA", TransitWarehouseStatuses.Codes.Staged);
			packageState = AddDLL(packageState);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: true);
		}

		#endregion

		#region TestTransportSelectedPackageShouldReturnNoMatches

		public void TestTransportReceiveWithBKDPackageShouldNOTSelectBKDPackage()
		{
			var packageState = CreatePackageState("PKGBKD", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: false);
		}

		public void TestTransportReceiveWithFLOPackageShouldNOTSelectFLOPackage()
		{
			var packageState = CreatePackageState("PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded);
			packageState = AddDLL(packageState);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: false);
		}

		public void TestTransportReceiveWithDEPPackageShouldNOTSelectDEPPackage()
		{
			var packageState = CreatePackageState("PKGDEP", TransitWarehouseStatuses.Codes.Departed);
			packageState = AddDLL(packageState);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: false);
		}

		public void TestTransportReceiveWithFINPackageShouldNOTSelectFINPackage()
		{
			var packageState = CreatePackageState("PKGFIN", TransitWarehouseStatuses.Codes.Finalized);
			packageState = AddDLL(packageState);
			Factory.SaveForTesting();
			var results = ReturnXMLResults();
			AssertBasedOnPackageStatus(results, packageState, packageShouldBePicked: false);
		}

		#endregion

		#region TestTransportReceivePackagesShouldReturnNoMatches

		public void TestTransportReceivePackagesShouldReturnNoMatches()
		{
			var location = CreateLocation("NOTARRBK");

			var receiveUnit = Helper.CreateReceiveTransportationUnit("RcvNoMatches", Data.Warehouse.PK, location.PK);
			var receive = Helper.CreateReceiveConsignment("RCVNoMatch", Data.Warehouse.PK, Data.Orgs.CRAHOLSYD, Data.Orgs.WUFSHIJNB, Data.Orgs.INTHEMSYD);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL001", Data.Warehouse.PK);
			var dispatchHeader = Helper.CreateDispatchTransportationUnit("DispHead", Data.Warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dispatchLoadList.PK, dispatchHeader.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("ConsignID", Data.Warehouse.PK);

			Helper.CreatePackageState(receive, 1, "PKG", "TESTBARCODE123", TransitWarehouseStatuses.Codes.Arrived, receiveUnit);
			Helper.CreatePackageState(receive, 1, "PKG", "TESTBARCODE345", TransitWarehouseStatuses.Codes.Committed, receiveUnit);
			Helper.CreatePackageState(receive, 1, "PKG", "TESTBARCODE567", TransitWarehouseStatuses.Codes.Putaway, receiveUnit);
			Helper.CreatePackageState(receive, 1, "PKG", "TESTBARCODE789", TransitWarehouseStatuses.Codes.Picked, receiveUnit);
			var staged = Helper.CreatePackageState(receive, 1, "PKG", "TESTBARCODE910", TransitWarehouseStatuses.Codes.Staged, receiveUnit);
			staged.WPS_WL_LastLocation = location.PK;
			staged.WPS_WL_ReceiveLocation = location.PK;
			dispatchLoadList = Helper.CreateDispatchLoadList("DLL002", Data.Warehouse.PK);
			staged.WPS_WDL_LoadList = dispatchLoadList.PK;

			var booked = Helper.CreatePackageState(receive, 1, "PKG", TestBarcode, TransitWarehouseStatuses.Codes.Booked, null);

			var departed = Helper.CreatePackageState(receive, 1, "PKG", TestBarcode, TransitWarehouseStatuses.Codes.Departed, receiveUnit);
			departed.WPS_WL_LastLocation = location.PK;
			departed.WPS_WL_ReceiveLocation = location.PK;
			dispatchLoadList = Helper.CreateDispatchLoadList("DLL003", Data.Warehouse.PK);
			departed.WPS_WDL_LoadList = dispatchLoadList.PK;
			departed.WPS_WDH_TransitDispatchHeader = dispatchHeader.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";
			departed.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;

			var freightLoaded = Helper.CreatePackageState(receive, 1, "PKG", TestBarcode, TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit);
			freightLoaded.WPS_WL_LastLocation = location.PK;
			freightLoaded.WPS_WL_ReceiveLocation = location.PK;
			dispatchLoadList = Helper.CreateDispatchLoadList("DLL004", Data.Warehouse.PK);
			freightLoaded.WPS_WDL_LoadList = dispatchLoadList.PK;
			freightLoaded.WPS_WDH_TransitDispatchHeader = dispatchHeader.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			freightLoaded.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;

			var finalised = Helper.CreatePackageState(receive, 1, "PKG", TestBarcode, TransitWarehouseStatuses.Codes.Finalized, receiveUnit);
			finalised.WPS_WL_LastLocation = location.PK;
			finalised.WPS_WL_ReceiveLocation = location.PK;
			dispatchLoadList = Helper.CreateDispatchLoadList("DLL005", Data.Warehouse.PK);
			finalised.WPS_WDL_LoadList = dispatchLoadList.PK;
			finalised.WPS_WDH_TransitDispatchHeader = dispatchHeader.PK;
			finalised.WPS_IsSecure = true;
			finalised.WPS_SecurityStatus = "SEC";
			finalised.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXML);
			var finder = GetFinder();

			var testPackageStates = GetPackageStateLikeBarcode("TESTBARCODE%");
			AssertEquals("Precondition - Should have 9 package states in the factory", 9, testPackageStates.Length);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should not have found any business objects", 0, results.Length);
		}

		#endregion

		#region TestErrorOnPackagesWithSameBarcodeInSameState

		public void TestPackagesWithSameBarcodeBothInArrivedStateShouldError()
		{
			ErrorOnPackagesWithSameBarcodeInSameState(TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestPackagesWithSameBarcodeBothInCommittedStateShouldError()
		{
			ErrorOnPackagesWithSameBarcodeInSameState(TransitWarehouseStatuses.Codes.Committed);
		}

		public void TestPackagesWithSameBarcodeBothPutawayStateShouldError()
		{
			ErrorOnPackagesWithSameBarcodeInSameState(TransitWarehouseStatuses.Codes.Putaway);
		}

		public void TestPackagesWithSameBarcodeBothInStagedStateShouldError()
		{
			ErrorOnPackagesWithSameBarcodeInSameState(TransitWarehouseStatuses.Codes.Staged);
		}

		public void TestPackagesWithSameBarcodeBothInPickedStateShouldError()
		{
			ErrorOnPackagesWithSameBarcodeInSameState(TransitWarehouseStatuses.Codes.Picked);
		}

		void ErrorOnPackagesWithSameBarcodeInSameState(string state)
		{
			var package1 = CreatePackageState($"RCDup{state}1", state);
			var package2 = CreatePackageState($"RCDup{state}2", state);

			if (state == TransitWarehouseStatuses.Codes.Staged)
			{
				var dll = Helper.CreateDispatchLoadList("DLL", Data.Warehouse.PK);
				package1.WPS_WDL_LoadList = dll.PK;
				var dll2 = Helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK);
				package2.WPS_WDL_LoadList = dll.PK;
			}

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXML);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should not have found any business objects", 0, results.Length);
			AssertEquals("Should have log errors", true, logger.HasErrors());
			AssertContains($"More than one package was returned with the same status for barcode {TestBarcode}", logger.Logs);
		}

		#endregion

		#region TestPackagesWithSameBarcodeInArrivedAndBookedStateShouldReturnArrived

		public void TestPackagesWithSameBarcodeInArrivedAndBookedStateShouldReturnArrived()
		{
			var arrivedPackage = CreatePackageState("RCSameArv", TransitWarehouseStatuses.Codes.Arrived);
			var bookedConsignment = CreatePackageState("RCSameBkd", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXML);
			var finder = GetFinder();

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);
			AssertType<PkgPackage>("Should be a PkgPackage", results[0]);

			var package = (PkgPackage)results[0];
			AssertEquals("Should be the arrived package", arrivedPackage.Package.PK, package.PK);
		}

		#endregion

		#region TestReceiveUniversalEventWithAdditionalFields

		public void TestReceiveUniversalEventWithAdditionalFields()
		{
			var arrivedPackage = CreatePackageState("RCSameArv", TransitWarehouseStatuses.Codes.Arrived);

			Factory.SaveForTesting();
			AssertEquals("PKG", arrivedPackage.Package.KP_F3_NKPackType);

			var additionalFieldsEvent = TransitReceiveUniversalEventWithAdditionalFields;
			var message = GetQueuedUniversalEventMessage(additionalFieldsEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			var package = Factory.Load<PkgPackage>(arrivedPackage.WPS_KP_Package);
			AssertEquals("BOX", package.KP_F3_NKPackType);
		}

		#endregion

		#region TestPackageShouldUpdateCorrectPackageWithMultipleWarehouses

		public void TestPackageShouldUpdateCorrectPackageWithMultipleWarehouses()
		{
			var newBranch = Helper.CreateGlbBranch("ALT");
			var warehouseAddress = Data.Orgs.WUFSHIJNB.MainAddress;
			var newWarehouse = Helper.CreateWarehouse("Transit Warehouse", "TW2", "A");
			newWarehouse.WW_GB_RelatedCompanyBranch = newBranch.PK;
			newWarehouse.WW_OA_WarehouseAddress = warehouseAddress.PK;
			newWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var newWarehouseLocation = CreateLocation("MULTILOC", newWarehouse);

			var firstArrivedPackage = CreatePackageState("RcvWhs1", TransitWarehouseStatuses.Codes.Arrived);

			var secondArrivedReceiveHeader = Helper.CreateReceiveTransportationUnit("RcvWhs2", newWarehouse.PK, newWarehouseLocation.PK);
			var secondArrivedConsignment = Helper.CreateReceiveConsignment("RCWhs2", newWarehouse.PK, Data.Orgs.CRAHOLSYD, Data.Orgs.WUFSHIJNB, Data.Orgs.INTHEMSYD);
			var secondArrivedPackage = CreatePackageState(secondArrivedConsignment, TransitWarehouseStatuses.Codes.Arrived, secondArrivedReceiveHeader);

			Factory.SaveForTesting();

			// getting the first package
			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXML);
			var finder = GetFinder();
			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);
			AssertType<PkgPackage>("Should be a PkgPackage", results[0]);

			var package = (PkgPackage)results[0];
			AssertEquals("Package that was selected should be the first one", firstArrivedPackage.Package.PK, package.PK);

			// getting the second package
			var secondXmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(newWarehouse.WW_WarehouseCode));
			results = finder.GetLogParentsForEvent(secondXmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);
			AssertType<PkgPackage>("Should be a PkgPackage", results[0]);

			var secondPackage = (PkgPackage)results[0];
			AssertEquals("Package that was selected should be the second one", secondArrivedPackage.Package.PK, secondPackage.PK);
		}

		#endregion

		#region CreatePackageState

		WhsItemPackageState CreatePackageState(string id, string status)
		{
			var location = CreateLocation($"L{id}");
			WhsItemReceiveTransportationUnit receiveUnit = null;
			if (status != TransitWarehouseStatuses.Codes.Booked)
			{
				receiveUnit = Helper.CreateReceiveTransportationUnit(id, Data.Warehouse.PK, location.PK, $"V{id}");
			}

			var consignment = Helper.CreateReceiveConsignment(id, Data.Warehouse.PK, Data.Orgs.CRAHOLSYD, Data.Orgs.WUFSHIJNB, Data.Orgs.INTHEMSYD);

			return CreatePackageState(consignment, status, receiveUnit);
		}

		WhsItemPackageState CreatePackageState(WhsItemReceiveConsignment consignment, string status, WhsItemReceiveTransportationUnit receiveUnit)
		{
			var packageState = Helper.CreatePackageState(consignment, 1, "PKG", TestBarcode, status, receiveUnit);
			packageState.Package.KP_DimensionUQ = "CM";
			packageState.Package.KP_WeightUQ = "KG";
			packageState.Package.KP_VolumeUQ = "CC";

			return packageState;
		}

		#endregion

		#region GetPackageState

		const string GetPackageStateLike = @"
WPS_PK IN
(
	SELECT
		WPS_PK
	FROM
		dbo.WhsItemPackageState AS PkgState
		JOIN dbo.PkgPackage AS Package ON KP_PK = PkgState.WPS_KP_Package
		JOIN dbo.PkgPackageHeader AS PkgHeader ON Package.KP_KPH_PackageHeader = KPH_PK
	WHERE
		PkgHeader.KPH_PackageID like @Barcode
		OR Package.KP_PreviousPackageID like @Barcode
)";

		WhsItemPackageState[] GetPackageStateLikeBarcode(string barcode)
		{
			var sqlParams = new ZSqlParameterCollection
			{
				{ "@Barcode", barcode, PkgPackageHeaderSchema.KPH_PackageID },
			};

			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			query.AddFilterAndZSQLParameterCollection(GetPackageStateLike, sqlParams);

			return Factory.Load<WhsItemPackageState>(query);
		}

		#endregion

		#region TestCESEvent_MatchingReceiveConsignmentIsUpdated

		public void TestCESEvent_MatchesReceiveConsignment_SingleMatchByConsignmentID()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("HOUSEBILL", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(matchingReceiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var nonMatchingConsignment = Helper.CreateReceiveConsignment("SomeRCN", "STD", Data.Warehouse.PK, "RC00000003");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(AutoEvents.CustomsEntryStatusCode, "HOUSEBILL", "CLR", "XXX"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			Factory.SaveForTesting();

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			selectedRCN.Reload();

			var selectedCRNCENNumber = selectedRCN.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertContains(@"Information - Found receive consignment with consignment id 'RC00000001' matching house bill 'HOUSEBILL'.
Information - Customs Release Number 'Customs Cleared' has been added to Receive Consignment 'HOUSEBILL'. It is now Cleared for Release.", logger.Logs);
				AssertEquals("Selected the 'only' receive consignment where its consignment id matches the house bill", matchingReceiveConsignment.PK, selectedRCN.PK);

				AssertEquals(1, selectedRCN.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedCRNCENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared");
				AssertEquals("CLR", selectedRCN.WRC_CustomsStatus);
			});
		}

		public void TestCESEvent_MatchesReceiveConsignment_MultipleMatchByConsignmentID()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("HOUSEBILL", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(matchingReceiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("HOUSEBILL", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreatePackageState(matchingReceiveConsignment2, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(AutoEvents.CustomsEntryStatusCode, "HOUSEBILL", "CLR", "BBB"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var selectedRCNCRNNumber = selectedRCN.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertContains(@"Warning - The house bill 'HOUSEBILL' has matched to more than one receive consignment, the latest one with consignment id 'RC00000002' is selected by default.", logger.Logs);
				AssertEquals("Selected the 'latest' receive consignment where its consignment id matches the house bill", matchingReceiveConsignment2.PK, selectedRCN.PK);

				AssertEquals(1, selectedRCN.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCNCRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared");
			});
		}

		public void TestCESEvent_MatchesReceiveConsignment_SingleMatchByAdditionalReference()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			matchingReceiveConsignment.WRC_HouseBillNumber = "HOUSEBILL";
			Helper.CreatePackageState(matchingReceiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var nonMatchingConsignment = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(AutoEvents.CustomsEntryStatusCode, "HOUSEBILL", "CLR", "CCL"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var selectedRCNCRNNumber = selectedRCN.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertContains(@"Information - Found receive consignment with consignment id 'RC00000001' matching house bill 'HOUSEBILL'.
Information - Customs Release Number 'Customs Cleared' has been added to Receive Consignment 'RC00000001'. It is now Cleared for Release.", logger.Logs);
				AssertEquals("Selected the 'only' receive consignment where its consignment id matches the house bill", matchingReceiveConsignment.PK, selectedRCN.PK);

				AssertEquals(1, selectedRCN.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCNCRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared");
			});
		}

		public void TestCESEvent_MatchesReceiveConsignment_MultipleMatchByAdditionalReference()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			matchingReceiveConsignment1.WRC_HouseBillNumber = "HOUSEBILL";
			Helper.CreatePackageState(matchingReceiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			matchingReceiveConsignment2.WRC_HouseBillNumber = "HOUSEBILL";
			Helper.CreatePackageState(matchingReceiveConsignment2, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(AutoEvents.CustomsEntryStatusCode, "HOUSEBILL", "CLR", "CCL"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var selectedRCNCRNNumber = selectedRCN.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertContains(@"Warning - The house bill 'HOUSEBILL' has matched to more than one receive consignment, the latest one with consignment id 'RC00000002' is selected by default.
Information - Customs Release Number 'Customs Cleared' has been added to Receive Consignment 'RC00000002'. It is now Cleared for Release.", logger.Logs);
				AssertEquals("Selected the 'latest' receive consignment where its HSB entry number matches the house bill", matchingReceiveConsignment2.PK, selectedRCN.PK);

				AssertEquals(1, selectedRCN.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCNCRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared");
			});
		}

		public void TestCESEvent_MatchesReceiveConsignment_SingleMatchByDataTarget()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(matchingReceiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var nonMatchingConsignment = Helper.CreateReceiveConsignment("SomeRCN", "STD", Data.Warehouse.PK, "RC00000002");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(AutoEvents.CustomsEntryStatusCode, "HOUSEBILL", "CLR", "XXX", "RC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var selectedRCNCRNNumber = selectedRCN.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertContains(@"Information - Customs Release Number 'Customs Cleared' has been added to Receive Consignment 'RCN1'. It is now Cleared for Release.", logger.Logs);
				AssertEquals("Selected the 'only' receive consignment where its consignment id matches the data target key.", matchingReceiveConsignment.PK, selectedRCN.PK);

				AssertEquals(1, selectedRCN.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCNCRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared");
			});
		}

		public void TestCESEvent_MatchesReceiveConsignment_NoCRNCreated()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("HOUSEBILL", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(matchingReceiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var nonMatchingConsignment = Helper.CreateReceiveConsignment("SomeRCN", "STD", Data.Warehouse.PK, "RC00000003");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(AutoEvents.CustomsEntryStatusCode, "HOUSEBILL", "HLD", "XXX"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertContains(@"Information - Found receive consignment with consignment id 'RC00000001' matching house bill 'HOUSEBILL'.", logger.Logs);
				AssertEquals("Selected the 'only' receive consignment where its consignment id matches the house bill", matchingReceiveConsignment.PK, selectedRCN.PK);

				AssertEquals(0, selectedRCN.CustomsReferenceNumbers.Count);
			});
		}

		public void TestCESEvent_NoMatchingReceiveConsignment()
		{
			var nonMatchingConsignment = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXMLBuilder(AutoEvents.CustomsEntryStatusCode, "HOUSEBILL", "CLR", "CCL"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 0 business object", 0, results.Length);
		}

		#endregion

		#region TestMAAEvent_MatchingReceiveConsignmentsAndPackageStates

		public void TestMAAEvent_MatchesReceiveConsignment_ByDataTarget_XMLVersion2012()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");

			var nonMatchingConsignment = Helper.CreateReceiveConsignment("SomeRCN", "STD", Data.Warehouse.PK, "RC00000002");

			AddCRESANoteToConsignment(matchingReceiveConsignment);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageAcceptedCode, customsReferenceNumber: "CRF123", referenceNumber: "RFN123", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var selectedRCNPANReference = selectedRCN.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertEquals("Selected the 'only' receive consignment where its consignment id matches the data target key.", matchingReceiveConsignment.PK, selectedRCN.PK);

				AssertEquals(1, selectedRCN.PortReferences.Count);
				AssertAdditionalReferece(selectedRCNPANReference, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CRF123", entryLineReference: "RFN123");

				var expectedCRESANote = @"User: CargoWise One Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Organization Provider ID:
Buyer Provider ID    Supplier Provider ID    Sending Party Provider ID    Forwarder Provider ID    Agent Provider ID
-                    -                       -                            -                        -
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
RC1                  RC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
				AssertMultilineASCIIEquals(expectedCRESANote, selectedRCN.FindOrCreateCRESAStmNote().ST_NoteText);
			});
		}

		public void TestMAAEvent_MatchesReceiveConsignment_ByDataTarget_XMLVersion2011()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");

			var nonMatchingConsignment = Helper.CreateReceiveConsignment("SomeRCN", "STD", Data.Warehouse.PK, "RC00000002");

			AddCRESANoteToConsignment(matchingReceiveConsignment);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=RFN123", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var selectedRCNPANReference = selectedRCN.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertEquals("Selected the 'only' receive consignment where its consignment id matches the data target key.", matchingReceiveConsignment.PK, selectedRCN.PK);

				AssertEquals(1, selectedRCN.PortReferences.Count);
				AssertAdditionalReferece(selectedRCNPANReference, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CRF123", entryLineReference: "RFN123");

				var expectedCRESANote = @"User: CargoWise One Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Organization Provider ID:
Buyer Provider ID    Supplier Provider ID    Sending Party Provider ID    Forwarder Provider ID    Agent Provider ID
-                    -                       -                            -                        -
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
RC1                  RC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
				AssertMultilineASCIIEquals(expectedCRESANote, selectedRCN.FindOrCreateCRESAStmNote().ST_NoteText);
			});
		}

		public void TestMAAEvent_NoMatchingReceiveConsignment_ByDataTarget_XMLVersion2011()
		{
			var nonMatchingConsignment = Helper.CreateReceiveConsignment("NonMatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=RFN123", dataTargetName: "TransitReceive", dataTargetKey: "RC00000002"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 0 business object", 0, results.Length);
		}

		public void TestMAAEvent_NoMatchingReceiveConsignment_ByDataTarget_XMLVersion2012()
		{
			var nonMatchingConsignment = Helper.CreateReceiveConsignment("NonMatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageAcceptedCode, customsReferenceNumber: "CR123", referenceNumber: "RFN123", dataTargetName: "TransitReceive", dataTargetKey: "RC00000002"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 0 business object", 0, results.Length);
		}

		#endregion

		#region TestCENEvent_MatchesReceiveConsignmentsAndPackageStates

		public void TestCENEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1CENNumber = selectedRCN1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var selectedRCN2CENNumber = selectedRCN2.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedRCN2.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCN1CENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
				AssertAdditionalReferece(selectedRCN2CENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
			});
		}

		public void TestCENEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2011()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=Customs|LOC=AUBNE"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1CENNumber = selectedRCN1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var selectedRCN2CENNumber = selectedRCN2.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedRCN2.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCN1CENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
				AssertAdditionalReferece(selectedRCN2CENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
			});
		}

		public void TestCENEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1CENNumber = selectedPackage1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var selectedPackage2CENNumber = selectedPackage2.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertEquals(1, selectedPackage1.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedPackage2.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedPackage1CENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
				AssertAdditionalReferece(selectedPackage2CENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
			});
		}

		public void TestCENEvent_MatchArrivedPackageStateAndReceiveConsignment_ByForwardingShipmentNumber()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignmentForBlindPackage = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			var matchingBlindPackageState = Helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingBlindPackageState, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage = results.OfType<PkgPackage>().Single();
			var selectedReceiveConsignment = results.OfType<WhsItemReceiveConsignment>().Single();
			var selectedPackageCENNumber = selectedPackage.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var selectedReceiveConsignmentCENNumber = selectedReceiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new BusinessObject[] { matchingBlindPackageState.Package, selectedReceiveConsignment }, results);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);

				AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertEquals(1, selectedPackage.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedReceiveConsignment.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedPackageCENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
				AssertAdditionalReferece(selectedReceiveConsignmentCENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
			});
		}

		public void TestCENEvent_MatchesReceiveConsignment_ByDataTarget()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsNumberEnteredCode, eventReference: "|CRF=CR123|DEP=Customs|LOC=AUBNE", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business objects", 1, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN1CENNumber = selectedRCN1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertEquals(1, selectedRCN1.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCN1CENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CR123");
			});
		}

		public void TestCENEvent_DoesNotMatchBookedPackageStatesAndHandlingUnits()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var packageStateWithShipmentNumber = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreateAdditionalReference(packageStateWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var handlingUnitWithShipmentNumber = Helper.CreateHandlingUnitPackage("HU", Helper.CreatePackageHandlingUnit(), receiveTransportationUnit);
			Helper.CreateAdditionalReference(handlingUnitWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 0 business objects", 0, results.Length);
		}

		public void TestCENEvent_MissingDataForMatchingFromEventParameters()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", "", "", "", "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.", logger.Logs);
			});
		}

		public void TestCENEvent_MissingDataForMatchingFromEventReference()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsNumberEnteredCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.", logger.Logs);
			});
		}

		#endregion

		#region TestCENEvent_MatchesReceiveConsignments_ByBillNumber

		public void TestCENEvent_MatchesReceiveConsignments_ByMasterAirWayBillNumber_Import()
		{
			MatchesReceiveConsignments_ByBillNumber("CEN", "IMP", "AIR", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms);
		}

		public void TestCENEvent_MatchesReceiveConsignments_ByMasterAirWayBillNumber_Export()
		{
			MatchesReceiveConsignments_ByBillNumber("CEN", "EXP", "AIR", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms);
		}

		public void TestCRNEvent_MatchesReceiveConsignments_ByMasterAirWayBillNumer_Import()
		{
			MatchesReceiveConsignments_ByBillNumber("CRN", "IMP", "AIR", TransitWarehouseCustomsStatuses.Codes.Cleared);
		}

		public void TestCRNEvent_MatchesReceiveConsignments_ByMasterAirWayBillNumer_Export()
		{
			MatchesReceiveConsignments_ByBillNumber("CRN", "EXP", "AIR", TransitWarehouseCustomsStatuses.Codes.Cleared);
		}

		public void TestCENEvent_MatchesReceiveConsignments_ByHouseAirWayBillNumber()
		{
			MatchesReceiveConsignments_ByBillNumber("CEN", "IMP", "AIR", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, WarehouseAdditionalReferenceTypes.Codes.HouseBill);
		}

		public void TestCENEvent_MatchesReceiveConsignments_ByMasterBillOfLadingNumber_Import()
		{
			MatchesReceiveConsignments_ByBillNumber("CEN", "IMP", "SEA", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms);
		}

		public void TestCENEvent_MatchesReceiveConsignments_ByMasterBillOfLadingNumber_Export()
		{
			MatchesReceiveConsignments_ByBillNumber("CEN", "EXP", "SEA", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms);
		}

		public void TestCRNEvent_MatchesReceiveConsignments_ByMasterBillOfLadingNumber_Import()
		{
			MatchesReceiveConsignments_ByBillNumber("CRN", "IMP", "SEA", TransitWarehouseCustomsStatuses.Codes.Cleared);
		}

		public void TestCRNEvent_MatchesReceiveConsignments_ByMasterBillOfLadingNumber_Export()
		{
			MatchesReceiveConsignments_ByBillNumber("CRN", "EXP", "SEA", TransitWarehouseCustomsStatuses.Codes.Cleared);
		}

		public void TestCENEvent_MatchesReceiveConsignments_ByHouseBillOfLadingNumber()
		{
			MatchesReceiveConsignments_ByBillNumber("CEN", "IMP", "SEA", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, WarehouseAdditionalReferenceTypes.Codes.HouseBill);
		}

		void MatchesReceiveConsignments_ByBillNumber(string eventType, string messageType, string transportType, string updatedRCNCustomsStatus, string additionalReferenceType = WarehouseAdditionalReferenceTypes.Codes.MasterBill)
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			if (messageType == "IMP")
			{
				warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRLIO";
			}
			else
			{
				warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			}

			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var additionalReference1 = Helper.CreateAdditionalReference(rcn, "113-26363466", WarehouseAdditionalReferenceTypes.Codes.MasterBill, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			if (additionalReferenceType == WarehouseAdditionalReferenceTypes.Codes.HouseBill)
			{
				rcn.WRC_HouseBillNumber = "FGDSDGFGFDS";
			}
			var additionalReference3 = Helper.CreateAdditionalReference(rcn, "3FR33159700500064", eventType == "CEN" ? TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber : TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);

			var packages = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var xml = ZString.Empty;
			if (transportType == "AIR")
			{
				xml = UniversalHelper.MasterAirWayBillEventXML(eventType, messageType, "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30");
			}
			else
			{
				xml = UniversalHelper.MasterBillOfLadingEventXML(eventType, messageType);
			}
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.MasterAirWayBillEventXML(eventType, messageType, "|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference3.PK));
			selectedRCN.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				if (additionalReferenceType == WarehouseAdditionalReferenceTypes.Codes.MasterBill)
				{
					AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching bill number '113-26363466'.
Information - Populating matching Receive Consignments.", logger.Logs);
				}
				else
				{
					AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching bill number 'FGDSDGFGFDS'.
Information - Populating matching Receive Consignments.", logger.Logs);
				}

				AssertEquals("should have two addOnValue", 2, addOnValues.Length);
				AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.SingleOrDefault(a => a.XV_Name == "TWOuterPackQty").XV_Data);
				AssertEquals("should have one addOnValue for inner packages", "30", addOnValues.SingleOrDefault(a => a.XV_Name == "TWInnerPackQty").XV_Data);
				AssertEquals("Customs Status should be upated", updatedRCNCustomsStatus, selectedRCN.WRC_CustomsStatus);
			});
		}

		public void TestCENEvent_MatchesReceiveConsignments_BillNumberIsEmpty()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRLIO";

			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");

			Helper.CreateAdditionalReference(rcn, "113-26363466", WarehouseAdditionalReferenceTypes.Codes.MasterBill, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			rcn.WRC_HouseBillNumber = "FGDSDGFGFDS";
			Helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);

			Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(noHouseBillOrMasterBillNumberEventXML);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should not find any business object", 0, results.Length);
		}

		public void TestCENEvent_MatchesReceiveConsignments_ByHouseAirWayBillNumber_CheckCountry()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRLIO";

			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var additionalReference1 = Helper.CreateAdditionalReference(rcn, "113-26363466", WarehouseAdditionalReferenceTypes.Codes.MasterBill, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			rcn.WRC_HouseBillNumber = "FGDSDGFGFDS";
			var additionalReference3 = Helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);

			var packages = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(masterAirWayBillEventXMLNoLocation);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, additionalReference3.PK));
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching bill number 'FGDSDGFGFDS'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals("should have two addOnValue", 2, addOnValues.Length);
				AssertEquals("should have one addOnValue for outer packages", "1", addOnValues.SingleOrDefault(a => a.XV_Name == "TWOuterPackQty").XV_Data);
				AssertEquals("should have one addOnValue for inner packages", "30", addOnValues.SingleOrDefault(a => a.XV_Name == "TWInnerPackQty").XV_Data);
			});
		}

		#endregion

		#region TestCRNEvent_MatchesReceiveConsignmentsAndPackageStates

		public void TestCRNEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1CRNNumber = selectedRCN1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var selectedRCN2CRNNumber = selectedRCN2.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedRCN2.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCN1CRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
				AssertAdditionalReferece(selectedRCN2CRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
			});
		}

		public void TestCRNEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2011()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=Customs|LOC=AUBNE"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1CRNNumber = selectedRCN1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var selectedRCN2CRNNumber = selectedRCN2.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedRCN2.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCN1CRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
				AssertAdditionalReferece(selectedRCN2CRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
			});
		}

		public void TestCRNEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1CRNNumber = selectedPackage1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var selectedPackage2CRNNumber = selectedPackage2.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedPackage2.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedPackage1CRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
				AssertAdditionalReferece(selectedPackage2CRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
			});
		}

		public void TestCRNEvent_MatchesReceiveConsignment_ByDataTarget()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsReleaseNumberEnteredCode, eventReference: "|CRF=CR123|DEP=Customs|LOC=AUBNE", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business objects", 1, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN1CRNNumber = selectedRCN1.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertEquals(1, selectedRCN1.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedRCN1CRNNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
			});
		}

		public void TestCRNEvent_MatchArrivedPackageStateAndReceiveConsignment_ByForwardingShipmentNumber()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignmentForBlindPackage = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			var matchingBlindPackageState = Helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingBlindPackageState, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage = results.OfType<PkgPackage>().Single();
			var selectedReceiveConsignment = results.OfType<WhsItemReceiveConsignment>().Single();
			var selectedPackageCENNumber = selectedPackage.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var selectedReceiveConsignmentCENNumber = selectedReceiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new BusinessObject[] { matchingBlindPackageState.Package, selectedReceiveConsignment }, results);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);

				AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertEquals(1, selectedPackage.CustomsReferenceNumbers.Count);
				AssertEquals(1, selectedReceiveConsignment.CustomsReferenceNumbers.Count);
				AssertAdditionalReferece(selectedPackageCENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
				AssertAdditionalReferece(selectedReceiveConsignmentCENNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CR123");
			});
		}

		public void TestCRNEvent_DoesNotMatchBookedPackageStatesAndHandlingUnits()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var packageStateWithShipmentNumber = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreateAdditionalReference(packageStateWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var handlingUnitWithShipmentNumber = Helper.CreateHandlingUnitPackage("HU", Helper.CreatePackageHandlingUnit(), receiveTransportationUnit);
			Helper.CreateAdditionalReference(handlingUnitWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 0 business objects", 0, results.Length);
		}

		public void TestCRNEvent_MissingDataForMatchingFromEventParameters()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", "", "", "", "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.", logger.Logs);
			});
		}

		public void TestCRNEvent_MissingDataForMatchingFromEventReference()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.CustomsReleaseNumberEnteredCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.", logger.Logs);
			});
		}

		#endregion

		#region TestSCMEvent_MatchesReceiveConsignmentsAndPackageStates

		public void TestSCMEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			AddCRESANoteToConsignment(matchingReceiveConsignment1);
			AddCRESANoteToConsignment(matchingReceiveConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", "CR123", "AUBNE", "", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SCMNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedRCN2SCMNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertAdditionalReferece(selectedRCN2SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);

				var expectedCRESANote = @"User: CargoWise One Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Organization Provider ID:
Buyer Provider ID    Supplier Provider ID    Sending Party Provider ID    Forwarder Provider ID    Agent Provider ID
-                    -                       -                            -                        -
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
RC1                  RC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
				AssertMultilineASCIIEquals(expectedCRESANote, matchingReceiveConsignment1.FindOrCreateCRESAStmNote().ST_NoteText);
				AssertMultilineASCIIEquals(expectedCRESANote, matchingReceiveConsignment2.FindOrCreateCRESAStmNote().ST_NoteText);
			});
		}

		public void TestSCMEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2011()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			AddCRESANoteToConsignment(matchingReceiveConsignment1);
			AddCRESANoteToConsignment(matchingReceiveConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=Customs|LOC=AUBNE", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SCMNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedRCN2SCMNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertAdditionalReferece(selectedRCN2SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);

				var expectedCRESANote = @"User: CargoWise One Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Organization Provider ID:
Buyer Provider ID    Supplier Provider ID    Sending Party Provider ID    Forwarder Provider ID    Agent Provider ID
-                    -                       -                            -                        -
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
RC1                  RC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
				AssertMultilineASCIIEquals(expectedCRESANote, matchingReceiveConsignment1.FindOrCreateCRESAStmNote().ST_NoteText);
				AssertMultilineASCIIEquals(expectedCRESANote, matchingReceiveConsignment2.FindOrCreateCRESAStmNote().ST_NoteText);
			});
		}

		public void TestSCMEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", "CR123", "AUBNE", "", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1SCMNumber = selectedPackage1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedPackage2SCMNumber = selectedPackage2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.PortReferences.Count);
				AssertEquals(1, selectedPackage2.PortReferences.Count);
				AssertAdditionalReferece(selectedPackage1SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertAdditionalReferece(selectedPackage2SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
			});
		}

		public void TestSCMEvent_MatchesReceiveConsignment_ByDataTarget()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.ClearanceCompletedCode, eventReference: "|CRF=CR123|DEP=Customs|LOC=AUBNE", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business objects", 1, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN1SCMNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
			});
		}

		public void TestSCMEvent_MatchArrivedPackageStateAndReceiveConsignment_ByForwardingShipmentNumber()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignmentForBlindPackage = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			var matchingBlindPackageState = Helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingBlindPackageState, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", "CR123", "AUBNE", "", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage = results.OfType<PkgPackage>().Single();
			var selectedReceiveConsignment = results.OfType<WhsItemReceiveConsignment>().Single();
			var selectedPackageCENNumber = selectedPackage.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedReceiveConsignmentCENNumber = selectedReceiveConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new BusinessObject[] { matchingBlindPackageState.Package, selectedReceiveConsignment }, results);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);

				AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertEquals(1, selectedPackage.PortReferences.Count);
				AssertEquals(1, selectedReceiveConsignment.PortReferences.Count);
				AssertAdditionalReferece(selectedPackageCENNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertAdditionalReferece(selectedReceiveConsignmentCENNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
			});
		}

		public void TestSCMEvent_DoesNotMatchBookedPackageStatesAndHandlingUnits()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var packageStateWithShipmentNumber = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreateAdditionalReference(packageStateWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var handlingUnitWithShipmentNumber = Helper.CreateHandlingUnitPackage("HU", Helper.CreatePackageHandlingUnit(), receiveTransportationUnit);
			Helper.CreateAdditionalReference(handlingUnitWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 0 business objects", 0, results.Length);
		}

		public void TestSCMEvent_ReceiveConsignmentDirection_Export() => TestSCMEvent_ReceiveConsignmentDirectionCore(TransitWarehouseConsignmentDirections.Codes.Export);

		public void TestSCMEvent_ReceiveConsignmentDirection_Import() => TestSCMEvent_ReceiveConsignmentDirectionCore(TransitWarehouseConsignmentDirections.Codes.Import);

		void TestSCMEvent_ReceiveConsignmentDirectionCore(string direction)
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignmentForBlindPackage = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			var matchingBlindPackageState = Helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingBlindPackageState, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var isExport = direction == TransitWarehouseConsignmentDirections.Codes.Export;
			var documentName = isExport ? "Goods Received(CRESA)" : "Goods Received";
			var customsReferenceNumber = "BBE11111";
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", customsReferenceNumber, "AUBNE", documentName: documentName));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var expectedPortReference = isExport ? TransitWarehousePortReferenceTypes.Codes.PortExport : TransitWarehousePortReferenceTypes.Codes.PortAuthority;
			var selectedPackage = results.OfType<PkgPackage>().Single();
			var selectedReceiveConsignment = results.OfType<WhsItemReceiveConsignment>().Single();
			var selectedPackagePortReferenceNumber = selectedPackage.PortReferences.First(expectedPortReference);
			var selectedReceiveConsignmentPortReferenceNumber = selectedReceiveConsignment.PortReferences.First(expectedPortReference);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new BusinessObject[] { matchingBlindPackageState.Package, selectedReceiveConsignment }, results);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);

				AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertEquals(1, selectedPackage.PortReferences.Count);
				AssertEquals(1, selectedReceiveConsignment.PortReferences.Count);
				AssertAdditionalReferece(selectedPackagePortReferenceNumber, TransitWarehouseReferenceCategories.Codes.PortReference, expectedPortReference, customsReferenceNumber, TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertAdditionalReferece(selectedReceiveConsignmentPortReferenceNumber, TransitWarehouseReferenceCategories.Codes.PortReference, expectedPortReference, customsReferenceNumber, TransitWarehouseReferenceStatus.Codes.Cleared);
			});
		}

		#endregion

		#region TestSHLEvent_MatchesReceiveConsignmentsAndPackageStates

		public void TestSHLEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", "CR123", "AUBNE", "", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SHLNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedRCN2SHLNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedRCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2012_ForImport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "", Facilities.Code.Depot, "CR123", "AUBNE", "Port Notification Import Status", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SHLNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedRCN2SHLNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedRCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2012_ForExport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "", Facilities.Code.Depot, "CR123", "AUBNE", "Port Notification Export Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SHLNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedRCN2SHLNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedRCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2011()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=Customs|LOC=AUBNE", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SHLNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedRCN2SHLNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedRCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2011_ForImport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|FAC=CFS|LOC=AUBNE|RES=clearance pending|MST=Port Notification Import Status", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SHLNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedRCN2SHLNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedRCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesReceiveConsignments_ByForwardingShipmentNumber_XMLVersion2011_ForExport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingReceiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			matchingReceiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingReceiveConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|FAC=CFS|LOC=AUBNE|RES=clearance pending|MST=Port Notification Export Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN2 = (WhsItemReceiveConsignment)results[1];
			var selectedRCN1SHLNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedRCN2SHLNumber = selectedRCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Receive Consignments 'RC00000001, RC00000002' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertEquals(1, selectedRCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedRCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", "CR123", "AUBNE", "", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1SHLNumber = selectedPackage1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedPackage2SHLNumber = selectedPackage2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.PortReferences.Count);
				AssertEquals(1, selectedPackage2.PortReferences.Count);
				AssertAdditionalReferece(selectedPackage1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedPackage2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2012_ForImport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "", Facilities.Code.Depot, "CR123", "AUBNE", "Port Notification Import Status", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1SHLNumber = selectedPackage1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedPackage2SHLNumber = selectedPackage2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.PortReferences.Count);
				AssertEquals(1, selectedPackage2.PortReferences.Count);
				AssertAdditionalReferece(selectedPackage1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedPackage2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2012_ForExport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "", Facilities.Code.Depot, "CR123", "AUBNE", "Port Notification Export Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1SHLNumber = selectedPackage1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedPackage2SHLNumber = selectedPackage2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.PortReferences.Count);
				AssertEquals(1, selectedPackage2.PortReferences.Count);
				AssertAdditionalReferece(selectedPackage1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedPackage2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2011()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=CUSTOMS|LOC=AUBNE", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1SHLNumber = selectedPackage1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedPackage2SHLNumber = selectedPackage2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.PortReferences.Count);
				AssertEquals(1, selectedPackage2.PortReferences.Count);
				AssertAdditionalReferece(selectedPackage1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedPackage2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2011_ForImport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|FAC=CFS|LOC=AUBNE|RES=clearance pending|MST=Port Notification Import Status", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1SHLNumber = selectedPackage1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedPackage2SHLNumber = selectedPackage2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.PortReferences.Count);
				AssertEquals(1, selectedPackage2.PortReferences.Count);
				AssertAdditionalReferece(selectedPackage1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
				AssertAdditionalReferece(selectedPackage2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesArrivedPackageStates_ByForwardingShipmentNumber_XMLVersion2011_ForExport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var matchingPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState1, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingPackageState2, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment3 = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			receiveConsignment3.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var matchingHandlingUnit = Helper.CreatePackageState(receiveConsignment3, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			matchingHandlingUnit.WPS_IsHandlingUnit = true;
			Helper.CreateAdditionalReference(matchingHandlingUnit, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment4 = Helper.CreateReceiveConsignment("RC00000004", "STD", Data.Warehouse.PK, "RC00000004");
			receiveConsignment4.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var nonMatchingPackageState = Helper.CreatePackageState(receiveConsignment4, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(nonMatchingPackageState, "SN321", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|FAC=CFS|LOC=AUBNE|RES=clearance pending|MST=Port Notification Export Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage1 = (PkgPackage)results[0];
			var selectedPackage2 = (PkgPackage)results[1];
			var selectedPackage1SHLNumber = selectedPackage1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedPackage2SHLNumber = selectedPackage2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			CombineAssertions(() =>
			{
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[] { matchingPackageState1.Package, matchingPackageState2.Package }, results);
				AssertEquals(1, selectedPackage1.PortReferences.Count);
				AssertEquals(1, selectedPackage2.PortReferences.Count);
				AssertAdditionalReferece(selectedPackage1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedPackage2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchesReceiveConsignment_ByDataTarget()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var nonmatchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000003", "STD", Data.Warehouse.PK, "RC00000003");
			nonmatchingReceiveConsignment.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, eventReference: "|CRF=CR123|DEP=Customs|LOC=AUBNE", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business objects", 1, results.Length);

			var selectedRCN1 = (WhsItemReceiveConsignment)results[0];
			var selectedRCN1SHLNumber = selectedRCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertEquals(1, selectedRCN1.PortReferences.Count);
				AssertAdditionalReferece(selectedRCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123", "");
			});
		}

		public void TestSHLEvent_MatchArrivedPackageStateAndReceiveConsignment_ByForwardingShipmentNumber()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreateAdditionalReference(matchingReceiveConsignment, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignmentForBlindPackage = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			var matchingBlindPackageState = Helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(matchingBlindPackageState, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=CUSTOMS|LOC=AUBNE", documentName: "Goods Received"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedPackage = results.OfType<PkgPackage>().Single();
			var selectedReceiveConsignment = results.OfType<WhsItemReceiveConsignment>().Single();
			var selectedPackageCENNumber = selectedPackage.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			var selectedReceiveConsignmentCENNumber = selectedReceiveConsignment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new BusinessObject[] { matchingBlindPackageState.Package, selectedReceiveConsignment }, results);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);

				AssertContains(@"Information - Found Receive Consignments 'RC00000001' matching shipment number 'SN123'.
Information - Populating matching Receive Consignments.", logger.Logs);

				AssertContains(@"Information - Found Package(s) matching shipment number 'SN123'.
Information - Populating matching Packages.", logger.Logs);

				AssertEquals(1, selectedPackage.PortReferences.Count);
				AssertEquals(1, selectedReceiveConsignment.PortReferences.Count);
				AssertAdditionalReferece(selectedPackageCENNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123");
				AssertAdditionalReferece(selectedReceiveConsignmentCENNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "CR123");
			});
		}

		public void TestSHLEvent_DoesNotMatchBookedPackageStatesAndHandlingUnits()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");
			var packageStateWithShipmentNumber = Helper.CreatePackageState(receiveConsignment1, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreateAdditionalReference(packageStateWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC00000002", "STD", Data.Warehouse.PK, "RC00000002");
			receiveConsignment2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			var handlingUnitWithShipmentNumber = Helper.CreateHandlingUnitPackage("HU", Helper.CreatePackageHandlingUnit(), receiveTransportationUnit);
			Helper.CreateAdditionalReference(handlingUnitWithShipmentNumber, "SN123", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 0 business objects", 0, results.Length);
		}

		protected override string MessageTypeForSHLEvent => "'Port Notification Import Status' or 'Port Notification Export Status'";

		#endregion

		#region TestISNEvent_MatchesReceiveConsignmentAndProcessed_SingleMatchByDataTarget

		public void TestISNEvent_MatchesReceiveConsignmentAndProcessed_SingleMatchByDataTarget()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK, "RC00000001");
			Helper.CreatePackageState(matchingReceiveConsignment, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var nonMatchingConsignment = Helper.CreateReceiveConsignment("SomeRCN", "STD", Data.Warehouse.PK, "RC00000002");
			Helper.CreatePackageState(nonMatchingConsignment, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Booked);

			AddCRESANoteToConsignment(matchingReceiveConsignment);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.InterchangeSentCode, department: "CargoWise", messageType: "Goods Received (CRESA)", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedRCN = (WhsItemReceiveConsignment)results[0];
			CombineAssertions(() =>
			{
				AssertType<WhsItemReceiveConsignment>("Should be a Receive Consignment", results[0]);
				AssertEquals("Selected the 'only' receive consignment where its consignment id matches the data target key.", matchingReceiveConsignment.PK, selectedRCN.PK);

				var expectedCRESANote = @"User: CargoWise One Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Organization Provider ID:
Buyer Provider ID    Supplier Provider ID    Sending Party Provider ID    Forwarder Provider ID    Agent Provider ID
-                    -                       -                            -                        -
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
RC1                  RC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
				AssertMultilineASCIIEquals(expectedCRESANote, selectedRCN.FindOrCreateCRESAStmNote().ST_NoteText);
			});
		}

		#endregion

		#region TestMRREvent_MatchesReceiveConsignment

		public void TestMRREvent_MatchesReceiveConsignment_MessageTypeIsCIN() => TestMRREvent_MatchesReceiveConsignment(CIN750NotificationConstants.CIN750MessageType);

		public void TestMRREvent_MatchesReceiveConsignment_MessageTypeIsNotCIN() => TestMRREvent_MatchesReceiveConsignment("AAA");

		void TestMRREvent_MatchesReceiveConsignment(string messageType)
		{
			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "CIN750InFromRCN";
			documentData.JDD_ParentID = matchingReceiveConsignment.PK;
			documentData.JDD_ParentTableCode = matchingReceiveConsignment.TablePrefix;

			var documentLog = Helper.CreateStmALog(documentData, CargoWise.Definitions.EventCodes.MessageSent, $"|HBL=-|JOB=RC00000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40");

			matchingReceiveConsignment.PopulateAddOnValue("CIN750InHistoryMessageIdPair1", "STR", $"00000000-0000-0000-0000-000000000000|{documentLog.PK}");

			Factory.SaveForTesting();
			documentData.Logs.LoadRelatedElements();
			matchingReceiveConsignment.Logs.LoadRelatedElements();
			TestMRJ_MRREvent_CreateCIN750Note(matchingReceiveConsignment);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageReceivedCode, referenceNumber: "RC00000001", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", reason: "Test Reason", requestNumber: "00000000-0000-0000-0000-000000000000", documentName: "CIN In Notification (750)", messageType: messageType));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);

			if (messageType == CIN750NotificationConstants.CIN750MessageType)
			{
				AssertEquals("Should have only found 1 business object", 1, results.Length);
				AssertEquals("Should have no rcn", 0, results.Count(r => r is WhsItemReceiveConsignment));
				AssertEquals("Should have 1 document data", 1, results.Count(r => r is VisualizerDocumentData));

				var note = matchingReceiveConsignment.FindOrCreateCIN750StmNote();
				AssertEquals(@"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: 00000000-0000-0000-0000-000000000000 has been sent successfully.
CIN 750 Notification:
    Message Type    Result     Ref Type    Ref Code           Enterprise Code    Customs Status    From CTO    CTO CIN Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    In              Succeed    REF         EDIDATRC0000001    EDIDAT             C                 WHTEST      C002            Header               C001
Goods Details:
    Quantity    Weight      Doc Type    Doc Ref    TSD     Description
    5           8.800 KG    T1          CEN1       TST1    Test Description

", note.ST_NoteText);
			}
			else
			{
				AssertEquals("Should have found no business objects", 0, results.Length);
			}

			AssertEquals("Document Log should not be cancelled", false, documentLog.IsCancelled);

			var rcnLog = matchingReceiveConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=RC00000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();

			AssertEquals("RCN Log should not be cancelled", false, rcnLog.IsCancelled);
		}

		#endregion

		#region TestMRJEvent_MatchesReceiveConsignmentAndProcessLogs

		public void TestMRJEvent_MatchesReceiveConsignmentAndProcessLogs_MessageTypeIsCRESA()
		{
			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "Goods Received (CRESA)";
			documentData.JDD_ParentID = matchingReceiveConsignment.PK;
			documentData.JDD_ParentTableCode = matchingReceiveConsignment.TablePrefix;

			AddCRESANoteToConsignment(matchingReceiveConsignment);
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageRejectedCode,
				dataTargetName: "TransitReceive",
				dataTargetKey: "RC00000001",
				reason: "Test Reason",
				documentName: "Goods Received (CRESA)",
				messageType: TransitWarehouseMessageTypes.CRESAMessageType));

			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);
			AssertEquals("Should have only 1 rcn", 1, results.Count(r => r is WhsItemReceiveConsignment));

			var expectedCRESAFailedNote = @"
Event Code: MRJ - Message Rejected
Failed Reason: Test Reason
";
			Assert((results.First() as WhsItemReceiveConsignment).FindOrCreateCRESAStmNote().ST_NoteText.Contains(expectedCRESAFailedNote));
		}

		public void TestMRJEvent_MatchesReceiveConsignmentAndProcessLogs_MessageTypeIsCIN()
		{
			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "CIN750InFromRCN";
			documentData.JDD_ParentID = matchingReceiveConsignment.PK;
			documentData.JDD_ParentTableCode = matchingReceiveConsignment.TablePrefix;

			var documentLog = Helper.CreateStmALog(documentData, CargoWise.Definitions.EventCodes.MessageSent, $"|HBL=-|JOB=RC00000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40");

			matchingReceiveConsignment.PopulateAddOnValue("CIN750InHistoryMessageIdPair1", "STR", $"00000000-0000-0000-0000-000000000000|{documentLog.PK}");
			TestMRJ_MRREvent_CreateCIN750Note(matchingReceiveConsignment);

			Factory.SaveForTesting();
			documentData.Logs.LoadRelatedElements();
			matchingReceiveConsignment.Logs.LoadRelatedElements();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageRejectedCode, referenceNumber: "RC00000001", dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", reason: "Test Reason", requestNumber: "00000000-0000-0000-0000-000000000000", documentName: "CIN In Notification (750)", messageType: CIN750NotificationConstants.CIN750MessageType));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);

			var rcnLog = matchingReceiveConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=RC00000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();

			AssertEquals("Should have only found 1 business object", 1, results.Length);
			AssertEquals("Should have no rcn", 0, results.Count(r => r is WhsItemReceiveConsignment));
			AssertEquals("Should have 1 document data", 1, results.Count(r => r is VisualizerDocumentData));

			AssertEquals("Document Log should be cancelled", true, documentLog.IsCancelled);
			AssertEquals("RCN Log should be cancelled", true, rcnLog.IsCancelled);

			var note = matchingReceiveConsignment.FindOrCreateCIN750StmNote();
			AssertEquals(@"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: 00000000-0000-0000-0000-000000000000 has been rejected. Reason: Test Reason.
CIN 750 Notification:
    Message Type    Result     Ref Type    Ref Code           Enterprise Code    Customs Status    From CTO    CTO CIN Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    In              Succeed    REF         EDIDATRC0000001    EDIDAT             C                 WHTEST      C002            Header               C001
Goods Details:
    Quantity    Weight      Doc Type    Doc Ref    TSD     Description
    5           8.800 KG    T1          CEN1       TST1    Test Description

", note.ST_NoteText);
		}

		public void TestMRJEvent_MatchesReceiveConsignmentAndProcessLogs_NullReferenceNumber()
		{
			var matchingReceiveConsignment = Helper.CreateReceiveConsignment("MatchingRCN", "STD", Data.Warehouse.PK, "RC00000001");

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "CIN750InFromRCN";
			documentData.JDD_ParentID = matchingReceiveConsignment.PK;
			documentData.JDD_ParentTableCode = matchingReceiveConsignment.TablePrefix;

			var documentLog = Helper.CreateStmALog(documentData, CargoWise.Definitions.EventCodes.MessageSent, $"|HBL=-|JOB=RC00000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40");

			matchingReceiveConsignment.PopulateAddOnValue("CIN750InHistoryMessageIdPair1", "STR", $"00000000-0000-0000-0000-000000000000|{documentLog.PK}");
			TestMRJ_MRREvent_CreateCIN750Note(matchingReceiveConsignment);

			Factory.SaveForTesting();
			documentData.Logs.LoadRelatedElements();
			matchingReceiveConsignment.Logs.LoadRelatedElements();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageRejectedCode, referenceNumber: null, dataTargetName: "TransitReceive", dataTargetKey: "RC00000001", reason: "Test Reason", requestNumber: "00000000-0000-0000-0000-000000000000", documentName: "CIN In Notification (750)", messageType: CIN750NotificationConstants.CIN750MessageType));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);

			var rcnLog = matchingReceiveConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=RC00000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();

			AssertEquals("Should have found no business objects", 0, results.Length);
			AssertEquals("Document Log should not be cancelled", false, documentLog.IsCancelled);
			AssertEquals("RCN Log should not be cancelled", false, rcnLog.IsCancelled);
		}

		void TestMRJ_MRREvent_CreateCIN750Note(WhsItemReceiveConsignment rcn)
		{
			var note = rcn.FindOrCreateCIN750StmNote();
			note.ST_NoteText = @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: 00000000-0000-0000-0000-000000000000 has been sent and is waiting for response.
CIN 750 Notification:
    Message Type    Result     Ref Type    Ref Code           Enterprise Code    Customs Status    From CTO    CTO CIN Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    In              Succeed    REF         EDIDATRC0000001    EDIDAT             C                 WHTEST      C002            Header               C001
Goods Details:
    Quantity    Weight      Doc Type    Doc Ref    TSD     Description
    5           8.800 KG    T1          CEN1       TST1    Test Description

";
		}

		#endregion

		#region TestEvent_MatchesReceiveConsignments_ByBillNumber_NoDataContext

		public void TestEvent_MatchesReceiveConsignments_ByBillNumber_NoDataContext()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRLIO";

			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var additionalReference1 = Helper.CreateAdditionalReference(rcn, "113-26363466", WarehouseAdditionalReferenceTypes.Codes.MasterBill, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var additionalReference2 = Helper.CreateAdditionalReference(rcn, "3FR33159700500064", TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference);

			var packages = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(noDataContext);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = Array.Empty<BusinessObject>();
			Factory.SaveForTesting();
			AssertNoExceptionThrown("should not throw exception", () =>
			{
				results = finder.GetLogParentsForEvent(xmlEvent);
			});
			AssertEquals("Should not find any consignment", 0, results.Length);
		}

		#endregion

		#region GetFinder 

		protected override EventParentFinder GetFinder(IXmlImportLogger logger = null)
		{
			return new WhsTransitReceiveConsignmentEventParentFinder(Factory.BOFactory, new WhsTransitReceiveConsignmentDataContextManager(), logger ?? new TestErrorLogger());
		}

		#endregion

		#region Implementation

		WhsItemPackageState AddDLL(WhsItemPackageState packageState)
		{
			var dll = Helper.CreateDispatchLoadList("DLL", Data.Warehouse.PK);
			packageState.WPS_WDL_LoadList = dll.PK;
			if (packageState.WPS_Status != "STA")
			{
				var dispatchConsignment = Helper.CreateDispatchConsignment("ConsignID", Data.Warehouse.PK);
				var dispatchHeader = Helper.CreateDispatchTransportationUnit("DispHead", Data.Warehouse.PK);
				packageState.WPS_WDH_TransitDispatchHeader = dispatchHeader.PK;
				packageState.WPS_SecurityStatus = "SEC";
				packageState.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;
			}
			return packageState;
		}

		BusinessObject[] ReturnXMLResults()
		{
			var xmlEvent = EventDeserializer.Parse(TransitReceiveEventXML);
			var finder = GetFinder();
			var results = finder.GetLogParentsForEvent(xmlEvent);
			return results;
		}

		void AssertBasedOnPackageStatus(BusinessObject[] results, WhsItemPackageState packageState, bool packageShouldBePicked)
		{
			AssertNotEquals("Output should not be null", null, results);
			if (packageShouldBePicked)
			{
				AssertEquals("Should have only found 1 business object", 1, results.Length);
				AssertType<PkgPackage>("Should be a PkgPackage", results[0]);

				var package = (PkgPackage)results[0];
				AssertEquals("Selected the generated packageState", packageState.Package.PK, package.PK);
			}
			else
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have no business object", 0, results.Length);
			}
		}

		#endregion

		#region VolCam Build Scanned Event XML

		const string TestBarcode = "TESTBARCODE123456789";

		string TransitReceiveEventXML => TransitReceiveEventXMLBuilder(Data.Warehouse.WW_WarehouseCode);

		string TransitReceiveEventXMLBuilder(string warehouse) => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>SSC</EventType>
		<EventReference>FAC=TWS|LOC=AUSYD</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>GoodsItemID</Type>
				<Value>{TestBarcode}</Value>
			</Context>
			<Context>
				<Type>WarehouseCode</Type>
				<Value>{warehouse}</Value>
			</Context>
			<Context>
				<Type>VolCamPlatformID</Type>
				<Value>00123</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region TransitReceiveUniversalEventWithAdditionalFields

		string TransitReceiveUniversalEventWithAdditionalFields => TransitReceiveEventXMLBuilderWithAdditionalFields(Data.Warehouse.WW_WarehouseCode);

		string TransitReceiveEventXMLBuilderWithAdditionalFields(string warehouse) => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>SSC</EventType>
		<EventReference>FAC=TWS|LOC=AUSYD</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>GoodsItemID</Type>
				<Value>{TestBarcode}</Value>
			</Context>
			<Context>
				<Type>WarehouseCode</Type>
				<Value>{warehouse}</Value>
			</Context>
			<Context>
				<Type>VolCamPlatformID</Type>
				<Value>00123</Value>
			</Context>
		</ContextCollection>

		<AdditionalFieldsToUpdateCollection>
		  <AdditionalFieldsToUpdate>
			<Type>PkgPackage.KP_F3_NKPackType</Type>
			<Value>BOX</Value>
		  </AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region Customs Entry Status Event XML

		string TransitReceiveEventXMLBuilder(string eventType, string housebill, string customsStatus, string reasonCode, string dataTargetKey = null) => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>{eventType}</EventType>
		<EventReference>|SER=PCS|TYP={customsStatus}|RES={reasonCode}</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
					<Key>{dataTargetKey}</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>3438</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>{housebill}</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>ANLU7766112</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region XML With No Data Context

		const string noDataContext = $@"<UniversalEvent>
	<Event>
		<EventTime>2023-10-24T08:23:20.523</EventTime>
		<EventType>MAA</EventType>
		<EventReference>|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>113-26363466</Value>
			</Context>
			<Context>
				<Type>MAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>MAWBDestinationIATAAirportCode</Type>
				<Value>LIO</Value>
			</Context>
			<Context>
				<Type>AgentsReference</Type>
				<Value>PPPWS  71P</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>FGDSDGFGFDS</Value>
			</Context>
			<Context>
				<Type>HAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>HAWBDestinationIATAAirportCode</Type>
				<Value>LIO</Value>
			</Context>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>3FR33159700500064-B228887</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>IMP</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>3FR33159700500064</Value>
			</Context>
			<Context>
				<Type>OuterPackQty</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>InnerPackQty</Type>
				<Value>50</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region NoHouseBillOrMasterBillNumberEventXML

		const string noHouseBillOrMasterBillNumberEventXML = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S02806818</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>FR1</Code>
        <Country>
          <Code>FR</Code>
          <Name>France</Name>
        </Country>
        <Name>BOLLORE LOGISTICS FRANCE</Name>
      </Company>
      <DataProvider>B52PROFR1</DataProvider>
      <EnterpriseID>B52</EnterpriseID>
      <EventBranch>
        <Code>F01</Code>
        <Name>BOLLORE LOGISTICS DUNKERQUE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>MAA</Code>
        <Description>Message Accepted</Description>
      </EventType>
      <EventUser>
        <Code>~AD</Code>
        <Name>Automated Data Import</Name>
      </EventUser>
      <ServerID>PRO</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2024-04-09T14:38:00</TriggerDate>
      <TriggerDescription>Send CRESA to TW</TriggerDescription>
      <TriggerReference>MST=Goods Received (CRESA)</TriggerReference>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>DTW</Code>
          <Description>Departure Transit Warehouse</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2024-04-09T14:38:00</EventTime>
    <EventType>MAA</EventType>
    <CreatedTime>2024-04-09T14:44:03.313</CreatedTime>
    <EventReference>|CRF=BBE01758065|DEP=Terminal|LOC=FRLEH|MST=Goods Received (CRESA)|RFN=GIR07491104</EventReference>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>HBOLNumber</Type>
        <Value>FR102806818</Value>
      </Context>
      <Context>
        <Type>HBOLOriginUNLOCO</Type>
        <Value>FRLEH</Value>
      </Context>
      <Context>
        <Type>HBOLDestinationUNLOCO</Type>
        <Value>GFDDC</Value>
      </Context>
      <Context>
        <Type Description=""Quick Response Code"">QRC</Type>
        <Value>FRF23|S1|63400</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>

";

		#endregion

		#region Master Air Way Bill Event XML No Location

		const string masterAirWayBillEventXMLNoLocation = $@"<UniversalEvent>
	<Event>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>WarehouseCustomsEntry</Type>
					<Key>3FR33159700500064-B228887</Key>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>DFR</Code>
				<Country>
					<Code>FR</Code>
					<Name>France</Name>
				</Country>
				<Name>FR - FRANCE</Name>
			</Company>
		</DataContext>
		<EventTime>2023-10-24T08:23:20.523</EventTime>
		<EventType>CEN</EventType>
		<EventReference>|CRF=3FR33159700500064|TYP=FGDSDGFGFDS|OTY=1|IPQ=30</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>113-26363466</Value>
			</Context>
			<Context>
				<Type>MAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>MAWBDestinationIATAAirportCode</Type>
				<Value>LIO</Value>
			</Context>
			<Context>
				<Type>AgentsReference</Type>
				<Value>PPPWS  71P</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>FGDSDGFGFDS</Value>
			</Context>
			<Context>
				<Type>HAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>HAWBDestinationIATAAirportCode</Type>
				<Value>LIO</Value>
			</Context>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>3FR33159700500064-B228887</Value>
			</Context>
			<Context>
				<Type>MessageType</Type>
				<Value>IMP</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>3FR33159700500064</Value>
			</Context>
			<Context>
				<Type>OuterPackQty</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>InnerPackQty</Type>
				<Value>50</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion
	}
}
