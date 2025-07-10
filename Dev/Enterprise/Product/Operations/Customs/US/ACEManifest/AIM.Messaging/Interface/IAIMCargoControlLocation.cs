using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMCargoControlLocation
	{
		ZString AirportOfArrival { get; }
		ZString CargoTerminalOperator { get; }
	}
}
