using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCustomLabelsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOT_FieldName_ListHasAGenericPart()
		{
			Assert("OT_FieldName_List.Count > 0", Label.Lookups.OT_FieldName_List.Count > 0);
		}

		public void TestOT_FieldName_ListHasCountrySpecificCodes()
		{
			Label.OT_Type = OrgConstants.CustomLabelType.Document;
			GlbCompany.CurrentCompany.SetCountry("ZA");
			AssertCommonLandedCostingDocumentLabels();
			foreach (CodeDescriptionPair pair in new ZACustomsWorksheetCustomDocumentLabelsList())
			{
				Assert(Label.Lookups.OT_FieldName_List.ContainsCode(pair.Code));
			}

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			CreateControllingBranch(orgHeader, "AU");
			Label.OT_OH = orgHeader.PK;
			Factory.Save();

			AssertEquals("WET", Label.Lookups.OT_FieldName_List.GetDescriptionFromCode("LandedCostingDocument.SpecialTax1"));
			AssertEquals("LCT", Label.Lookups.OT_FieldName_List.GetDescriptionFromCode("LandedCostingDocument.SpecialTax2"));
			AssertEquals("Wood Levy", Label.Lookups.OT_FieldName_List.GetDescriptionFromCode("LandedCostingDocument.SpecialTax3"));
			Assert("Quarantine fee is added", Label.Lookups.OT_FieldName_List.ContainsCode("LandedCostingDocument.QuarantineFee"));
			AssertCommonLandedCostingDocumentLabels();

			CreateControllingBranch(orgHeader, "AU");
			Label.OT_OH = orgHeader.PK;
			Factory.Save();

			orgHeader.MainAddress.OA_RN_NKCountryCode = "NZ";
			AssertEquals("ALAC", Label.Lookups.OT_FieldName_List.GetDescriptionFromCode("LandedCostingDocument.SpecialTax1"));
			AssertEquals("HERA", Label.Lookups.OT_FieldName_List.GetDescriptionFromCode("LandedCostingDocument.SpecialTax2"));
			AssertEquals("ACC Fuel", Label.Lookups.OT_FieldName_List.GetDescriptionFromCode("LandedCostingDocument.SpecialTax3"));
			AssertCommonLandedCostingDocumentLabels();

			orgHeader.OH_RL_NKClosestPort = "USLAX";
			AssertEquals(USLandedCostingCustomDocumentLabelsList.Descriptions.SellPrice1, Label.Lookups.OT_FieldName_List.GetDescriptionFromCode(USLandedCostingCustomDocumentLabelsList.Codes.SellPrice1));
			AssertEquals(USLandedCostingCustomDocumentLabelsList.Descriptions.SellPrice2, Label.Lookups.OT_FieldName_List.GetDescriptionFromCode(USLandedCostingCustomDocumentLabelsList.Codes.SellPrice2));
			AssertEquals(USLandedCostingCustomDocumentLabelsList.Descriptions.SellPrice3, Label.Lookups.OT_FieldName_List.GetDescriptionFromCode(USLandedCostingCustomDocumentLabelsList.Codes.SellPrice3));
			AssertCommonLandedCostingDocumentLabels();
		}

		void AssertCommonLandedCostingDocumentLabels()
		{
			foreach (CodeDescriptionPair pair in new LandedCostingCustomDocumentLabelsList())
			{
				Assert(Label.Lookups.OT_FieldName_List.ContainsCode(pair.Code));
			}
		}
		void CreateControllingBranch(OrgHeader org, ZString countryCode)
		{
			OrgCompanyData orgCompanyData = org.CompanyDataCollection.AddNew();

			GlbCompany glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_IsActive = true;

			GlbBranch glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_IsActive = true;
			glbBranch.GB_RN_NKCountryCode = countryCode;

			orgCompanyData.OB_GC = glbCompany.PK;
			orgCompanyData.OB_GB_ControllingBranch = glbBranch.PK;
		}

		OrgCustomLabels Label;
		protected override void SetUp()
		{
			base.SetUp();
			Label = Factory.New<OrgCustomLabels>();
		}
	}
}
