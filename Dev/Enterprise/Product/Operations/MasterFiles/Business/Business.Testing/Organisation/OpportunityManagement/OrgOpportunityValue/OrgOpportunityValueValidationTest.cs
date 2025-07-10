using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgOpportunityValueValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRevenueTypeValidation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			OrgOpportunityValue valueItem = opp.ValueItems.AddNew();

			valueItem.PV_RevenueType = "XXX";
			AssertHasErrors(valueItem.PV_RevenueTypeInfo);

			valueItem.PV_RevenueType = "CFS";
			AssertNoErrors(valueItem.PV_RevenueTypeInfo);

			valueItem.PV_RevenueType = "";
			AssertHasErrors(valueItem.PV_RevenueTypeInfo);

			var valueTypes = new CodeDescriptionPairList(valueItem.Lookups.ValueTypes);
			valueTypes.AddPair("XXX");
			Factory.ClearCachedValue<CodeDescriptionPairList>("Enterprise.MasterFiles.Business.OrgOpportunityValueLookups.ValueTypes");
			Factory.GetCachedValue("Enterprise.MasterFiles.Business.OrgOpportunityValueLookups.ValueTypes", () => { return valueTypes; });

			valueItem.PV_RevenueType = "XXX";
			Factory.Save();
			valueItem.Validation.ValidatePV_RevenueType();
			AssertNoErrors(valueItem.PV_RevenueTypeInfo);
			AssertHasWarnings(valueItem.PV_RevenueTypeInfo);

			valueItem.PV_RevenueType = "FOR";
			Factory.Save();
			valueItem.Validation.ValidatePV_RevenueType();
			AssertNoErrors(valueItem.PV_RevenueTypeInfo);
			AssertHasWarnings(valueItem.PV_RevenueTypeInfo);
		}
	}
}
