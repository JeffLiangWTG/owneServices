using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface ITariffDataGroupingRelatedBusinessObject
	{
		ZString DataGrouping { get; }
	}

	public interface ITariffEffectiveDatesRelatedBusinessObject : ITariffDataGroupingRelatedBusinessObject
	{
		ZDateTime StartDate { get; }
		ZDateTime EndDate { get; }
	}
}
