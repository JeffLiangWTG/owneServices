using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsortiumShippingProviderCollection))]
	sealed class ConsortiumShippingProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ConsortiumShippingProviderCollection(Factory);
		}

		public void TestFilter()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgHeader consortium = Factory.New<OrgHeader>();

			carrier.OH_IsShippingProvider = true;

			consortium.OH_IsShippingProvider = true;
			consortium.OH_IsShippingConsortium = true;

			Consortiums.Load();
			Assert("Header should not be in list", !Consortiums.Contains(header));
			Assert("Carrier should not be in list", !Consortiums.Contains(carrier));
			Assert("Consortium should be in list", Consortiums.Contains(consortium));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader organisation = Consortiums.AddNew();
			Assert("ShippingProvider is selected", organisation.OH_IsShippingProvider);
			Assert("Consortium is selected", organisation.OH_IsShippingConsortium);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Consortiums.AddNew();
			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsShippingConsortium = false;
			Consortiums.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider has error", organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("Consortium has error", organisation.OH_IsShippingConsortiumInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			Consortiums.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("Consortium has error", organisation.OH_IsShippingConsortiumInfo.HasErrors());

			organisation.OH_IsShippingConsortium = true;
			Consortiums.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("Consortium does not have error", !organisation.OH_IsShippingConsortiumInfo.HasErrors());
		}

		#region Implementation

		ConsortiumShippingProviderCollection Consortiums;

		protected override void SetUp()
		{
			base.SetUp();
			Consortiums = new ConsortiumShippingProviderCollection(Factory);
		}

		#endregion
	}
}
