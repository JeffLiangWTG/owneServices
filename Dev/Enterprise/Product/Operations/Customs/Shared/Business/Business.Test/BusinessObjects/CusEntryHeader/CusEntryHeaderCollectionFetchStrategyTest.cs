using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class CusEntryHeaderCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewActiveTableFetchHints()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var header1 = Factory.New<CusEntryHeader>();
			header1.CH_JE = declaration.PK;
			var entryLine1 = header1.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 12m);

			var header2 = Factory.New<CusEntryHeader>();
			header2.CH_JE = declaration.PK;
			var entryLine2 = header2.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 33m);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var parent = factory.Load<BaseJobDeclaration>(declaration.PK);
			var collection = new CusEntryHeaderCollection<CusEntryHeader>(parent, factory);
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertEquals(0, factory.ActiveFetchHintsForTable(CusEntryLineFeeSchema.Constants.TableName));

			var strategy = new CusEntryHeaderCollectionFetchStrategy(collection);
			var tc1 = new TableColumn(CusEntryHeaderSchema.Constants.TableName, CusEntryHeader.Schema.GSTAmount);
			strategy.FetchForView(collection.ToArray(), new[] { tc1 });
			AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryLineFeeSchema.Constants.TableName));
		}
	}
}
