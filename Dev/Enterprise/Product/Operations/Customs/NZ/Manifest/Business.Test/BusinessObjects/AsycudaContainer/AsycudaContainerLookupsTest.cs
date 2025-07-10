using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class AsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackLocationList()
		{
			AssertType<ContainerYardCollection>(container.Lookups.PackLocationOrganisations);
		}

		public void TestDeliveryDestinationPartyOrganisations()
		{
			AssertType<OrganisationsFindBoxCollection>(container.Lookups.DeliveryDestinationPartyOrganisations);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			container = (AsycudaContainer)header.Containers.AddNew();
		}

		AsycudaContainer container;
	}
}
