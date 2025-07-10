using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgHasMainCompetitorModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCompetitorTypeValid()
		{
			var testCompetitorCode = "TST";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.AddSystemDefined(testCompetitorCode, (NoResString)"TST REGISTRY", false);

			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				var filter = new OrgHasMainCompetitorModuleFilter("description", (a, b) => new ZQuery());
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
