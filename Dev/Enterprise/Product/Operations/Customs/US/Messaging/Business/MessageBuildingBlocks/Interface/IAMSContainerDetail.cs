using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IAMSContainerDetail
	{
		ZString VIN { get; }
		ZString ContainerOperatorLine { get; }
		ZString ForeignPort { get; }
		ZString FactoryCarOrderNumber { get; }
	}
}
