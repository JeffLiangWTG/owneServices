using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaManifestHeaderDocWrapperTest : TestCaseWithFactory
	{
		public const string CalculatedDutyReport = "Calculated Duty Report";

		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals(Header, Wrapper.Manifest);
				AssertEquals("Date At Customs Office", Wrapper.DateAtCustomsOffice, ZDateTime.Today);
				AssertEquals("Company Name", Wrapper.CompanyName, "Eagle Datamation International");
				AssertEquals("Registration Number", Wrapper.RegistrationNumber, "XXX1234");
				AssertEquals("Customs Office", Wrapper.CustomsOffice, "İstanbul Gümrük Dairesi");
				AssertEquals("Job Reference", Wrapper.JobReference, "Test-Ref");
				AssertEquals("Registration Date", Wrapper.RegistrationDate, ZDateTime.Today);
				AssertEquals("ExchangeRate", Wrapper.ExchangeRate, (ZDecimal)8.5);
				AssertEquals("TotalDutyOfBills", Wrapper.TotalDutyOfBills, (ZDecimal)3056.6M);
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
			helper.SetManifestValues(Header, Company, Branch, Organization, Shipper, Carrier);
			Wrapper = new AsycudaManifestHeaderDocWrapper(Header);
		}

		public AsycudaManifestHeaderDocWrapper Wrapper;
		public AsycudaManifestHeader Header;
		public GlbCompany Company;
		public GlbBranch Branch;
		public OrgHeader Organization;
		public OrgAddress Shipper;
		public OrgAddress Carrier;
	}
}
