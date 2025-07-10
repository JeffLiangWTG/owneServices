using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ReceiveController))]
	internal class ReceiveControllerBasherTest : WhsControllerBaseBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsReceive;
		}

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.None, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsReceiveView, Controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.WhsReceiveNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsReceiveEdit, Controller.GetCheckPointForEdit(null));
		}

		#endregion

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var receive = helper.CreateWhsReceive(helper.CreateClient().PK, helper.CreateWarehouse("WHS1").PK);
			Factory.Save();
			return receive;
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<WhsReceive>();
			bizObj.Client.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<WhsReceive>.AssertController(new ReceiveController(), bizObj, Env.Security.WhsReceiveCRMSecurity);
		}
	}
}
