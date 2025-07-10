using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMAirWaybill
	{
		ZString AirWaybillPrefix { get; }
		ZString AWBSerialNumber { get; }
		ZBool IsMasterAirWaybill { get; }
		ZString HAWBNumber { get; }
		ZString PackageTrackingIdentifier { get; }
		ZString PartArrivalReference { get; }
	}
}
