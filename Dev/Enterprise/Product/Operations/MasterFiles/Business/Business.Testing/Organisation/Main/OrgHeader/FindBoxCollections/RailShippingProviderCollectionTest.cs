using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RailShippingProviderCollection))]
	public class RailShippingProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new RailShippingProviderCollection(Factory, orgDefaults);
		}

		public void TestFilter()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgHeader railProvider = Factory.New<OrgHeader>();

			carrier.OH_IsShippingProvider = true;

			railProvider.OH_IsShippingProvider = true;
			railProvider.OH_IsRailProvider = true;

			RailProviders.Load();
			Assert("Header should not be in list", !RailProviders.Contains(header));
			Assert("Carrier should not be in list", !RailProviders.Contains(carrier));
			Assert("Rail provider should be in list", RailProviders.Contains(railProvider));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader organisation = RailProviders.AddNew();
			Assert("ShippingProvider is selected", organisation.OH_IsShippingProvider);
			Assert("RailProvider is selected", organisation.OH_IsRailProvider);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = RailProviders.AddNew();
			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsRailProvider = false;
			RailProviders.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider has error", organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("RailProvider has error", organisation.OH_IsRailProviderInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			RailProviders.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("RailProvider has error", organisation.OH_IsRailProviderInfo.HasErrors());

			organisation.OH_IsRailProvider = true;
			RailProviders.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("RailProvider does not have error", !organisation.OH_IsRailProviderInfo.HasErrors());
		}

		#region Implementation

		RailShippingProviderCollection RailProviders;

		protected override void SetUp()
		{
			base.SetUp();
			RailProviders = GetNewRailShippingProviderCollection();
		}

		protected virtual RailShippingProviderCollection GetNewRailShippingProviderCollection()
		{
			return new RailShippingProviderCollection(Factory);
		}

		#endregion
	}
}
