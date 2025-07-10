using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class OutturnProviderForDCNTest : TestCaseWithFactory
	{
		#region TestOverriddenAviationSecurityInspectionType

		public void TestOverriddenAviationSecurityInspectionType()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var receiveTransportationUnit = SetupReceiveTransportationUnitForTesting(warehouse, "Good Vehicle");
			var packageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, receiveUnit: receiveTransportationUnit);
			var packageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, receiveUnit: receiveTransportationUnit);
			packageState1.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			packageState2.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Required;
			Factory.Save();

			var package1 = packageState1.Package;
			IOutturnProvider outturnProvider1 = new OutturnProviderForDCN(package1);
			var package2 = packageState2.Package;
			IOutturnProvider outturnProvider2 = new OutturnProviderForDCN(package2);

			AssertEquals("APP", outturnProvider1.OverriddenAviationSecurityInspectionType);
			AssertEquals(ZString.Empty, outturnProvider2.OverriddenAviationSecurityInspectionType);
		}

		#endregion

		WhsItemReceiveTransportationUnit SetupReceiveTransportationUnitForTesting(WhsWarehouse warehouse, ZString reference, string vehicleNumber = "V1")
		{
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock1", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock1-1-1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit(reference, warehouse.PK, location.PK, vehicleNumber);
			receiveTransportationUnit.WRH_VehicleReference = vehicleNumber;

			return receiveTransportationUnit;
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(this.Factory));
		WhsTransitTestHelper helper;
	}
}
