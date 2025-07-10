using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CarrierCollection))]
	sealed class CarrierCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewChildDefaults()
		{
			var collection = new CarrierCollection(Factory);

			var org1 = collection.AddNew();
			AssertEquals("Consignee is selected", true, org1.OH_IsShippingProvider);

			Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed = false;

			var org2 = collection.AddNew();
			AssertEquals("Consignee is not selected", false, org2.OH_IsShippingProvider);
		}

		public void TestValidateEntityOnSaving()
		{
			var collection = new CarrierCollection(Factory);

			var organisation = collection.AddNew();
			organisation.OH_IsShippingProvider = false;
			collection.ValidateEntityOnSaving(organisation);
			var expectedErrorMessage = "An Organization selected from here must have an Organization Type of Carrier selected.";
			AssertHasError("Error - Carrier not selected", organisation.OH_IsShippingProviderInfo, expectedErrorMessage);

			organisation.OH_IsShippingProvider = true;
			collection.ValidateEntityOnSaving(organisation);
			AssertNoErrors("No error - Carrier selected", organisation.OH_IsShippingProviderInfo);

			collection.AllowOtherOrgTypes = true;
			organisation.OH_IsShippingProvider = false;
			collection.ValidateEntityOnSaving(organisation);
			AssertNoErrors("No error - Carrier not selected", organisation.OH_IsShippingProviderInfo);
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			var collection = new CarrierCollection(Factory);
			AssertEquals(true, collection.AllowNewTemporaryOrganisations);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			var collection = new CarrierCollection(Factory);
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property4"));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new CarrierCollection(Factory, orgDefaults);
		}

		#endregion
	}
}
