using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CompetitorCollection))]
	sealed class CompetitorCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new CompetitorCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader organisation = Competitors.AddNew();
			Assert("Competitor is selected", organisation.OH_IsCompetitor);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Competitors.AddNew();
			organisation.OH_IsCompetitor = false;
			Competitors.ValidateEntityOnSaving(organisation);
			Assert("Error - Competitor not selected", organisation.OH_IsCompetitorInfo.HasErrors());

			organisation.OH_IsCompetitor = true;
			Competitors.ValidateEntityOnSaving(organisation);
			Assert("No error - Competitor selected", !organisation.OH_IsCompetitorInfo.HasErrors());
		}

		#region Implementation

		CompetitorCollection Competitors;

		protected override void SetUp()
		{
			base.SetUp();
			Competitors = new CompetitorCollection(Factory);
		}

		#endregion
	}
}
