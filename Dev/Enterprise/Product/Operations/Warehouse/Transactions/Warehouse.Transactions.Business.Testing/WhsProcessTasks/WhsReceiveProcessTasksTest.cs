using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveProcessTasks))]
	public class WhsReceiveProcessTasksTest : WhsDocketProcessTasksTest
	{
		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsReceive(Client, Warehouse);
		}
	}
}
