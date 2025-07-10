namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IBillMessageHeader : ICommonMessageHeader
	{
		IAIMCBPShipmentDescription CPBShipmentDescription { get; }
	}
}
