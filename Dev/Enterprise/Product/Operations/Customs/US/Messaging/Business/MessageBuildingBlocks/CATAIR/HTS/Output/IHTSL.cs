using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTSL
	{
		ZString TariffNumber { get; }
		ZString ParticipatingGovernmentAgencies { get; }
	}
}
