using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface IZZConditionSelectionCriteria : IZZApplicabilitySelectionCriteria
	{
		ZString ConditionClass { get; }
		ZString ConditionType { get; }
		ConditionChecker.ConditionDirection Direction { get; }
	}

	public class ZZConditionSelectionCriteria : IZZConditionSelectionCriteria
	{
		public ZZConditionSelectionCriteria(
			ZDateTime effectiveDate,
			ZString tradeGroupCountry,
			ZString primaryPreference,
			ISet<ZString> additionalCodes,
			ZString concessionOrder,
			ZString dataGrouping,
			ConditionChecker.ConditionDirection direction,
			ZString conditionClass,
			ZString conditionType,
			ISet<ZString> secondTradeGroups = null)
		{
			EffectiveDate = effectiveDate;
			TradeGroupCountry = tradeGroupCountry;
			PrimaryPreference = primaryPreference;
			AdditionalCodes = additionalCodes;
			ConcessionOrder = concessionOrder;
			DataGrouping = dataGrouping;
			Direction = direction;
			ConditionClass = conditionClass;
			ConditionType = conditionType;
			SecondTradeGroups = secondTradeGroups;
		}
		public ZDateTime EffectiveDate { get; }
		public ZString TradeGroupCountry { get; }
		public ZString PrimaryPreference { get; }
		public ISet<ZString> AdditionalCodes { get; }
		public ZString ConcessionOrder { get; }
		public ZString DataGrouping { get; }
		public ConditionChecker.ConditionDirection Direction { get; }
		public ZString ConditionClass { get; }
		public ZString ConditionType { get; }
		public ISet<ZString> SecondTradeGroups { get; }
	}
}
