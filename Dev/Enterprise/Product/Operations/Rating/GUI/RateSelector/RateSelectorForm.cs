using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.Rating.GUI.RateSelector.UIControls;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static System.FormattableString;
#if !WINZOR
using CargoWise.Interop;
#endif

namespace Enterprise.Rating.GUI.RateSelector
{
	public sealed partial class RateSelectorForm : ZChildForm
	{
		/// <summary>
		///		Constructor for tests.
		/// </summary>
		public RateSelectorForm(RatingCriteria criteria, IRatingContext ratingContext, IDialogService dialogService, IRateViewModelsProvider[] providers = null)
		{
			this.Criteria = Argument.NotNull(criteria, nameof(criteria));
			this.RatingContext = Argument.NotNull(ratingContext, nameof(ratingContext));
			this.dialogService = dialogService;
			this.Logger = new MemoryLogger();
			this.providers = providers;

			uiLoadStopwatch.Start();
			InitializeComponent();
		}

		public RateSelectorForm(RatingCriteria criteria, IRatingContext ratingContext)
			: this(criteria, ratingContext, null, null)
		{
			this.dialogService = new DialogService(this);
		}

		public bool IsRateSelectionSkipped { get; private set; }

		void WriteLogToAutoratingLog()
		{
			var rateSelectorLogs = Logger.Logs.Where(l => l.Level != LogType.Debug).ToArray();
			if (rateSelectorLogs.Any())
			{
				RatingContext.Logger.Log(LogType.Information, "========== BEGIN RATE SELECTOR LOGS ===========");   // Just a log string

				foreach (var log in rateSelectorLogs)
				{
					RatingContext.Logger.Log(log.Level, log.Message);
				}

				RatingContext.Logger.Log(LogType.Information, "========== END RATE SELECTOR LOGS ===========");     // Just a log string
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				// InitialiseMoreComponents occurs in a BeginInvoke to reduce the chances
				// of a not-enough-quota exception occurring. From googling online this exception
				// can occur when the UI message queue is full. So, moving this further initialisation to
				// after the OnLoad has been proceessed and hopefully the message queue has been cleared
				// will prevent the not-enough-quota exception.
				BeginInvoke(new MethodInvoker(InitialiseMoreComponents));
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			search?.Cancel();

			base.OnFormClosing(e);
		}

		void InitialiseMoreComponents()
		{
			var ratingContext = Business.RatingContext.CreateForManualSelect(
				RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.Value
					? Logger
					: new MemoryLogger(),
				dialogService);

			if (providers == null)
			{
				providers = new IRateViewModelsProvider[]
				{
					new CW1RateViewModelsProvider(
						ratingContext,
						Logger),
					new RateServiceRateViewModelsProvider(
						ObjectFactory.New<IWiseRatesClientFactory>(),
						dialogService,
						Logger)
				};
			}

			filters = new RateSelectorFilterStripBusinessObject(Criteria, Logger);

			if (ViewModel != null)
			{
				ErrorReporter.ReportOnce("RateSelectorForm ViewModel not null in InitialiseMoreComponents", "InitialiseMoreComponents should be run only once, otherwise that might be cause issue 01782017 and issue 01546298");
			}
			ViewModel = Criteria.IsContainerised
				? new ContainerizedRatesViewModel(filters, providers, ratingContext, Logger)
				: new NonContainerizedRatesViewModel(filters, providers, ratingContext, Logger);
			ViewModel.PropertyChanged += ViewModelOnPropertyChanged;

			if (filterControl != null)
			{
				ErrorReporter.ReportOnce("RateSelectorForm ViewModel not null in InitialiseMoreComponents", "InitialiseMoreComponents should be run only once, otherwise that might be cause issue 01782017 and issue 01546298");
			}
			// Create the filter strip control after the form is restored to the saved position and size.
			// Otherwise it causes the form to be shown in its default position/size and then drawn again.
			filterControl = new RateSelectorFilterStripControl(filters);
			filterControl.ShouldRunSearchOnStripsInitialized = ShouldRunSearchOnShowingForm;
			filterControl.Dock = DockStyle.Fill;
			filterControl.Name = "stripControl";
			filterControl.AutoSize = true;
			filterControl.TabIndex = 0;
			filterControl.PerformSearch += FilterControlOnPerformSearch;

			if (RateSelectorControl != null)
			{
				ErrorReporter.ReportOnce("RateSelectorForm RateSelectorControl not null in InitialiseMoreComponents", "InitialiseMoreComponents should be run only once, otherwise that might be cause issue 01782017 and issue 01546298");
			}

			RateSelectorControl = new RateSelectorControl(ViewModel);
			tableLayoutPanel.Controls.Add(RateSelectorControl, 0, 2);
			RateSelectorControl.Dock = DockStyle.Fill;

			txtJobMode.Text = string.Join("   ",
				Res.GetString("E00C98FA-E197-45DF-86E2-CC66E51A7608", "Transport:") + " " + ViewModel.TransportMode,
				Res.GetString("0B6E0907-D2D4-4585-BEBF-2D6B89AF8087", "Container Mode:") + " " + ViewModel.ContainerMode);

			// The not-enough-quota problem has occurred previously when adding the filterControl
			// to the tableLayoutPanel. We've mitigated this risk by calling InitialiseMoreComponents
			// from a BeginInvoke. But, in case it does happen again this `using` below will avoid
			// us getting an issue manager issue about it. The user will still see a message box about
			// not-enough-quota
			using (QuotaExhaustedExceptionSuppressor.SuppressException())
			{
				tableLayoutPanel.Controls.Add(filterControl, 0, 1);
			}

			uiLoadStopwatch.Stop();
		}

		void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RatesViewModel.CanApply))
			{
				btnApply.Enabled = ViewModel.CanApply;
			}
		}

		async void FilterControlOnPerformSearch(object sender, EventArgs e)
		{
			search = new CancellationTokenSource();
			filterControl.StartSearching();

			await ViewModel.SearchAsync(search.Token);

			try
			{
				filterControl.DoneSearching();
			}
			catch (Win32Exception ex)
			{
				// issue 01546298 [WI00610650] please remove at next occurrence
				var message = new StringBuilder();
				message.AppendLine(ex.Message);
				message.AppendLine($"FilterControl: {GetDisposeInfo(filterControl)}");
				message.AppendLine($"FilterControl.Parent: {GetDisposeInfo(filterControl.Parent)}");
				message.AppendLine($"FilterControl.Parent.Parent: {GetDisposeInfo(filterControl.Parent.Parent)}");
				message.AppendLine($"Total Rates: {ViewModel.Rates.Count()}");
#if !WINZOR
				var gdiObjectsCount = NativeMethods.GetGuiResources(Process.GetCurrentProcess().Handle, 0u);
				message.AppendLine($"GDIObjectsCount: {gdiObjectsCount}/{UIResources.Instance.MaximumGdiObjectCount}");
				var userObjectsCount = NativeMethods.GetGuiResources(Process.GetCurrentProcess().Handle, 1u);
				message.AppendLine($"USERObjectsCount: {userObjectsCount}/{UIResources.Instance.MaximumUserObjectCount}");
#endif
				ErrorReporter.ReportOnce("RatesViewModel search has encounter an ErrorCreatingHandle exception [issue-01546298]", message.ToString(), ex);
				throw;
			}
			search?.Dispose();
			search = null;

			string GetDisposeInfo(Control control)
			{
				if (control == null)
				{
					return "IsNull";
				}

				var status = control.IsDisposed
					? "IsDisposed"
					: control.Disposing
						? (NoResString)"Disposing"
						: control.IsDisposedOrHasDisposedParent()
							? "IsDisposedOrHasDisposedParent"
							: "NotDisposed";

				return $"{control} - Status:{status}";
			}
		}

		internal void BtnApplyClick(object sender, EventArgs e)
		{
			search?.Cancel();

			if (!CanPopulateZeroAmount())
			{
				return;
			}

			if (!ViewModel.MapSelectedRates())
			{
				return;
			}

			var selectedRates = ViewModel.GetSelectedRates();

			// We apply the first rate as for LSE there will be 1 rate and for ULD all rates are currently restricted to have the same attributes.
			// Later this may be reconsidered.
			var rateToApplyToJob = selectedRates.First();
			var action = PopulateJob(rateToApplyToJob);
			if (action != ApplyAction.Apply)
			{
				return;
			}

			ReportRateSelected(ViewModel.Rates.ToList(), rateToApplyToJob);

			DialogResult = DialogResult.OK;
			WriteLogToAutoratingLog();

			SelectedRates = ViewModel?.GetSelectedCharges();

			Close();
		}

		bool CanPopulateZeroAmount()
		{
			ViewModel.ApplyZeroCharges = true;
			ApplyZeroCharges = ViewModel.ApplyZeroCharges;

			var zeroCharges = ViewModel
				.GetSelectedRates()
				.SelectMany(rate => rate.GetZeroCharges().Select(charge => new ZString(charge)));
			if (zeroCharges.Any())
			{
				var dialogResult = dialogService.PromptUserApplyZeroCharges(new List<ZString>(zeroCharges));

				if (dialogResult == ZDialogResult.Cancel)
				{
					return false;
				}

				if (dialogResult == ZDialogResult.No)
				{
					ViewModel.ApplyZeroCharges = false;
					ApplyZeroCharges = ViewModel.ApplyZeroCharges;
				}
			}

			return true;
		}

		enum ApplyAction
		{
			Apply,
			SkipRateSelector,
			ReturnToRateSelector
		}

		void BtnCancelClick(object sender, EventArgs e)
		{
			RatingUsageCollector.ReportRateSelector(RatingUsageCollector.RateSelectorAction.Cancel);

			search?.Cancel();
			DialogResult = DialogResult.Cancel;
			Close();
		}

		internal void BtnSkipRateSelectionClick(object sender, EventArgs e)
		{
			RatingUsageCollector.ReportRateSelector(RatingUsageCollector.RateSelectorAction.Skip);

			search?.Cancel();
			DialogResult = DialogResult.Cancel;
			WriteLogToAutoratingLog();
			IsRateSelectionSkipped = true;
			Close();
		}

		void ReportRateSelected(IEnumerable<RateViewModel> foundRates, RateViewModel selectedRate)
		{
			var searchResult = new UsageRatesSearchResult();
			searchResult.Cargoguide.TotalRates = foundRates.OfType<CargoguideRateViewModel>().Count();
			searchResult.CW1.TotalRates = foundRates.OfType<CW1RateViewModel>().Count();

			var selectedProvider = selectedRate is CargoguideRateViewModel
				? RatingUsageCollector.RateProvider.Cargoguide
				: RatingUsageCollector.RateProvider.CW1;

			RatingUsageCollector.ReportRateSelector(RatingUsageCollector.RateSelectorAction.Select, uiLoadStopwatch.ElapsedMilliseconds,selectedProvider, searchResult);
		}

		#region Populate Data Back to Job

		ApplyAction PopulateJob(RateViewModel rate)
		{
			if
				(
					!CanPopulateCarrier(rate, out var populateCarrier)
					|| !CanPopulateOrigin(rate, out var populateOrigin)
					|| !CanPopulateDestination(rate, out var populateDestination)
					|| !CanPopulateCarrierContractNumber(rate, out var populateCarrierContractNumber)
				)
			{
				return ApplyAction.ReturnToRateSelector;
			}

			populateCarrier();
			populateOrigin();
			populateDestination();
			PopulateServiceLevel(rate);
			PopulatePaymentTerms(rate);
			populateCarrierContractNumber();
			ApplyEffectiveDateBackToJobIfNeeded();
			return PopulateCommodity(rate);
		}

		void ApplyEffectiveDateBackToJobIfNeeded()
		{
			var canUpdateDate = filters.OriginalCriteria?.AutoRating != null
								&& filters.OriginalCriteria.AutoRating is AutoRatingProxy autoRatingProxy
								&& autoRatingProxy.AutoRating is IJobDataUpdater jobDataUpdater
								&& jobDataUpdater.CanUpdateDate;
			var lastEffectiveDate = new ZDate(filters.EffectiveDate);
			var originalEffectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(filters.OriginalCriteria,
				contractNumbersFromFilters: canUpdateDate ? filters.ContractNumbersFromFilters : null,
				carriersFromFilters: canUpdateDate ? filters.Carriers : null);

			if (lastEffectiveDate.IsValid && !lastEffectiveDate.IsEmpty && lastEffectiveDate != originalEffectiveDate)
			{
				Criteria.UpdateAutoratingDate(lastEffectiveDate, isCosting: true);
			}
		}

		bool CanPopulateCarrier(RateViewModel rate, out Action commit)
		{
			commit = () => { };

			var carrier = rate.GetCarrierToApplyToJob();

			if (rate is CargoguideRateViewModel && carrier == null)
			{
				#region SuppressResourceStringsCheckRegion

				var sb = new ZStringBuilder();
				sb.AppendLine("Rate with unmappable carrier is being applied");
				sb.AppendLine(Invariant($"CarrierCode = {rate.CarrierCode}"));
				sb.AppendLine(Invariant($"CarrierName = {rate.CarrierName}"));

				ErrorReporter.ReportOnce("AttemptToApplyRateWithNoCarrier", sb.ToString());
				return false;

				#endregion
			}

			// If this carrier is null, then the rate entry had no transport provider and
			// the rate had no service provider. This makes it a standard costing.
			// Standard costing rates are allowed to be selected even if they have no
			// carrier to apply. Hence we return true.
			if (carrier == null)
			{
				return true;
			}

			if (filters.JobServiceProviders.Any(s => s.OH_Code == carrier.OH_Code))
			{
				return true;
			}

			if (Criteria.UpdateCarrierConfirmationIsNeeded(carrier.OH_Code, out var message))
			{
				if (message != null && !dialogService.PromptToApplyCarrierToJob(Criteria.HumanReadableName(), message))
				{
					return false;
				}
			}

			commit = () => Criteria.UpdateCarrier(carrier);

			return true;
		}

		bool CanPopulateOrigin(RateViewModel rate, out Action commit)
		{
			commit = () => { };

			if (Criteria.IsMultiRouteEnabled())
			{
				return true;
			}

			if (LocationHelper.GetLocationType(rate.Origin) == LocationHelper.LocationType.Port)
			{
				if (Criteria.UpdateOriginConfirmationIsNeeded(rate.Origin, out var message))
				{
					var promptResult = dialogService.PromptToApplyOriginToJob(Criteria.HumanReadableName(), message);
					switch (promptResult)
					{
						case ZDialogResult.No:
							// Continue populating other values but skip updating the origin
							return true;

						case ZDialogResult.Cancel:
							return false;
					}
				}
			}

			commit = () => Criteria.UpdateOrigin(rate.Origin);
			return true;
		}

		bool CanPopulateDestination(RateViewModel rate, out Action commit)
		{
			commit = () => { };

			if (Criteria.IsMultiRouteEnabled())
			{
				return true;
			}

			if (LocationHelper.GetLocationType(rate.Destination) == LocationHelper.LocationType.Port)
			{
				if (Criteria.UpdateDestinationConfirmationIsNeeded(rate.Destination, out var message))
				{
					var promptResult = dialogService.PromptToApplyDestinationToJob(Criteria.HumanReadableName(), message);
					switch (promptResult)
					{
						case ZDialogResult.No:
							// Continue populating other values but skip updating the destination
							return true;

						case ZDialogResult.Cancel:
							return false;
					}
				}
			}

			commit = () => Criteria.UpdateDestination(rate.Destination);
			return true;
		}

		void PopulateServiceLevel(RateViewModel rate)
		{
			if (string.IsNullOrEmpty(rate.CarrierServiceLevel))
			{
				return;
			}

			Criteria.UpdateServiceLevel(rate.CarrierServiceLevel);
		}

		void PopulatePaymentTerms(RateViewModel rate)
		{
			if (string.IsNullOrEmpty(rate.PaymentTerms))
			{
				return;
			}

			Criteria.UpdatePaymentTerms(rate.PaymentTerms);
		}

		bool CanPopulateCarrierContractNumber(RateViewModel rate, out Action commit)
		{
			commit = () => { };

			var contractNumber = rate.ContractNumber;
			if (string.IsNullOrEmpty(contractNumber))
			{
				return true;
			}

			var contractNumberList = new[] { contractNumber };
			var checkResult = Criteria.CanUpdateCarrierContractNumber(contractNumberList, dialogService, true);
			if (!checkResult.CanUpdate)
			{
				return false;
			}

			if (checkResult.ConfirmationMessageForOverridingJobContractNumber != null &&
				!_Rating.Interactor.YesNoWarning(checkResult.ConfirmationMessageForOverridingJobContractNumber))
			{
				return false;
			}

			commit = () => Criteria.UpdateCarrierContractNumber(checkResult.Token);

			return true;
		}

		ApplyAction PopulateCommodity(RateViewModel rate)
		{
			var shallContinueApplyingRate = true;

			var isCWRateWithCommodity = !string.IsNullOrEmpty(rate.Commodities) && rate is CW1RateViewModel;
			var isRateServiceRateWithCommodityGroup = rate.CommodityGroups?.Any() ?? false;

			var commodityCodesFromJobContainers = filters.CommodityCodesFromJobContainers;
			var commodityCodesFromJobPacklines = filters.CommodityCodesFromJobPacklines;
			var commodityFromJobContainersOrPacklines = commodityCodesFromJobContainers.Union(commodityCodesFromJobPacklines).Distinct();

			if (isRateServiceRateWithCommodityGroup && commodityCodesFromJobContainers.Any())
			{
				var commodityGroupsFromJob = filters.UniversalCommodityGroupsFromJob.ToArray();
				if (!commodityGroupsFromJob.Intersect(rate.CommodityGroups).Any())
				{
					shallContinueApplyingRate =
						dialogService.PromptToApplyRateWithDifferentUniversalCommodityGroups(
							rate.CommodityGroups.First(),
							commodityCodesFromJobContainers,
							commodityGroupsFromJob);
				}
			}
			else if (isCWRateWithCommodity && commodityFromJobContainersOrPacklines.Any())
			{
				if (!commodityFromJobContainersOrPacklines.Any(x => x == rate.Commodities))
				{
					shallContinueApplyingRate =
						dialogService.PromptToApplyRateWithDifferentCommodityCode(
							rate.Commodities,
							commodityFromJobContainersOrPacklines);
				}
			}

			if (shallContinueApplyingRate)
			{
				return ApplyAction.Apply;
			}
			return ApplyAction.ReturnToRateSelector;
		}

		public bool ShouldRunSearchOnShowingForm { get; set; } = true;

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				search?.Dispose();
				components?.Dispose();

				if (ViewModel != null)
				{
					ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
					ViewModel.Dispose();
					ViewModel = null;
				}
			}

			base.Dispose(disposing);
		}

		// This logger lives only during the RateSelector's existence. It is not the autroating log
		// At the end of the RateSelector it is copied into the Autorating Log.
		readonly MemoryLogger Logger;
		IRateViewModelsProvider[] providers;

		public RatesViewModel ViewModel { get; private set; }
		public RatingCriteria Criteria { get; }
		public bool ApplyZeroCharges { get; private set; }
		public AutoRateInfoCollection SelectedRates { get; private set; }

#if DEBUG
		internal
#endif
		CancellationTokenSource search;

		RateSelectorFilterStripControl filterControl;
		RateSelectorFilterStripBusinessObject filters;
		readonly IDialogService dialogService;
		IRatingContext RatingContext { get; }

		RateSelectorControl RateSelectorControl { get; set; }

		readonly Stopwatch uiLoadStopwatch = new Stopwatch();
	}
}
