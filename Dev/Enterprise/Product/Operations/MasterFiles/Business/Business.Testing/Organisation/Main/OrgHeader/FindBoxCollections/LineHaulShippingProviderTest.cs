using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(LineHaulShippingProviderCollection))]
	public class LineHaulShippingProviderTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new LineHaulShippingProviderCollection(Factory, orgDefaults);
		}

		public void TestFilter()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgHeader lineHauler = Factory.New<OrgHeader>();

			carrier.OH_IsShippingProvider = true;

			lineHauler.OH_IsShippingProvider = true;
			lineHauler.OH_IsLineHaulProvider = true;

			LineHaulers.Load();
			Assert("Header should not be in list", !LineHaulers.Contains(header));
			Assert("Carrier should not be in list", !LineHaulers.Contains(carrier));
			Assert("Local transport provider should be in list", LineHaulers.Contains(lineHauler));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader organisation = LineHaulers.AddNew();
			Assert("ShippingProvider is selected", organisation.OH_IsShippingProvider);
			Assert("LineHaulProvider is selected", organisation.OH_IsLineHaulProvider);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = LineHaulers.AddNew();
			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsLineHaulProvider = false;
			LineHaulers.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider should have an error", organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("LineHaulProvider should have error", organisation.OH_IsLineHaulProviderInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			LineHaulers.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider should not have an error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("LineHaulProvider should have an error", organisation.OH_IsLineHaulProviderInfo.HasErrors());

			organisation.OH_IsLineHaulProvider = true;
			LineHaulers.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider should not have an error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("LineHaulProvider should not have an error", !organisation.OH_IsLineHaulProviderInfo.HasErrors());
		}

		#region Implementation

		LineHaulShippingProviderCollection LineHaulers;

		protected override void SetUp()
		{
			base.SetUp();
			LineHaulers = GetNewLineHaulShippingProviderCollection();
		}

		protected virtual LineHaulShippingProviderCollection GetNewLineHaulShippingProviderCollection()
		{
			return new LineHaulShippingProviderCollection(Factory);
		}

		#endregion
	}
}
