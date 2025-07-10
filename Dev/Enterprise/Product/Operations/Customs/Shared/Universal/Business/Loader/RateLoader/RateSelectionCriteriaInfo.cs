using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class RateSelectionCriteriaInfo
	{
		public RateSelectionCriteriaInfo()
		{
		}

		public RateSelectionCriteriaInfo(ZDateTime effectiveDate
			, ZString tradeGroupCountry
			, ZString rateType
			, ZString rateCode
			, ZString zZT_OrderNumber
			, ZString zZT_AdditionalCode
			, ZString zZA_TradeGroup
			, ZString zZA_Description
			, ZString zZS_Preference
			, ZString zZS_Description
			, string secondTradeGroup = ""
			, string translatedPreferenceDescription = ""
			, RateDirection direction = RateDirection.Both)
		{
			EffectiveDate = effectiveDate;
			TradeGroupCountry = tradeGroupCountry;
			RateType = rateType;
			RateCode = rateCode;
			ZZT_OrderNumber = zZT_OrderNumber;
			ZZT_AdditionalCode = zZT_AdditionalCode;
			ZZA_TradeGroup = zZA_TradeGroup;
			ZZA_Description = zZA_Description;
			ZZS_Preference = zZS_Preference;
			ZZS_Description = zZS_Description;
			SecondTradeGroup = secondTradeGroup;
			TranslatedPreferenceDescription = translatedPreferenceDescription;
			Direction = direction;
		}

		public ZDateTime EffectiveDate { get; set; }
		public ZString TradeGroupCountry { get; set; }
		public ZString RateType { get; set; }
		public ZString RateCode { get; set; }
		public ZString ZZT_OrderNumber { get; set; }
		public ZString ZZT_AdditionalCode { get; set; }
		public ZString ZZA_TradeGroup { get; set; }
		public ZString ZZA_Description { get; set; }
		public ZString ZZS_Preference { get; set; }
		public ZString ZZS_Description { get; set; }
		public ZString SecondTradeGroup { get; set; }
		public ZString TranslatedPreferenceDescription { get; set; }
		public RateDirection Direction { get; set; }

		public bool MatchExcludingAdditionalCodes(IZZRateSelectionCriteria criteria) => Match(criteria, excludingAdditionalCodes: true);

		public bool MatchExcludingConcessionOrder(IZZRateSelectionCriteria criteria) => Match(criteria, excludingConcessionOrder: true);

		public bool MatchExcludingPrimaryPreference(IZZRateSelectionCriteria criteria) => Match(criteria, excludingPrimaryPreference: true);

		public bool MatchExcludingTradeGroup(IZZRateSelectionCriteria criteria) => Match(criteria, excludingTradeGroup: true);

		public bool Match(IZZRateSelectionCriteria criteria, bool excludingAdditionalCodes = false, bool excludingConcessionOrder = false, bool excludingPrimaryPreference = false, bool excludingTradeGroup = false)
		{
			var rateType = criteria.RateType;
			var rateCode = criteria.RateCode;
			var direction = criteria.Direction;

			return (criteria.EffectiveDate - EffectiveDate).Duration() < System.TimeSpan.FromMinutes(1)
				&& criteria.TradeGroupCountry == TradeGroupCountry
				&& (rateType.IsEmpty || rateType == RateType)
				&& (rateCode.IsEmpty || rateCode == RateCode)
				&& (direction == RateDirection.Both || direction == Direction)
				&& MatchPrimaryPreference(criteria, excludingPrimaryPreference, excludingTradeGroup)
				&& MatchConcessionOrder(criteria, excludingConcessionOrder)
				&& MatchAdditionalCodes(criteria, excludingAdditionalCodes)
				&& MatchSecondTradeGroups(criteria);
		}

		bool MatchPrimaryPreference(IZZRateSelectionCriteria criteria, bool excludingPrimaryPreference = false, bool excludingTradeGroup = false)
		{
			var primaryPreference = criteria.PrimaryPreference;
			return excludingPrimaryPreference || ZZS_Preference.IsEmpty || (primaryPreference.IsEmpty && !excludingTradeGroup) || primaryPreference == ZZS_Preference;
		}

		bool MatchSecondTradeGroups(IZZRateSelectionCriteria criteria)
		{
			var secondTradeGroups = criteria.SecondTradeGroups;
			return SecondTradeGroup.IsEmpty || secondTradeGroups == null || secondTradeGroups.All(s => s.IsEmpty) || secondTradeGroups.Contains(SecondTradeGroup);
		}

		bool MatchAdditionalCodes(IZZRateSelectionCriteria criteria, bool excludingAdditionalCodes = false)
		{
			var additionalCodes = criteria.AdditionalCodes;
			return excludingAdditionalCodes || ZZT_AdditionalCode.IsEmpty || additionalCodes == null || !additionalCodes.Any() || additionalCodes.All(l => l.IsEmpty) || additionalCodes.Contains(ZZT_AdditionalCode);
		}

		bool MatchConcessionOrder(IZZRateSelectionCriteria criteria, bool excludingConcessionOrder = false)
		{
			var concessionOrder = criteria.ConcessionOrder;
			return excludingConcessionOrder || (ZZS_Preference.IsEmpty && ZZT_OrderNumber.IsEmpty) || concessionOrder.IsEmpty || concessionOrder == ZZT_OrderNumber;
		}
	}
}
