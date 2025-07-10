using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesOrganisationCollection))]
	sealed class SalesOrganisationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new SalesOrganisationCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = SalesOrgs.AddNew();
			AssertEquals("SalesOrg is selected", true, org1.OH_IsSalesLead);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSal.IsAllowed = false;

			OrgHeader org2 = SalesOrgs.AddNew();
			AssertEquals("SalesOrg is not selected", false, org2.OH_IsSalesLead);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = SalesOrgs.AddNew();
			organisation.OH_IsSalesLead = false;
			SalesOrgs.ValidateEntityOnSaving(organisation);
			Assert("Error - Sales not selected", organisation.OH_IsSalesLeadInfo.HasErrors());

			organisation.OH_IsSalesLead = true;
			SalesOrgs.ValidateEntityOnSaving(organisation);
			Assert("No error - Sales selected", !organisation.OH_IsSalesLeadInfo.HasErrors());
		}

		#region Implementation

		SalesOrganisationCollection SalesOrgs;

		protected override void SetUp()
		{
			base.SetUp();
			SalesOrgs = new SalesOrganisationCollection(Factory);
		}

		#endregion
	}
}
