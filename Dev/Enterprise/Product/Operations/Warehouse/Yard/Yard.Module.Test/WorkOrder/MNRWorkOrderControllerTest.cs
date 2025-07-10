using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRWorkOrderController))]
	public class MNRWorkOrderControllerTest : CYDControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MNRWorkOrder;
		}

		#region ModuleID

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.MNRWorkOrder, new MNRWorkOrderController().ModuleID);
		}

		#endregion

		#region Override GetBusinessObjectThatIsInTheDatabase

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var workOrderHeader = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			Factory.Save();
			return workOrderHeader;
		}

		#endregion
	}
}
