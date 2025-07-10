using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	[TestedType(typeof(ExportNonBelnApplicator))]
	sealed class ExportNonBelnApplicatorTest : LockedOperationalActionMethodApplicatorTest<ExportNonBelnApplicator>
	{
		protected override BusinessObject GetNewBusinessObject() => new ExportNonBelnApplicator(Factory, () => false);

		protected override OperationalActionMethodApplicator GetOtherApplicator() => new ExbondForUnderReceiptsApplicator(Factory);

		public void TestRecordFilter()
		{
			var (warehousePk, productOwnerPk, _, transactions) = ExportApplicatorTestHelper.SetupTestDataForRecordFilterTesting(Factory);
			Applicator.Build(transactions.Select(x => x.PK).ToArray());

			AssertEquals("Records.Count", 1, Applicator.Records.Count);
			AssertEquals("Records[0].ExportType", WarehouseOperatorTransactionExportTypeList.Codes.EXP, Applicator.Records[0].ExportType);
			AssertEquals("Records[0].OwnerReference", "RefForVAL", Applicator.Records[0].OwnerReference);
			AssertEquals("Records[0].WarehouseAddress", warehousePk, Applicator.Records[0].WarehouseAddress);
			AssertEquals("Records[0].ProductOwner", productOwnerPk, Applicator.Records[0].ProductOwner);

			Applicator.Unlock();
		}

		new ExportNonBelnApplicator Applicator => base.Applicator;
	}
}
