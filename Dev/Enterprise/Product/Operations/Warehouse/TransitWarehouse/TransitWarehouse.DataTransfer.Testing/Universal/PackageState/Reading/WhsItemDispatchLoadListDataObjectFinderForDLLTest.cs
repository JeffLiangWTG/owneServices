using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsItemDispatchLoadListDataObjectFinderForDLLTest : TransitUniversalTestCase
	{
		#region TestFindPackageStatesByDLL

		public void TestFindPackageStatesByDLL()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL002", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var packageState3 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll2);
			Factory.SaveForTesting();

			var packageStates = new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, new List<WhsItemDispatchLoadList> { dll1 }).Find().ToList();

			AssertEquals("count of packageStates is 2", 2, packageStates.Count);
		}

		#endregion

		#region TestFindPackageStatesByDCN

		public void TestFindPackageStatesByDCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DC001", warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DC002", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn1);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn1);
			var packageState3 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn2);
			Factory.SaveForTesting();

			var packageStates = new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, new List<WhsItemDispatchConsignment> { dcn1 }).Find().ToList();

			AssertEquals("count of packageStates is 2", 2, packageStates.Count);
		}

		#endregion

		#region TestFindPackageStatesForContainer

		public void TestFindPackageStatesForContainer()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location.PK);
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DC001", warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DC002", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn1);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn1);

			var hu1 = Helper.CreateHandlingUnitPackage("HU1", Helper.CreatePackageHandlingUnit(), rtu1);
			var subPackageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-Sub-1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn2);

			Helper.PackPackageIntoHandlingUnit(hu1, subPackageState1, DateTime.Now, "~BP", topHandlingUnit: hu1);

			var packingLine1 = Helper.CreatePackingLine("PKG-1", "WTLLWC00000001", Constants.PkgUnit.Package);
			var packingLineSub1 = Helper.CreatePackingLine("PKG-Sub-1", "WTLLWC00000001", Constants.PkgUnit.Package);
			Factory.SaveForTesting();

			var packageStates = new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, new List<WhsItemDispatchConsignment> { dcn1 }, [packingLine1, packingLineSub1]).Find().ToList();

			AssertEquals("count of packageStates is 2", 2, packageStates.Count);

			AssertCollectionContains("packageStates contains PKG-1 and PKG-Sub-1", packageStates, p => p.Package.KP_PackageID == "PKG-1" || p.Package.KP_PackageID == "PKG-Sub-1");
		}

		#endregion

		#region TestThrowExceptionWhenParamIsNull

		public void TestThrowExceptionWhenParamIsNull()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dll1 = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DC001", warehouse.PK);
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var packingLine = Helper.CreatePackingLine("PKG-1", "WTLLWC00000001", Constants.PkgUnit.Package);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn1);
			Factory.SaveForTesting();

			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForDLL(null, dlls: new List<WhsItemDispatchLoadList> { dll1 }));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForDLL(null, dcns: new List<WhsItemDispatchConsignment> { dcn1 }));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, dlls: null));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, dcns: null));
			AssertNoExceptionThrown(() => new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, dlls: new List<WhsItemDispatchLoadList> { dll1 }));
			AssertNoExceptionThrown(() => new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, dcns: new List<WhsItemDispatchConsignment> { dcn1 }));
			AssertNoExceptionThrown(() => new WhsTransitPackageStateBusinessObjectFinderForDLL(Factory, new List<WhsItemDispatchConsignment> { dcn1 }, [packingLine]));
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
