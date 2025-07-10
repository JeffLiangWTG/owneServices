using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	[TestedType(typeof(ExportBelnApplicator))]
	sealed class ExportBelnApplicatorTest : LockedOperationalActionMethodApplicatorTest<ExportBelnApplicator>
	{
		protected override BusinessObject GetNewBusinessObject() => new ExportBelnApplicator(Factory, () => false);

		public void TestRecordFilter()
		{
			var (warehousePk, productOwnerPk, _, transactions) = ExportApplicatorTestHelper.SetupTestDataForRecordFilterTesting(Factory);
			Applicator.Build(transactions.Select(x => x.PK).ToArray());

			AssertEquals("Records.Count", 1, Applicator.Records.Count);
			AssertEquals("Records[0].ExportType", WarehouseOperatorTransactionExportTypeList.Codes.BLN, Applicator.Records[0].ExportType);
			AssertEquals("Records[0].OwnerReference", "RefForVAL", Applicator.Records[0].OwnerReference);
			AssertEquals("Records[0].WarehouseAddress", warehousePk, Applicator.Records[0].WarehouseAddress);
			AssertEquals("Records[0].ProductOwner", productOwnerPk, Applicator.Records[0].ProductOwner);

			Applicator.Unlock();
		}

		protected override OperationalActionMethodApplicator GetOtherApplicator() => new ExportNonBelnApplicator(Factory, () => false);
	}
}
