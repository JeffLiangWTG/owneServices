using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ShippingProviderCollection))]
	sealed class ShippingProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ShippingProviderCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = ShippingProviders.AddNew();
			AssertEquals("ShippingProvider is selected", true, org1.OH_IsShippingProvider);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed = false;

			OrgHeader org2 = ShippingProviders.AddNew();
			AssertEquals("ShippingProvider is not selected", false, org2.OH_IsShippingProvider);
		}

		public void TestFilterDefaultShippingLine()
		{
			var shippingProviders = new ShippingProviderCollection(Factory, true);
			var secondaryTypeDefault = shippingProviders.FilterBusinessObjectDefaults["Secondary Type:Property"];
			AssertNotNull(secondaryTypeDefault);
			AssertEquals("Secondary Type", secondaryTypeDefault.FilterName);
			AssertEquals("Property", secondaryTypeDefault.PropertyName);
			AssertEquals("Carrier - Shipping Line", secondaryTypeDefault.Value);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = ShippingProviders.AddNew();
			organisation.OH_IsShippingProvider = false;
			ShippingProviders.ValidateEntityOnSaving(organisation);
			Assert("Error - ShippingProvider not selected", organisation.OH_IsShippingProviderInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			ShippingProviders.ValidateEntityOnSaving(organisation);
			Assert("No error - ShippingProvider selected", !organisation.OH_IsShippingProviderInfo.HasErrors());

			ShippingProviders.AllowOtherOrgTypes = true;
			organisation.OH_IsShippingProvider = false;
			ShippingProviders.ValidateEntityOnSaving(organisation);
			Assert("No error - ShippingProvider not selected", !organisation.OH_IsShippingProviderInfo.HasErrors());
		}

		#region Implementation

		ShippingProviderCollection ShippingProviders;

		protected override void SetUp()
		{
			base.SetUp();
			ShippingProviders = new ShippingProviderCollection(Factory);
		}

		#endregion
	}
}
