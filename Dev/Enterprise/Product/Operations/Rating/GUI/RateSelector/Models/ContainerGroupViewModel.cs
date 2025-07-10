using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using static System.FormattableString;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	/// <summary>
	///		Represents a group of uld rates for a particular container-commodity pair.
	///
	///		Basically, ULD view models have the following structure:
	///
	///		ContainerizedRatesViewModel (provides details about all groups of rates, i.e. tabs)
	///			|- ContainerGroupViewModel[] (provides details about a group with its rates)
	///				|- RateViewModel[] (provides details about a rate)
	/// </summary>
	public class ContainerGroupViewModel : SortableRatesViewModel
	{
		/// <summary>
		///		Used by ContainerizedRatesViewModelSample.
		/// </summary>
		public ContainerGroupViewModel(string containerCode, string commodityCode, int containerCount, IEnumerable<RateViewModel> rates)
		{
			ContainerCode = containerCode;
			CommodityCode = commodityCode;
			ContainerCount = containerCount;
			Rates = new ObservableCollection<RateViewModel>(rates);
		}

		public ContainerGroupViewModel(RefContainer container, RefCommodityCode commodity, int containerCount)
		{
			ContainerCode = container?.RC_Code;
			CommodityCode = commodity?.RH_Code;
			Container = container;
			Commodity = commodity;
			ContainerCount = containerCount;
			Rates = new ObservableCollection<RateViewModel>();
			Rates.CollectionChanged += RatesOnCollectionChanged;
		}

		public RefCommodityCode Commodity { get; }
		public RefContainer Container { get; }

		#region Binding

		public string ContainerCode { get; }
		public string CommodityCode { get; }
		public int ContainerCount { get; }
		public decimal GoodsWeight { get; set; }
		public decimal GoodsVolume { get; set; }
		public string GoodsWeightForBonding => Invariant($"{GoodsWeight:0.##} KG");
		public string GoodsVolumeForBonding => Invariant($"{GoodsVolume:0.##} M3");
		public string Header => Invariant($"{ContainerCount} x {ContainerCode} ({CommodityCode})");

		public ObservableCollection<RateViewModel> Rates { get; }

		public RateViewModel SelectedRate
		{
			get => selectedRate;
			set
			{
				if (selectedRate == value)
				{
					return;
				}

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
				OnPropertyChanged(nameof(RateIsNotSelectedText));
			}
		}

		public bool CanApply
		{
			get => canApply;
			protected set
			{
				canApply = value;
				OnPropertyChanged(nameof(CanApply));
			}
		}

		public bool IsLoadingRates
		{
			get => isLoadingRates;
			set
			{
				isLoadingRates = value;
				OnPropertyChanged(nameof(IsLoadingRates));
				OnPropertyChanged(nameof(RateIsNotSelectedText));
			}
		}

		public string RateIsNotSelectedText
		{
			get
			{
				if (IsLoadingRates)
				{
					return Res.GetString("D7D8CCCD-F70D-4FC6-8F47-3AA7AE48921", "Loading rates...");
				}

				if (SelectedRate != null)
				{
					return string.Empty;
				}

				if (Rates.Any())
				{
					return Res.GetString("90FF4846-774F-4109-A7A1-C8786AEFFC35", "Please select a rate");
				}

				return Res.GetString("A921EFFF-A68C-405A-AC39-9F45FAF6BBC1", "No rates found");
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (selectedRate != null)
				{
					selectedRate.PropertyChanged -= SelectedRateOnPropertyChanged;
				}

				Rates?.ForEach(r => r.Dispose());
			}

			base.Dispose(disposing);
		}

		void RatesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(RateIsNotSelectedText));

			ReSort();
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

		protected override object GetRatesSource() => Rates;

		protected override object GetSortProperty(object o, SortOptionViewModel selectedSort)
		{
			var vm = (RateViewModel)o;
			switch (selectedSort.PropertyName)
			{
				case (nameof(vm.TotalPriceAmount)):
					// From the base constructor on SortableRatesViewModel
					return vm.TotalPriceAmount;
				default:
					// No other sort options added, therefore unexpected
					return null; // unrecogised, don't sort
			}
		}

		RateViewModel selectedRate;
		bool isLoadingRates;
		bool canApply;
	}
}
