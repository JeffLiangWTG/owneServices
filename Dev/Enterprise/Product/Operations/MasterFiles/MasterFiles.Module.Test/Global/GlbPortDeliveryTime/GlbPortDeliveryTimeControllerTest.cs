using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbPortDeliveryTimeController))]
	sealed class GlbPortDeliveryTimeControllerTest : ZControllerBasherTest
	{
		public void TestCheckpoints()
		{
			BusinessObject obj = GetBusinessObjectThatIsInTheDatabase();
			AssertEquals(Env.Security.GlbPortDeliveryTimeView, Controller.GetCheckPointForView(obj));
			AssertEquals(Env.Security.GlbPortDeliveryTimeNew, Controller.GetCheckPointForNew(obj));
			AssertEquals(Env.Security.GlbPortDeliveryTimeEdit, Controller.GetCheckPointForEdit(obj));
			AssertEquals(Env.Security.GlbPortDeliveryTimeDelete, Controller.GetCheckPointForDelete(obj));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbPortDeliveryTime;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			GlbPortDeliveryTime deliveryTime = Factory.NewWithValidTestData<GlbPortDeliveryTime>();
			Factory.Save();
			return deliveryTime;
		}
	}
}
