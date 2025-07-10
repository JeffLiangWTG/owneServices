using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelection;
using Enterprise.ZArchitecture.Core;
using WiseRates.Constants;

namespace Enterprise.Rating.GUI
{
	public class ChooserContainerCommodityViewModel : ViewModelWithNotificationBase
	{
		public ChooserContainerCommodityViewModel(ChooserContainerCommodity model, IRateChooserImages providerIcons)
		{
			Model = model;
			this.providerIcons = providerIcons;
			Rates = new ObservableCollection<ChooserRateRow>();

			if (model?.ContainerRef != null)
			{
				TabName = FormattableString.Invariant($"{model.ContainerRef.RC_Code} ({model.ContainerQuality}) ({model.CommodityCode})");
			}
			else
			{
				// for LCL
				TabName = "tabLCLRates"; // not to be translated
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		readonly static string[] ratesOrderPreferences = { string.Empty, WRConstants.RateProviders.CargoSphere, WRConstants.RateProviders.CargoGuide };

		readonly IRateChooserImages providerIcons;

		public ChooserContainerCommodity Model { get; }
		public string TabName { get; }

		/// <summary>
		/// use only for FCL tab pages
		/// </summary>
		public string TabText
			=> FormattableString.Invariant($"{Model.ContainerCount} x {Model.ContainerRef.RC_Code} ({Model.ContainerQuality}) ({Model.CommodityCode}) - {Rates.Count} {Res.GetString("9d13f3f7-0eac-414d-ae87-ce3228b1cd33", "Rate(s)")}");

		public string NoRateFoundForContainer
			=> Res.GetString("62d75db9-0868-40cf-841e-d60672ba5c0b", "No rate found for this container.");

		public string NoRateFoundForLCL
			=> Res.GetString("47a0d5b8-f7f6-4420-bcd4-6639fb1042b7", "No rate found.");

		public string NoRatesFound { get; set; }

		public bool NoRateVisibility
		{
			get => noRateVisibility;
			set
			{
				if (noRateVisibility != value)
				{
					noRateVisibility = value;
					OnPropertyChanged(nameof(NoRateVisibility));
				}
			}
		}
		bool noRateVisibility;

		public bool RateVisibility
		{
			get => rateVisibility;
			set
			{
				if (rateVisibility != value)
				{
					rateVisibility = value;
					OnPropertyChanged(nameof(RateVisibility));
				}
			}
		}
		bool rateVisibility = true;

		public ObservableCollection<ChooserRateRow> Rates { get; private set; }

		public ChooserRateRow SelectedRow
		{
			get
			{
				return Rates.SingleOrDefault(r => r.IsSelected);
			}
		}

		public event EventHandler SelectRelatedRates;

		public bool ContainsRateRelatedTo(ChooserRateEntry rate)
		{
			return rate != null && Rates.Any(r => r.Rate.IsRelatedTo(rate));
		}

		public bool IsSelectedRateRelatedTo(ChooserRateEntry rate)
		{
			return rate != null && SelectedRow != null && SelectedRow.Rate.IsRelatedTo(rate);
		}

		internal void RefreshRates(string criteriaJobID, IEnumerable<ChooserContainerCommodityViewModel> otherContainerCommodityViewModels)
		{
			RateVisibility = Model.Rates.Any();
			NoRateVisibility = !Model.Rates.Any();

			if (NoRateVisibility)
			{
				NoRatesFound = Model.ContainerRef == null ? NoRateFoundForLCL : NoRateFoundForContainer;
				OnPropertyChanged(nameof(NoRatesFound));
			}

			DoRefreshRates(criteriaJobID, otherContainerCommodityViewModels);
		}

		internal void DoRefreshRates(string criteriaJobID, IEnumerable<ChooserContainerCommodityViewModel> chooserContainerCommodities)
		{
			// Rates.Clear will reset the SelectedRow property which will reset Model.SelectedRate.
			// So, we need to save it.
			var selectedRate = Model.SelectedRate;

			Rates.ForEach(r => r.SelectionChanged -= SelectedRowChanged);
			Rates.Clear();

			foreach (var rate in Model.Rates)
			{
				var row = new ChooserRateRow(Model, rate, providerIcons, chooserContainerCommodities);
				row.SelectionChanged += SelectedRowChanged;
				Rates.Add(row);
			}

			var firstRate = Rates.FirstOrDefault(r => r.Rate == selectedRate);
			if (firstRate != null)
			{
				firstRate.IsSelected = true;
			}

			SortRates(criteriaJobID);
		}

		void SortRates(string criteriaJobID)
		{
			if (Rates.Count > 1)
			{
				var sortedRates = Rates
					.OrderByDescending(x =>
						!string.IsNullOrWhiteSpace(criteriaJobID) && x.Rate?.WiseRateEntry?.ReservedForJobIDs != null &&
						x.Rate.WiseRateEntry.ReservedForJobIDs.Contains(criteriaJobID))
					.ThenBy(x => Array.IndexOf(ratesOrderPreferences, x.Rate?.RateProvider))
					.ThenBy(x =>
					{
						if (x.CW1FreightCharges == null)
						{
							return decimal.MaxValue;
						}

						return x.CW1FreightCharges.TotalPrice;
					})
					.ThenBy(x =>
					{
						if (x.CW1BillOfLadingCharges == null)
						{
							return decimal.MaxValue;
						}

						return x.CW1BillOfLadingCharges.TotalPrice;
					});

				Rates = new ObservableCollection<ChooserRateRow>(sortedRates.ToList());
			}

			OnPropertyChanged(nameof(Rates));
		}

		public void RaiseSelectRelatedRates()
		{
			SelectRelatedRates?.Invoke(this, EventArgs.Empty);
		}

		void SelectedRowChanged(object sender, EventArgs args)
		{
			if (sender != null && sender is ChooserRateRow selectedRow && selectedRow.IsSelected)
			{
				foreach (var row in Rates)
				{
					if (row != selectedRow)
					{
						row.IsSelected = false;
					}
				}
			}

			Model.SelectedRate = Rates.SingleOrDefault(x => x.IsSelected)?.Rate;

			OnPropertyChanged(nameof(SelectedRow));
		}
	}
}
