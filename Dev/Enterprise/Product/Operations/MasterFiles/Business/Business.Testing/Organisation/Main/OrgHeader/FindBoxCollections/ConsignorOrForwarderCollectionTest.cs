using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsignorOrForwarderCollection))]
	sealed class ConsignorOrForwarderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ConsignorOrForwarderCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Consignors.AddNew();
			AssertEquals("Consignor is selected", true, org1.OH_IsConsignor);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagSP.IsAllowed = false;

			OrgHeader org2 = Consignors.AddNew();
			AssertEquals("Consignor is not selected", false, org2.OH_IsConsignor);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Consignors.AddNew();
			Consignors.ValidateEntityOnSaving(organisation);
			AssertEquals("No errors - Consignor selected", false, organisation.OH_IsConsignorInfo.HasErrors());

			organisation.OH_IsConsignor = false;
			organisation.OH_IsForwarder = true;
			Consignors.ValidateEntityOnSaving(organisation);
			AssertEquals("No errors - Forwarder selected", false, organisation.OH_IsConsignorInfo.HasErrors());

			organisation.OH_IsForwarder = false;
			Consignors.ValidateEntityOnSaving(organisation);
			AssertEquals("Error - Consignor or Forwarder not selected", true, organisation.OH_IsConsignorInfo.HasErrors());

			Consignors.AllowOtherOrgTypes = true;
			Consignors.ValidateEntityOnSaving(organisation);
			AssertEquals("No errors - AllowOtherOrgTypes", false, organisation.OH_IsConsignorInfo.HasErrors());
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(Consignors.AllowNewTemporaryOrganisations);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(Consignors.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3"));
			Assert(Consignors.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));
		}

		#region Implementation

		ConsignorOrForwarderCollection Consignors;

		protected override void SetUp()
		{
			base.SetUp();
			Consignors = new ConsignorOrForwarderCollection(Factory);
		}

		#endregion
	}
}
