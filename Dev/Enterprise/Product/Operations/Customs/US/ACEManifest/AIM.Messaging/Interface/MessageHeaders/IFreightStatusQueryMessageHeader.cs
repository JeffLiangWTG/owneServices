namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IFreightStatusQueryMessageHeader : IAIMMessageHeader
	{
		IAIMCargoControlLocation CargoControlLine { get; }
		IAIMAirWaybill AirWaybill { get; }
		IFreightStatusQuery FreightStatusQuery { get; }
	}
}
