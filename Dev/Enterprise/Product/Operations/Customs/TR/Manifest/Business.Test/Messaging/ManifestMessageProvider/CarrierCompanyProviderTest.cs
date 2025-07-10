using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class CarrierCompanyProviderTest : TestCaseWithFactory
	{
		public void TestCarrierCompanyMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				ICarrierCompany carrierCompany = sumDec.CarrierCompany.FirstOrDefault();
				CombineAssertions("Carrier Company Members", () =>
				{
					AssertEquals("xCarrier Company Name", carrierCompany.CarrierName);
					AssertEquals("xAdress1 xAdress2", carrierCompany.StreetNo);
					AssertEquals("+90 212 212 26 92", carrierCompany.Phone);
					AssertEquals("+90 212 212 26 92", carrierCompany.Fax);
					AssertEquals("IST", carrierCompany.ProvinceDistrict);
					AssertEquals(ZString.Empty, carrierCompany.IdentificationNumber);
					AssertEquals(TurkishConstants.IdentityType, carrierCompany.IdentityType);
					AssertEquals("340300", carrierCompany.PostCode);
					AssertEquals("052", carrierCompany.CountryCode);
					AssertEquals(ZString.Empty, carrierCompany.TaxOfficeCode);
				});
			}
		}
	}
}
