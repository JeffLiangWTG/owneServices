using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentProcessTasksCollection))]
	public class WhsAdjustmentProcessTasksCollectionTest : WhsDocketProcessTasksCollectionTest
	{
		#region Overrides

		protected override WhsDocketProcessTasksCollection GetNewCollection()
		{
			return new WhsAdjustmentProcessTasksCollection((WhsAdjustment)Docket);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsAdjustment(Client, Warehouse);
		}

		#endregion
	}
}
