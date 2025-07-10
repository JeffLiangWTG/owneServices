using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface IJobDeclarationSailingDateUpdater
		{
			ZString UpdateETA(ZDateTime arrivalDate, long ticks);
			ZString UpdateETD(ZDateTime departureDate, long ticks);
		}
	}
}
