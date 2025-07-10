using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PackDepotCollection))]
	sealed class PackDepotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new PackDepotCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = PackDepots.AddNew();
			AssertEquals("PackDepot is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("PackDepot is selected", true, org1.OH_IsPackDepot);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = PackDepots.AddNew();
			AssertEquals("PackDepot is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("PackDepot is selected", true, org2.OH_IsPackDepot);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = PackDepots.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsPackDepot = false;
			PackDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot has error", organisation.OH_IsPackDepotInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			PackDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot has error", organisation.OH_IsPackDepotInfo.HasErrors());

			organisation.OH_IsPackDepot = true;
			PackDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot does not have error", !organisation.OH_IsPackDepotInfo.HasErrors());
		}

		#region Implementation

		PackDepotCollection PackDepots;

		protected override void SetUp()
		{
			base.SetUp();
			PackDepots = new PackDepotCollection(Factory);
		}

		#endregion
	}
}
