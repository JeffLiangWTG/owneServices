using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferCollection))]
	class WhsTransferCollectionTest : WhsDocketCollectionTestCase<WhsTransferCollection>
	{
		#region Implementation

		protected override WhsTransferCollection GetCollectionToTest()
		{
			return new WhsTransferCollection(Factory);
		}

		protected override string[] DocketTypesForTest
		{
			get { return new[] { DocketType.Codes.Transfer }; }
		}

		#endregion
	}
}
