using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UnpackDepotCollection))]
	sealed class UnpackDepotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new UnpackDepotCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = UnpackDepots.AddNew();
			AssertEquals("UnpackDepot is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("UnpackDepot is selected", true, org1.OH_IsUnpackDepot);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = UnpackDepots.AddNew();
			AssertEquals("UnpackDepot is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("UnpackDepot is selected", true, org2.OH_IsUnpackDepot);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = UnpackDepots.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsUnpackDepot = false;
			UnpackDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("UnpackDepot has error", organisation.OH_IsUnpackDepotInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			UnpackDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("UnpackDepot has error", organisation.OH_IsUnpackDepotInfo.HasErrors());

			organisation.OH_IsUnpackDepot = true;
			UnpackDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("UnpackDepot does not have error", !organisation.OH_IsUnpackDepotInfo.HasErrors());
		}

		#region Implementation

		UnpackDepotCollection UnpackDepots;

		protected override void SetUp()
		{
			base.SetUp();
			UnpackDepots = new UnpackDepotCollection(Factory);
		}

		#endregion
	}
}
