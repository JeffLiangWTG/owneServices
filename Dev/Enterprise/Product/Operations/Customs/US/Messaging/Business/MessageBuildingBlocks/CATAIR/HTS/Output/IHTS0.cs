using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTS0
	{
		ZString FromTariffNumber { get; }
		ZDate AsOfDate { get; }
		ZString ToTariffNumber { get; }
		ZString NarrativeMessage { get; }
	}
}
