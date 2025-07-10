using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IINPP01
	{
		ZString PortOfUnlading { get; }
		ZDate EstimatedDate { get; }
	}
}
