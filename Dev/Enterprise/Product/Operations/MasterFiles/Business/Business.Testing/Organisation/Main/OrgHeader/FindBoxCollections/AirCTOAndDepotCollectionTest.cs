using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AirCTOAndDepotCollection))]
	sealed class AirCTOAndDepotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AirCTOAndDepotCollection(Factory);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = TestCollection.AddNew();
			AssertEquals("Org is selected", true, org1.OH_IsMiscFreightServices);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = TestCollection.AddNew();
			AssertEquals("Org is not selected", false, org2.OH_IsMiscFreightServices);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = TestCollection.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			TestCollection.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot does not have error", !organisation.OH_IsPackDepotInfo.HasErrors());
			Assert("UnpackDepot does not have error", !organisation.OH_IsUnpackDepotInfo.HasErrors());
			Assert("AirCTO dose not have error", !organisation.OH_IsAirCTOInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			TestCollection.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot has error", organisation.OH_IsPackDepotInfo.HasErrors());
			Assert("UnpackDepot has error", organisation.OH_IsUnpackDepotInfo.HasErrors());
			Assert("AirCTO has error", organisation.OH_IsAirCTOInfo.HasErrors());

			organisation.OH_IsPackDepot = true;
			TestCollection.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("PackDepot does not have error", !organisation.OH_IsPackDepotInfo.HasErrors());
			Assert("UnpackDepot does not have error", !organisation.OH_IsUnpackDepotInfo.HasErrors());
			Assert("AirCTO dose not have error", !organisation.OH_IsAirCTOInfo.HasErrors());
		}

		public void TestFilter()
		{
			OrgHeader seaCTO = Factory.New<OrgHeader>();
			seaCTO.OH_IsMiscFreightServices = true;
			seaCTO.OH_IsSeaCTO = true;
			seaCTO.OH_IsAirCTO = false;
			seaCTO.OH_IsPackDepot = false;

			OrgHeader airCTO = Factory.New<OrgHeader>();
			airCTO.OH_IsMiscFreightServices = true;
			airCTO.OH_IsSeaCTO = false;
			airCTO.OH_IsAirCTO = true;
			airCTO.OH_IsPackDepot = false;

			OrgHeader depot = Factory.New<OrgHeader>();
			depot.OH_IsMiscFreightServices = true;
			depot.OH_IsSeaCTO = false;
			depot.OH_IsAirCTO = false;
			depot.OH_IsPackDepot = true;

			TestCollection.Load();
			Assert("Collection should contain Depot", TestCollection.Contains(depot));
			Assert("Collection should contain AirCTO", TestCollection.Contains(airCTO));
			Assert("Collection should not contain SeaCTO", !TestCollection.Contains(seaCTO));
		}

		#region Implementation

		AirCTOAndDepotCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new AirCTOAndDepotCollection(Factory);
		}

		#endregion
	}
}
