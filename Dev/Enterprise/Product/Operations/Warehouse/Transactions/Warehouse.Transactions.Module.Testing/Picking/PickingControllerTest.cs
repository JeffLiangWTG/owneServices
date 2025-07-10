using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickingController))]
	public class PickingControllerTest : WhsControllerBaseBasherTest
	{
		#region TestNewPickHasPickType

		public void TestNewPickHasPickType() => AssertNewPickHasPickType(new PickingControllerForTest(), PickType.Codes.Order);
		public void TestNewPickHasPickType_Order() => AssertNewPickHasPickType(new PickingControllerForTest(PickType.Codes.Order), PickType.Codes.Order);
		public void TestNewPickHasPickType_HeldInventoryOrder() => AssertNewPickHasPickType(new PickingControllerForTest(PickType.Codes.HeldInventoryOrder), PickType.Codes.HeldInventoryOrder);
		public void TestNewPickHasPickType_WorkOrder() => AssertNewPickHasPickType(new PickingControllerForTest(PickType.Codes.WorkOrder), PickType.Codes.WorkOrder);
		public void TestNewPickHasPickType_DynamicWorkOrder() => AssertNewPickHasPickType(new PickingControllerForTest(PickType.Codes.DynamicWorkOrder), PickType.Codes.DynamicWorkOrder);

		void AssertNewPickHasPickType(PickingControllerForTest controller, string expectedPickType)
		{
			var pick = (WhsPick)controller.GetNewBusinessEntityInLocalFactoryForTest();

			AssertEquals(expectedPickType, pick.WP_PickType);
		}

		#endregion

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.None, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsPickingView, Controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.WhsPickingNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsPickingEdit, Controller.GetCheckPointForEdit(null));
		}

		#endregion

		#region Implementation

		class PickingControllerForTest : PickingController
		{
			public PickingControllerForTest()
				: base()
			{ }
			public PickingControllerForTest(string pickType)
				: base(pickType)
			{ }

			public IBusiness GetNewBusinessEntityInLocalFactoryForTest()
			{
				return base.GetNewBusinessEntityInLocalFactory();
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsPicking;
		}

		#endregion
	}
}
