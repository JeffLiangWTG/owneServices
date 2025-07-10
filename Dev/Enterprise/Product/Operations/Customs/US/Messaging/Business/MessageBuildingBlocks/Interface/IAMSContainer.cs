using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IAMSContainer
	{
		ZString ContainerNumber { get; }
		ZString SealNumber1 { get; }
		ZString SealNumber2 { get; }
	}
}
