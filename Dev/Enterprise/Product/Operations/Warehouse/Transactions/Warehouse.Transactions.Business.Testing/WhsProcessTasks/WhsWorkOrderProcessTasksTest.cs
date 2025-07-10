using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderProcessTasks))]
	public class WhsWorkOrderProcessTasksTest : WhsDocketProcessTasksTest
	{
		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsWorkOrder(Client, Warehouse);
		}
	}
}
