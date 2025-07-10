using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsigneeOrForwarderCollection))]
	class ConsigneeOrForwarderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ConsigneeOrForwarderCollection(Factory, orgDefaults);
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
			Consignees.ValidateEntityOnSaving(organisation);
			AssertEquals("No errors - Consignee selected", false, organisation.OH_IsConsigneeInfo.HasErrors());

			organisation.OH_IsConsignee = false;
			organisation.OH_IsForwarder = true;
			Consignees.ValidateEntityOnSaving(organisation);
			AssertEquals("No errors - Forwarder selected", false, organisation.OH_IsConsigneeInfo.HasErrors());

			organisation.OH_IsForwarder = false;
			Consignees.ValidateEntityOnSaving(organisation);
			AssertEquals("Error - Consignee or Forwarder not selected", true, organisation.OH_IsConsigneeInfo.HasErrors());

			Consignees.AllowOtherOrgTypes = true;
			Consignees.ValidateEntityOnSaving(organisation);
			AssertEquals("No errors - AllowOtherOrgTypes", false, organisation.OH_IsConsigneeInfo.HasErrors());
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(Consignees.AllowNewTemporaryOrganisations);
		}

		public virtual void TestSetFilterBusinessObjectDefaults()
		{
			Assert(Consignees.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			Assert(Consignees.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));
		}

		#region Implementation

		ConsigneeOrForwarderCollection Consignees;

		protected override void SetUp()
		{
			base.SetUp();
			Consignees = new ConsigneeOrForwarderCollection(Factory);
		}

		#endregion
	}
}
