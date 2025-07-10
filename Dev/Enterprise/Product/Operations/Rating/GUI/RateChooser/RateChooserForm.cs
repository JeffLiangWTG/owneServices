using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI
{
	public sealed partial class RateChooserForm : ZChildForm
	{
		readonly MultilingualString lclRatesTitle = ResString.GetMultilingualString("A616823E-EB47-4278-B3DC-866EBAD9F8AB", "LCL Rates");

		readonly Stopwatch uiLoadStopwatch = new Stopwatch();

		public RateChooserForm()
		{
			InitializeComponent();
		}

		public RateChooserForm(RateChooserViewModel viewModel, IDialogService dialogService)
			: base(viewModel)
		{
			uiLoadStopwatch.Start();
			ViewModel = viewModel;
			if (!DesignModeFinder.IsDesigning)
			{
				filterBusinessObject = new RateChooserFilterStripBusinessObject(ViewModel.Model.Criteria);
			}

			Initialize();

			this.dialogService = dialogService ?? new DialogService(this);
		}

		public RateChooserForm(RateChooserViewModel viewModel)
			: this(viewModel, null)
		{
		}

		void Initialize()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				var filterBackgroundColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.FilterBackgroundColor;
				topPanel.BackColor = filterBackgroundColor;
			}

			// Design time serializer doesn't support this property...
			this.toolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16);

			SetupButtons();

			BuildTabs();
			summaryCardsControl.SetViewModel(ViewModel);
			ViewModel.RateSelectionChanged += ViewModel_RateSelectionChanged;
			ViewModel.TabSelectionChanged += ViewModel_TabSelectionChanged;
			ViewModel.SelectRelatedRatesClicked += ViewModel_SelectRelatedRatesClicked;

			if (isFCL)
			{
				// Not using CaptionResourceString since need to add pad spaces
				summaryTabPage.Text = Res.GetString("D15E2616-9D3C-4859-9238-9BFB964B8E8A", "Rate Selection")
										+ TabTextPaddingForIcon;
			}
			else
			{
				//For LCL, we don't need to show summary tab
				summaryTabPage.TabVisible = false;
			}

			jobValuesLabel.Text = string.Join("   ",
				Res.GetString("E00C98FA-E197-45DF-86E2-CC66E51A7608", "Transport:") + " " + ViewModel.Model.TransportMode,
				Res.GetString("0B6E0907-D2D4-4585-BEBF-2D6B89AF8087", "Container Mode:") + " " + ViewModel.Model.ContainerMode);
		}

		void ViewModel_SelectRelatedRatesClicked(object sender, EventArgs e)
		{
			var tabPages = containerTabControl.TabPages.Cast<ZTabPage>();

			foreach (var tabPage in tabPages)
			{
				containerTabControl.Invalidate(containerTabControl.GetTabRect(containerTabControl.TabPages.IndexOf(tabPage)));
			}
		}

		ZToolStripButton loadMoreButton;
		ZToolStripButton applyButton;
		ZToolStripButton cancelButton;
		ZToolStripButton clearAllButton;
		ZToolStripButton skipRateSelectionButton;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				BeginInvoke(new MethodInvoker(() =>
				{
					// InitializeFilterControl occurs in a BeginInvoke to reduce the chances
					// of a not-enough-quota exception occurring. From googling online this exception
					// can occur when the UI message queue is full. So, moving this filter initialisation to
					// after the OnLoad has been processed and hopefully the message queue has been cleared
					// will prevent the not-enough-quota exception.
					//
					// Note: Initializing the filter strip performs a search
					InitializeFilterControl();
				}));
			}
		}

		/// <summary>
		/// Don't need the base class DisplayMode logic.
		/// </summary>
		public override ODisplayMode DisplayMode { get => ODisplayMode.Edit; set { } }

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			// Don't need base class check for record modified
		}

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			var spotBookingRequestDialogRst = ZDialogResult.No;

			if (Model.NeedShowBookingRequestDialog)
			{
				spotBookingRequestDialogRst = dialogService.PromptToSendBookingInformationToCarrier(Model.GetCarrierFromSelectedRates()?.OH_FullName);
				if (spotBookingRequestDialogRst == ZDialogResult.Cancel)
				{
					return;
				}
			}

			var (hasApplied, showError) = ApplySelectedRatesToJob();

			if (hasApplied)
			{
				var rates = Model.ContainerGroups.SelectMany(r => r.Rates).ToList();
				var selectedRate = Model.ContainerGroups.FirstOrDefault()?.SelectedRate;
				ReportRateSelected(rates, selectedRate);

				DialogResult = DialogResult.OK;
				WriteLogToAutoratingLog();
				Close();
				if (spotBookingRequestDialogRst == ZDialogResult.Yes)
				{
					SendBookingInformationToCarrier();
				}
			}

			if (showError)
			{
				Globals.Message.ShowError(Res.GetString("B5397D9A-1049-4741-9269-F6438193ED16", "Please correct the errors and try again"));
			}
		}

		void ReportRateSelected(IEnumerable<ChooserRateEntry> foundRates, ChooserRateEntry selectedRate)
		{
			var searchResult = new UsageRatesSearchResult();
			searchResult.CargoSphere.TotalRates = foundRates.Count(r => r.WiseRateEntry != null);
			searchResult.CW1.TotalRates = foundRates.Count(r => r.WiseRateEntry == null);

			var selectedProvider = selectedRate != null
				? selectedRate.WiseRateEntry != null
					? RatingUsageCollector.RateProvider.CargoSphere
					: RatingUsageCollector.RateProvider.CW1
				: (RatingUsageCollector.RateProvider?)null;

			RatingUsageCollector.ReportRateSelector(RatingUsageCollector.RateSelectorAction.Select, uiLoadStopwatch.ElapsedMilliseconds, selectedProvider, searchResult);
		}

		(bool hasApplied, bool showError) ApplySelectedRatesToJob()
		{
			var zeroCharges = Model.GetZeroCharges();
			Model.RemoveZeroCharges = false;

			if (zeroCharges.Any())
			{
				var dialogResult = dialogService.PromptUserApplyZeroCharges(zeroCharges);

				if (dialogResult == ZDialogResult.Cancel)
				{
					return (false, false);
				}

				if (dialogResult == ZDialogResult.No)
				{
					Model.RemoveZeroCharges = true;
				}
			}

			Model.Validate(zeroCharges);

			MapCarriersIfRequired();
			if (!Model.CarrierIsMapped)
			{
				return (false, true);
			}

			if (!Model.ServiceLevelIsMapped)
			{
				// by now there should be [1 and only 1] service provider and [1 and only 1] unmapped service level across the selections
				// otherwise there is a validation error and Apply button is disabled.
				var chooserRateEntry = Model.NotMappedServiceLevelRates.FirstOrDefault();

				var mappingServiceLevelResult = PromptUserToMapServiceLevels(chooserRateEntry.ServiceProvider, chooserRateEntry.CarrierServiceLevel);
				if (!mappingServiceLevelResult)
				{
					return (false, true);
				}

				Model.UpdateMappings();

				if (!Model.ServiceLevelIsMapped)
				{
					return (false, true);
				}
			}

			if (!Model.IsValid)
			{
				var unmappedCharges = Model.UnmappedCharges;
				if (unmappedCharges.Count > 0)
				{
					ShowMapChargesForm(unmappedCharges);
					Model.UpdateMappings();
					Model.ValidateAllChargeCodesAreMapped(zeroCharges);
				}
			}

			if (Model.IsValid)
			{
				if
					(
						!CanApplyCarrierBackToJob(out var applyCarrierBackToJob)
						|| !CanApplyLocationsBackToJob(out var applyLocationsBackToJob)
						|| !CanApplyPenaltiesBackToJob(out var applyPenaltiesBackToJob)
						|| !CanApplyCarrierContractNumberBackToJob(out var applyContractNumbersBackToJob)
					)
				{
					return (false, false);
				}

				applyLocationsBackToJob();
				applyCarrierBackToJob();

				ApplyNamedAccountBackToJobIfNeeded();
				Model.ApplyServiceLevelBackToJobIfNeeded();
				Model.ApplyCarrierQuoteNumberBackToJob();
				Model.ApplySpotBookingTermsBackToJobIfNeeded();
				applyContractNumbersBackToJob();

				applyPenaltiesBackToJob();

				ApplyTransportsBackToJobIfNeeded();
				ApplyEffectiveDateBackToJobIfNeeded();

				return (true, false);
			}

			return (false, !Model.IsValid);
		}

		void SendBookingInformationToCarrier()
		{
			Model.SendBookingInformationToCarrier();
		}

		void WriteLogToAutoratingLog()
		{
			var warnings = Model.WarningsFromSearch;
			var errors = Model.ErrorsFromSearch;

			if (warnings.Any() || errors.Any())
			{
				Model.Logger.Information((NoResString)"========== BEGIN RATE SELECTOR LOGS ===========");   // Just a log string

				foreach (var warning in warnings)
				{
					Model.Logger.Warning(warning);
				}
				foreach (var error in errors)
				{
					Model.Logger.Error(error);
				}

				Model.Logger.Information((NoResString)"========== END RATE SELECTOR LOGS ===========");   // Just a log string
			}
		}

		void ApplyTransportsBackToJobIfNeeded()
		{
			var scheduleDetails = Model.GetSelectedScheduleDetails();

			if (scheduleDetails?.Any() == true)
			{
				var carrier = Model.GetCarrierFromSelectedRates();
				var transports = TransportLegConverter.Convert(Model.Factory, carrier, scheduleDetails, Model.Logger);
				Model.ApplyTransportsBackToJob(transports);
			}
		}

		bool CanApplyPenaltiesBackToJob(out Action commit)
		{
			commit = () => { };

			var penalties = Model.GetContainerPenaltiesToApplyToJob();
			if (penalties.Any())
			{
				var deleteExistingPenalties = false;

				if (Model.Criteria.UpdateContainerPenaltiesConfirmationIsNeeded(penalties, out var message)
					&& !dialogService.PromptToApplyContainerPenaltiesToJob(Model.Criteria.HumanReadableName(), message, out deleteExistingPenalties)
					)
				{
					return false;
				}

				commit = () => Model.ApplyContainerPenaltiesBackToJob(penalties, deleteExistingPenalties);
				return true;
			}

			return true;
		}

		bool CanApplyCarrierBackToJob(out Action commit)
		{
			commit = () => { };

			var newCarrier = Model.GetNewCarrierToApplyToJob();

			if (newCarrier != null && Model.Criteria.UpdateCarrierConfirmationIsNeeded(newCarrier.OH_FullName, out var message))
			{
				if (message != null && !dialogService.PromptToApplyCarrierToJob(Model.Criteria.HumanReadableName(), message))
				{
					return false;
				}
			}

			commit = () => Model.ApplyCarrierBackToJob();

			return true;
		}

		bool CanApplyCarrierContractNumberBackToJob(out Action commit)
		{
			commit = () => { };

			var numbers = Model.SelectedDistinctNonBlankContractNumbers;
			var checkResult = Model.Criteria.CanUpdateCarrierContractNumber(numbers, dialogService, true);
			if (!checkResult.CanUpdate)
			{
				return false;
			}

			if (checkResult.ConfirmationMessageForOverridingJobContractNumber != null &&
				_Rating.IsOn && _Rating.Interactor != null &&
				!_Rating.Interactor.YesNoWarning(checkResult.ConfirmationMessageForOverridingJobContractNumber))
			{
				return false;
			}

			commit = () => Model.ApplyContractNumberBackToJob(checkResult.Token);
			return true;
		}

		bool CanApplyLocationsBackToJob(out Action commit)
		{
			commit = () => { };

			if (Model.Criteria.IsMultiRouteEnabled())
			{
				return true;
			}

			var newLocations = Model.GetNewLocationsToApplyBackToJob();
			Action updateCallback = () => Model.ApplyLocationsBackToJob(newLocations.Origin, newLocations.Destination);

			if (Model.SelectedRatesHaveSpotRate)
			{
				commit = updateCallback;
				return true;
			}

			if (!newLocations.Origin.IsEmpty
				&& Model.Criteria.UpdateOriginConfirmationIsNeeded(newLocations.Origin, out var originMessage))
			{
				var promptResult = dialogService.PromptToApplyOriginToJob(Model.Criteria.HumanReadableName(), originMessage);
				switch (promptResult)
				{
					case ZDialogResult.No:
						// Continue populating other values but skip updating the origin
						return true;

					case ZDialogResult.Cancel:
						return false;
				}
			}

			if (!newLocations.Destination.IsEmpty
				&& Model.Criteria.UpdateDestinationConfirmationIsNeeded(newLocations.Destination, out var destinationMessage))
			{
				var promptResult = dialogService.PromptToApplyDestinationToJob(Model.Criteria.HumanReadableName(), destinationMessage);
				switch (promptResult)
				{
					case ZDialogResult.No:
						// Continue populating other values but skip updating the destination
						return true;

					case ZDialogResult.Cancel:
						return false;
				}
			}

			commit = updateCallback;

			return true;
		}

		void MapCarriersIfRequired()
		{
			if (!Model.CarrierIsMapped)
			{
				Model.MapCarrier((unmappedChooserRateEntry) => PromptUserToMapCarrier(unmappedChooserRateEntry.RateEntry, Model.ConversionContext.Response));
				Model.UpdateMappings();
			}
		}

		void ApplyNamedAccountBackToJobIfNeeded()
		{
			var allNamedAccounts = Model.AllNamedAccounts;
			if (allNamedAccounts.Count() <= 1)
			{
				Model.ApplyNamedAccountBackToJob(allNamedAccounts.FirstOrDefault());
				return;
			}

			using (var form = new SingleValueSelectForm(
				allNamedAccounts,
				Res.GetString("0a234ba7-839d-4758-92d5-90b9928ea2ac", "Rates applied to the job do NOT have the same Named Account. Please select which Named Account to be populated to the job."),
				Res.GetData("93622ff4-06bf-4901-ad2c-50779597608b", "Named Account")))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					Model.ApplyNamedAccountBackToJob(form.SelectedName);
				}
			}
		}

		void ApplyEffectiveDateBackToJobIfNeeded()
		{
			var canUpdateDate = filterBusinessObject.CanUpdateDate;
			var originalEffectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(filterBusinessObject.OriginalCriteria,
				contractNumbersFromFilters: canUpdateDate ? filterBusinessObject.ContractNumbersFromFilters : null,
				carriersFromFilters: canUpdateDate ? filterBusinessObject.Carriers : null);

			if (lastEffectiveDate.IsValid && !lastEffectiveDate.IsEmpty && lastEffectiveDate != originalEffectiveDate)
			{
				Model.Criteria.UpdateAutoratingDate(lastEffectiveDate, isCosting: true);
			}
		}

		void ShowMapChargesForm(UniversalChargeCodeMapBizoCollection unmappedCharges)
		{
			var newFactory = new BusinessObjectFactory();
			var unmappedChargesForEdit = unmappedCharges.CopyCodeAndDescriptionToAnotherFactory(newFactory);
			using (var form = new MapChargeCodesForm(unmappedChargesForEdit))
			{
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		bool PromptUserToMapServiceLevels(OrgHeader serviceProvider, string serviceLevel)
		{
			if (serviceProvider == null)
			{
				return false;
			}

			var msg = Res.GetString("ccfb9f93-2498-4bca-8769-8ee2d224e5b2", "The Universal Service Level '{0}' has NOT been assigned to Carrier '{1}'. Would you like to complete the assignment and apply rates to the job?", serviceLevel, serviceProvider.OH_Code);
			if (DialogResult.Yes != Globals.Message.Show(msg, this.CaptionResourceString.Caption, MessageBoxButtons.YesNo, DialogResult.No))
			{
				return false;
			}

			return WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				Res.GetString("0B0D1B01-305E-4DF2-BBE5-1D6778890B84", "Carrier Service Levels"),
				(securityCore) => securityCore.OrgCarrierModify,
				false,
				CaptionResourceString.Caption,
				() =>
				{
					var form = new OrganizationFormForCarrierServiceLevelsMapping(
						this,
						serviceProvider,
						Model.UniversalServiceLevels,
						new[]
						{
							new UnmappedForeignCode(serviceLevel, Core.Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel)
						}
					);

					return form.ShowModal();
				}
			);
		}

		(OrgHeader, string) PromptUserToMapCarrier(IRateEntry wiseRate, RatesSearchResponse ratesSearchResponse)
		{
			OrgHeader mappedCarrier = null;

			var rateEntry = new WiseEntryView(wiseRate, ratesSearchResponse);

			var scacOrC1Code = !string.IsNullOrEmpty(rateEntry?.RefCarrier?.SCACCode)
				? rateEntry.RefCarrier.SCACCode
				: rateEntry?.RefCarrier?.C1Code;

			var msg = Res.GetString("487C34C1-6CD3-4F04-AB55-3BB079402D2F9", "The following Carrier '{0}' could NOT be found.  Would you like to map the missing Carrier?", scacOrC1Code);
			if (DialogResult.Yes != Globals.Message.Show(msg, this.CaptionResourceString.Caption, MessageBoxButtons.YesNo, DialogResult.No))
			{
				return (null, string.Empty);
			}

			WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				Res.GetString("BF0B4E4F-992B-4477-B149-D5DE62CB2816", "Carrier"),
				(securityCore) => securityCore.OrganisationModify,
				true,
				CaptionResourceString.Caption,
				() =>
				{
					var command = new AssignCarrierCodeCommand();
					var (assigned, orgHeader) = command.Assign(rateEntry);

					if (assigned && orgHeader != null)
					{
						mappedCarrier = orgHeader;

						var cacheKey = !string.IsNullOrEmpty(rateEntry?.RefCarrier?.SCACCode)
							? WiseRatesConverter.GetSCACCacheKey(scacOrC1Code, Model.ConversionContext.ConversionSessionID)
							: WiseRatesConverter.GetC1CCacheKey(scacOrC1Code, Model.ConversionContext.ConversionSessionID);
						rateEntry?.Factory?.ClearCachedValue<ZGuid>(cacheKey);
					}

					return true;
				}
			);

			return (mappedCarrier, scacOrC1Code);
		}

		void SetupButtons()
		{
			applyButton = new ZToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				Image = Icons.GetImage(IconTypes.BlackWhite_Save),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = Res.GetString("C62F2E64-096D-48EB-938F-9E1F8626BDDD", "Apply"),
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right,
				Enabled = false
			};
			applyButton.Click += OnApplyButtonClick;

			var cancelInfo = ZFormPostingButtonsStrategy.CancelButtonText(null);
			cancelButton = new ZToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				Image = cancelInfo.Image,
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = cancelInfo.Text,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right
			};
			cancelButton.Click += CancelButton_Click;

			clearAllButton = new ZToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				Image = Icons.GetImage(IconTypes.ClearButtonRest),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = Res.GetString("ad0e567b-8a42-4a03-a71b-12b88a611f64", "Clear All"),
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right,
				Enabled = true
			};
			clearAllButton.Click += ClearAllButton_Click;

			skipRateSelectionButton = new ZToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				Image = Icons.GetImage(IconTypes.BlackWhite_Forward),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = Res.GetString("e10cbd6f-a1cc-48a2-8f70-917be8b51d18", "Skip Rate Selection"),
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right,
				Enabled = true
			};
			skipRateSelectionButton.Click += SkipRateSelectionButton_Click;

			loadMoreButton = new ZToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				Image = Icons.GetImage(IconTypes.FindButtonRest),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = Res.GetString("3EF1B03C-1528-4951-9FEA-60791C9CB5F7", "More Rates ..."),
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right,
				Enabled = false,
				Name = "loadMoreButton",
			};
			loadMoreButton.Click += LoadMoreButton_Click;

			toolStrip.Items.Add(applyButton);
			toolStrip.Items.Add(clearAllButton);
			toolStrip.Items.Add(skipRateSelectionButton);
			toolStrip.Items.Add(cancelButton);
			toolStrip.Items.Add(loadMoreButton);
		}

		internal void SkipRateSelectionButton_Click(object sender, EventArgs e)
		{
			RatingUsageCollector.ReportRateSelector(RatingUsageCollector.RateSelectorAction.Skip);
			DialogResult = DialogResult.Cancel;
			WriteLogToAutoratingLog();
			IsRateSelectionSkipped = true;
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			RatingUsageCollector.ReportRateSelector(RatingUsageCollector.RateSelectorAction.Cancel);
			DialogResult = DialogResult.Cancel;
			Close();
		}

		public bool IsRateSelectionSkipped { get; private set; }

		void ClearAllButton_Click(object sender, EventArgs e)
		{
			ViewModel.ClearAll();
		}

		void LoadMoreButton_Click(object sender, EventArgs e)
		{
			using (new ZWaitCursorChanger(this))
			{
				ViewModel.ShowMoreRates();

				UpdateUi();

				var model = ViewModel.Model;
				filterControl.SetErrorsAndWarnings(model.ErrorsAndWarningsFromSearch, model.RawData);
			}
		}

		RateChooserViewModel ViewModel { get; }
		public RateChooserModel Model => ViewModel.Model;
		readonly RateChooserFilterStripBusinessObject filterBusinessObject;

		void InitializeFilterControl()
		{
			// Create the filter strip control after the form is restored to the saved position and size.
			// Otherwise it causes the form to be shown in its default position/size and then drawn again.
			filterControl = new RateChooserFilterStripControl(ViewModel, filterBusinessObject);
			filterControl.ShouldRunSearchOnStripsInitialized = ShouldRunSearchOnShowingForm;
			filterControl.Dock = System.Windows.Forms.DockStyle.Top;
			filterControl.Name = "stripControl";
			filterControl.AutoSize = true;
			filterControl.TabIndex = 0;
			filterControl.PerformSearch += FilterControl_PerformSearch;

			// The not-enough-quota probem has occured previously when adding the filterControl
			// to the filterAndResultsPanel. We've mitigated this risk by calling InitializeFilterControl
			// from a BeginInvoke. But, in case it does happen again this `using` below will avoid
			// us getting an issue manager issue about it. The user will still see a message box about
			// not-enough-quota
			using (QuotaExhaustedExceptionSuppressor.SuppressException())
			{
				filterAndResultsPanel.Controls.Add(this.filterControl);
			}

			uiLoadStopwatch.Stop();
		}

		public bool ShouldRunSearchOnShowingForm { get; set; } = true;

		void FilterControl_PerformSearch(object sender, PerformSearchEventArgs e)
		{
			// If you decide to do search asynchronously, this is what obstacles you will have to fight with:
			// - CS rate selector business logic uses a lot of FilterStripBusinessObject which in CW1 is part of UI
			// - FilterStripBusinessObject is not allowed to be used from a different thread than the one it was created from.
			//   You will get DeveloperOnlyException here and there asking to release the object in one thread and take ownership in another.
			// - I believe it is because  filter strips are of type BusinessObject rather than NonPersistentBusinessObject, have no idea why they are not NonPersistent
			//
			// So, even though functionally it works and does improve user experience (the rate selector feels faster and more responsive),
			// we cannot use it because of these silly limitations.
			DoSearch();

			UpdateUi();
			filterControl.SetErrorsAndWarnings(ViewModel.Model.ErrorsAndWarningsFromSearch, ViewModel.Model.RawData);
		}

		void DoSearch()
		{
			var filterBusinessStripObject = (RateChooserFilterStripBusinessObject)filterControl.FilterBusinessObject;

			filterBusinessStripObject.RunPreSaveValidation();
			if (filterBusinessStripObject.HasErrors())
			{
				return;
			}

			var viewModel = ViewModel;
			var model = viewModel.Model;
			(model.WiseRatesProviderLogger as ElementaryLogger)?.ClearLogs();

			var (ratesQuery, isValidForRatesService) = filterBusinessStripObject.BuildRatesQuery(model.WiseRatesProviderLogger);
			lastEffectiveDate = new ZDate(ratesQuery.EffectiveDate);

			viewModel.SendRatesRequest(
				filter: filterBusinessStripObject,
				ratesQuery: ratesQuery,
				isValidForRatesService: isValidForRatesService,
				universalCarrierLevels: filterBusinessStripObject.UniversalCarrierServiceLevelsFromJob);
		}

		/// <summary>
		/// The actual searched date, not the one showing on filter.
		/// </summary>
		ZDate lastEffectiveDate;

		void UpdateUi()
		{
			ViewModel.RefreshRates();

			foreach (ZTabPage tabPage in containerTabControl.TabPages)
			{
				if (tabPage.Tag is ChooserContainerCommodityViewModel containerViewModel)
				{
					UpdateTabText(tabPage, containerViewModel);
				}
			}
			UpdateApplyButton();
			UpdateLoadMoreButton();
		}

		void UpdateLoadMoreButton()
		{
			loadMoreButton.Enabled = ViewModel.HasMorePages;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1111:DoNotUseSystemWindowsFormsTabDrawModeOwnerDrawFixed", Justification = "custom tab rendering")]
		void BuildTabs()
		{
			containerTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed; // custom tab rendering
			containerTabControl.DrawItem += ContainerTabControl_DrawItem;

			int index = 0;
			foreach (var containerViewModel in ViewModel.ContainerTabs)
			{
				var tabPage = new ZTabPage();
				tabPage.Name = containerViewModel.TabName;
				tabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
				tabPage.UseVisualStyleBackColor = true;
				tabPage.Tag = containerViewModel;
				UpdateTabText(tabPage, containerViewModel);
				containerTabControl.TabPages.Insert(tabPage, index++);

				var pageControl = new RateChooserCardsControl(ViewModel, containerViewModel);
				tabPage.Controls.Add(pageControl);

				pageControl.Dock = System.Windows.Forms.DockStyle.Fill;
				pageControl.AutoScroll = true;
				pageControl.AutoScrollMinSize = ControlDpiScalingHelper.NewScaledSize(2000, 200, true);
			}
		}

		void UpdateTabText(ZTabPage tabPage, ChooserContainerCommodityViewModel containerViewModel)
		{
			if (isFCL)
			{
				tabPage.Text = containerViewModel.TabText + TabTextPaddingForIcon;
			}
			else
			{
				tabPage.Text = lclRatesTitle + Res.GetString("a6ed95ad-295c-4d0d-b0be-9228e57d7d28", "- {0} Rate(s)", containerViewModel?.Rates?.Count) + TabTextPaddingForIcon;
			}
		}

		void ViewModel_TabSelectionChanged(object sender, EventArgs e)
		{
			var containerViewModel = (ChooserContainerCommodityViewModel)sender;
			var tabPage = containerTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Tag == containerViewModel);
			containerTabControl.SelectedTab = tabPage;
		}

		/// <summary>
		/// Called when the user selects or unselects a rate
		/// </summary>
		void ViewModel_RateSelectionChanged(object sender, RateSelectionChangedEventArgs e)
		{
			var containerViewModel = e.ViewModel;
			var tabPage = containerTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Tag == containerViewModel);
			UpdateTabText(tabPage, containerViewModel);
			containerTabControl.Invalidate(containerTabControl.GetTabRect(containerTabControl.TabPages.IndexOf(tabPage)));
			UpdateApplyButton();
		}

		void UpdateApplyButton()
		{
			applyButton.Enabled = ViewModel.IsApplyEnabled;
		}

		void ContainerTabControl_DrawItem(object sender, DrawItemEventArgs e)
		{
			var tabPage = containerTabControl.TabPages[e.Index];
			var containerViewModel = tabPage.Tag as ChooserContainerCommodityViewModel;
			bool isChecked = (containerViewModel != null && containerViewModel.SelectedRow != null)
				|| (containerViewModel == null && ViewModel.AllTabsHaveSelection);

			if (isChecked)
			{
				e.Graphics.FillRectangle(Brushes.LightGreen, e.Bounds);
			}
			else if (e.State == DrawItemState.Selected)
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
			}

			var paddedBounds = e.Bounds;
			int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
			paddedBounds.Offset(1, yOffset);
