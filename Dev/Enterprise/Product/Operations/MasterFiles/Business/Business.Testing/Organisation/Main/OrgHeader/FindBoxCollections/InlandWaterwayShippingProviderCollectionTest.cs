using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(InlandWaterwayShippingProviderCollection))]
	public class InlandWaterwayShippingProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new InlandWaterwayShippingProviderCollection(Factory, orgDefaults);
		}

		public void TestFilter()
		{
			var header = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();
			var inlandWaterwayProvider = Factory.New<OrgHeader>();

			carrier.OH_IsShippingProvider = true;

			inlandWaterwayProvider.OH_IsShippingProvider = true;
			inlandWaterwayProvider.OH_IsInlandWaterwayProvider = true;

			InlandWaterwayProviders.Load();
			Assert("Header should not be in list", !InlandWaterwayProviders.Contains(header));
			Assert("Carrier should not be in list", !InlandWaterwayProviders.Contains(carrier));
			Assert("Inland waterway provider should be in list", InlandWaterwayProviders.Contains(inlandWaterwayProvider));
		}

		public void TestNewChildDefaults()
		{
			var organisation = InlandWaterwayProviders.AddNew();
			Assert("ShippingProvider is selected", organisation.OH_IsShippingProvider);
			Assert("InlandWaterwayProvider is selected", organisation.OH_IsInlandWaterwayProvider);
		}

		public void TestValidateEntityOnSaving()
		{
			var organisation = InlandWaterwayProviders.AddNew();
			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsInlandWaterwayProvider = false;
			InlandWaterwayProviders.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider has error", organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("InlandWaterwayProvider has error", organisation.OH_IsInlandWaterwayProviderInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			InlandWaterwayProviders.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("InlandWaterwayProvider has error", organisation.OH_IsInlandWaterwayProviderInfo.HasErrors());

			organisation.OH_IsInlandWaterwayProvider = true;
			InlandWaterwayProviders.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("InlandWaterwayProvider does not have error", !organisation.OH_IsInlandWaterwayProviderInfo.HasErrors());
		}

		#region Implementation

		protected InlandWaterwayShippingProviderCollection InlandWaterwayProviders;

		protected override void SetUp()
		{
			base.SetUp();
			InlandWaterwayProviders = GetNewInlandWaterwayShippingProviderCollection();
		}

		protected virtual InlandWaterwayShippingProviderCollection GetNewInlandWaterwayShippingProviderCollection()
		{
			return new InlandWaterwayShippingProviderCollection(Factory);
		}

		#endregion
	}
}
