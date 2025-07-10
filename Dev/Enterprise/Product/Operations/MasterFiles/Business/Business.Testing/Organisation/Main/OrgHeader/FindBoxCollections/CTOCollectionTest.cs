using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CTOCollection))]
	sealed class CTOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new CTOCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = CTOs.AddNew();
			AssertEquals("CTO is selected", true, org1.OH_IsMiscFreightServices);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = CTOs.AddNew();
			AssertEquals("CTO is not selected", false, org2.OH_IsMiscFreightServices);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = CTOs.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			CTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("AirCTO does not have error", !organisation.OH_IsAirCTOInfo.HasErrors());
			Assert("SeaCTO does not have error", !organisation.OH_IsSeaCTOInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			CTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("AirCTO has error", organisation.OH_IsAirCTOInfo.HasErrors());
			Assert("SeaCTO has error", organisation.OH_IsSeaCTOInfo.HasErrors());

			organisation.OH_IsAirCTO = true;
			CTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("AirCTO does not have error", !organisation.OH_IsAirCTOInfo.HasErrors());
			Assert("SeaCTO does not have error", !organisation.OH_IsSeaCTOInfo.HasErrors());
		}

		#region Implementation

		CTOCollection CTOs;

		protected override void SetUp()
		{
			base.SetUp();
			CTOs = new CTOCollection(Factory);
		}

		#endregion
	}
}
