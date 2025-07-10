using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderController))]
	internal class OrderControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestID()
		{
			var controller = new OrderController();
			AssertEquals(ControllerIDs.WhsOrder, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = new OrderController();
			AssertEquals(ModuleIDs.WhsOrder, controller.ModuleID);
		}

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.None, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsOrderView, Controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.WhsOrderNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsOrderEdit, Controller.GetCheckPointForEdit(null));
		}

		#endregion

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new OrderController();
			AssertEquals(typeof(WhsOrder), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestBillingPlugInAuditSecurity()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			using (var form = (OrderEntryForm)controller.ShowNewForm())
			{
				AssertEquals("AuditSecurity", Env.Security.WhsOrderAuditBilling, ((IJobInvoicingPlugIn)form.BusinessEntity).InvoicingSupporter.AuditSecurity);
			}
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<WhsOrder>();
			bizObj.Client.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<WhsOrder>.AssertController(new OrderController(), bizObj, Env.Security.WhsOrderCRMSecurity);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsOrder;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(helper.CreateClient().PK, helper.CreateWarehouse("WHS1").PK);
			Factory.Save();
			return order;
		}
	}
}
