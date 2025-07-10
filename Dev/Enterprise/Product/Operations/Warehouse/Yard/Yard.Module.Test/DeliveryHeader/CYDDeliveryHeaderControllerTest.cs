using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDDeliveryHeaderController))]
	public class CYDDeliveryHeaderControllerTest : CYDControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CYDDeliveryHeader;
		}

		#region ModuleID

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.CYDDeliveryHeader, new CYDDeliveryHeaderController().ModuleID);
		}

		#endregion

		#region Override GetBusinessObjectThatIsInTheDatabase

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var deliveryHeader = Factory.NewWithValidTestData<CYDDeliveryHeader>();
			Factory.Save();
			return deliveryHeader;
		}

		#endregion
	}
}
