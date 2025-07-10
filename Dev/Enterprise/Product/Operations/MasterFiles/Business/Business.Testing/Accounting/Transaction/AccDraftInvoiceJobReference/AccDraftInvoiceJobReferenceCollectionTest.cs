using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceJobReferenceCollection))]
	sealed class AccDraftInvoiceJobReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var jobReferenceCollection = GetCollectionToTest();
			Assert(!jobReferenceCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var jobReferenceCollection = GetCollectionToTest();
			Assert(!jobReferenceCollection.AllowRemove);
		}
		protected override BusinessObjectCollection GetCollectionToTest() => new AccDraftInvoiceJobReferenceCollection(Factory);
	}
}
