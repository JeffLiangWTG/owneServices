using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderLineController))]
	public class OrderLineControllerBasherTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsOrderLine;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderLine.FillWithValidTestData();
			Factory.Save();
			return orderLine;
		}
	}
}
