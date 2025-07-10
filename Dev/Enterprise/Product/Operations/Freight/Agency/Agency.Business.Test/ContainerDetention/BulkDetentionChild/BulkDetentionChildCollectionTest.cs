using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BulkDetentionChildCollection))]
	internal class BulkDetentionChildCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BulkDetentionChildCollection>
	{
		public void TestAllowAddRemove()
		{
			BulkDetentionChildCollection collection = new BulkDetentionChildCollection(Factory);
			AssertEquals("Allow New", false, collection.AllowNew);
			AssertEquals("Allow Remove", false, collection.AllowRemove);
		}

		#region Implementation
		protected override BulkDetentionChildCollection GetCollectionToTest()
		{
			return new BulkDetentionChildCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BulkDetentionChild(Factory);
		}
		#endregion
	}
}
