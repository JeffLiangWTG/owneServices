using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDPickupHeaderController))]
	public class CYDPickupHeaderControllerTest : CYDControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CYDPickupHeader;
		}

		#region ModuleID

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.CYDPickupHeader, new CYDPickupHeaderController().ModuleID);
		}

		#endregion

		#region Override GetBusinessObjectThatIsInTheDatabase

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var pickupHeader = Factory.NewWithValidTestData<CYDPickupHeader>();
			Factory.Save();
			return pickupHeader;
		}

		#endregion
	}
}
