using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceJobClusterCollection))]
	sealed class AccDraftInvoiceJobClusterCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var jobClusterCollection = GetCollectionToTest();
			Assert(!jobClusterCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var jobClusterCollection = GetCollectionToTest();
			Assert(!jobClusterCollection.AllowRemove);
		}
		protected override BusinessObjectCollection GetCollectionToTest() => new AccDraftInvoiceJobClusterCollection(Factory);
	}
}
