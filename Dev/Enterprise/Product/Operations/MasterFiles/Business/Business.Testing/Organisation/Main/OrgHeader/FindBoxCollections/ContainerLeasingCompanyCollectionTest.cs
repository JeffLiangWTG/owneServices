using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContainerLeasingCompanyCollection))]
	public class ContainerLeasingCompanyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new ContainerLeasingCompanyCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			var org1 = ContainerLeasingCompanies.AddNew();
			AssertEquals("ContainerLeasingCompany is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("ContainerLeasingCompany is selected", true, org1.OH_IsContainerLeasingCompany);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			var org2 = ContainerLeasingCompanies.AddNew();
			AssertEquals("ContainerLeasingCompany is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("ContainerLeasingCompany is selected", true, org2.OH_IsContainerLeasingCompany);
		}

		public void TestValidateEntityOnSaving()
		{
			var organisation = ContainerLeasingCompanies.AddNew();
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsContainerLeasingCompany = false;
			ContainerLeasingCompanies.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices has error", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("ContainerLeasingCompany has error", organisation.OH_IsContainerLeasingCompanyInfo.HasErrors());

			organisation.OH_IsMiscFreightServices = true;
			ContainerLeasingCompanies.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("ContainerLeasingCompany has error", organisation.OH_IsContainerLeasingCompanyInfo.HasErrors());

			organisation.OH_IsContainerLeasingCompany = true;
			ContainerLeasingCompanies.ValidateEntityOnSaving(organisation);
			Assert("MiscFreightServices does not have error", !organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("ContainerLeasingCompany does not have error", !organisation.OH_IsContainerLeasingCompanyInfo.HasErrors());
		}

		#region Implementation

		protected ContainerLeasingCompanyCollection ContainerLeasingCompanies;

		protected override void SetUp()
		{
			base.SetUp();
			ContainerLeasingCompanies = new ContainerLeasingCompanyCollection(Factory);
		}

		#endregion
	}
}
