using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class FilteredCusRefApplicabilityViewCollection : CusRefApplicabilityViewCollection
	{
		public FilteredCusRefApplicabilityViewCollection(RateView master, bool enableEffectiveDataGrouping)
			: base(master)
		{
			rate = master;
			tariff = rate.CusTariff;
			wrapper = tariff?.Wrapper;
			enableDataGrouping = enableEffectiveDataGrouping;
			SetWrapperEvent(enableEffectiveDataGrouping);
		}

		public FilteredCusRefApplicabilityViewCollection(RefCusCondition master, bool enableEffectiveDataGrouping)
			: base(master)
		{
			tariff = master.CusTariff;
			wrapper = tariff?.Wrapper;
			enableDataGrouping = enableEffectiveDataGrouping;
			SetWrapperEvent(enableEffectiveDataGrouping);
		}

		public FilteredCusRefApplicabilityViewCollection(TariffAdditionalCodeView master, bool enableEffectiveDataGrouping)
			: base(master)
		{
			tariff = master.CusTariff;
			wrapper = tariff?.Wrapper;
			enableDataGrouping = enableEffectiveDataGrouping;
			SetWrapperEvent(enableEffectiveDataGrouping);
		}

		readonly RateView rate;
		readonly TariffView tariff;
		readonly TariffViewWrapper wrapper;
		readonly bool enableDataGrouping;

		protected override bool MatchesFilterCore(CusRefApplicabilityView element, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && IsDateValid(element) && IsDataGroupingValid(element) && IsTradeGroupValid(element);
		}

		protected override void SetDefaultsForNewElementCore(CusRefApplicabilityView newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ZZT_StartDate = rate.ZZ2_StartDate;
			newElement.ZZT_EndDate = rate.ZZ2_EndDate;
		}

		void SetWrapperEvent(bool enableEffectiveDataGrouping)
		{
			if (wrapper != null)
			{
				if (enableEffectiveDataGrouping)
				{
					wrapper.EffectiveDataGroupingInfo.ValueChanged += RefreshCollection;
				}
				wrapper.RatesApplyToCountryInfo.ValueChanged += RefreshCollection;
				wrapper.EffectiveDateInfo.ValueChanged += RefreshCollection;
			}
		}

		bool IsDataGroupingValid(CusRefApplicabilityView element)
		{
			var result = true;
			if (enableDataGrouping)
			{
				if (tariff != null)
				{
					var dataGrouping = element.ZZT_TradeGroupDataGrouping;
					result = tariff.MatchDataGrouping(dataGrouping) && tariff.Wrapper.MatchEffectiveDataGrouping(dataGrouping);
				}
			}
			return result;
		}

		bool IsDateValid(CusRefApplicabilityView element)
		{
			var result = true;
			if (wrapper != null)
			{
				result = wrapper.IsWithInEffectiveDate(element.ZZT_StartDate, element.ZZT_EndDate);
			}
			return result;
		}

		bool IsTradeGroupValid(CusRefApplicabilityView element)
		{
			var isTradeGroupValid = true;
			var ratesApplyToCountry = wrapper?.RatesApplyToCountry ?? ZString.Empty;
			if (!ratesApplyToCountry.IsEmpty)
			{
				isTradeGroupValid = element.IsApplicable(ratesApplyToCountry, wrapper.EffectiveDate);
			}
			return isTradeGroupValid;
		}

		void RefreshCollection(object sender, EventArgs e)
		{
			((IActiveBusinessObjectCollection)this).Refresh();
		}
	}
}
