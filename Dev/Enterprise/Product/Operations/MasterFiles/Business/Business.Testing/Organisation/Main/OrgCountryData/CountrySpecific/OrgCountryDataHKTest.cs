using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCountryDataHK))]
	sealed class OrgCountryDataHKTest : OrgCountryDataEUStyleTest<OrgCountryDataHK>
	{
		protected override IEnumerable<string> CountriesToTest
		{
			get { return new[] { "HK" }; }
		}

		public void TestReadOnlyMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();

				var countryData = Factory.New<OrgCountryDataHK>();
				countryData.OV_OH_OrgHeader = organisation.PK;

				foreach (var countryCode in CountriesToTest)
				{
					countryData.OV_RN_NKClientCountryRelation = countryCode;

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
					countryData.OV_OA_ApprovedLocation = organisation.Addresses[0].PK;
					countryData.OV_EXApprovalExpiryDate = ZDate.Today;

					Assert("Read / Write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read / Write expiry date", !countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;

					Assert("Read only number", countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read only expiry date", countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					AssertEquals("Number has been blanked", "", countryData.OV_EXApprovalNumber);
					AssertEquals("Expiry has been blanked", ZDateTime.Empty, countryData.OV_EXApprovalExpiryDate);

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					countryData.OV_EXApprovalNumber = "12345";
					countryData.OV_EXApprovalExpiryDate = ZDate.Today;

					Assert("Read / Write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read / Write expiry date", !countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

					Assert("Read / write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read only expiry date", countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					AssertEquals("Number remains", "12345", countryData.OV_EXApprovalNumber);
					AssertEquals("Expiry is blanked", ZDateTime.Empty, countryData.OV_EXApprovalExpiryDate);
				}
			}
		}
	}
}
