using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SeaCTOCollection))]
	sealed class SeaCTOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new SeaCTOCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = SeaCTOs.AddNew();
			AssertEquals("SeaCTO is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("SeaCTO is selected", true, org1.OH_IsSeaCTO);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = SeaCTOs.AddNew();
			AssertEquals("SeaCTO is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("SeaCTO is selected", true, org2.OH_IsSeaCTO);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = SeaCTOs.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsSeaCTO = false;
			SeaCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("SeaCTO has error", organisation.OH_IsSeaCTOInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			SeaCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("SeaCTO has error", organisation.OH_IsSeaCTOInfo.HasErrors());

			organisation.OH_IsSeaCTO = true;
			SeaCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("SeaCTO does not have error", !organisation.OH_IsSeaCTOInfo.HasErrors());
		}

		public void TestAdditionalFilter()
		{
			OrgHeader org1 = SeaCTOs.AddNew();
			org1.OH_RL_NKClosestPort = "AAAAA";

			OrgHeader org2 = SeaCTOs.AddNew();
			org2.OH_RL_NKClosestPort = "BBBBB";

			OrgHeader org3 = SeaCTOs.AddNew();
			org3.OH_RL_NKClosestPort = "BBBBB";

			SeaCTOs.Load();
			AssertEquals(true, SeaCTOs.Count >= 3);

			SeaCTOs.ClosestPortUNLOCO = "AAAAA";
			SeaCTOs.Load();
			AssertEquals(1, SeaCTOs.Count);
			SeaCTOs.ClosestPortUNLOCO = "BBBBB";
			SeaCTOs.Load();
			AssertEquals(2, SeaCTOs.Count);
			SeaCTOs.ClosestPortUNLOCO = "";
			SeaCTOs.Load();
			AssertEquals(true, SeaCTOs.Count >= 3);
		}

		#region Implementation

		SeaCTOCollection SeaCTOs;

		protected override void SetUp()
		{
			base.SetUp();
			SeaCTOs = new SeaCTOCollection(Factory);
		}

		#endregion
	}
}
