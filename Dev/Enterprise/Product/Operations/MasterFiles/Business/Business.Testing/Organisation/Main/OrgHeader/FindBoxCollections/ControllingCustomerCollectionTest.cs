using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ControllingCustomerCollection))]
	sealed class ControllingCustomerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new ControllingCustomerCollection(Factory, orgDefaults);
		}

		public void TestValidateEntityOnSaving()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader organisation = ControllingCustomers.AddNew();
			organisation.OH_IsControllingCustomer = false;
			ControllingCustomers.ValidateEntityOnSaving(organisation);
			Assert("Error - Controlling Customer is not selected", organisation.OH_IsControllingCustomerInfo.HasErrors());

			organisation.OH_IsControllingCustomer = true;
			ControllingCustomers.ValidateEntityOnSaving(organisation);
			Assert("No error - Controlling Customer is selected", !organisation.OH_IsControllingCustomerInfo.HasErrors());

			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			organisation.OH_IsControllingCustomer = false;
			ControllingCustomers.ValidateEntityOnSaving(organisation);
			Assert("No error - Controlling Customer is not selected", !organisation.OH_IsControllingCustomerInfo.HasErrors());
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(
				ControllingCustomers.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" +
																					 FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property13"));
		}

		#region Implementation

		ControllingCustomerCollection ControllingCustomers;

		protected override void SetUp()
		{
			base.SetUp();
			ControllingCustomers = new ControllingCustomerCollection(Factory);
		}

		#endregion
	}
}
