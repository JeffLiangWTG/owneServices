using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	[TestedType(typeof(ExbondForUnderReceiptsApplicator))]
	sealed class ExbondForUnderReceiptsApplicatorTest : LockedOperationalActionMethodApplicatorTest<ExbondForUnderReceiptsApplicator>
	{
		public void TestRecordFilter()
		{
			var (warehousePk, productOwnerPk, _, transactions) = ExportApplicatorTestHelper.SetupTestDataForRecordFilterTesting(Factory);
			Applicator.Build(transactions.Select(x => x.PK).ToArray());

			AssertEquals("Records.Count", 1, Applicator.Records.Count);
			AssertEquals("Records[0].TransactionType", WarehouseOperatorTransactionTypeList.Codes.ADJ, Applicator.Records[0].TransactionType);
			AssertEquals("Records[0].OwnerReference", "RefForVAL", Applicator.Records[0].OwnerReference);
			AssertEquals("Records[0].WarehouseAddress", warehousePk, Applicator.Records[0].WarehouseAddress);
			AssertEquals("Records[0].ProductOwner", productOwnerPk, Applicator.Records[0].ProductOwner);

			Applicator.Unlock();
		}

		protected override OperationalActionMethodApplicator GetOtherApplicator() => new ClearExpiredStockApplicator(Factory, () => ZDate.Invalid);
	}
}