#if !WINZOR
			TextRenderer.DrawText(e.Graphics, tabPage.Text, Font, paddedBounds, tabPage.ForeColor);
#endif
			var showNoRateWarning = (tabPage != summaryTabPage) && (containerViewModel == null || !containerViewModel.Rates.Any());

			var size = isChecked
				? ControlDpiScalingHelper.NewScaledSize(8, 8)
				: ControlDpiScalingHelper.NewScaledSize(12, 12);

			int midY = (paddedBounds.Top + paddedBounds.Bottom) / 2;
			int midX = paddedBounds.Right - size.Width / 2 - 4;
			if (e.State == DrawItemState.Selected)
			{
				midX -= 4;
			}
			var rect = ControlDpiScalingHelper.NewScaledRectangle(midX - size.Width / 2, midY - size.Height / 2, size.Width, size.Height, false);

			if (isChecked)
			{
#if !WINZOR
				e.Graphics.DrawImage(Properties.RatingResources.CheckYes16, rect);
#else
				e.Graphics.DrawImageUnscaled(Properties.RatingResources.CheckYes16, rect);
#endif
			}
			else if (showNoRateWarning)
			{
#if !WINZOR
				e.Graphics.DrawImage(WarningIcon, rect);
#else
				e.Graphics.DrawImageUnscaled(WarningIcon, rect);
#endif
			}
		}

#if DEBUG
		public void ApplyButtonClick()
		{
			applyButton.Enabled = true;
			applyButton.PerformClick();
		}

		public void SkipButtonClick()
		{
			skipRateSelectionButton.PerformClick();
		}

		public void CancelButtonClick()
		{
			cancelButton.PerformClick();
		}

		public void SwitchToFirstTab() => containerTabControl.SelectedTab = containerTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(p => p.Name != "summaryTabPage");
#endif

		Bitmap WarningIcon => warningIcon ?? (warningIcon = SystemIcons.Warning.ToBitmap());

		Bitmap warningIcon;

		RateChooserFilterStripControl filterControl;
		const string TabTextPaddingForIcon = "      ";

		readonly IDialogService dialogService;

		bool isFCL => Model.Criteria.IsContainerised;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ViewModel.RateSelectionChanged -= ViewModel_RateSelectionChanged;
				ViewModel.TabSelectionChanged -= ViewModel_TabSelectionChanged;
				ViewModel.SelectRelatedRatesClicked -= ViewModel_SelectRelatedRatesClicked;

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
