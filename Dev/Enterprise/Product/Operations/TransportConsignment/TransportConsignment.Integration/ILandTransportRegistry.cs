using Enterprise.Integration;

namespace Enterprise.TransportConsignment.Integration
{
	public interface ILandTransportRegistry
	{
		IRegistryItem DefaultJobLoadingFixedDuration { get; }
	}
}
