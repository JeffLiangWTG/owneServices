using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class FilteredRateViewCollection : RateViewCollection
	{
		public FilteredRateViewCollection(TariffView parentTariff, bool enableEffectiveDataGrouping)
			: base(parentTariff, enableEffectiveDataGrouping, true)
		{
			wrapper = parentTariff.Wrapper;
			SetWrapperEvent();
		}

		readonly TariffViewWrapper wrapper;

		protected override bool MatchesFilterCore(RateView element, bool fetchOnlyFromLocalCache)
		{
			var filteringByRatesApplyToCountry = !(wrapper?.RatesApplyToCountry ?? ZString.Empty).IsEmpty;
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && (!filteringByRatesApplyToCountry || AnyFilteredRateApplicability(element));
		}

		protected override bool AdditionalMatchesFilter(TariffView master, RateView element)
		{
			return element.RateApplicabilities.Count == 0 ? base.AdditionalMatchesFilter(master, element) : IsRateOrApplicabilitesWithinEffectiveDate(element);
		}

		protected override void SetDefaultsForNewElementCore(RateView newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ZZ2_ZZ1_ParentTariffOrNationalCode = parentTariff.PK;
			newElement.ZZ2_StartDate = parentTariff.ZZ1_StartDate;
			newElement.ZZ2_EndDate = parentTariff.ZZ1_EndDate;
		}

		protected override bool AllowNew => true;

		#region Implementation

		void SetWrapperEvent()
		{
			if (wrapper != null)
			{
				wrapper.RatesApplyToCountryInfo.ValueChanged += RatesApplyToCountryInfo_ValueChanged;
			}
		}

		ZBool AnyFilteredRateApplicability(RateView element)
		{
			var filteredRateApplicabilitiesCollection = element.FilteredRateApplicabilities;
			RefreshCollection(filteredRateApplicabilitiesCollection);
			return filteredRateApplicabilitiesCollection.Count > 0;
		}

		void RatesApplyToCountryInfo_ValueChanged(object sender, System.EventArgs e)
		{
			RefreshCollection(this);
		}

		void RefreshCollection(IActiveBusinessObjectCollection collection) => collection.Refresh();

		ZBool IsRateOrApplicabilitesWithinEffectiveDate(RateView element)
		{
			var startDate = element.RateApplicabilities.Max(app => app.ZZT_StartDate);
			if (startDate < element.ZZ2_StartDate)
			{
				startDate = element.ZZ2_StartDate;
			}
			var endDate = element.RateApplicabilities.Min(app => app.ZZT_EndDate);
			if (endDate > element.ZZ2_EndDate)
			{
				endDate = element.ZZ2_EndDate;
			}
			return wrapper.IsWithInEffectiveDate(startDate, endDate);
		}

		#endregion
	}
}
