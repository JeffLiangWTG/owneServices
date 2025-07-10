using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	/// <summary>
	///		Represents groups of uld rates by container-commodity pair so that they could be displayed in separate tabs.
	///
	///		Basically, ULD view models have the following structure:
	///
	///		ContainerizedRatesViewModel (provides details about all groups of rates, i.e. tabs)
	///			|- ContainerGroupViewModel[] (provides details about a group with its rates)
	///				|- RateViewModel[] (provides details about a rate)
	/// </summary>
	public class ContainerizedRatesViewModel : RatesViewModel
	{
		#region Init

		public ContainerizedRatesViewModel()
		{
			ContainerGroups.CollectionChanged += ContainerGroupsOnCollectionChanged;
			PropertyChanged += ContainerizedRatesViewModel_PropertyChanged;

			var totalPriceSort = SupportedSortOptions[0];
			totalPriceSort.PropertyChanged += TotalPriceSort_PropertyChanged;
		}

		public ContainerizedRatesViewModel(RateSelectorFilterStripBusinessObject filters, IEnumerable<IRateViewModelsProvider> rateProviders, IRatingContext ratingContext, MemoryLogger logger)
			: base(filters, rateProviders, ratingContext, logger)
		{
			ContainerGroups.CollectionChanged += ContainerGroupsOnCollectionChanged;
			InitTabs(filters);
			PropertyChanged += ContainerizedRatesViewModel_PropertyChanged;

			var totalPriceSort = SupportedSortOptions[0];
			totalPriceSort.PropertyChanged += TotalPriceSort_PropertyChanged;
		}

		void InitTabs(RateSelectorFilterStripBusinessObject filters)
		{
			var containerGroups = filters.OriginalCriteria.RateableMeasures
				.GetContainerGroups()
				.GroupBy(g => (g.ContainerTypePK, g.CommodityCode))
				.ToList();

			var containerTypePKs = containerGroups.Select(g => g.Key.ContainerTypePK).Distinct();
			var refContainers = filters.Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.PK, containerTypePKs));

			var commodityCodes = containerGroups.Select(g => g.Key.CommodityCode).Distinct();
			var refCommodities = filters.Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, commodityCodes));

			foreach (var group in containerGroups)
			{
				var container = refContainers.First(c => c.PK == group.Key.ContainerTypePK);
				var commodity = refCommodities.FirstOrDefault(c => c.RH_Code == group.Key.CommodityCode);
				var containerCount = group.Sum(g => g.ContainerCount);
				var weight = group.Sum(g => g.Containers.Sum(i => i.ContainerWeightInKG));
				var volume = group.Sum(g => g.Containers.Sum(i => i.ContainerVolumeInM3));

				var tab = new ContainerGroupViewModel(container, commodity, containerCount)
				{
					GoodsWeight = weight,
					GoodsVolume = volume
				};

				ContainerGroups.Add(tab);
			}
		}

		#endregion

		public override string TransportMode => "AIR";
		public override string ContainerMode => "ULD";
		public ObservableCollection<ContainerGroupViewModel> ContainerGroups { get; } = new ObservableCollection<ContainerGroupViewModel>();
		public override IEnumerable<RateViewModel> Rates => ContainerGroups.SelectMany(g => g.Rates);

		// This ViewModel has no rates of its own, it has groups of rates.
		// Requests to ReSort / changes to 
		readonly IEnumerable<object> emptyRates = Array.Empty<object>();
		protected override object GetRatesSource() => emptyRates;

		public event EventHandler SearchCompleted;

		public override IEnumerable<RateViewModel> GetSelectedRates()
		{
			var rates = ContainerGroups
				.Where(g => g.SelectedRate != null)
				.Select(g => g.SelectedRate)
				.ToList();

			return rates;
		}

		protected override void RefreshCharges()
		{
			// Recalculate charges
			foreach (var group in ContainerGroups)
			{
				foreach (var rate in group.Rates.OfType<CargoguideRateViewModel>())
				{
					rate.PopulateLines();
					Calculate(group, rate);
				}
			}
		}

		protected override void OnSearchBegin(RateSelectorFilterStripBusinessObject filter)
		{
			base.OnSearchBegin(filter);

			foreach (var group in ContainerGroups)
			{
				group.Rates.Clear();
				group.IsLoadingRates = true;
			}
		}

		protected override void OnRatesFound(IEnumerable<RateViewModel> rates)
		{
			base.OnRatesFound(rates);

			if (rates.IsNullOrEmpty())
			{
				return;
			}

			var cw1Rates = rates.OfType<CW1RateViewModel>().ToList();
			var isCW1Rate = cw1Rates.Any();

			foreach (var group in ContainerGroups)
			{
				var ratesToBeCalculated = !isCW1Rate
					? rates
					: cw1Rates.Where(r =>
							r.RelatedTabContainerTypePk == (group.Container?.PK ?? ZGuid.Empty) &&
							r.RelatedTabCommodityCode == (group.Commodity?.RH_Code ?? ZString.Empty));

				foreach (var rate in ratesToBeCalculated)
				{
					if (isCW1Rate || IsInGroup(group, rate))
					{
						var copy = (RateViewModel)rate.Clone();
						Calculate(group, copy);
						group.Rates.Add(copy);
					}
				}
			}
		}

		protected override void OnSearchCompleted()
		{
			base.OnSearchCompleted();

			ContainerGroups.ForEach(c => c.IsLoadingRates = false);

			SearchCompleted?.Invoke(this, EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				PropertyChanged -= ContainerizedRatesViewModel_PropertyChanged;

				ContainerGroups.ForEach(g =>
				{
					g.PropertyChanged -= ContainerGroupPropertyChanged;
					g.Dispose();
				});

				var totalPriceSort = SupportedSortOptions[0];
				totalPriceSort.PropertyChanged -= TotalPriceSort_PropertyChanged;
			}

			base.Dispose(disposing);
		}

		static bool IsInGroup(ContainerGroupViewModel ratesGroup, RateViewModel rate)
		{
			var matchedByContainer =
				string.IsNullOrEmpty(rate.ContainerType) ||
				string.Equals(rate.ContainerType, ratesGroup.ContainerCode, StringComparison.OrdinalIgnoreCase);

			var matchedByCommodity =
				ratesGroup.Commodity == null ||
				string.IsNullOrEmpty(rate.Commodities) && !rate.CommodityGroups.Any() ||
				string.Equals(rate.Commodities, ratesGroup.Commodity.RH_Code, StringComparison.OrdinalIgnoreCase) ||
				rate.CommodityGroups.Any(group => ratesGroup.Commodity.HasGroup(group)) ||

				// General and Non Classified commodities can be used in any tab
				rate.CommodityGroups.Contains(RefCommodityCode.UniversalGroups.General) ||
				rate.CommodityGroups.Contains(RefCommodityCode.UniversalGroups.NotClassified);

			return matchedByContainer && matchedByCommodity;
		}

		void Calculate(ContainerGroupViewModel ratesGroup, RateViewModel rate)
		{
			var criteria = Filters.CreateCriteria();
			using (var measureChanger = criteria.JobMeasures.BeginTemporaryChanges())
			{
				measureChanger.SetTemporaryContainerTypeAndCommodityFilter(
					filterContainerTypePk: ratesGroup.Container.PK,
					filterContainerQuality: null, // Only applicable for CoargoSphere rates
					filterCommodityCode: ratesGroup.Commodity?.RH_Code ?? ZString.Empty);
				rate.Calculate(criteria, Filters.OriginalCriteria.Creditors?.ChargeCodeGroups);
			}
		}

		void ContainerGroupsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ContainerGroupViewModel oldGroup in e.OldItems)
				{
					oldGroup.PropertyChanged -= ContainerGroupPropertyChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (ContainerGroupViewModel newGroup in e.NewItems)
				{
					newGroup.PropertyChanged += ContainerGroupPropertyChanged;
				}
			}
		}

		void ContainerGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(ContainerGroupViewModel.CanApply):
					{
						var groupsWithSelectedRates = ContainerGroups
							.Where(g => g.SelectedRate != null)
							.ToList();

						CanApply = groupsWithSelectedRates.Any() && groupsWithSelectedRates.All(g => g.CanApply);
						break;
					}
				case nameof(ContainerGroupViewModel.SelectedRate):
					{
						var currentGroup = (ContainerGroupViewModel)sender;

						var selectedRate = currentGroup.SelectedRate;
						if (selectedRate == null)
						{
							return;
						}

						if (string.IsNullOrEmpty(selectedRate.CarrierCode) ||
							string.IsNullOrEmpty(selectedRate.CarrierServiceLevel))
						{
							// We can't select other rates in other tabs if we don't know the mapped carrier or service level.
							// Otherwise, the user may select rates from multiple carriers if we select rates with empty carrier. Same with service level.
							return;
						}

						foreach (var otherGroup in ContainerGroups.Where(g => g != currentGroup))
						{
							var matchingOtherRates = otherGroup.Rates
								.Where(r => string.Equals(r.CarrierCode, selectedRate.CarrierCode, StringComparison.OrdinalIgnoreCase))
								.Where(r => string.Equals(r.CarrierServiceLevel, selectedRate.CarrierServiceLevel, StringComparison.CurrentCultureIgnoreCase))
								.OrderBy(r => r.TotalPrice)
								.ToList();

							if (!matchingOtherRates.Contains(otherGroup.SelectedRate))
							{
								otherGroup.SelectedRate = matchingOtherRates.FirstOrDefault();
							}
						}

						break;
					}
			}
		}

		void TotalPriceSort_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// Propagate any change in sorting to the groups so they can re-sort
			// their results
			foreach (var cg in ContainerGroups)
			{
				var parent = SupportedSortOptions[0];
				var child = cg.SupportedSortOptions[0];
				child.Direction = parent.Direction;
			}
		}

		void ContainerizedRatesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SortedRates))
			{
				// Propagate any ReSort request to the groups so they can re-sort
				// their results
				foreach (var cg in ContainerGroups)
				{
					cg.ReSort();
				}
			}
		}
	}
}
