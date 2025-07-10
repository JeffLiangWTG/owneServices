using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgSalesMainCompetitorModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidCompetitor()
		{
			var filter = new OrgSalesMainCompetitorModuleFilter("description", (a, b) => new ZQuery());
			AssertNoNotifications(filter.CompetitorInfo);

			filter.Competitor = ZGuid.Invalid;
			filter.Validation.ValidateAll();
			AssertHasError("Invalid Competitor should get an error", filter.CompetitorInfo, "Enter a valid selection.");
		}

		public void TestCheckEitherCompetitorOrTypeIsEmpty()
		{
			var filter = new OrgSalesMainCompetitorModuleFilter("description", (a, b) => new ZQuery());
			AssertNoNotifications(filter.CompetitorInfo);
			AssertNoNotifications(filter.CompetitorTypeInfo);

			var expectedWarning = "Both fields need to be set to filter results";

			filter.Competitor = ZGuid.Invalid;
			AssertHasWarning("Should add warning when Competitor isn't empty but CompetitorType is.", filter.CompetitorTypeInfo, expectedWarning);

			filter.CompetitorType = "CMB";
			AssertNoWarning("Should have no warning when both Competitor and CompetitorType are filled.", filter.CompetitorTypeInfo, expectedWarning);

			filter.Competitor = ZGuid.Empty;
			AssertHasWarning("Should add warning when CompetitorType isn't empty but Competitor is.", filter.CompetitorInfo, expectedWarning);

			filter.Competitor = ZGuid.Invalid;
			filter.CompetitorType = "";

			filter.Validation.ValidateAll();
			AssertHasWarning("Should add warning when Competitor isn't empty but CompetitorType is.", filter.CompetitorTypeInfo, expectedWarning);
			AssertHasError("Invalid Competitor should get an error", filter.CompetitorInfo, "Enter a valid selection.");

			filter.Competitor = ZGuid.Empty;
			AssertNoWarning("Should have no warning when both Competitor and CompetitorType are empty.", filter.CompetitorTypeInfo, expectedWarning);
			AssertNoWarning("Should have no warning when both Competitor and CompetitorType are empty.", filter.CompetitorInfo, expectedWarning);
		}

		public void TestCheckCompetitorTypeValid()
		{
			var testCompetitorCode = "TST";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.AddSystemDefined(testCompetitorCode, (NoResString)"TST REGISTRY", false);

			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				var filter = new OrgSalesMainCompetitorModuleFilter("description", (a, b) => new ZQuery());
				AssertNoNotifications(filter.CompetitorTypeInfo);
				filter.CompetitorType = "TS2";
				AssertHasWarning("Should add warning when CompetitorType isn't valid.",
						filter.CompetitorTypeInfo,
						"You have not entered a valid code.");
				filter.CompetitorType = testCompetitorCode;
				AssertNoNotifications(filter.CompetitorTypeInfo);
			}
		}
	}
}
