using Enterprise.Core;

namespace Enterprise.Freight.Business
{
	public static class ContainerRatingExtensions
	{
		public static string GetOwnership(this CommonContainer container)
			=> container.JC_IsShipperOwned
				? Constants.ContainerOwnership.Codes.ShipperOwned
				: Constants.ContainerOwnership.Codes.CarrierOwned;
	}
}
