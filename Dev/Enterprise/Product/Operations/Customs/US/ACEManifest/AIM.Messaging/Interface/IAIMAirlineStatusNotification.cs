using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMAirlineStatusNotification
	{
		ZString StatusCode { get; }
		ZString ActionExplanation { get; }
	}
}
