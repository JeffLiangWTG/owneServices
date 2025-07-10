using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging
{
	public interface IInterchangeSenderIdProvider
	{
		ZString SenderID { get; }
	}
}
