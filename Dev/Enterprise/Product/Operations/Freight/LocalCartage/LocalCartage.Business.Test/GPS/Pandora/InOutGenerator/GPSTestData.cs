using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	public class GPSTestData
	{
		public GPSTestData(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public void CreateVehicleAndRunSheet(string vehicleReg, ZDateTime runSheetStartTime)
		{
			Vehicle = Helper.CreateTruck(vehicleReg);
			RunSheet = Helper.CreateRunSheet(Vehicle);
			RunSheet.EY_StartTime = runSheetStartTime;
			RunSheet.EY_EndTime = runSheetStartTime.AddDays(1);
		}

		public RefEquipment Vehicle { get; private set; }

		public CommonWorkSheet RunSheet { get; private set; }

		public CommonCartageLeg CreateAndAssignLeg(OrgAddress fromAddress, OrgAddress toAddress)
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var leg = cartage.CartageLegs[0];
			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(fromAddress, DocAddressType.LocalCartageMSC).PK;
			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(toAddress, DocAddressType.LocalCartageMSC).PK;
			leg.JU_PlannedPickupTime = RunSheet.EY_StartTime.AddHours(RunSheet.CartageLegs.Count + 1);
			leg.JU_EY_RunSheet = RunSheet.PK;
			return leg;
		}

		GPSTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new GPSTestHelper(Factory));
			}
		}

		GPSTestHelper helper;
	}
}
