using System.Collections.ObjectModel;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface ISerialiserSupporter
	{
		string Serialise(bool humanFriendly, string outgoingApplicationIdentifier, ReadOnlyCollection<MessageBlock> messageBlocks);
	}
}
