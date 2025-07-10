using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ModuleCusWHSOperatorTransactionsCollection))]
	sealed class ModuleCusWHSOperatorTransactionsCollectionTest : ActiveBusinessObjectCollectionTestCase<ModuleCusWHSOperatorTransactionsCollection>
	{
		protected override ModuleCusWHSOperatorTransactionsCollection GetCollectionToTest() => new ModuleCusWHSOperatorTransactionsCollection(Factory, GlbCompany.CurrentCompany);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = CreateOperatorTransaction(GlbCompany.CurrentCompany, 1);
			Factory.Save();
			return result;
		}

		public void TestOnlyLoadOperatorTransactionsFromCurrentCompany()
		{
			CreateOperatorTransaction(GlbCompany.CurrentCompany, 1);
			CreateOperatorTransaction(GlbCompany.CurrentCompany, 2);

			var anotherCompany = Factory.New<GlbCompany>();
			CreateOperatorTransaction(anotherCompany, 3);
			CreateOperatorTransaction(anotherCompany, 4);

			Factory.Save();

			var collection = new ModuleCusWHSOperatorTransactionsCollection(Factory, GlbCompany.CurrentCompany);
			AssertEquals(2, collection.Count);
			AssertEquals(GlbCompany.CurrentCompany.PK, collection[0].Batch.WOB_GC_Company);
			AssertEquals(GlbCompany.CurrentCompany.PK, collection[1].Batch.WOB_GC_Company);
		}

		CusWHSOperatorTransaction CreateOperatorTransaction(GlbCompany company, int quantity)
		{
			var result = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			result.Batch.WOB_GC_Company = company.PK;
			result.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			result.WOT_ExportType = ZString.Empty;
			result.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			result.WOT_Quantity = quantity;
			return result;
		}
	}
}
