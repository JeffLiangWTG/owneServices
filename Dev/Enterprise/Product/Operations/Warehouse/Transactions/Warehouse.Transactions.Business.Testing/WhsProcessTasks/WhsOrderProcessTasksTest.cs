using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderProcessTasks))]
	public class WhsOrderProcessTasksTest : WhsDocketProcessTasksTest
	{
		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsOrder(Client, Warehouse);
		}
	}
}
