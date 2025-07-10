using CargoWise.Types;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Integration
{
	public interface IContainerPenaltyMatchResult
	{
		ZByte FreeDays { get; }
		ZString FreeDayType { get; }
		ZString PenaltyType { get; }
		ZString CreditorType { get; }
		IContainerPenaltyDayExclusion FreeDayExclusion { get; }
		IContainerPenaltyDayExclusion DurationExclusion { get; }
	}
}
