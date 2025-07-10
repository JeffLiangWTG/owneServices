using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IAutoRateDate
	{
		ZString DateType { get; }
		ZString JobType { get; }
		ZString DirectionCode { get; }
		ZString Mode { get; }
		ZString RateType { get; }
		ZString ContainerMode { get; }
		ZString Location { get; }
		ZBool IsFallbackDisabled { get; }
	}
}
