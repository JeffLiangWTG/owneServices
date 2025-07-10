using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.DataTransfer.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateBusinessObjectFinderForDCN;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitPackageStateBusinessObjectFinderForDCNTest : TransitUniversalTestCase
	{
		#region TestFindExistingPackageState

		public void TestFindExistingPackageState()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RC0001", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC0001", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.SaveForTesting();

			var finder = new WhsTransitPackageStateBusinessObjectFinderForDCN(dcn, Logger, Factory, FindOption.ByDCN);
			var existingPackageState = finder.Find();
			AssertEquals("There is 1 existing package state", 1, existingPackageState.Count());
		}

		#endregion

		#region TestFindByPackageID

		public void TestFindByPackageID()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			Factory.SaveForTesting();

			var dcn = Factory.New<WhsItemDispatchConsignment>();
			dcn.WDC_WW_Warehouse = Data.Warehouse.PK;
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDCN(dispatchConsignmentDataObject, dcn, Logger, Factory, FindOption.ByPackageID);
			var packageStates = finder.Find();
			AssertNotNull(packageStates);
			AssertEquals("There are 2 packages matched by Package ID", 2, packageStates.Count());
		}

		#endregion

		#region TestFindByPackingLineID

		public void TestFindByPackingLineID()
		{
			var consignmentID = "CONID123";
			var consignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds(consignmentID, new Tuple<string, int>("PLT", 2), new Tuple<string, int>("BOX", 3));
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var package1 = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, "PLT")).First();
			package1.KP_ExternalReference = "WTLLWC00000001";
			var package2 = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, "BOX")).First();
			package2.KP_ExternalReference = "WTLLWC00000002";
			Factory.SaveForTesting();

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = "PLT" }, PackingLineID = "WTLLWC00000001", ReferenceNumber = string.Empty };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, PackType = new PackageType { Code = "BOX" }, PackingLineID = "WTLLWC00000002", ReferenceNumber = string.Empty };
			var packingLineCollection = new DataObjectList<PackingLine>() { packingLine1, packingLine2 };

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(consignmentID);
			dispatchConsignmentDataObject.SetPackingLineCollection(() => packingLineCollection);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dcn = Factory.New<WhsItemDispatchConsignment>();
			dcn.WDC_WW_Warehouse = Data.Warehouse.PK;
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDCN(dispatchConsignmentDataObject,dcn, Logger, Factory, FindOption.ByPackingLineID);
			var packageStates = finder.Find();
			AssertNotNull(packageStates);
			AssertEquals("There are 2 packages matched by PackingLine ID", 2, packageStates.Count());
		}

		#endregion

		#region TestFindByRCNQty

		public void TestFindByRCNQty()
		{
			var consignmentID = "CONID123";
			var consignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds(consignmentID, new Tuple<string, int>("PLT", 2), new Tuple<string, int>("BOX", 3));
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = "PLT" }, PackingLineID = "WTLLWC00000001" };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, PackType = new PackageType { Code = "BOX" }, PackingLineID = "WTLLWC00000002" };
			var packingLineCollection = new DataObjectList<PackingLine>() { packingLine1, packingLine2 };

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(consignmentID);
			dispatchConsignmentDataObject.SetPackingLineCollection(() => packingLineCollection);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dcn = Factory.New<WhsItemDispatchConsignment>();
			dcn.WDC_WW_Warehouse = Data.Warehouse.PK;

			var dcnWarningInfo = new Dictionary<PackingLine, DispatchInstructionWarningInfo>();
			Func<UniversalShipment, WhsItemReceiveConsignment> findRCN = sourceDataObject =>
			{
				return receiveConsignment;
			};

			var finder = new WhsTransitPackageStateBusinessObjectFinderForDCN(dispatchConsignmentDataObject, dcn, Logger, Factory, findRCN, dcnWarningInfo, FindOption.ByRCN);
			var packageStates = finder.Find();
			AssertNotNull(packageStates);
			AssertEquals("There are 2 packages matched by PackingLine ID", 2, packageStates.Count());
		}

		#endregion

		#region TestThrowExceptionWhenParamIsNull

		public void TestThrowExceptionWhenParamIsNull()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DC0001", warehouse.PK);
			Factory.SaveForTesting();

			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForDCN(null, null, null, FindOption.ByDCN));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForDCN(null, dcn, Logger, Factory, FindOption.ByPackageID));
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
