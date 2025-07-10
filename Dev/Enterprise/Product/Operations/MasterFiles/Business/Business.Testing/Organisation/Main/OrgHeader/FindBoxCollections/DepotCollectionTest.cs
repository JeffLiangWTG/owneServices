using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DepotCollection))]
	sealed class DepotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new DepotCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Depots.AddNew();
			AssertEquals("Org is selected", true, org1.OH_IsMiscFreightServices);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = Depots.AddNew();
			AssertEquals("Org is not selected", false, org2.OH_IsMiscFreightServices);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Depots.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			Depots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot does not have error", !organisation.OH_IsPackDepotInfo.HasErrors());
			Assert("UnpackDepot does not have error", !organisation.OH_IsUnpackDepotInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			Depots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot has error", organisation.OH_IsPackDepotInfo.HasErrors());
			Assert("UnpackDepot has error", organisation.OH_IsUnpackDepotInfo.HasErrors());

			organisation.OH_IsPackDepot = true;
			Depots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot does not have error", !organisation.OH_IsPackDepotInfo.HasErrors());
			Assert("UnpackDepot does not have error", !organisation.OH_IsUnpackDepotInfo.HasErrors());
		}

		#region Implementation

		DepotCollection Depots;

		protected override void SetUp()
		{
			base.SetUp();
			Depots = new DepotCollection(Factory);
		}

		#endregion
	}
}
