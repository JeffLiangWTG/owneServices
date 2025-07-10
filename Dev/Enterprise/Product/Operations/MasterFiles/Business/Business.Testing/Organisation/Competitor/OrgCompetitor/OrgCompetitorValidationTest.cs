using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCompetitorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOCP_OH_Competitor()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();
			competitor.OH_IsBroker = false;

			var orgCompetitor = parent.Competitors.AddNew();
			orgCompetitor.OCP_Type = CompetitorTypeList.Codes.Customs;
			orgCompetitor.OCP_OH_Competitor = competitor.PK;

			AssertHasError(orgCompetitor.OCP_OH_CompetitorInfo, "Organization is invalid for the selected Competitor Type.");
		}

		public void TestCheckOCP_Type()
		{
			var competitorTypeList = new OverrideImmuneCodeDescriptionBoolCollection { { "ACT", null, true }, { "DIS", null, false } };
			OrganisationsDataRegistry.Instance.CompetitorType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, competitorTypeList);

			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();
			var competitor2 = Factory.NewWithValidTestData<OrgHeader>();

			var orgCompetitorActive = parent.Competitors.AddNew();
			orgCompetitorActive.OCP_Type = "ACT";
			orgCompetitorActive.OCP_OH_Competitor = competitor.PK;

			var orgCompetitorDisabled = parent.Competitors.AddNew();
			orgCompetitorDisabled.OCP_Type = "DIS";
			orgCompetitorDisabled.OCP_OH_Competitor = competitor.PK;

			var orgCompetitorInvalid = parent.Competitors.AddNew();
			orgCompetitorInvalid.OCP_Type = "INV";
			orgCompetitorInvalid.OCP_OH_Competitor = competitor.PK;
			Factory.Save();

			var orgCompetitorNotSaved = parent.Competitors.AddNew();
			orgCompetitorNotSaved.OCP_Type = "INV";
			orgCompetitorNotSaved.OCP_OH_Competitor = competitor.PK;

			orgCompetitorActive.Validation.ValidateOCP_Type();
			AssertNoErrors(orgCompetitorActive.OCP_TypeInfo);

			orgCompetitorDisabled.Validation.ValidateOCP_Type();
			AssertHasWarning(orgCompetitorDisabled.OCP_TypeInfo, "This competitor type is disabled. Please use a valid competitor type.");

			orgCompetitorInvalid.Validation.ValidateOCP_Type();
			AssertHasWarning(orgCompetitorInvalid.OCP_TypeInfo, "This competitor type is invalid. Please use a valid competitor type.");

			orgCompetitorNotSaved.Validation.ValidateOCP_Type();
			AssertHasError(orgCompetitorNotSaved.OCP_TypeInfo, "This competitor type is invalid. Please use a valid competitor type.");

			orgCompetitorActive.OCP_OH_Competitor = competitor2.PK;
			AssertNoErrors(orgCompetitorActive.OCP_TypeInfo);

			orgCompetitorDisabled.OCP_OH_Competitor = competitor2.PK;
			AssertHasError(orgCompetitorDisabled.OCP_TypeInfo, "This competitor type is disabled. Please use a valid competitor type.");

			orgCompetitorInvalid.OCP_OH_Competitor = competitor2.PK;
			AssertHasError(orgCompetitorInvalid.OCP_TypeInfo, "This competitor type is invalid. Please use a valid competitor type.");

			orgCompetitorNotSaved.OCP_OH_Competitor = competitor2.PK;
			AssertHasError(orgCompetitorNotSaved.OCP_TypeInfo, "This competitor type is invalid. Please use a valid competitor type.");
		}

		public void TestValidateIsInCollectionAlready()
		{
			var competitorTypeList = new OverrideImmuneCodeDescriptionBoolCollection { { "ACT", null, true }, { "DIS", null, true } };
			OrganisationsDataRegistry.Instance.CompetitorType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, competitorTypeList);

			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();
			var competitor2 = Factory.NewWithValidTestData<OrgHeader>();

			var orgCompetitorActive = parent.Competitors.AddNew();
			orgCompetitorActive.OCP_Type = "ACT";
			orgCompetitorActive.OCP_OH_Competitor = competitor.PK;
			orgCompetitorActive.CompanyLevel = CompanyLevelList.Codes.ENT;

			var duplicateCompetitor = parent.Competitors.AddNew();
			duplicateCompetitor.OCP_Type = "ACT";
			duplicateCompetitor.OCP_OH_Competitor = competitor.PK;
			duplicateCompetitor.CompanyLevel = CompanyLevelList.Codes.ENT;
			TestIsInCollectionAlreadyRowError(duplicateCompetitor, expectError: true);

			duplicateCompetitor.OCP_Type = "DIS";
			TestIsInCollectionAlreadyRowError(duplicateCompetitor, expectError: false);

			duplicateCompetitor.OCP_Type = "ACT";
			duplicateCompetitor.OCP_OH_Competitor = competitor2.PK;
			TestIsInCollectionAlreadyRowError(duplicateCompetitor, expectError: false);

			duplicateCompetitor.OCP_OH_Competitor = competitor.PK;
			duplicateCompetitor.CompanyLevel = CompanyLevelList.Codes.COM;
			TestIsInCollectionAlreadyRowError(duplicateCompetitor, expectError: false);
		}

		void TestIsInCollectionAlreadyRowError(OrgCompetitor duplicateCompetitor, bool expectError)
		{
			duplicateCompetitor.Validation.ValidateOCP_Type();
			duplicateCompetitor.Validation.ValidateOCP_OH_Competitor();
			duplicateCompetitor.Validation.ValidateCompanyLevel();

			if (expectError)
			{
				AssertEquals(1, duplicateCompetitor.RowErrors.Count());
				AssertEquals("There is already a competitor with the same type, organization and company level. You can only specify a single competitor organization for this combination.", duplicateCompetitor.RowErrors.First().Message);
			}
			else
			{
				Assert(!duplicateCompetitor.HasRowErrors);
			}
		}
	}
}
