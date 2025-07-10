using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCountryDataEU))]
	sealed class OrgCountryDataEUTest : OrgCountryDataEUStyleTest<OrgCountryDataEU>
	{
		protected override IEnumerable<string> CountriesToTest
		{
			get { return ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Union(new[] { Constants.CountryCodes.Norway, Constants.CountryCodes.Switzerland }); }
		}

		public void TestReadOnlyMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				foreach (var countryCode in CountriesToTest)
				{
					var organisation = Factory.NewWithValidTestData<OrgHeader>();

					var countryData = Factory.New<OrgCountryDataEU>();
					countryData.OV_OH_OrgHeader = organisation.PK;
					countryData.OV_RN_NKClientCountryRelation = countryCode;

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
					countryData.OV_OA_ApprovedLocation = organisation.Addresses[0].PK;
					countryData.OV_EXApprovalExpiryDate = ZDate.Today;

					Assert("Read / Write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read only expiry date", countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;

					Assert("Read only number", countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read only expiry date", countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					AssertEquals("Number has been blanked", "", countryData.OV_EXApprovalNumber);
					AssertEquals("Expiry has been blanked", ZDate.Empty, countryData.OV_EXApprovalExpiryDate);

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					countryData.OV_EXApprovalNumber = "12345";
					countryData.OV_EXApprovalExpiryDate = ZDate.Today;

					Assert("Read / Write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read / Write expiry date", !countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

					Assert("Read / write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Read / write expiry date", !countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

					AssertEquals("Number remains", "12345", countryData.OV_EXApprovalNumber);
					AssertEquals("Expiry remains", ZDate.Today, countryData.OV_EXApprovalExpiryDate);
				}
			}
		}

		public void TestCountryDataAddedForOneEUCountryIsAvailableInAnother()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "ABC123";

				var orgCountryData = org.MainAddress.KnownShipperDetails.AddNew();
				orgCountryData.OV_OH_OrgHeader = org.PK;
				orgCountryData.OV_EXApprovalNumber = "12345XYZ";

				Factory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var org = (new BusinessObjectFactory()).LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABC123");
				AssertEquals("12345XYZ", org.MainAddress.KnownShipperDetails[0].OV_EXApprovalNumber);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var org = (new BusinessObjectFactory()).LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABC123");
				AssertEquals("Not loaded for AU", 0, org.MainAddress.KnownShipperDetails.Count);
			}
		}
	}
}
