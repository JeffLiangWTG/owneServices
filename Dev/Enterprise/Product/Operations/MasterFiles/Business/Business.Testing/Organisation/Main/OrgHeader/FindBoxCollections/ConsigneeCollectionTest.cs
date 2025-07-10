using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsigneeCollection))]
	public class ConsigneeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ConsigneeCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Consignees.AddNew();
			AssertEquals("Consignee is selected", true, org1.OH_IsConsignee);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed = false;

			OrgHeader org2 = Consignees.AddNew();
			AssertEquals("Consignee is not selected", false, org2.OH_IsConsignee);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Consignees.AddNew();
			organisation.OH_IsConsignee = false;
			Consignees.ValidateEntityOnSaving(organisation);
			Assert("Error - Consignee not selected", organisation.OH_IsConsigneeInfo.HasErrors());

			organisation.OH_IsConsignee = true;
			Consignees.ValidateEntityOnSaving(organisation);
			Assert("No error - Consignee selected", !organisation.OH_IsConsigneeInfo.HasErrors());

			Consignees.AllowOtherOrgTypes = true;
			organisation.OH_IsConsignee = false;
			Consignees.ValidateEntityOnSaving(organisation);
			Assert("No error - Consignee not selected", !organisation.OH_IsConsigneeInfo.HasErrors());
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(Consignees.AllowNewTemporaryOrganisations);
		}

		public virtual void TestSetFilterBusinessObjectDefaults()
		{
			Assert(Consignees.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			Assert(Consignees.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2", SearchType.Index));
		}

		#region Implementation

		ConsigneeCollection Consignees;

		protected override void SetUp()
		{
			base.SetUp();
			Consignees = new ConsigneeCollection(Factory);
		}

		#endregion
	}
}
