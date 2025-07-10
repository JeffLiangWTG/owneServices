using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusOutturn))]
	sealed class CusOutturnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackCondDesc()
		{
			var outturn = Factory.New<CusOutturn>();
			outturn.PackCondDesc = "123";
			Factory.Save();
			outturn = new BusinessObjectFactory().Load<CusOutturn>(outturn.PK);
			AssertEquals("123", outturn.PackCondDesc);
		}

		public void TestExcessShortInd()
		{
			var outturn = Factory.New<CusOutturn>();
			outturn.PackCondDesc = "123";
			Factory.Save();
			outturn = new BusinessObjectFactory().Load<CusOutturn>(outturn.PK);
			AssertEquals("123", outturn.PackCondDesc);
		}

		public void TestContShouldBe()
		{
			var outturn = Factory.New<CusOutturn>();
			outturn.ExcessShortInd = "1";
			Factory.Save();
			outturn = new BusinessObjectFactory().Load<CusOutturn>(outturn.PK);
			AssertEquals("1", outturn.ExcessShortInd);
		}

		public void TestValidation()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertType<CusOutturnValidation>(outturn.Validation);
		}

		public void TestLookups()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertType<CusOutturnLookups>(outturn.Lookups);
		}

		public void TestHeader()
		{
			var outturn1 = Factory.New<CusOutturn>();
			AssertNull("Null header", outturn1.Header);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("Header", header, pack.Outturn.Header);
		}

		public void TestPack()
		{
			var outturn1 = Factory.New<CusOutturn>();
			AssertNull("Null pack", outturn1.Pack);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("Pack", pack, pack.Outturn.Pack);
		}

		public void TestDefaultValues()
		{
			var outturn = Factory.New<CusOutturn>();

			AssertEquals("C5_VolumeOutturnedUQ", string.Empty, outturn.C5_VolumeOutturnedUQ);
			AssertEquals("C5_WeightOutturnedUQ", string.Empty, outturn.C5_WeightOutturnedUQ);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusOutturn>();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<CusOutturn>();
	}
}
