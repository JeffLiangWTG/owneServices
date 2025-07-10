using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderLineModule))]
	public class OrderLineModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsOrderLine;
		}

		protected override void AddNewForLoadedBoundListCheck(IBusinessObjectCollection collection)
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var orderLIne = order.Lines.AddNew();
			collection.Add(orderLIne);
		}
	}
}
