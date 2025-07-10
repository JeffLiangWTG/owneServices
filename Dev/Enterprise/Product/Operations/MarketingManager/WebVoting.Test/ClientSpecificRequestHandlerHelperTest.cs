using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.WebVoting
{
	class ClientSpecificRequestHandlerHelperTest : TestCaseWithFactory
	{
		public void TestGetCompanyCodeFromQueryString()
		{
			SecureQueryString qs = new SecureQueryString { { "test1", "value1" }, { "test2", "value2" } };
			qs.ExpireTime = TimeSpan.FromMinutes(-1);
			var result = ClientSpecificRequestHandlerHelper.GetCompanyCodeFromQueryString(qs.ToString());
			AssertEquals(result, string.Empty);
		}

		public void TestGetCompanyCodeFromQueryString_WithCampaignPrimaryKey()
		{
			const string expectedCompanyCode = "TST";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_Code = expectedCompanyCode;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = company.PK;

			Factory.Save();

			var secureQueryString = new SecureQueryString();
			secureQueryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();

			string encryptedQueryString = secureQueryString.ToString();

			var result = ClientSpecificRequestHandlerHelper.GetCompanyCodeFromQueryString(encryptedQueryString);

			AssertEquals("Incorrect Company Code returned", expectedCompanyCode, result);
		}

		public void TestGetCompanyCodeFromQueryString_WithCampaignItemPrimaryKey()
		{
			const string expectedCompanyCode = "TST";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_Code = expectedCompanyCode;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = company.PK;

			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			campaignItem.G8_G0 = campaign.PK;

			Factory.Save();

			var secureQueryString = new SecureQueryString();
			secureQueryString[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString();

			string encryptedQueryString = secureQueryString.ToString();

			var result = ClientSpecificRequestHandlerHelper.GetCompanyCodeFromQueryString(encryptedQueryString);

			AssertEquals("Incorrect Company Code returned", expectedCompanyCode, result);
		}
	}
}
