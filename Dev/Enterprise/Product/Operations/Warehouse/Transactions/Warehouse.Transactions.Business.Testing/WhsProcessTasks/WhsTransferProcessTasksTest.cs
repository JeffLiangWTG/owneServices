using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferProcessTasks))]
	public class WhsTransferProcessTasksTest : WhsDocketProcessTasksTest
	{
		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsTransfer(Client, Warehouse);
		}
	}
}
