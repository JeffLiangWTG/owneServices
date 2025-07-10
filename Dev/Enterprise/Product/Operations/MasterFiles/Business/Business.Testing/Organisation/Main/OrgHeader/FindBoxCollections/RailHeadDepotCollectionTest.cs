using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RailHeadDepotCollection))]
	sealed class RailHeadDepotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new RailHeadDepotCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = RailHeadDepots.AddNew();
			AssertEquals("RailHeadDepot is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("RailHeadDepot is selected", true, org1.OH_IsRailHead);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = RailHeadDepots.AddNew();
			AssertEquals("RailHeadDepot is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("RailHeadDepot is selected", true, org2.OH_IsRailHead);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = RailHeadDepots.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsRailHead = false;
			RailHeadDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("RailHeadDepots has error", organisation.OH_IsRailHeadInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			RailHeadDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("RailHeadDepots has error", organisation.OH_IsRailHeadInfo.HasErrors());

			organisation.OH_IsRailHead = true;
			RailHeadDepots.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("RailHeadDepots does not have error", !organisation.OH_IsRailHeadInfo.HasErrors());
		}

		#region Implementation

		RailHeadDepotCollection RailHeadDepots;

		protected override void SetUp()
		{
			base.SetUp();
			RailHeadDepots = new RailHeadDepotCollection(Factory);
		}

		#endregion
	}
}
