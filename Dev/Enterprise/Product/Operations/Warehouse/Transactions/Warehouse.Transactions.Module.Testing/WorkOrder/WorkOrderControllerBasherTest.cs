using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WorkOrderController))]
	internal class WorkOrderControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestID()
		{
			var controller = new WorkOrderController();
			AssertEquals(ControllerIDs.WhsWorkOrder, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = new WorkOrderController();
			AssertEquals(ModuleIDs.WhsWorkOrder, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new WorkOrderController();
			AssertEquals(typeof(WhsWorkOrder), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestBillingPlugInAuditSecurity()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			using (var form = (WorkOrderEntryForm)controller.ShowNewForm())
			{
				AssertEquals("AuditSecurity", Env.Security.WhsWorkOrderAuditBilling, ((IJobInvoicingPlugIn)form.BusinessEntity).InvoicingSupporter.AuditSecurity);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsWorkOrder;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var docket = helper.CreateWhsWorkOrder(helper.CreateClient().PK, helper.CreateWarehouse("WHS1").PK);
			Factory.Save();
			return docket;
		}
	}
}
