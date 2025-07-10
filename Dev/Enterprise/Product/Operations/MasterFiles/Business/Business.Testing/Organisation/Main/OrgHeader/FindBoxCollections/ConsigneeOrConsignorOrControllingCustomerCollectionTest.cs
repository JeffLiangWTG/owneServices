using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsigneeOrConsignorOrControllingCustomerCollection))]
	sealed class ConsigneeOrConsignorOrControllingCustomerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewChildDefaults()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org1 = TestCollection.AddNew();
			Assert("Consignee is selected", org1.OH_IsConsignee);
			Assert("Consignor is selected", org1.OH_IsConsignor);
			Assert("Controlling Customer is selected", org1.OH_IsControllingCustomer);

			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var org2 = TestCollection.AddNew();
			Assert("Consignee is selected", org2.OH_IsConsignee);
			Assert("Consignor is selected", org2.OH_IsConsignor);
			Assert("Controlling Customer is not selected", !org2.OH_IsControllingCustomer);

			Environment.Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed = false;
			var org3 = TestCollection.AddNew();
			Assert("Consignee is not selected", !org3.OH_IsConsignee);
			Assert("Consignor is not selected", !org3.OH_IsConsignor);
			Assert("Controlling Customer is not selected", !org3.OH_IsControllingCustomer);
		}

		public void TestValidateEntityOnSaving()
		{
			var org = TestCollection.AddNew();
			TestCollection.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Consignee and Consignor and Controlling Customer selected", false, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("No errors - Consignee and Consignor and Controlling Customer selected", false, org.OH_IsConsignorInfo.HasErrors());

			org.OH_IsConsignee = false;
			org.OH_IsControllingCustomer = false;
			org.OH_IsConsignor = true;
			TestCollection.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Consignor selected", false, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("No errors - Consignor selected", false, org.OH_IsConsignorInfo.HasErrors());

			org.OH_IsConsignor = false;
			TestCollection.ValidateEntityOnSaving(org);
			AssertEquals("Error - Neither a consignee, controlling customer nor a consignor selected", true, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("Error - Neither a consignee, controlling customer nor a consignor selected", true, org.OH_IsConsignorInfo.HasErrors());

			TestCollection.AllowOtherOrgTypes = true;
			TestCollection.ValidateEntityOnSaving(org);
			AssertEquals("No errors - AllowOtherOrgTypes", false, org.OH_IsConsigneeInfo.HasErrors());
			AssertEquals("No errors - AllowOtherOrgTypes", false, org.OH_IsConsignorInfo.HasErrors());
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(TestCollection.AllowNewTemporaryOrganisations);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(TestCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			Assert(TestCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3"));
			Assert(TestCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property13"));
		}

		#region Implementation

		ConsigneeOrConsignorOrControllingCustomerCollection TestCollection;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new ConsigneeOrConsignorOrControllingCustomerCollection(Factory, orgDefaults);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new ConsigneeOrConsignorOrControllingCustomerCollection(Factory);
		}

		#endregion
	}
}
