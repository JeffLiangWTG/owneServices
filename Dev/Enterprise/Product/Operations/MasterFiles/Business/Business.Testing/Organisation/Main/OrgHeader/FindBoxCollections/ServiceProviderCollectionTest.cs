using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ServiceProviderCollection))]
	sealed class ServiceProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ServiceProviderCollection(new BusinessObjectFactory(), orgDefaults);
		}

		public void TestCollectionReturnsAllPossibleCompanies()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			var services = Factory.NewWithValidTestData<OrgHeader>();

			creditor.CompanyData.OB_IsCreditor = true;
			carrier.OH_IsShippingProvider = true;
			forwarder.OH_IsForwarder = true;
			broker.OH_IsBroker = true;
			services.OH_IsMiscFreightServices = true;

			var nonMatching = Factory.NewWithValidTestData<OrgHeader>();
			nonMatching.CompanyData.OB_IsCreditor = false;
			nonMatching.OH_IsShippingProvider = false;
			nonMatching.OH_IsForwarder = false;
			nonMatching.OH_IsBroker = false;
			nonMatching.OH_IsMiscFreightServices = false;

			Factory.Save();

			var collection = new ServiceProviderCollection(new BusinessObjectFactory());
			AssertEquals("Collection.Count", 0, collection.Count);

			collection.Load();

			Assert("Collection.Count > 0", collection.Count > 0);
			Assert("Collection contains Creditor", collection.Contains(creditor.PK));
			Assert("Collection contains Creditor", collection.Contains(carrier.PK));
			Assert("Collection contains Creditor", collection.Contains(forwarder.PK));
			Assert("Collection contains Creditor", collection.Contains(broker.PK));
			Assert("Collection contains Creditor", collection.Contains(services.PK));
			Assert("Collection does not contain non matching company", !collection.Contains(nonMatching.PK));
		}

		public void TestNewChildDefaults()
		{
			var org1 = ServiceProviders.AddNew();
			AssertEquals("Creditor is selected", true, org1.CompanyData.OB_IsCreditor);
			AssertEquals("Carrier is selected", true, org1.OH_IsShippingProvider);
			AssertEquals("Forwarder is selected", true, org1.OH_IsForwarder);
			AssertEquals("Services is selected", true, org1.OH_IsMiscFreightServices);
			AssertEquals("Broker is selected", true, org1.OH_IsBroker);

			Environment.Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = false;
			Environment.Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed = false;
			Environment.Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed = false;
			Environment.Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed = false;
			Environment.Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;

			var org2 = ServiceProviders.AddNew();
			AssertEquals("Creditor is not selected", false, org2.CompanyData.OB_IsCreditor);
			AssertEquals("Carrier is not selected", false, org2.OH_IsShippingProvider);
			AssertEquals("Forwarder is not selected", false, org2.OH_IsForwarder);
			AssertEquals("Services is not selected", false, org2.OH_IsMiscFreightServices);
			AssertEquals("Broker is not selected", false, org2.OH_IsBroker);
		}

		public void TestValidateEntityOnSaving()
		{
			var org = ServiceProviders.AddNew();
			using (org.SuspendValidationTesting())
			using (org.CompanyData.SuspendValidationTesting())
			{
				org.CompanyData.OB_IsCreditor = false;
				org.OH_IsShippingProvider = false;
				org.OH_IsForwarder = false;
				org.OH_IsMiscFreightServices = false;
				org.OH_IsBroker = false;

				ServiceProviders.ValidateEntityOnSaving(org);
				Assert("Error - Creditor not selected", org.OH_IsCreditorInfo.HasErrors());
				Assert("Error - Forwarder not selected", org.OH_IsForwarderInfo.HasErrors());
				Assert("Error - Carrier not selected", org.OH_IsShippingProviderInfo.HasErrors());
				Assert("Error - Broker not selected", org.OH_IsBrokerInfo.HasErrors());
				Assert("Error - Services not selected", org.OH_IsMiscFreightServicesInfo.HasErrors());

				org.CompanyData.OB_IsCreditor = true;
				ServiceProviders.ValidateEntityOnSaving(org);
				AssertNoValidationErrors(org);

				org.CompanyData.OB_IsCreditor = false;
				org.OH_IsShippingProvider = true;
				ServiceProviders.ValidateEntityOnSaving(org);
				AssertNoValidationErrors(org);

				org.OH_IsShippingProvider = false;
				org.OH_IsForwarder = true;
				ServiceProviders.ValidateEntityOnSaving(org);
				AssertNoValidationErrors(org);

				org.OH_IsForwarder = false;
				org.OH_IsMiscFreightServices = true;
				ServiceProviders.ValidateEntityOnSaving(org);
				AssertNoValidationErrors(org);

				org.OH_IsMiscFreightServices = true;
				org.OH_IsBroker = false;

				ServiceProviders.ValidateEntityOnSaving(org);
				AssertNoValidationErrors(org);

				org.CompanyData.OB_IsCreditor = true;
				org.OH_IsShippingProvider = true;
				org.OH_IsForwarder = true;
				org.OH_IsMiscFreightServices = true;
				org.OH_IsBroker = true;
				ServiceProviders.ValidateEntityOnSaving(org);

				AssertNoValidationErrors(org);
			}
		}

		void AssertNoValidationErrors(OrgHeader org)
		{
			Assert("No error - Creditor selected", !org.OH_IsCreditorInfo.HasErrors());
			Assert("No error - Forwarder not selected", !org.OH_IsForwarderInfo.HasErrors());
			Assert("No error - Carrier not selected", !org.OH_IsShippingProviderInfo.HasErrors());
			Assert("No error - Broker not selected", !org.OH_IsBrokerInfo.HasErrors());
			Assert("No error - Services not selected", !org.OH_IsMiscFreightServicesInfo.HasErrors());
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(ServiceProviders.AllowNewTemporaryOrganisations);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(ServiceProviders.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			Assert(ServiceProviders.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property4"));
			Assert(ServiceProviders.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));
			Assert(ServiceProviders.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property8"));
			Assert(ServiceProviders.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property9"));
		}

		#region Implementation

		ServiceProviderCollection ServiceProviders;

		protected override void SetUp()
		{
			base.SetUp();
			ServiceProviders = new ServiceProviderCollection(new BusinessObjectFactory());
		}

		#endregion
	}
}
