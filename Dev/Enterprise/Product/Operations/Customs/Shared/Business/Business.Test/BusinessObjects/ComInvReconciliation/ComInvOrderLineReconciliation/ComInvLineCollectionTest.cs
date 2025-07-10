using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvLineCollection))]
	sealed class ComInvLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			ComInvLineCollection collection = new ComInvLineCollection(Factory);
			ComInvOrderLineReconciliation comInvLine = collection.AddNew();
			comInvLine.JO_LineNo = 1;
			comInvLine.JO_Quantity = 1m;

			ComInvOrderLineReconciliation comInvLine2 = collection.AddNew();
			comInvLine2.JO_LineNo = 2;
			comInvLine2.JO_Quantity = 1m;

			ComInvOrderLineReconciliation comInvLine3 = collection.AddNew();
			comInvLine3.JO_LineNo = 2;
			comInvLine3.JO_Quantity = 1m;

			AssertNotNull(collection[0]);
			AssertEquals(comInvLine, collection[0]);
			AssertNotNull(collection[1]);
			AssertEquals(comInvLine2, collection[1]);
			AssertNotNull(collection[2]);
			AssertEquals(comInvLine3, collection[2]);
		}

		public void TestAllowNew()
		{
			ComInvLineCollection collection = new ComInvLineCollection(Factory);
			AssertEquals(false, collection.AllowNew);
		}

		public void TestCopyInvoiceLinesPersistentValuesToAnotherFactory()
		{
			ComInvLineCollection collection = new ComInvLineCollection(Factory);
			ComInvOrderLineReconciliation comInvLine = collection.AddNew();
			comInvLine.JO_LineNo = 1;
			comInvLine.JO_Quantity = 13m;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ComInvOrderLineReconciliation newComInvLine = newFactory.Load<ComInvOrderLineReconciliation>(comInvLine.PK);
			AssertNull(newComInvLine);

			collection.CopyPersistentValuesToAnotherFactory(newFactory);
			newComInvLine = newFactory.Load<ComInvOrderLineReconciliation>(comInvLine.PK);
			AssertNotNull(newComInvLine);
			AssertEquals(comInvLine.JO_Quantity, newComInvLine.JO_Quantity);

			comInvLine.JO_Quantity = 43m;
			AssertEquals(false, comInvLine.JO_Quantity == newComInvLine.JO_Quantity);

			collection.CopyPersistentValuesToAnotherFactory(newFactory);
			newComInvLine = newFactory.Load<ComInvOrderLineReconciliation>(comInvLine.PK);
			AssertNotNull(newComInvLine);
			AssertEquals(comInvLine.JO_Quantity, newComInvLine.JO_Quantity);
			AssertEquals(43m, newComInvLine.JO_Quantity);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ComInvLineCollection(Factory);
		}

		#endregion
	}
}
