using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsignorCollection))]
	sealed class ConsignorCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ConsignorCollection(Factory, orgDefaults);
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
			organisation.OH_IsConsignor = false;
			Consignors.ValidateEntityOnSaving(organisation);
			Assert("Error - Consignor not selected", organisation.OH_IsConsignorInfo.HasErrors());

			organisation.OH_IsConsignor = true;
			Consignors.ValidateEntityOnSaving(organisation);
			Assert("No error - Consignor selected", !organisation.OH_IsConsignorInfo.HasErrors());

			Consignors.AllowOtherOrgTypes = true;
			organisation.OH_IsConsignor = false;
			Consignors.ValidateEntityOnSaving(organisation);
			Assert("No error - Consignor not selected", !organisation.OH_IsConsignorInfo.HasErrors());
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(Consignors.AllowNewTemporaryOrganisations);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(Consignors.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3"));
			Assert(Consignors.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3", SearchType.Index));
		}

		#region Implementation

		ConsignorCollection Consignors;

		protected override void SetUp()
		{
			base.SetUp();
			Consignors = new ConsignorCollection(Factory);
		}

		#endregion
	}
}
