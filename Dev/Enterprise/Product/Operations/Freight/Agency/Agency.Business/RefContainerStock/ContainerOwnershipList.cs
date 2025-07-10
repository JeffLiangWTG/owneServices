using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerOwnershipList : CodeDescriptionPairList
	{
		public ContainerOwnershipList()
		{
			AddPair(Constants.ContainerOwnership.Codes.CarrierOwned, Constants.ContainerOwnership.Descriptions.CarrierOwned);
			AddPair(Constants.ContainerOwnership.Codes.Leased, Constants.ContainerOwnership.Descriptions.Leased);
			AddPair(Constants.ContainerOwnership.Codes.ShipperOwned, Constants.ContainerOwnership.Descriptions.ShipperOwned);
		}
	}
}
