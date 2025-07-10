using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsigneeOrConsignorCollection))]
	sealed class ConsigneeOrConsignorCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewChildDefaults()
		{
			var org1 = ConsigneeOrConsignors.AddNew();
			Assert("Consignee is selected", org1.OH_IsConsignee);
			Assert("Consignor is selected", org1.OH_IsConsignor);

			Environment.Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed = false;
			var org2 = ConsigneeOrConsignors.AddNew();
			Assert("Consignee is not selected", !org2.OH_IsConsignee);
			Assert("Consignor is not selected", !org2.OH_IsConsignor);
		}

		public void TestValidateEntityOnSaving()
		{
			var org = ConsigneeOrConsignors.AddNew();
			ConsigneeOrConsignors.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Consignee and Consignor selected", false, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("No errors - Consignee and Consignor selected", false, org.OH_IsConsignorInfo.HasErrors());

			org.OH_IsConsignee = false;
			org.OH_IsConsignor = true;
			ConsigneeOrConsignors.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Consignor selected", false, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("No errors - Consignor selected", false, org.OH_IsConsignorInfo.HasErrors());

			org.OH_IsConsignor = false;
			ConsigneeOrConsignors.ValidateEntityOnSaving(org);
			AssertEquals("Error - Neither a consignee nor a consignor selected", true, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("Error - Neither a consignee nor a consignor selected", true, org.OH_IsConsignorInfo.HasErrors());

			ConsigneeOrConsignors.AllowOtherOrgTypes = true;
			ConsigneeOrConsignors.ValidateEntityOnSaving(org);
			AssertEquals("No errors - AllowOtherOrgTypes", false, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("No errors - AllowOtherOrgTypes", false, org.OH_IsConsignorInfo.HasErrors());
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(ConsigneeOrConsignors.AllowNewTemporaryOrganisations);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(ConsigneeOrConsignors.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			Assert(ConsigneeOrConsignors.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3"));
		}

		#region Implementation

		ConsigneeOrConsignorCollection ConsigneeOrConsignors;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new ConsigneeOrConsignorCollection(Factory, orgDefaults);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ConsigneeOrConsignors = new ConsigneeOrConsignorCollection(Factory);
		}

		#endregion
	}
}
