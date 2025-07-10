using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveCollection))]
	public class WhsReceiveCollectionTest : WhsDocketCollectionTestCase<WhsReceiveCollection>
	{
		#region Implementation

		protected override WhsReceiveCollection GetCollectionToTest()
		{
			return new WhsReceiveCollection(Factory);
		}

		protected override string[] DocketTypesForTest
		{
			get { return new[] { DocketType.Codes.Receive }; }
		}

		#endregion
	}
}
