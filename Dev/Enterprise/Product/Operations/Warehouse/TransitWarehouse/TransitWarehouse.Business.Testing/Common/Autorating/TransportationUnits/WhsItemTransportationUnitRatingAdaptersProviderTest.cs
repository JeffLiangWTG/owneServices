using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemTransportationUnitRatingAdaptersProviderTest : WhsTransitTestCaseWithFactory
	{
		public void TestGetAdapters_ReceiveTransportationUnit_CannotDecideTransportMode()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.Sea;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();
			var adaptersProvider = new WhsItemTransportationUnitRatingAdaptersProvider<WhsItemReceiveTransportationUnit>(rtu);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);

			AssertEquals("Adapters count", 0, adapters.Count);
			AssertCollectionContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("Revenue: Could not determine Transport Mode."), interactor.errors);
		}

		public void TestGetAdapters_DispatchTransportationUnit_CannotDecideTransportMode()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll2.WDL_TransportMode = TransportModes.Sea;
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: dtu);
			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();
			var adaptersProvider = new WhsItemTransportationUnitRatingAdaptersProvider<WhsItemDispatchTransportationUnit>(dtu);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);

			AssertEquals("Adapters count", 0, adapters.Count);
			AssertCollectionContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("Cost: Could not determine Transport Mode."), interactor.errors);
		}

		public void TestGetAdapters_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.AirSea;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();
			var adaptersProvider = new WhsItemTransportationUnitRatingAdaptersProvider<WhsItemReceiveTransportationUnit>(rtu);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);
			AssertEquals(1, adapters.Count);
			AssertEquals(typeof(WhsItemTransportationUnitRatingAdapter<WhsItemReceiveTransportationUnit>), adapters[0].GetType());
		}

		public void TestGetAdapters_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll2.WDL_TransportMode = TransportModes.Air;
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: dtu);
			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();
			var adaptersProvider = new WhsItemTransportationUnitRatingAdaptersProvider<WhsItemDispatchTransportationUnit>(dtu);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);
			AssertEquals(1, adapters.Count);
			AssertEquals(typeof(WhsItemTransportationUnitRatingAdapter<WhsItemDispatchTransportationUnit>), adapters[0].GetType());
		}
	}
}
