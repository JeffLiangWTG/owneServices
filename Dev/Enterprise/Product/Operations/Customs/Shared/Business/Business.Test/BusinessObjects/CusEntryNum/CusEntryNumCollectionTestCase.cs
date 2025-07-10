using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryNumCollectionTestCase : TestCaseWithFactory
	{
		public void TestDefaultAdditionalFilterAlwaysReturnsZeroRows()
		{
			Factory.New(typeof(CusEntryNumber));
			CusEntryNumCollection collection = new CusEntryNumCollection(Factory);
			collection.Load();
			AssertEquals(0, collection.Count);
		}
	}
}
