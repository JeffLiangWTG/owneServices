using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderProcessTasksCollection))]
	public class WhsOrderProcessTasksCollectionTest : WhsDocketProcessTasksCollectionTest
	{
		#region Overrides

		protected override WhsDocketProcessTasksCollection GetNewCollection()
		{
			return new WhsOrderProcessTasksCollection((WhsOrder)Docket);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsOrder(Client, Warehouse);
		}

		#endregion
	}
}
