using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FumigationContractorCollection))]
	sealed class FumigationContractorCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new FumigationContractorCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Fumigatorers.AddNew();
			AssertEquals("Fumigatorer is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("Fumigatorer is selected", true, org1.OH_IsFumigationContractor);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = Fumigatorers.AddNew();
			AssertEquals("Fumigatorer is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("Fumigatorer is selected", true, org2.OH_IsFumigationContractor);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Fumigatorers.AddNew();
			organisation.OH_IsFumigationContractor = false;
			Fumigatorers.ValidateEntityOnSaving(organisation);
			Assert("Error - OH_IsFumigationContractor not selected", organisation.OH_IsFumigationContractorInfo.HasErrors());

			organisation.OH_IsFumigationContractor = true;
			Fumigatorers.ValidateEntityOnSaving(organisation);
			Assert("No error - OH_IsFumigationContractor selected", !organisation.OH_IsFumigationContractorInfo.HasErrors());
		}

		#region Implementation

		FumigationContractorCollection Fumigatorers;

		protected override void SetUp()
		{
			base.SetUp();
			Fumigatorers = new FumigationContractorCollection(Factory);
		}

		#endregion
	}
}
