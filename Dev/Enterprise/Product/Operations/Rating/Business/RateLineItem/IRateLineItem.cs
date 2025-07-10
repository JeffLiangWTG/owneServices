using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public interface IRateLineItem : IIndexer, IRate
	{
		IRateLine ParentRateLine { get; }
		ZString TM_Text { get; }
		ZGuid TM_AC { get; }
		ZString TM_Type { get; }
		ZDecimal TM_Break { get; }
		ZDecimal TM_BreakHourRate { get; }
		ZDecimal TM_BreakHour { get; }
		ZDecimal TM_RelevantValue { get; }
		ZDecimal TM_FlatAmount { get; }
		ZDecimal TM_BreakMinimum { get; }
		ZString TM_BreakWeightVolume { get; }
		ZString TM_F1Zone { get; }
		ZGuid TM_TZ_DomesticZone { get; }
		ZBool TM_CallForPricing { get; }
		ZDecimal TM_Value { get; }
		ZDecimal TM_AgentDeclaredRate { get; }
		ZInt TM_UnitMultiple { get; }
		ZString ApplyToDescription { get; }
		ZString CalculationOrderOrPercentOf { get; }
		BusinessObjectFactory Factory { get; }
		IEnumerable<IRateLineItem> RateLineItemsFromSameGroup { get; }
		void OverrideBreak(ZDecimal breakValue);
		void ResetBreakToOriginalValue();
	}
}
