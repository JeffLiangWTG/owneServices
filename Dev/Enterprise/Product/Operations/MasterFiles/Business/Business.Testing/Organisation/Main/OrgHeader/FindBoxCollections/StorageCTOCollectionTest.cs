using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StorageCTOCollection))]
	sealed class StorageCTOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StorageCTOCollection(Factory);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = StorageCTOs.AddNew();
			AssertEquals("StorageCTOs is selected", true, org1.OH_IsMiscFreightServices);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			OrgHeader org2 = StorageCTOs.AddNew();
			AssertEquals("StorageCTOs is not selected", false, org2.OH_IsMiscFreightServices);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = StorageCTOs.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			StorageCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			StorageCTOs.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
		}

		#region Implementation

		StorageCTOCollection StorageCTOs;

		protected override void SetUp()
		{
			base.SetUp();
			StorageCTOs = new StorageCTOCollection(Factory);
		}

		#endregion
	}
}
