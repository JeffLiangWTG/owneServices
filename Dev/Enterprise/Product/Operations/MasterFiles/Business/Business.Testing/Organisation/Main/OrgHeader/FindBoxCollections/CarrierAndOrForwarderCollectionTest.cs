using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CarrierAndOrForwarderCollection))]
	sealed class CarrierAndOrForwarderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewChildDefaults()
		{
			var collection = new CarrierAndOrForwarderCollection(Factory);

			var org1 = collection.AddNew();
			Assert("Forwarder is selected", org1.OH_IsForwarder);
		}

		public void TestValidateEntityOnSaving()
		{
			var collection = new CarrierAndOrForwarderCollection(Factory);

			var org = collection.AddNew();
			collection.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Forwarder and Carrier selected", false, org.OH_IsForwarderInfo.HasErrors());
			AssertEquals("No errors - Forwarder and Carrier selected", false, org.OH_IsShippingProviderInfo.HasErrors());

			org.OH_IsForwarder = false;
			org.OH_IsShippingProvider = true;
			collection.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Forwarder not selected", false, org.OH_IsForwarderInfo.HasErrors());
			AssertEquals("No errors - Carrier selected", false, org.OH_IsShippingProviderInfo.HasErrors());

			org.OH_IsShippingProvider = false;
			collection.ValidateEntityOnSaving(org);
			AssertEquals("Error - Neither a forwarder nor a carrier selected", true, org.OH_IsForwarderInfo.HasErrors());
			AssertEquals("Error - Neither a forwarder nor a carrier selected", true, org.OH_IsShippingProviderInfo.HasErrors());

			org.OH_IsForwarder = true;
			org.OH_IsShippingProvider = true;
			collection.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Forwarder selected", false, org.OH_IsForwarderInfo.HasErrors());
			AssertEquals("No errors - Carrier selected", false, org.OH_IsShippingProviderInfo.HasErrors());

			org.OH_IsShippingProvider = false;
			collection.ValidateEntityOnSaving(org);
			AssertEquals("No errors - Forwarder selected", false, org.OH_IsForwarderInfo.HasErrors());
			AssertEquals("No errors - Carrier not selected", false, org.OH_IsShippingProviderInfo.HasErrors());
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			var collection = new CarrierAndOrForwarderCollection(Factory);
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property4"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new CarrierAndOrForwarderCollection(Factory, orgDefaults);
		}

		#endregion
	}
}
