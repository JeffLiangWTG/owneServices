using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTS3
	{
		ZString TariffNumber { get; }
		ZString GeneralizedSystemOfPreferencesGSPExcludedCountries { get; }
		ZString AntidumpingDutyFlag { get; }
		ZString QuotaIndicator { get; }
		ZString CategoryNumber { get; }
		ZString SpecialProgramsIndicatorSPICode { get; }
	}
}
