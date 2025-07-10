using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsHoldOrderLineCollection))]
	public class WhsHoldOrderLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsHoldOrderLineCollection>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsHoldOrderLine(Factory);
		}

		protected override WhsHoldOrderLineCollection GetCollectionToTest()
		{
			return new WhsHoldOrderLineCollection(Factory);
		}

		#endregion
	}
}
