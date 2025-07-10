using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryLineFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			dec = newFactory.Load<JobDeclaration>(dec.PK);
			entry = dec.CustomsEntryHeaders[0];
			entryLine = entry.AllEntryLines[0];
			var tableSelects = newFactory.TableSelects;
			AssertEquals("FetchForLoad Hints should be added for CusEntryLine", 1, newFactory.GetTableHitCount(CusEntryLineSchema.Constants.TableName));
		}
	}
}
