using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SeaShippingProviderCollection))]
	sealed class SeaShippingProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new SeaShippingProviderCollection(Factory, orgDefaults);
		}

		public void TestFilter()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgHeader shippingLine = Factory.New<OrgHeader>();

			carrier.OH_IsShippingProvider = true;

			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_IsSeaWholesaler = true;

			ShippingLines.Load();
			Assert("Header should not be in list", !ShippingLines.Contains(header));
			Assert("Carrier should not be in list", !ShippingLines.Contains(carrier));
			Assert("Shipping line should be in list", ShippingLines.Contains(shippingLine));

			shippingLine.OH_IsShippingLine = false;
			shippingLine.OH_IsSeaWholesaler = true;
			ShippingLines.Load();

			Assert("Header should not be in list", !ShippingLines.Contains(header));
			Assert("Carrier should not be in list", !ShippingLines.Contains(carrier));
			Assert("Shipping line should be in list", ShippingLines.Contains(shippingLine));

			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_IsSeaWholesaler = false;
			ShippingLines.Load();

			Assert("Header should not be in list", !ShippingLines.Contains(header));
			Assert("Carrier should not be in list", !ShippingLines.Contains(carrier));
			Assert("Shipping line should be in list", ShippingLines.Contains(shippingLine));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader organisation = ShippingLines.AddNew();
			Assert("ShippingProvider is selected", organisation.OH_IsShippingProvider);
			Assert("ShippingLine is selected", organisation.OH_IsShippingLine);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = ShippingLines.AddNew();
			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsShippingLine = false;
			ShippingLines.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider has error", organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("ShippingLine has error", organisation.OH_IsShippingLineInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			ShippingLines.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("ShippingLine has error", organisation.OH_IsShippingLineInfo.HasErrors());

			organisation.OH_IsShippingLine = true;
			ShippingLines.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("ShippingLine does not have error", !organisation.OH_IsShippingLineInfo.HasErrors());
		}

		#region Implementation

		SeaShippingProviderCollection ShippingLines;

		protected override void SetUp()
		{
			base.SetUp();
			ShippingLines = new SeaShippingProviderCollection(Factory);
		}

		#endregion
	}
}
