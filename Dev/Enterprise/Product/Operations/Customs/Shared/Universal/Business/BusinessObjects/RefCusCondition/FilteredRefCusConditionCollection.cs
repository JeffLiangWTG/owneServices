using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class FilteredRefCusConditionCollection : RefCusConditionCollection
	{
		public FilteredRefCusConditionCollection(TariffView parentTariff, bool enableEffectiveDataGrouping)
			: base(parentTariff, enableEffectiveDataGrouping, true)
		{
			wrapper = parentTariff.Wrapper;
			SetWrapperEvent();
		}
		readonly TariffViewWrapper wrapper;

		protected override bool MatchesFilterCore(RefCusCondition element, bool fetchOnlyFromLocalCache)
		{
			var filteringByRatesApplyToCountry = !(wrapper?.RatesApplyToCountry ?? ZString.Empty).IsEmpty;
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && (!filteringByRatesApplyToCountry || AnyFilteredRateApplicability(element));
		}

		void SetWrapperEvent()
		{
			if (wrapper != null)
			{
				wrapper.RatesApplyToCountryInfo.ValueChanged += RatesApplyToCountryInfo_ValueChanged;
			}
		}

		bool AnyFilteredRateApplicability(RefCusCondition element)
		{
			var filteredRateApplicabilitiesCollection = element.FilteredApplicabilities;
			RefreshCollection(filteredRateApplicabilitiesCollection);
			return filteredRateApplicabilitiesCollection.Count > 0;
		}

		void RatesApplyToCountryInfo_ValueChanged(object sender, System.EventArgs e) => RefreshCollection(this);

		void RefreshCollection(IActiveBusinessObjectCollection collection) => collection.Refresh();
	}
}
