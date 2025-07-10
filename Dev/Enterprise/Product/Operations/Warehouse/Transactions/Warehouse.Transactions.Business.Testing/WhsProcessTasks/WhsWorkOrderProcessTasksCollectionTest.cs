using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderProcessTasksCollection))]
	public class WhsWorkOrderProcessTasksCollectionTest : WhsDocketProcessTasksCollectionTest
	{
		#region Overrides

		protected override WhsDocketProcessTasksCollection GetNewCollection()
		{
			return new WhsWorkOrderProcessTasksCollection((WhsWorkOrder)Docket);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsWorkOrder(Client, Warehouse);
		}

		#endregion
	}
}
