using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AddressFormatterTest : TestCaseWithFactory
	{
		public void TestOverrideAdditionalInfo()
		{
			var address = Factory.New<OrgAddress>();
			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Address1 = "ADDRESS 1";
			translatedAddress.OTA_Address2 = "ADDRESS 2";
			var wrapper = new AddressFormatter(Factory, translatedAddress, (NoResString)"COUNTRYNAME", "AU", true);
			AssertEquals("ADDRESS 1\nADDRESS 2\nCOUNTRYNAME", wrapper.PostalAddress());

			wrapper = new AddressFormatter(Factory, translatedAddress, (NoResString)"COUNTRYNAME", "AU", true, null);
			AssertEquals("ADDRESS 1\nADDRESS 2\nCOUNTRYNAME", wrapper.PostalAddress());

			wrapper = new AddressFormatter(Factory, translatedAddress, (NoResString)"COUNTRYNAME", "AU", true, string.Empty);
			AssertEquals("ADDRESS 1\nADDRESS 2\nCOUNTRYNAME", wrapper.PostalAddress());

			wrapper = new AddressFormatter(Factory, translatedAddress, (NoResString)"COUNTRYNAME", "AU", true, "ADDITIONAL");
			AssertEquals("ADDITIONAL\nADDRESS 1\nADDRESS 2\nCOUNTRYNAME", wrapper.PostalAddress());
		}
	}
}
