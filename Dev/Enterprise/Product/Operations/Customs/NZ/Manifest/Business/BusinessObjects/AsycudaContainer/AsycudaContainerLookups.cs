using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(AsycudaContainer parent)
			: base(parent)
		{
		}

		public ContainerYardCollection PackLocationOrganisations => new ContainerYardCollection(Factory);

		public OrganisationsFindBoxCollection DeliveryDestinationPartyOrganisations => new OrganisationsFindBoxCollection(Factory);
	}
}
