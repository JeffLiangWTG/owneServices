using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContainerYardCollection))]
	sealed class ContainerYardCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new ContainerYardCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			var org1 = ContainerYards.AddNew();
			AssertEquals("ContainerYard is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("ContainerYard is selected", true, org1.OH_IsContainerYard);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			var org2 = ContainerYards.AddNew();
			AssertEquals("ContainerYard is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("ContainerYard is selected", true, org2.OH_IsContainerYard);
		}

		public void TestValidateEntityOnSaving()
		{
			var organisation = ContainerYards.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsContainerYard = false;
			ContainerYards.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("ContainerYard has error", organisation.OH_IsContainerYardInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			ContainerYards.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("ContainerYard has error", organisation.OH_IsContainerYardInfo.HasErrors());

			organisation.OH_IsContainerYard = true;
			ContainerYards.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("ContainerYard does not have error", !organisation.OH_IsContainerYardInfo.HasErrors());
		}

		#region Implementation

		ContainerYardCollection ContainerYards;

		protected override void SetUp()
		{
			base.SetUp();
			ContainerYards = new ContainerYardCollection(Factory);
		}

		#endregion
	}
}
