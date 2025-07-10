using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class FilteredTariffUOMViewCollection : TariffUOMViewCollection
	{
		public FilteredTariffUOMViewCollection(TariffView parentTariff, bool enableEffectiveDataGrouping)
			: base(parentTariff, enableEffectiveDataGrouping)
		{
			wrapper = parentTariff.Wrapper;
			if (wrapper != null)
			{
				SetWrapperEvent(enableEffectiveDataGrouping);
			}
		}

		readonly TariffViewWrapper wrapper;

		protected override bool AllowNew => true;

		protected override bool MatchesFilterCore(TariffUOMView element, bool fetchOnlyFromLocalCache)
			=> base.MatchesFilterCore(element, fetchOnlyFromLocalCache)
			&& IsTradeGroupValid(element);

		internal bool IsTradeGroupValid(TariffUOMView element)
		{
			var ratesApplyToCountry = wrapper?.RatesApplyToCountry ?? ZString.Empty;
			if (!ratesApplyToCountry.IsEmpty)
			{
				return element.IsApplicable(ratesApplyToCountry);
			}
			return true;
		}

		void SetWrapperEvent(bool enableEffectiveDataGrouping)
		{
			wrapper.RatesApplyToCountryInfo.ValueChanged += WrapperFilterChanged;
			if (enableEffectiveDataGrouping)
			{
				wrapper.EffectiveDataGroupingInfo.ValueChanged += WrapperFilterChanged;
			}
		}

		void WrapperFilterChanged(object sender, System.EventArgs e) => RefreshCollection(this);

		void RefreshCollection(IActiveBusinessObjectCollection collection) => collection.Refresh();
	}
}
