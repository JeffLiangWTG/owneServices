using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RoadDepotTransitShedCollection))]
	sealed class RoadDepotTransitShedCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new RoadDepotTransitShedCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = RoadDepotTransitSheds.AddNew();
			AssertEquals("RoadDepotTransitShed is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("RoadDepotTransitShed is selected", true, org1.OH_IsRoadFreightDepot);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = RoadDepotTransitSheds.AddNew();
			AssertEquals("RoadDepotTransitShed is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("RoadDepotTransitShed is selected", true, org2.OH_IsRoadFreightDepot);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = RoadDepotTransitSheds.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsRoadFreightDepot = false;
			RoadDepotTransitSheds.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("RoadDepotTransitSheds has error", organisation.OH_IsRoadFreightDepotInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			RoadDepotTransitSheds.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("RoadDepotTransitSheds has error", organisation.OH_IsRoadFreightDepotInfo.HasErrors());

			organisation.OH_IsRoadFreightDepot = true;
			RoadDepotTransitSheds.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("RoadDepotTransitSheds does not have error", !organisation.OH_IsRoadFreightDepotInfo.HasErrors());
		}

		#region Implementation

		RoadDepotTransitShedCollection RoadDepotTransitSheds;

		protected override void SetUp()
		{
			base.SetUp();
			RoadDepotTransitSheds = new RoadDepotTransitShedCollection(Factory);
		}

		#endregion
	}
}
