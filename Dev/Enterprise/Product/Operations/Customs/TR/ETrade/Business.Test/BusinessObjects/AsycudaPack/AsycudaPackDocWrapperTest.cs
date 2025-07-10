using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaPackDocWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			CombineAssertions("With Data", () =>
			{
				AssertEquals("CustomsValueCurrency", Wrapper.CustomsValueCurrency, "EUR");
				AssertEquals("BillNumber", Wrapper.BillNumber, "Bill-No-123");
				AssertEquals("ConsigneeName", Wrapper.ConsigneeName, "Consignee name");
				AssertEquals("Container", Wrapper.Container, 1);
				AssertEquals("ContainerType", Wrapper.ContainerType, "BI");
				AssertEquals("CustomsValue", Wrapper.CustomsValue, (ZDecimal)620M);
				AssertEquals("Description", Wrapper.Description, "Description");
				AssertEquals("DischargeCountry", Wrapper.DischargeCountry, "004");
				AssertEquals("GrossWeight", Wrapper.GrossWeight, (ZDecimal)1.5);
				AssertEquals("LoadingCountry", Wrapper.LoadingCountry, "004");
				AssertEquals("Measures", Wrapper.TotalQuantity, (ZDecimal)1.5);
				AssertEquals("OriginCountry", Wrapper.OriginCountry, "004");
				AssertEquals("Procedure", Wrapper.Procedure, "9041");
				AssertEquals("ShipperName", Wrapper.ShipperName, "Shipper name");
				AssertEquals("Tariff", Wrapper.Tariff, "1000");
				AssertEquals("TaxAmount", Wrapper.TaxAmount, (ZDecimal)3056.6M);
				AssertEquals("TaxBase", Wrapper.TaxBase, (ZDecimal)5270M);
				AssertEquals("TotalTaxRate", Wrapper.TotalTaxRate, (ZDecimal)58);
				AssertEquals("TradeCountry", Wrapper.TradeCountry, "004");
				AssertEquals("Unit", Wrapper.Unit, "KGM");
			});
		}
		protected override void SetUp()
		{
			base.SetUp();
			CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
			Header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Branch = Factory.NewWithValidTestData<GlbBranch>();
			Organization = Factory.NewWithValidTestData<OrgHeader>();
			Company = Factory.NewWithValidTestData<GlbCompany>();
			helper.SetPackValues(Header, Company, Branch, Organization, Shipper, Carrier);
			var pack = Header.Bills.Cast<AsycudaBill>().First().Packs.Cast<AsycudaPack>().First();
			Wrapper = new AsycudaPackDocWrapper(pack);
		}
		public AsycudaPackDocWrapper Wrapper;
		public AsycudaManifestHeader Header;
		public GlbCompany Company;
		public GlbBranch Branch;
		public OrgHeader Organization;
		public OrgAddress Shipper;
		public OrgAddress Carrier;
	}
}
