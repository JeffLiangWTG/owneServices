using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveProcessTasksCollection))]
	public class WhsReceiveProcessTasksCollectionTest : WhsDocketProcessTasksCollectionTest
	{
		#region Overrides

		protected override WhsDocketProcessTasksCollection GetNewCollection()
		{
			return new WhsReceiveProcessTasksCollection((WhsReceive)Docket);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsReceive(Client, Warehouse);
		}

		#endregion
	}
}
