using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CusEntryNumCollectionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIndexer()
		{
			TestCollection.AddNew();
			BusinessObject bO = TestCollection[0];
		}

		public void TestGetFirstNonEmpty()
		{
			CusEntryNumCollection collection = new CusEntryNumCollection(Factory);
			AssertNull(collection.GetFirstNonEmpty());

			CusEntryNumber cusEntryNum1 = collection.AddNew();
			cusEntryNum1.CE_EntryNum = ZString.Empty;
			cusEntryNum1.CE_EntryType = ZString.Empty;
			AssertNull(collection.GetFirstNonEmpty());

			CusEntryNumber cusEntryNum2 = collection.AddNew();
			cusEntryNum2.CE_EntryNum = "666";
			cusEntryNum2.CE_EntryType = "NOB";
			AssertEquals(cusEntryNum2, collection.GetFirstNonEmpty());
		}

		#region Implementation

		CusEntryNumCollection TestCollection;
		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new CusEntryNumCollection(Factory);
		}

		#endregion
	}
}
