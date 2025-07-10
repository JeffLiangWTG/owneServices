using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	public class CusEntryHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var header1 = Factory.New<CusEntryHeader>();
			header1.CH_JE = Master.PK;
			var entryLine1 = header1.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 12m);

			var header2 = Factory.New<CusEntryHeader>();
			header2.CH_JE = Master.PK;
			var entryLine2 = header2.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 33m);
			Factory.Save();

			TestFetchForView(header1, header2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			var parent = factory.Load<BaseJobDeclaration>(Master.PK);
			return new CusEntryHeaderCollection<CusEntryHeader>(parent, factory);
		}

		BaseJobDeclaration Master
		{
			get
			{
				return master ?? (master = Factory.New<BaseJobDeclaration>());
			}
		}
		BaseJobDeclaration master;
	}
}
