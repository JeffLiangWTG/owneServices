using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderController))]
	public class CYDAdHocServiceOrderControllerTest : CYDControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CYDAdHocServiceOrder;
		}

		#region ModuleID

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.CYDAdHocServiceOrder, new CYDAdHocServiceOrderController().ModuleID);
		}

		#endregion

		#region Override GetBusinessObjectThatIsInTheDatabase

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var adHocServiceOrder = Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
			Factory.Save();
			return adHocServiceOrder;
		}

		#endregion
	}
}
