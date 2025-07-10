using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class OrgAddressMessageDataLookupsTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var wrapper = OrgHeaderWrapper.New(Factory.New<OrgHeader>());
			var lookups = new OrgAddressMessageData(wrapper).Lookups;
			AssertEquals(typeof(OrgContactDependentCollection), lookups.Contacts.GetType());
			AssertEquals(typeof(ImporterAddressTypesList), lookups.MailingAddressTypeList.GetType());
			AssertEquals(typeof(NumberOfEntriesPlanningList), lookups.NumberOfEntriesList.GetType());
			AssertEquals(typeof(ImporterProgramCodeList), lookups.ProgramCodesList.GetType());

			AssertEquals("Importer Type List", 8, lookups.ImporterTypesList.Count);
			AssertEquals(true, lookups.ImporterTypesList.ContainsCode(ImporterTypeList.Codes.LLC));
			AssertEquals("Physical Address Type List", 6, lookups.PhysicalAddressTypeList.Count);
			AssertEquals(false, lookups.PhysicalAddressTypeList.ContainsCode(ImporterAddressTypesList.Codes._06));
			AssertEquals(false, lookups.PhysicalAddressTypeList.ContainsCode(ImporterAddressTypesList.Codes._07));
		}

		public void TestStateAddressList()
		{
			var wrapper = OrgHeaderWrapper.New(Factory.New<OrgHeader>());
			var orgAddressMessageData = new OrgAddressMessageData(wrapper);
			var lookups = orgAddressMessageData.Lookups;

			orgAddressMessageData.US_CountryISOCode = "US";
			AssertEquals(56, lookups.CertificateCountryStateList.Count);
			AssertEquals(true, lookups.CertificateCountryStateList.ContainsCode("AK"));
			AssertEquals(true, lookups.CertificateCountryStateList.ContainsCode("WY"));

			orgAddressMessageData.US_BankCountry = "US";
			AssertEquals(56, lookups.BankCountryStateList.Count);
			AssertEquals(true, lookups.BankCountryStateList.ContainsCode("AK"));
			AssertEquals(true, lookups.BankCountryStateList.ContainsCode("WY"));
		}
	}
}
