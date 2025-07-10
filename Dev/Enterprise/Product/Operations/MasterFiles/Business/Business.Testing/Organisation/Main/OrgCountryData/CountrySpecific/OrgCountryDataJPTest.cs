using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCountryDataJP))]
	sealed class OrgCountryDataJPTest : OrgCountryDataTest
	{
		public void TestExpiryDateIsDefaultedFromDocument()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			using (FreightDataRegistry.Instance.ApprovedOrganisationRequiredDocType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "KCA"))
			{
				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();

				OrgCountryDataJP countryData = Factory.New<OrgCountryDataJP>();
				countryData.OV_OH_OrgHeader = organisation.PK;
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Japan;

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalNumber = "12345";

				AssertEquals("Expiry is not defaulted as no required document is attached", ZDate.Empty, countryData.OV_EXApprovalExpiryDate);

				var requiredDoc = organisation.RequiredDocuments.AddNew("KCA");
				requiredDoc.EQ_DocPeriod = "PER";
				requiredDoc.EQ_DocNumber = "321";
				requiredDoc.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);

				countryData.OV_EXApprovalNumber = "321";

				AssertEquals("Expiry date is not updated for expired required doc", ZDate.Empty, countryData.OV_EXApprovalExpiryDate);

				organisation.RequiredDocuments.AddNew("KCA");
				requiredDoc.EQ_DocPeriod = "PER";
				requiredDoc.EQ_DocNumber = "999";
				requiredDoc.EQ_ValidToDate = ZDateTime.Today.AddDays(1);

				countryData.OV_EXApprovalNumber = "999";

				AssertEquals("The expiry date has been populated from the required document", ZDate.Today.AddDays(1), countryData.OV_EXApprovalExpiryDate);
			}
		}

		public void TestReadOnlyMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();

				var countryData = Factory.New<OrgCountryDataJP>();
				countryData.OV_OH_OrgHeader = organisation.PK;
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Japan;

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_OA_ApprovedLocation = organisation.Addresses[0].PK;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today;

				Assert("Read / Write number", !countryData.OV_EXApprovalNumberInfo.ReadOnly);
				Assert("Read / Write expiry date", !countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

				Assert("Read only address", countryData.OV_OA_ApprovedLocation_ReadOnly);

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;

				Assert("Read only number", countryData.OV_EXApprovalNumberInfo.ReadOnly);
				Assert("Read only expiry date", countryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

				AssertEquals("Number has been blanked", "", countryData.OV_EXApprovalNumber);
				AssertEquals("Expiry has been blanked", ZDate.Empty, countryData.OV_EXApprovalExpiryDate);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgCountryDataJP>();
		}
	}
}
