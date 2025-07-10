using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMMessageHeader
	{
		ZString MessageType { get; }
		ZString Reference { get; }
	}
}
