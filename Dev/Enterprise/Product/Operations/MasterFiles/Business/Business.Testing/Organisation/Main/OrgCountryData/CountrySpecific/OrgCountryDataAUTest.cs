using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCountryDataAU))]
	sealed class OrgCountryDataAUTest : OrgCountryDataTest
	{
		public void TestReadOnlyMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();

				var countryData = Factory.New<OrgCountryDataAU>();
				countryData.OV_OH_OrgHeader = organisation.PK;
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Australia;

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				countryData.OV_OA_ApprovedLocation = organisation.Addresses[0].PK;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today;

				Assert("Read / Write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
				Assert("Read / Write expiry date", !countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegularCustomer;

				Assert("Read only number", countryData.OV_EXApprovalNumberInfo.ReadOnly);
				Assert("Read only expiry date", countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

				AssertEquals("Number has been blanked", "", countryData.OV_EXApprovalNumber);
				AssertEquals("Expiry has been blanked", ZDate.Empty, countryData.OV_EXApprovalExpiryDate);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgCountryDataAU>();
		}
	}
}
