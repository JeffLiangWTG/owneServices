using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferProcessTasksCollection))]
	public class WhsTransferProcessTasksCollectionTest : WhsDocketProcessTasksCollectionTest
	{
		#region Overrides

		protected override WhsDocketProcessTasksCollection GetNewCollection()
		{
			return new WhsTransferProcessTasksCollection((WhsTransfer)Docket);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsTransfer(Client, Warehouse);
		}

		#endregion
	}
}
