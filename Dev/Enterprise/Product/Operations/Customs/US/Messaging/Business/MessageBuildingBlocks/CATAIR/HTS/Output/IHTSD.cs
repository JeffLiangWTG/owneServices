using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTSD
	{
		ZString TariffNumber { get; }
		ZString SpecialProgramsIndicatorSPICode { get; }
	}
}
