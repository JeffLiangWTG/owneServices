using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public enum ViewMode
	{
		CardView,
		Warnings
	}

	public abstract class RatesViewModel : SortableRatesViewModel
	{
		#region Init

		protected RatesViewModel()
		{
			// Used by XAML Designer
		}

		protected RatesViewModel(RateSelectorFilterStripBusinessObject filters, IEnumerable<IRateViewModelsProvider> rateProviders, IRatingContext ratingContext, MemoryLogger logger) : base(Argument.NotNull(logger, nameof(logger)))
		{
			this.RatingContext = Argument.NotNull(ratingContext, nameof(ratingContext));
			this.Filters = Argument.NotNull(filters, nameof(filters));
			this.rateProviders = Argument.NotNull(rateProviders, nameof(rateProviders));
			this.ViewMode = ViewMode.CardView;
			this.FoundRates = new List<RateViewModel>();
		}

		#endregion

		#region Properties

		#region Binding

		public abstract string TransportMode { get; }
		public abstract string ContainerMode { get; }

		public ViewMode ViewMode
		{
			get => viewMode;
			set
			{
				viewMode = value;
				OnPropertyChanged(nameof(ViewMode));
			}
		}

		public bool CanApply
		{
			get => canApply;
			protected set
			{
				if (canApply != value)
				{
					canApply = value;
					OnPropertyChanged(nameof(CanApply));
				}
			}
		}

		#endregion

		public abstract IEnumerable<RateViewModel> Rates { get; }
		protected RateSelectorFilterStripBusinessObject Filters { get; }
		protected IRatingContext RatingContext { get; }
		protected List<RateViewModel> FoundRates { get; set; }

		#endregion

		/// <summary>
		/// Calls the rates providers to get the rates and updates the UI as
		/// each rate provider completes. A failing rate provider will not affect the
		/// results of the others.
		///
		/// This function needs to be called from the UI thread and will exit in the UI thread.
		/// </summary>
		public async Task SearchAsync(CancellationToken cts)
		{
			OnSearchBegin(Filters);
			Logger.Clear();

			StatusText = Res.GetString("f6c88016-f92e-478e-bab8-4e996181aa8b", "Loading rates...");

			var ratesCount = 0;
			var (hadExceptions, rateTasks) = StartGetRatesAsync(cts);

			long wiseRatesElapsedMilliseconds = 0;
			long cw1ElapsedMilliseconds = 0;

			while (!cts.IsCancellationRequested && rateTasks.Count > 0)
			{
				var finishedTask = await Task.WhenAny(rateTasks);
				rateTasks.Remove(finishedTask);

				if (await ReportExceptionsAsync(finishedTask))
				{
					hadExceptions = true;
					continue;
				}

				if (cts.IsCancellationRequested)
				{
					break;
				}

				try
				{
					var result = finishedTask.Result;
					if (result != null)
					{
						if (result.ProviderType == RateProviderType.RatesService)
						{
							wiseRatesElapsedMilliseconds = result.ElapsedMilliseconds;
						}
						else if (result.ProviderType == RateProviderType.CW1)
						{
							cw1ElapsedMilliseconds = result.ElapsedMilliseconds;
						}

						OnRatesFound(result.Rates);
						ratesCount += result.Rates.Count();
					}
				}
				catch (Exception ex)
				{
					hadExceptions = true;
					ReportUnhandledException(ex);
				}
			}

			if (hadExceptions)
			{
				StatusText = Res.GetString("0517d6e3-64d1-4316-8d14-0681816c6220", "Found {0} rates but there were some errors", ratesCount);
			}
			else
			{
				StatusText = Res.GetString("d7c62952-f568-4816-b284-d97e7900612e", "Found {0} rates", ratesCount);
			}

			ReportSearchUsage(wiseRatesElapsedMilliseconds, cw1ElapsedMilliseconds);
			RefreshViewMode();
			OnSearchCompleted();
		}

		public bool MapSelectedRates()
		{
			var rates = GetSelectedRates();

			// Only rates service rates have to be mapped. CW1 rates are already mapped.
			var ratesToMap = rates
				.OfType<CargoguideRateViewModel>()
				.ToList();

			return MapCarrier(ratesToMap) && MapCarrierServiceLevel(ratesToMap) && MapCommodityGroup(ratesToMap) && MapChargeCodes(ratesToMap);
		}

		public abstract IEnumerable<RateViewModel> GetSelectedRates();

		public AutoRateInfoCollection GetSelectedCharges()
		{
			var selectedRates = GetSelectedRates();
			if (!selectedRates.Any())
			{
				throw new InvalidOperationException("Should not be called when the rates is not selected");
			}

			var carrierChargesCollection = new AutoRateInfoCollection(Filters.Factory);
			carrierChargesCollection.AddRange(GetCarrierCharges(selectedRates));

			return new RateSelectorAllChargesProvider(
				carrierChargesCollection,
				Filters.OriginalCriteria,
				Logger,
				ApplyZeroCharges,
				selectedRates.Select(x => x.GetCarrierToApplyToJob()).WhereNotNull().FirstOrDefault()
			).GetAllCharges();
		}

		IEnumerable<AutoRateInfo> GetCarrierCharges(IEnumerable<RateViewModel> selectedRates)
		{
			var charges = new List<AutoRateInfo>();

			foreach (var rate in selectedRates)
			{
				foreach (var rateCharge in rate.GetAutoRateInfos())
				{
					if (!IsChargeApplicable(rateCharge))
					{
						continue;
					}

					// Filter out Flat charges which already have been added
					//
					// It is for a case if there is a flat charge without container, i.e. it applies once regardless of how many containers are on the job.
					// But, since in rate selector we allow a user to select a completely calculated rate per each container group (i.e. tabs in rate selector),
					// such flat charge gets duplicated to cards in each group. For example:
					//
					//	The shipment has:
					//		2 x AKE container
					//		1 x RKN container

					//	Charges for Emirates:
					//		FRT, $1000 per AKE container
					//		FRT, $1500 per RKN container
					//		FUL, $500
					//	We will have the following cards in rate selector:
					//
					//	2xAKE			1xRKN
					//	Emirates		Emirates
					//	FRT $2000		FRT $1500
					//	FUL $500		FUL
					//
					//	But, when these 2 cards are selected, the following charges should be applied:
					//	FRT $2000
					//	FRT $1500
					//	FUL $500
					//
					//	So, we are making sure that only one FLT charge comes through.
					if (
						rateCharge.Line != null && 
						rateCharge.Line.TL_RateCalculator == FlatCalculator.Code
						&& charges.Any(c => c.Line == rateCharge.Line))
					{
						continue;
					}

					charges.Add(rateCharge);
				}
			}

			return charges;
		}

		public bool ApplyZeroCharges { get; set; } = true;

		protected abstract void RefreshCharges();

		protected virtual void OnSearchBegin(RateSelectorFilterStripBusinessObject filter)
		{
			FoundRates.Clear();
			CanApply = false;
		}

		protected virtual void OnSearchCompleted()
		{
			foreach (var rate in FoundRates)
			{
				rate.CleanUpAfterSearch();
			}
			OnPropertyChanged(nameof(Rates));
		}

		protected virtual void OnRatesFound(IEnumerable<RateViewModel> rates)
		{
			foreach (var rate in rates)
			{
				FoundRates.Add(rate);
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

		/// <summary>
		/// Unpacks and reports on the exceptions the finished task may have.
		/// </summary>
		/// <returns>True if there were exceptions to report, false otherwise</returns>
		async Task<bool> ReportExceptionsAsync(Task<RateViewModelsProviderResult> finishedTask)
		{
			try
			{
				await finishedTask;
				return false;
			}
			catch (AutoRaterException ex1)
			{
				// An AutoRaterException is typically when CW1 rates provider could not provide rates.
				// other rate providers can still continue to provide rates
				Logger.Error(Res.GetString("847f7134-d0c8-40d8-93d3-0ef57983ff4a", "Loading rates failed due to: {0}", ex1.Message));
			}
			catch (Exception ex2)
			{
				// Any exception being caught is something unexpected. Log an issue and display warning.
				// other rate providers can still continue to provide rates
				ReportUnhandledException(ex2);
			}

			return true;
		}

		void ReportUnhandledException(Exception ex)
		{
			ErrorReporter.ReportOnce($"RateSelector: An unhandled error occurred when loading rates", ex);
			Logger.Error(Res.GetString("a843a647-d7f7-40b5-b181-aa591678d55d", "An unexpected error occurred while loading rates"));

			if (!Env.Instance.IsProductionSystem)
			{
				Logger.Debug(ex.ToString());
			}
		}

		(bool, List<Task<RateViewModelsProviderResult>>) StartGetRatesAsync(CancellationToken cts)
		{
			var pendingRequests = new List<Task<RateViewModelsProviderResult>>();
			var hadExceptions = false;
			var applicableRateProviders = rateProviders.Where(x => x.IsApplicable(Filters)).ToList();

			foreach (var rateProvider in applicableRateProviders)
			{
				if (cts.IsCancellationRequested)
				{
					break;
				}

				try
				{
					// Some rate providers may execute synchronously so we need to catch those
					// exceptions up front.
					var task = rateProvider.GetRatesAsync(Filters, cts);
					pendingRequests.Add(task);
				}
				catch (AutoRaterException error)
				{
					// An AutoRaterException is typically when CW1 rates provider could not provide rates.
					// other rate providers can still continue to provide rates
					Logger.Error(Res.GetString("dd571617-c431-4468-a295-1c65d39cb17c", "Loading rates from {0} failed due to: {1}", rateProvider.ProviderName(), error.Message));
					hadExceptions = true;
				}
				catch (Exception e)
				{
					// An Exception being caught is something unexpected. Log an issue and display warning.
					// other rate providers can still continue to provide rates
					var errorMessage = Res.GetString("9811da73-7b17-4d03-8926-7fdb66f4a889", "An unexpected error occurred while loading rates from {0}", rateProvider.ProviderName());
					ErrorReporter.ReportOnce(errorMessage, e);
					Logger.Error(errorMessage);
					hadExceptions = true;
				}
			}

			return (hadExceptions, pendingRequests);
		}

		bool IsChargeApplicable(AutoRateInfo charge)
		{
			// Exclude zero charges if needed
			if (charge.Amount == 0 && !charge.IsInclusiveCalculator && !ApplyZeroCharges)
			{
				return false;
			}

			// Exclude non-consol level charges if needed
			if (charge.ChargeCode.AC_IsGroupageCharge && _Rating.ExcludeConsolLevelChargesOnCosting)
			{
				return false;
			}

			return true;
		}

		#region Mapping

		bool MapCarrier(IEnumerable<CargoguideRateViewModel> ratesToMap)
		{
			var carriersToMap = ratesToMap
				.Where(m => m.CarrierErrorLevel == ErrorLevel.Warning)
				.Where(m => !string.IsNullOrEmpty(m.RawCarrier?.IATACode))
				.Select(m => m.RawCarrier.IATACode)
				.Distinct()
				.ToList();

			if (!carriersToMap.Any())
			{
				return true;
			}

			var mappedCarriers = carriersToMap
				.Select(c => RatingContext.DialogService.MapCarrier(c))
				.ToList();

			// Refresh mapping on rates just in case if the user changed mapping on any carrier
			var cache = new Dictionary<string, OrgHeader>();
			Rates.OfType<CargoguideRateViewModel>().ForEach(r => r.PopulateCarrier(cache));

			return mappedCarriers.All(c => c != null);
		}

		bool MapCarrierServiceLevel(IEnumerable<CargoguideRateViewModel> ratesToMap)
		{
			var levelsToMap = ratesToMap
				.Where(m => m.CarrierServiceLevelErrorLevel == ErrorLevel.Warning)
				.Where(m => m.CarrierOrg != null)
				.Where(m => m.RawCarrierServiceLevel != null)
				.Select(m => (m.CarrierOrg, m.RawCarrierServiceLevel))
				.Distinct()
				.ToList();

			var mappingResults = levelsToMap
				.Select(l => RatingContext.DialogService.MapServiceLevel(l.CarrierOrg, l.RawCarrierServiceLevel))
				.ToList();

			// Refresh mappings on rates with the same carrier just in case if the user changed mapping for the carrier
			var carriersToRefresh = levelsToMap
				.Select(l => l.CarrierOrg)
				.Distinct()
				.ToList();

			var ratesToRefresh = Rates
					.OfType<CargoguideRateViewModel>()
					.Where(r => carriersToRefresh.Contains(r.CarrierOrg))
					.ToList();

			ratesToRefresh.ForEach(r => r.PopulateCarrierServiceLevel());

			return mappingResults.All(mapped => mapped);
		}

		bool MapCommodityGroup(IEnumerable<CargoguideRateViewModel> ratesToMap)
		{
			var groupsToMap = ratesToMap
				.Where(m => m.CommodityGroupErrorLevel == ErrorLevel.Warning)
				.Where(m => !string.IsNullOrEmpty(m.RawRate.Commodity))
				.Select(m => m.RawRate.Commodity)
				.Distinct()
				.ToList();

			if (!groupsToMap.Any())
			{
				return true;
			}

			var results = groupsToMap
				.Select(c => RatingContext.DialogService.MapCommodity(c))
				.ToList();

			// Refresh mapping on rates just in case if the user changed mapping on any carrier
			var cache = new Dictionary<string, IEnumerable<RefCommodityCode>>();
			Rates.OfType<CargoguideRateViewModel>().ForEach(r => r.PopulateCommodity(cache));

			return results.All(success => success);
		}

		bool MapChargeCodes(IEnumerable<CargoguideRateViewModel> ratesToMap)
		{
			var chargeCodes = GetChargeCodesToMap(ratesToMap);
			if (chargeCodes.Count == 0)
			{
				return true;
			}

			var mapped = RatingContext.DialogService.MapUniversalChargeCodes(chargeCodes);

			// Refresh mapping on charges just in case if the user changed mapping on any charge code
			// TODO: Make it smart. I.e. try to identify what charges codes were affected and only refresh those charges with
			// those charge codes rather than refreshing all charges on all rates.
			RefreshCharges();

			return mapped;
		}

		UniversalChargeCodeMapBizoCollection GetChargeCodesToMap(IEnumerable<CargoguideRateViewModel> ratesToMap)
		{
			bool ApplyZeroOrIncludedCharge(RatesServiceChargeViewModel charge, IEnumerable<RatesServiceChargeViewModel> relatedCharges)
				=> ApplyZeroCharges
					|| charge.Amount > 0
					|| (charge.IsIncluded
						&& relatedCharges.Single(relatedCharge => relatedCharge.RawCharge.ChargeCode == charge.RawCharge.FreightInclusiveCarriageCharge).Amount > 0);

			var chargeCodeHashSet = new HashSet<string>();
			var result = new UniversalChargeCodeMapBizoCollection(Filters.Factory);
			foreach (var rateCharges in ratesToMap.SelectMany(rate => rate.Charges))
			{
				var charges = rateCharges.Charges.OfType<RatesServiceChargeViewModel>().ToArray();
				foreach (var charge in charges)
				{
					if (charge.IsSelected
						&& charge.AccChargeCode == null
						&& charge.ChargeCodeErrorLevel == ErrorLevel.Warning
						&& ApplyZeroOrIncludedCharge(charge, charges)
						&& !chargeCodeHashSet.Contains(charge.ChargeCode))
					{
						result.AddNew(charge.ChargeCode, charge.ChargeCodeDescription);
						chargeCodeHashSet.Add(charge.ChargeCode);
					}
				}
			}

			return result;
		}

		#endregion

		void RefreshViewMode()
		{
			if (Rates.Any())
			{
				ViewMode = ViewMode.CardView;
				return;
			}

			if (WarningsCount > 0)
			{
				ViewMode = ViewMode.Warnings;
			}
		}

		void ReportSearchUsage(long wiseRatesElapsedMilliseconds, long cw1ElapsedMilliseconds)
		{
			var rates = Rates.ToList();
			var cargoguideRates = rates.OfType<CargoguideRateViewModel>().ToArray();
			var cw1Rates = rates.OfType<CW1RateViewModel>().ToArray();

			var result = new UsageRatesSearchResult();
			result.Cargoguide.TotalRates = cargoguideRates.Length;
			result.Cargoguide.ValidRates = cargoguideRates.Count(r => r.ErrorLevel == ErrorLevel.None);
			result.Cargoguide.WarningRates = cargoguideRates.Count(r => r.ErrorLevel.HasFlag(ErrorLevel.Warning));
			result.Cargoguide.ErrorRates = cargoguideRates.Count(r => r.ErrorLevel.HasFlag(ErrorLevel.Error));
			result.Cargoguide.ElapsedTime = (int)wiseRatesElapsedMilliseconds;
			result.CW1.TotalRates = cw1Rates.Length;
			result.CW1.ValidRates = cw1Rates.Count(r => r.ErrorLevel == ErrorLevel.None);
			result.CW1.WarningRates = cw1Rates.Count(r => r.ErrorLevel.HasFlag(ErrorLevel.Warning));
			result.CW1.ErrorRates = cw1Rates.Count(r => r.ErrorLevel.HasFlag(ErrorLevel.Error));
			result.CW1.ElapsedTime = (int)cw1ElapsedMilliseconds;
			
			RatingUsageCollector.ReportRateSelectorSearch(result);
		}

		readonly IEnumerable<IRateViewModelsProvider> rateProviders;
		ViewMode viewMode;
		bool canApply;
	}
}
