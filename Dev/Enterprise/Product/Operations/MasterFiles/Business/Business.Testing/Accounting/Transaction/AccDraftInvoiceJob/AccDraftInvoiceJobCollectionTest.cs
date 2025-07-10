using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceJobCollection))]
	sealed class AccDraftInvoiceJobCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var jobCollection = GetCollectionToTest();
			Assert(!jobCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var jobCollection = GetCollectionToTest();
			Assert(!jobCollection.AllowRemove);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AccDraftInvoiceJobCollection(Factory);
	}
}
