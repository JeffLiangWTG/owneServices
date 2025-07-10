using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemTransferLineCollection))]
	public class WhsItemTransferLineCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemTransferLineCollection>
	{
		#region Implementation

		protected override WhsItemTransferLineCollection GetCollectionToTest()
		{
			return new WhsItemTransferLineCollection(Factory);
		}

		#endregion
	}
}
