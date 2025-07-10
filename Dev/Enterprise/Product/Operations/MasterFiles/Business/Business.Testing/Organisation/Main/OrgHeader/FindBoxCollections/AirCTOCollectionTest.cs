using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AirCTOCollection))]
	sealed class AirCTOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AirCTOCollection(Factory);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = AirCTOs.AddNew();
			AssertEquals("AirCTO is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("AirCTO is selected", true, org1.OH_IsAirCTO);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = AirCTOs.AddNew();
			AssertEquals("AirCTO is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("AirCTO is selected", true, org2.OH_IsAirCTO);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = AirCTOs.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsAirCTO = false;
			AirCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("AirCTO has error", organisation.OH_IsAirCTOInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			AirCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("AirCTO has error", organisation.OH_IsAirCTOInfo.HasErrors());

			organisation.OH_IsAirCTO = true;
			AirCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("AirCTO does not have error", !organisation.OH_IsAirCTOInfo.HasErrors());
		}

		#region Implementation

		AirCTOCollection AirCTOs;

		protected override void SetUp()
		{
			base.SetUp();
			AirCTOs = new AirCTOCollection(Factory);
		}

		#endregion
	}
}
