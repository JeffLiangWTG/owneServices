using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrdersModule))]
	public class OrdersModule_Test : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (OrdersModule module = new OrdersModule())
			{
				AssertEquals(ModuleIDs.Orders, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (OrdersModule module = new OrdersModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Order business context should be returned", BusinessContext.Order, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		[RequiresSTA]
		public void TestChildEditableServiceSetToOrder()
		{
			using (var module = new OrdersModuleForTest())
			{
				AssertEquals(ChildEditableServiceStates.Order, ChildEditableService.GetState(module.GridCollection.Factory));

				module.PerformSearch();
				AssertEquals(ChildEditableServiceStates.Order, ChildEditableService.GetState(module.Factory));

				module.PerformSearch();
				AssertEquals(ChildEditableServiceStates.Order, ChildEditableService.GetState(module.Factory));
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Orders;
		}

		[RequiresSTA]
		public void TestBulkUpdateFormUsesOwnFactory()
		{
			using (var module = new OrdersModuleForTest())
			{
				var updateMenuItem = module.ContextMenu.FindByText("Bulk Update") as ZMenuItem;
				updateMenuItem.PerformClick();

				using (var bulkUpdateForm = Application.OpenForms.OfType<OrderDetailsBulkUpdateForm>().FirstOrDefault())
				{
					AssertNotNull(bulkUpdateForm);
					AssertNotEquals("Check that the Factory ID is different", module.Factory, bulkUpdateForm.BusinessEntity.Factory);
				}
			}
		}
	}

	[TestExcludeZFilterGridModulesAllHaveModuleBashers]
	class OrdersModuleForTest : OrdersModule
	{
		public void PerformSearch() => base.PerformSearch();

		public new BusinessObjectFactory Factory => base.Factory;

		public new MenuItem[] ContextMenu => base.ContextMenu;
	}
}
