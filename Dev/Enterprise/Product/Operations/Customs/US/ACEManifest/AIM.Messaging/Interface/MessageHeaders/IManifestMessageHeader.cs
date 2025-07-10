namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IManifestMessageHeader : ICommonMessageHeader
	{
		IAIMCargoControlLocation CargoControlLine { get; }
		IAIMArrival Arrival { get; }
		IAIMAgent Agent { get; }
	}
}
