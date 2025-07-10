using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NCTSOrganizationProviderTest : TestCaseWithFactory
	{
		public void TestCompanyMembersForHeader()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				var nctsPrincipalCompany = nctsHeaderProvider.Principal;
				CombineAssertions("Principal Company Members", () =>
				{
					AssertStartsWith("CompanyName", "xPrincipal Company Name", nctsPrincipalCompany.CompanyName);
					AssertLessThanOrEqualTo("CompanyName Length", nctsPrincipalCompany.CompanyName.Length, 35);
					AssertStartsWith("CompanyAddress", "xAdress1 xAdress2", nctsPrincipalCompany.Address);
					AssertLessThanOrEqualTo("CompanyAddress Length", nctsPrincipalCompany.Address.Length, 35);
					AssertEquals("CompanyPostCode", "340300", nctsPrincipalCompany.PostCode);
					AssertEquals("CompanyCity", "IST", nctsPrincipalCompany.City);
					AssertEquals("CompanyCountryCode", Core.Constants.CountryCodes.Turkey, nctsPrincipalCompany.CountryCode);
					AssertEquals("CompanyLanguage", Core.Constants.CountryCodes.Turkey, nctsPrincipalCompany.Language);
					AssertEquals("CompanyVATID", "1234567890", nctsPrincipalCompany.VATID);
				});

				var nctsCarrierCompany = nctsHeaderProvider.Carrier;
				CombineAssertions("Carrier Company Members", () =>
				{
					AssertStartsWith("CompanyName", "xCarrier Company Name", nctsCarrierCompany.CompanyName);
					AssertLessThanOrEqualTo("CompanyName Length", nctsCarrierCompany.CompanyName.Length, 35);
					AssertStartsWith("CompanyAddress", "xyzAdress1 xyzAdress2", nctsCarrierCompany.Address);
					AssertLessThanOrEqualTo("CompanyAddress Length", nctsCarrierCompany.Address.Length, 35);
					AssertEquals("CompanyPostCode", "340302", nctsCarrierCompany.PostCode);
					AssertEquals("CompanyCity", "IST", nctsCarrierCompany.City);
					AssertEquals("CompanyCountryCode", Core.Constants.CountryCodes.Turkey, nctsCarrierCompany.CountryCode);
					AssertEquals("CompanyLanguage", Core.Constants.CountryCodes.Turkey, nctsCarrierCompany.Language);
					AssertEquals("CompanyVATID", "1234567892", nctsCarrierCompany.VATID);
				});

				var nctsConsignorCompany = nctsHeaderProvider.Consignor;
				CombineAssertions("Consignor Company Members", () =>
				{
					AssertStartsWith("CompanyName", "xConsignor Company Name", nctsConsignorCompany.CompanyName);
					AssertLessThanOrEqualTo("CompanyName Length", nctsConsignorCompany.CompanyName.Length, 35);
					AssertStartsWith("CompanyAddress", "xyAdress1 xyAdress2", nctsConsignorCompany.Address);
					AssertLessThanOrEqualTo("CompanyAddress Length", nctsConsignorCompany.Address.Length, 35);
					AssertEquals("CompanyPostCode", "340301", nctsConsignorCompany.PostCode);
					AssertEquals("CompanyCity", "IST", nctsConsignorCompany.City);
					AssertEquals("CompanyCountryCode", Core.Constants.CountryCodes.Turkey, nctsConsignorCompany.CountryCode);
					AssertEquals("CompanyLanguage", Core.Constants.CountryCodes.Turkey, nctsConsignorCompany.Language);
					AssertEquals("CompanyVATID", "1234567891", nctsConsignorCompany.VATID);
				});

				var nctsConsigneeCompany = nctsHeaderProvider.Consignee;
				CombineAssertions("Consignee Company Members", () =>
				{
					AssertStartsWith("CompanyName", "xConsignee Company Name", nctsConsigneeCompany.CompanyName);
					AssertLessThanOrEqualTo("CompanyName Length", nctsConsigneeCompany.CompanyName.Length, 35);
					AssertStartsWith("CompanyAddress", "xyzAdress1 xyzAdress2", nctsConsigneeCompany.Address);
					AssertLessThanOrEqualTo("CompanyAddress Length", nctsConsigneeCompany.Address.Length, 35);
					AssertEquals("CompanyPostCode", "340302", nctsConsigneeCompany.PostCode);
					AssertEquals("CompanyCity", "IST", nctsConsigneeCompany.City);
					AssertEquals("CompanyCountryCode", Core.Constants.CountryCodes.Turkey, nctsConsigneeCompany.CountryCode);
					AssertEquals("CompanyLanguage", Core.Constants.CountryCodes.Turkey, nctsConsigneeCompany.Language);
					AssertEquals("CompanyVATID", "1234567892", nctsConsigneeCompany.VATID);
				});
			}
		}

		public void TestCompanyMembersForDetail()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				header.Consignor.E2_OA_Address = ZGuid.Empty;
				header.Consignee.E2_OA_Address = ZGuid.Empty;

				var nctsHeaderProvider = new NCTSHeaderProvider(header);
				var nctsGoodsItem = nctsHeaderProvider.GoodsItems.ToArray();

				CombineAssertions("NCTS Header Companies", () =>
				{
					AssertNotNull("Principal", nctsHeaderProvider.Principal);
					AssertNotNull("Carrier", nctsHeaderProvider.Carrier);
					AssertNull("Consignor", nctsHeaderProvider.Consignor);
					AssertNull("Consignee", nctsHeaderProvider.Consignee);
				});

				var nctsConsignorCompanyforGoods = nctsGoodsItem[0].Consignor;
				CombineAssertions("Consignor Company Members for goodsItems", () =>
				{
					AssertStartsWith("CompanyName", "xConsignor Company Name 2", nctsConsignorCompanyforGoods.CompanyName);
					AssertLessThanOrEqualTo("CompanyName Length", nctsConsignorCompanyforGoods.CompanyName.Length, 35);
					AssertStartsWith("CompanyAddress", "xyAdress1 xyAdress2", nctsConsignorCompanyforGoods.Address);
					AssertLessThanOrEqualTo("CompanyAddress Length", nctsConsignorCompanyforGoods.Address.Length, 35);
					AssertEquals("CompanyPostCode", "340301", nctsConsignorCompanyforGoods.PostCode);
					AssertEquals("CompanyCity", "IST", nctsConsignorCompanyforGoods.City);
					AssertEquals("CompanyCountryCode", Core.Constants.CountryCodes.Turkey, nctsConsignorCompanyforGoods.CountryCode);
					AssertEquals("CompanyLanguage", Core.Constants.CountryCodes.Turkey, nctsConsignorCompanyforGoods.Language);
					AssertEquals("CompanyVATID", "1234567891", nctsConsignorCompanyforGoods.VATID);
				});

				var nctsConsigneeCompanyforGoods = nctsGoodsItem[0].Consignee;
				CombineAssertions("Consignee Company Members for goodsItems", () =>
				{
					AssertStartsWith("CompanyName", "xConsignee Company Name 2", nctsConsigneeCompanyforGoods.CompanyName);
					AssertLessThanOrEqualTo("CompanyName Length", nctsConsigneeCompanyforGoods.CompanyName.Length, 35);
					AssertStartsWith("CompanyAddress", "xyzAdress1 xyzAdress2", nctsConsigneeCompanyforGoods.Address);
					AssertLessThanOrEqualTo("CompanyAddress Length", nctsConsigneeCompanyforGoods.Address.Length, 35);
					AssertEquals("CompanyPostCode", "340302", nctsConsigneeCompanyforGoods.PostCode);
					AssertEquals("CompanyCity", "IST", nctsConsigneeCompanyforGoods.City);
					AssertEquals("CompanyCountryCode", Core.Constants.CountryCodes.Turkey, nctsConsigneeCompanyforGoods.CountryCode);
					AssertEquals("CompanyLanguage", Core.Constants.CountryCodes.Turkey, nctsConsigneeCompanyforGoods.Language);
					AssertEquals("CompanyVATID", "1234567892", nctsConsigneeCompanyforGoods.VATID);
				});
			}
		}
	}
}
