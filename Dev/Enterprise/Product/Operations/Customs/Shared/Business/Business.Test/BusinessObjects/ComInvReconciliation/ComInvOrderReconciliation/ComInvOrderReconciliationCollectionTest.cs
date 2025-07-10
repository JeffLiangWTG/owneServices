using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvOrderReconciliationCollection))]
	sealed class ComInvOrderReconciliationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			ComInvOrderReconciliationCollection collection = new ComInvOrderReconciliationCollection(Factory);
			ComInvOrderReconciliation comInvOrder = collection.AddNew();
			comInvOrder.JD_OrderNumber = "ORDER1";

			ComInvOrderReconciliation comInvOrder2 = collection.AddNew();
			comInvOrder2.JD_OrderNumber = "ORDER2";

			ComInvOrderReconciliation comInvOrder3 = collection.AddNew();
			comInvOrder3.JD_OrderNumber = "ORDER3";

			AssertNotNull(collection[0]);
			AssertEquals(comInvOrder, collection[0]);
			AssertNotNull(collection[1]);
			AssertEquals(comInvOrder2, collection[1]);
			AssertNotNull(collection[2]);
			AssertEquals(comInvOrder3, collection[2]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ComInvOrderReconciliationCollection(Factory);
		}
	}
}
