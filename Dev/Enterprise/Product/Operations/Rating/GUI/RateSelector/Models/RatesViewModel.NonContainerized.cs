using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Services;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class NonContainerizedRatesViewModel : RatesViewModel
	{
		#region Init

		public NonContainerizedRatesViewModel()
		{
			// Used by XAML designer
		}

		public NonContainerizedRatesViewModel(RateSelectorFilterStripBusinessObject filters, IEnumerable<IRateViewModelsProvider> rateProviders, IRatingContext ratingContext, MemoryLogger logger)
			: base(filters, rateProviders, ratingContext, logger)
		{
		}

		#endregion

		#region Properties

		public override string TransportMode => "AIR";
		public override string ContainerMode => "LSE";
		public override IEnumerable<RateViewModel> Rates => rates;

		public RateViewModel SelectedRate
		{
			get => selectedRate;
			set
			{
				if (selectedRate != null)
				{
					selectedRate.PropertyChanged -= SelectedRateOnPropertyChanged;
				}

				selectedRate = value;

				if (selectedRate != null)
				{
					selectedRate.PropertyChanged += SelectedRateOnPropertyChanged;
				}

				SelectedRateOnPropertyChanged(selectedRate, null);
				OnPropertyChanged(nameof(SelectedRate));
			}
		}

		#endregion

		public override IEnumerable<RateViewModel> GetSelectedRates()
		{
			return SelectedRate != null
				? new[] { SelectedRate }
				: Array.Empty<RateViewModel>();
		}

		protected override void RefreshCharges()
		{
			foreach (var rate in Rates.OfType<CargoguideRateViewModel>())
			{
				rate.PopulateLines();
				Calculate(rate);
			}
		}

		protected override void OnSearchBegin(RateSelectorFilterStripBusinessObject filter)
		{
			base.OnSearchBegin(filter);

			rates.ForEach(r => r.Dispose());
			rates.Clear();

			ReSort();
		}

		protected override void OnRatesFound(IEnumerable<RateViewModel> newRates)
		{
			base.OnRatesFound(newRates);

			newRates.ForEach(r =>
			{
				Calculate(r);
				rates.Add(r);
			});

			ReSort();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (selectedRate != null)
				{
					selectedRate.PropertyChanged -= SelectedRateOnPropertyChanged;
				}

				Rates.ForEach(r => r.Dispose());
			}

			base.Dispose(disposing);
		}

		void SelectedRateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (sender is RateViewModel rate)
			{
				CanApply = rate.IsValid;
			}
			else
			{
				CanApply = false;
			}
		}

		void Calculate(RateViewModel rate)
		{
			var criteria = Filters.CreateCriteria();
			rate.Calculate(criteria, Filters.OriginalCriteria.Creditors?.ChargeCodeGroups);
		}

		RateViewModel selectedRate;
		protected readonly List<RateViewModel> rates = new List<RateViewModel>();
	}
}
