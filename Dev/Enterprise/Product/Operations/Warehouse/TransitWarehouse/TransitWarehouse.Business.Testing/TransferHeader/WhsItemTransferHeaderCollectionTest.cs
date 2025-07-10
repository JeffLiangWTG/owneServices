using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemTransferHeaderCollection))]
	public class WhsItemTransferHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemTransferHeaderCollection>
	{
		#region Implementation

		protected override WhsItemTransferHeaderCollection GetCollectionToTest()
		{
			return new WhsItemTransferHeaderCollection(Factory);
		}

		#endregion
	}
}
