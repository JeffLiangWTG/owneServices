using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomLabelProviderTest : TestCaseWithFactory
	{
		public void TestGetLabelFallbackToCompanyOrgProxy()
		{
			OrgCustomLabels label;
			OrgHeader companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			OrgCustomLabelsCollection companyCollection = new OrgCustomLabelsCollection(companyOrgProxy);

			label = companyCollection.AddNew();
			label.OT_Type = OrgConstants.CustomLabelType.Document;
			label.OT_FieldName = "override_company";
			label.OT_Caption = "company";
			label = companyCollection.AddNew();
			label.OT_Type = OrgConstants.CustomLabelType.Document;
			label.OT_FieldName = "fallback_to_company";
			label.OT_Caption = "company";

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgCustomLabelsCollection organisationCollection = new OrgCustomLabelsCollection(organisation);
			label = organisationCollection.AddNew();
			label.OT_Type = OrgConstants.CustomLabelType.Report;
			label.OT_FieldName = "override_company";
			label.OT_Caption = "organisation";

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = companyOrgProxy.PK;

			OrgCustomLabels organisationLabel = new CustomLabelProvider(organisationCollection).GetLabelFallbackToCompanyOrgProxy(company, "fallback_to_company");
			AssertNull("Label type is not specified and there is one configuration for organisation so should not fall back to OrgProxy", organisationLabel);

			//as long as Document label is concerned, there is zero configuration and therefore, it should fallback to OrgProxy.
			organisationLabel = new CustomLabelProvider(organisationCollection).GetLabelFallbackToCompanyOrgProxy(company, "fallback_to_company", OrgConstants.CustomLabelType.Document);
			AssertEquals("Company should be overridden by organisation", "company", organisationLabel.OT_Caption);
			AssertNull("For Report labels, it should not fall back to OrgProxy as Organisation has its own configurations", new CustomLabelProvider(organisationCollection).GetLabelFallbackToCompanyOrgProxy(company, "fallback_to_company", OrgConstants.CustomLabelType.Report));

			label.OT_Type = OrgConstants.CustomLabelType.Document;
			OrgCustomLabels companyOverriddenLabel = new CustomLabelProvider(organisationCollection).GetLabelFallbackToCompanyOrgProxy(company, "override_company", OrgConstants.CustomLabelType.Document);
			AssertEquals("Company should be overridden by organisation", "organisation", companyOverriddenLabel.OT_Caption);
		}

		public void TestGetLabelIgnoringCase()
		{
			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var companyCollection = new OrgCustomLabelsCollection(companyOrgProxy);

			var label = companyCollection.AddNew();
			label.OT_Type = OrgConstants.CustomLabelType.Document;
			label.OT_FieldName = LandedCostingCustomDocumentLabelsList.Codes.MarkupPercentage1;
			label.OT_Caption = "Blah";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = companyOrgProxy.PK;

			var organisationLabel = new CustomLabelProvider(companyCollection).GetLabelFallbackToCompanyOrgProxy(company, "LandedCostingDocument.MarkUpPercentage1");
			AssertNotNull("It should find a label ignoring case difference", organisationLabel);
		}

		[ExpectNoExceptions]
		public void TestGetLabelFallbackToCompanyOrgProxy_NullOrgProxy()
		{
			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var companyCollection = new OrgCustomLabelsCollection(companyOrgProxy);
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var organisationLabel = new CustomLabelProvider(companyCollection).GetLabelFallbackToCompanyOrgProxy(company, "LandedCostingDocument.MarkUpPercentage1");
			AssertNull("Null Org Proxy should not crash and return null", organisationLabel);
		}
	}
}
