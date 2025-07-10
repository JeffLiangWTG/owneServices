using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.AddressCleansing.Common;
using static Enterprise.MasterFiles.GUI.DuplicateAlertControlHelper;
using ISupportWebAddressValidation = Enterprise.MasterFiles.Business.ISupportWebAddressValidation;

namespace Enterprise.MasterFiles.GUI
{
	public partial class NameAndAddressDetailsUserControl : ZUserControl, IReadOnlyToggleControl, ISupportWebAddressValidationControl, ISupportDuplicationAlertControl
	{
		public NameAndAddressDetailsUserControl()
		{
			InitializeComponent();
			SetCharacterCasing();
			SetupNavigateToWebButton();
			SetToolTip();

			if (!DesignModeFinder.IsDesigning)
			{
				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					cancellationToken = new CancellationTokenSource();
					HandleCreated += (o, e) =>
					{
						if (ParentForm != null)
						{
							ParentForm.FormClosed += (x, y) =>
							{
								if (cancellationToken != null)
								{
									cancellationToken.Cancel();
								}
							};
						}
					};

					HookISupportWebAddressValidationControlChangeFocusEvents();
					HookResizeEvent();
				}
				else
				{
					ValidateAddressButton.Visible = false;
					ClearFieldsButton.Visible = false;
				}
			}

			ValidationJustForced = false;

#if DEBUG
			TypeDescriptor.AddAttributes(MainAddressDetailsGroupBox, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(GoToUrlButton, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(RegistrationNumberTypeLabel, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(RegistrationNumberTypeLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		void HookResizeEvent()
		{
			Resize += NameAndAddressDetailsUserControl_Resize;
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void SetToolTip()
		{
			ToolTipService.SetToolTip(ScreenButtonCoverLabel, Res.GetString("FF61250D-F3FC-47BB-B800-040779FC0F82", "The Denied Party Screening service is not available for inactive organizations."));
		}

		protected virtual void NameAndAddressDetailsUserControl_Resize(object sender, EventArgs e)
		{
			var suggestionControl = SupportWebAddressValidationControlHelper.FindAddressSuggestionControl(this);
			if (suggestionControl != null && suggestionControl.Visible)
			{
				SupportWebAddressValidationControlHelper.ShowAddressSuggestionControl(this, MaxWidth, 0, true);
			}
		}

		#region Deduplication

		public void ShowDeduplicationStatus()
		{
			DuplicateDetectionStatusIcon.Image = Properties.Resources.loader;
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationStatus);
			DuplicateDetectionStatusLabel.Text = getTextAndColor.Text;
			DuplicateDetectionStatusLabel.ForeColor = getTextAndColor.ForeColor;
			DuplicateDetectionStatusLabel.Location = ControlDpiScalingHelper.NewScaledPoint(582, 74, true);
			DuplicateDetectionStatusLabel.Visible = true;
			DuplicateDetectionStatusIcon.Visible = true;
		}

		protected IDuplicationEventArgs currentDuplicationEventArgs;

		public void ShowDuplicatesFound(IDuplicationEventArgs duplicationEventArgs)
		{
			currentDuplicationEventArgs = duplicationEventArgs;
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDuplicatesFound);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterDuplicatesLabelEventHandlers();
			RegisterDuplicatesLabelEventHandlers();

			ShowExclusions(currentDuplicationEventArgs);
		}

		void ShowExclusions(IDuplicationEventArgs duplicationEventArgs)
		{
			var arg = (DuplicationEventArgs)duplicationEventArgs;

			if (arg.OrgHeaderExclusionManager != null)
			{
				DuplicateExclusionsLabel.Text = ResString.GetMultilingualString("6cfc3ef6-e976-4079-b968-44f9934c5652", "Excluded records: ({0})", (arg.OrgHeaderExclusionManager.ItemsCount).ToString(CultureInfo.InvariantCulture));
				DuplicateExclusionsLabel.ForeColor = Color.Blue;
				DuplicateExclusionsLabel.Left = DuplicateDetectionStatusLabel.Width + DuplicateDetectionStatusLabel.Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				DuplicateExclusionsLabel.Visible = arg.OrgHeaderExclusionManager.ItemsCount > 0;
			}
		}

		public void ShowNotEnoughInformation()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowNotEnoughInformation);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
		}

		public void ShowNoDuplicatesFound(IDuplicationEventArgs duplicationEventArgs)
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowNoDuplicatesFound);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterDuplicatesLabelEventHandlers();
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public void ShowExcludedDuplicationMessage()
		{
			var arg = currentDuplicationEventArgs as DuplicationEventArgs;
			DeregisterDuplicatesLabelEventHandlers();
			if (arg?.OrgHeaderExclusionManager != null)
			{
				SetDuplicateDetectionStatusLabel(ResString.GetMultilingualString("8EC5ECD4-57D1-4201-9808-53BDB8DD3EBD", "Excluded from De-duplication due to size limits"), Color.OrangeRed);
				if (!string.IsNullOrWhiteSpace(arg.OrgHeaderExclusionManager?.DisplayInfo))
				{
					ToolTipService.SetToolTip(DuplicateDetectionStatusLabel, arg.OrgHeaderExclusionManager.DisplayInfo);
				}

				RegisterDuplicatesLabelEventHandlers();
			}
			else
			{
				var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowExcludedDuplicationMessage);
				SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public void ShowDeduplicationTimeoutMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationTimeoutMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterDuplicatesLabelEventHandlers();
			RegisterDuplicatesLabelEventHandlers();
			if (CanAccessMDMAdminPanel)
			{
				DuplicateDetectionStatusLabel.ForeColor = Color.Blue;
				ToolTipService.SetToolTip(DuplicateDetectionStatusLabel, ResString.GetMultilingualString("b7800627-a157-4ce1-a118-0f8909565d3f", "Duplicate results for this record can be accessed in the MDM admin panel"));
			}
		}

		public void ShowDeduplicationErrorOccurredMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationErrorOccurredMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterDuplicatesLabelEventHandlers();
		}

		public void SetDuplicateDetectionStatusLabel(ResourceString text, Color foreColor)
		{
			ToolTipService.ClearTooltip(DuplicateDetectionStatusLabel);
			DuplicateDetectionStatusLabel.Text = text;
			DuplicateDetectionStatusLabel.ForeColor = foreColor;
			DuplicateDetectionStatusLabel.Location = ControlDpiScalingHelper.NewScaledPoint(549, 74, true);
			DuplicateDetectionStatusLabel.Visible = true;
			DuplicateDetectionStatusIcon.Visible = false;
		}

		void RegisterDuplicatesLabelEventHandlers()
		{
			DuplicateDetectionStatusLabel.MouseEnter += DuplicateDetectionStatusLabelOnMouseEnter;
			DuplicateDetectionStatusLabel.MouseLeave += DuplicateDetectionStatusLabelOnMouseLeave;
			DuplicateDetectionStatusLabel.Click += DuplicateDetectionStatusLabelOnClick;
		}

		void DeregisterDuplicatesLabelEventHandlers()
		{
			DuplicateDetectionStatusLabel.MouseEnter -= DuplicateDetectionStatusLabelOnMouseEnter;
			DuplicateDetectionStatusLabel.MouseLeave -= DuplicateDetectionStatusLabelOnMouseLeave;
			DuplicateDetectionStatusLabel.Click -= DuplicateDetectionStatusLabelOnClick;
		}

		public void DuplicateDetectionStatusLabelOnMouseEnter(object sender, EventArgs eventArgs)
		{
			if ((DuplicateDetectionStatusLabel.Text == GetTextAndColor(DuplicationConditions.ShowDeduplicationTimeoutMessage).Text && CanAccessMDMAdminPanel)
				|| (DuplicateDetectionStatusLabel.Text == GetTextAndColor(DuplicationConditions.ShowDuplicatesFound).Text))
			{
				SetHyperLinkMouseEnter(DuplicateDetectionStatusLabel, true);
			}
		}

		public void DuplicateDetectionStatusLabelOnMouseLeave(object sender, EventArgs e)
		{
			if ((DuplicateDetectionStatusLabel.Text == GetTextAndColor(DuplicationConditions.ShowDeduplicationTimeoutMessage).Text && CanAccessMDMAdminPanel)
				|| (DuplicateDetectionStatusLabel.Text == GetTextAndColor(DuplicationConditions.ShowDuplicatesFound).Text))
			{
				SetHyperLinkMouseEnter(DuplicateDetectionStatusLabel, false);
			}
		}

		void SetHyperLinkMouseEnter(ZLabel label, bool mouseEnter)
		{
			label.Font = new Font(DuplicateDetectionStatusLabel.Font, mouseEnter ? FontStyle.Underline : FontStyle.Regular);
			label.Cursor = mouseEnter ? Cursors.Hand : Cursors.Default;
		}

		public void DuplicateDetectionStatusLabelOnClick(object sender, EventArgs eventArgs)
		{
			var timeoutOrOutOfSizeLimitation = DuplicateDetectionStatusLabel.Text == GetTextAndColor(DuplicationConditions.ShowDeduplicationTimeoutMessage).Text
				|| DuplicateDetectionStatusLabel.Text == ResString.GetMultilingualString("8EC5ECD4-57D1-4201-9808-53BDB8DD3EBD", "Excluded from De-duplication due to size limits");

			if (timeoutOrOutOfSizeLimitation && CanAccessMDMAdminPanel)
			{
				DuplicateDetectionStatusLabel.ForeColor = Color.Purple;
				CreateAndShowAdminPanelWithDefault(AdministrationPanelForm.DeduplicationTargetType.Organization, new FilterBusinessObjectDefaults { new FilterBusinessObjectDefault("Code", "Property", Organisation.OH_Code) });
			}
			else if (DuplicateDetectionStatusLabel.Text == GetTextAndColor(DuplicationConditions.ShowDuplicatesFound).Text)
			{
				var header = (OrgHeader)((ZForm)ParentForm).BusinessEntity;
				header.FindDuplicates();
			}
#if WINZOR
			// Web will not trigger the mouse leave event if a new element cover it,
			// so DuplicateDetectionStatusLabel still keeps the style as mouse entered (have a underline and a Hand cursor) after click,
			// thus we need change DuplicateDetectionStatusLabel's style in OnClick actively for Winzor
			SetHyperLinkMouseEnter(DuplicateDetectionStatusLabel, false);
#endif
		}

		bool CanAccessMDMAdminPanel => Env.Security.MdmAdministrationPanel.IsAllowed
			&& Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed
			&& SystemDataRegistry.Instance.MdmAdministrationPanelDuplicatesTabEnabled.Value
			&& SystemDataRegistry.Instance.MdmAdministrationPanelOrganizationDuplicatesTabEnabled.Value;

		protected void CreateAndShowAdminPanelWithDefault(AdministrationPanelForm.DeduplicationTargetType dedupeDefaultsTargetType, FilterBusinessObjectDefaults defaults)
		{
			var openForm = ZApplication.GetOpenForms().OfType<AdministrationPanelForm>().FirstOrDefault();
			if (openForm == null)
			{
				var form = new AdministrationPanelForm(new AdministrationPanelManager(new BusinessObjectFactory()), dedupeDefaultsTargetType, defaults);
				form.Show();
			}
			else
			{
				openForm.SwitchToDuplicatesTabWithDefaults(AdministrationPanelForm.DeduplicationTargetType.Organization, defaults);
			}
		}

		public void HideDeduplicationStatus()
		{
			DuplicateDetectionStatusLabel.Visible = false;
			DuplicateDetectionStatusIcon.Visible = false;
			DuplicateExclusionsLabel.Visible = false;
		}

		public bool DeduplicationStatusVisible
		{
			get { return DuplicateDetectionStatusLabel.Visible; }
		}

		public string DeduplicationStatusText
		{
			get { return DuplicateDetectionStatusLabel.Text; }
		}

		public void DuplicationDetected(object sender, IDuplicationEventArgs e)
		{
			ShowDuplications(e);

			if (e.Master is OrgHeader header)
			{
				header.AddOrUpdateDDRLogInfo(e);
			}
		}

		public void ShowDuplications(IDuplicationEventArgs e)
		{
			HideDeduplicationStatus();
			DeduplicationHelper.ShowDuplicateAlert(this, e);
		}

		public void DuplicationStarted(object sender, EventArgs e)
		{
			ShowDeduplicationStatus();
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public void DuplicationEnded(object sender, IDuplicationEventArgs e)
		{
			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			var arg = (DuplicationEventArgs)e;

			if (arg.OrgHeaderExclusionManager != null)
			{
				ToolTipService.SetToolTip(DuplicateExclusionsLabel, arg.OrgHeaderExclusionManager.DisplayInfo);
			}

			if (!e.Results.IsNullOrEmpty())
			{
				ShowDuplicatesFound(arg);
			}
			else
			{
				//resultsmodels == null means that minimum requirements are not met and dedup hasn't been performed. if it's performed the object is not null but empty.
				// TODO: refactor deduplication flow so it's clear to see if dedup has been perfomed or not and why
				if (e.IsErrorOccurred)
				{
					ShowDeduplicationErrorOccurredMessage();
				}
				else if (e.IsTimeout)
				{
					ShowDeduplicationTimeoutMessage();
				}
				else if (e.Results == null && e.ResultsModels == null && e.TargetObjects == null)
				{
					currentDuplicationEventArgs = arg;
					ShowExcludedDuplicationMessage();
				}
				else if (e.ResultsModels == null)
				{
					ShowNotEnoughInformation();
				}
				else
				{
					ShowNoDuplicatesFound(e);
				}

				DeduplicationHelper.CloseExistingDuplicateAlert();
			}
		}

		public void DeduplicationActionOccurred(object sender, IDuplicationEventArgs e)
		{
			if (DuplicateDetectionStatusLabel.Visible && e.InvokedAction != DeduplicationAction.None)
			{
				HideDeduplicationStatus();
			}
		}

		public DuplicateAlertControlHelper DeduplicationHelper => duplicateAlertControlHelper ?? (duplicateAlertControlHelper = new DuplicateAlertControlHelper());

		DuplicateAlertControlHelper duplicateAlertControlHelper;

		public void HookDuplicationDetectEvents()
		{
			Organisation.DuplicationDetected += DuplicationDetected;
			Organisation.DeduplicationStarted += DuplicationStarted;
			Organisation.DeduplicationEnded += DuplicationEnded;
			Organisation.DeduplicationActionOccurred += DeduplicationActionOccurred;
		}

		public void UnHookDuplicationDetectEvents()
		{
			Organisation.DuplicationDetected -= DuplicationDetected;
			Organisation.DeduplicationStarted -= DuplicationStarted;
			Organisation.DeduplicationEnded -= DuplicationEnded;
			Organisation.DeduplicationActionOccurred -= DeduplicationActionOccurred;
		}

		#endregion

		void HookAddressChangedInGUI()
		{
			var address = Organisation.MainAddress;

			address.Address1Info.ValueChanged += AddressChanged_InGUI;
			address.Address2Info.ValueChanged += AddressChanged_InGUI;
			address.CityInfo.ValueChanged += AddressChanged_InGUI;
			address.PostcodeInfo.ValueChanged += AddressChanged_InGUI;
			address.StateCodeInfo.ValueChanged += AddressChanged_InGUI;
			address.OA_RN_NKCountryCodeInfo.ValueChanged += AddressChanged_InGUI;
		}

		void UnhookAddressChangedInGUI()
		{
			var address = Organisation.MainAddress;

			address.Address1Info.ValueChanged -= AddressChanged_InGUI;
			address.Address2Info.ValueChanged -= AddressChanged_InGUI;
			address.CityInfo.ValueChanged -= AddressChanged_InGUI;
			address.PostcodeInfo.ValueChanged -= AddressChanged_InGUI;
			address.StateCodeInfo.ValueChanged -= AddressChanged_InGUI;
			address.OA_RN_NKCountryCodeInfo.ValueChanged -= AddressChanged_InGUI;

			address.AddressValidationStatusChanged -= MainAddress_AddressValidationStatusChanged;
		}

		void AddressChanged_InGUI(object sender, EventArgs e)
		{
			if (Organisation != null)
			{
				Organisation.MainAddress.HasBeenChangedByUser = true;
			}
		}

		#region ProcessCmdKey
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ShouldValidateAddress() && ProcessManuallyVerifyShortCutKey(ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, ReadOnly, this, AddressForValidation))
			{
				return true;
			}
			return false;
		}
		#endregion

		void HookISupportWebAddressValidationControlChangeFocusEvents()
		{
			SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(this, ISupportWebAddressValidationControl_GotFocus, ISupportWebAddressValidationControl_LostFocus);
		}

		void ISupportWebAddressValidationControl_GotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, AddressForValidation, this, MaxWidth);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		void RegistrationNumberTypeDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (RegistrationNumberTypeDropEdit.CodeBox.Focused)
			{
				RegistrationNumberTypeDropEdit.CommitBoundValue();
				Organisation.RefreshBinding();
				Organisation.PrimaryRegistrationNumber.Validation.ValidateNumber();
			}
		}

		int MaxWidth
		{
			get
			{
				var location = ControlDpiScalingHelper.NewScaledPoint(ValidateAddressButton.Left + ValidateAddressButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), ValidateAddressButton.Top, false);
				return Parent.Width - location.X - ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
			}
		}

		#region Character Case on Org Fields

		void SetCharacterCasing()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				CharacterCasing requiredCasing = Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper;

				OH_FullNameBoundTextBox.CharacterCasing = requiredCasing;
				Address1BoundTextBox.CharacterCasing = requiredCasing;
				Address2BoundTextBox.CharacterCasing = requiredCasing;
				CityBoundTextBox.CharacterCasing = requiredCasing;
				StateBoundDropEdit.CharacterCasing = requiredCasing;

				EmailBoundTextBox.CharacterCasing = CharacterCasing.Normal;
				OH_WebBoundTextBox.CharacterCasing = CharacterCasing.Normal;
			}
		}

		#endregion

		#region Navigate To Web

		void SetupNavigateToWebButton()
		{
			GoToUrlButton.FlatStyle = FlatStyle.Standard;
			GoToUrlButton.BackgroundImage = Icons.GetImage(IconTypes.Globe20x16);
		}

		void GoToUrlButton_Click(object sender, EventArgs e)
		{
			OrgHeader header = (OrgHeader)((ZForm)ParentForm).BusinessEntity;
			if (header.MainWebURL.PU_URL != "" && !header.MainWebURL.PU_URLInfo.HasErrors())
			{
				NavigateToWeb(header.MainWebURL.PU_URL);
			}
		}

		protected virtual void NavigateToWeb(string webAddress)
		{
			WebUrlLauncher.Launch(webAddress);
		}

		#endregion

		#region IReadOnlyToggleControl Members

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				ScreenButton.ReadOnly = value;
				ValidateAddressButton.ReadOnly = value;
				ClearFieldsButton.ReadOnly = value;
			}
		}
		bool readOnly;

		#endregion

		#region Screen Button

		async void ScreenButton_Click(object sender, EventArgs e)
		{
			await new DeniedPartyScreeningPresentationManager().PerformScreening((ZForm)ParentForm, true, !DeniedPartyScreenerAsync.HasExcludedList(Organisation?.Factory));
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Organisation != null)
			{
				UnhookAddressChangedInGUI();
			}
		}

		bool firstLoad = true;

		protected override async void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Organisation != null)
			{
				HookAddressChangedInGUI();
				HookDuplicationDetectEvents();

				var isFirstLoad = false;
				if (firstLoad)
				{
					firstLoad = false;
					isFirstLoad = true;
				}

				AllBranchesLink.Visible = Organisation.CompanyDataCollection.Count > 1;

				Organisation.PrimaryRegistrationNumber.Validation.ValidateNumber();

				if (!Organisation.OH_IsActive)
				{
					ScreenButton.Visible = false;
					ScreenButtonCoverLabel.Visible = true;
				}

				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					if (ShouldValidate)
					{
						using (Organisation.MainAddress.SuspendSettingHasChanges())
						{
							await ValidateAddress(isFirstLoad);
						}
					}

					RefreshValidationStatus();

					//The form may have closed or the binding may have changed by the time the address has been validated, nothing to do if null.
					if (Organisation != null)
					{
						var mainAddress = Organisation.MainAddress;
						mainAddress.AddressValidationStatusChanged += MainAddress_AddressValidationStatusChanged;
						AddressSuggestionControlHelper.RegisterPropertyChangedEvent(mainAddress, ValidateAddressWithoutCheckIsFirstLoad);
						CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(mainAddress, GetCityTownAsync);

						ValidateAddressButton.ReadOnly = ClearFieldsButton.ReadOnly = mainAddress.OA_Address1Info.ReadOnly;
					}
				}
			}
		}

		bool ShouldValidate
		{
			get
			{
				var mainAddress = Organisation.MainAddress;
				return TopLevelControl is ZForm topLevelZForm && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly
					&& !string.IsNullOrEmpty(AddressForValidation.Address1)
					&& mainAddress.ValidationStatus != AddressValidationStatus.ManuallyVerified
					&& !mainAddress.OA_Address1Info.ReadOnly;
			}
		}

		OrgHeader Organisation
		{
			get { return (OrgHeader)CurrentDataItem; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetDefaultLanguage();
		}

		void SetDefaultLanguage()
		{
			if (Organisation != null && !Organisation.IsInDatabase && Organisation.MainAddress != null)
			{
				var language = Organisation.MainAddress.OA_Language;
				if (!language.IsEmpty)
				{
					Organisation.OH_Language = language;
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (cancellationToken != null)
				{
					cancellationToken.Cancel();
					cancellationToken.Dispose();
					cancellationToken = null;
				}
				if (Organisation != null)
				{
					UnhookAddressChangedInGUI();
					UnHookDuplicationDetectEvents();
				}

				DuplicateDetectionStatusLabel.Dispose();
				DuplicateDetectionStatusIcon.Dispose();
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		protected void AllBranchesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (Env.Security.OrganisationViewBranchesForAllCompanies.IsAllowed)
			{
				ZFormModaliser.Show(new ControllingBranchesForm(Organisation), ParentForm);
			}
			else
			{
				Env.Security.OrganisationViewBranchesForAllCompanies.ShowError();
			}
		}

		#region Address Validation

		protected void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			var modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities = Organisation.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(Organisation.MainAddress);
			if (modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities.Any())
			{
				Env.Security.ShowError(modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities);
			}
			else if (!((OrgHeader)CurrentDataItem).IsInDatabase || Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed)
			{
				SupportWebAddressValidationControlHelper.ClearFields(AddressForValidation, this);
			}
			else
			{
				Env.Security.OrgDetailsModifyNameAndAddress.ShowError();
			}
		}

		protected async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			if (Organisation == null)
			{
				return; //No validation required.
			}

			var modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities = Organisation.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(Organisation.MainAddress);
			if (modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities.Any())
			{
				Env.Security.ShowError(modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities);
			}
			else if (!Organisation.IsInDatabase || Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed)
			{
				ValidationJustForced = true;
				await ValidateAddress();

				//The form may have closed or the binding may have changed by the time the address has been validated, nothing to do if null.
				if (AddressForValidation != null)
				{
					if (AddressForValidation.ValidationStatus == AddressValidationStatus.Unverifiable || AddressForValidation.ValidationStatus == AddressValidationStatus.CountryNotAvailable)
					{
						AddressForValidation.ValidatePostcodeAndStateForAddress();
					}
				}
			}
			else
			{
				Env.Security.OrgDetailsModifyNameAndAddress.ShowError();
			}
		}

		async Task ValidateAddressWithoutCheckIsFirstLoad()
		{
			await ValidateAddress();
		}

		async Task ValidateAddress(bool isFirstLoad = false)
		{
			try
			{
				if (IsOnCurrentSelectedTab() && AddressValidationService.IsAddressNeedValidation(AddressForValidation))
				{
					CleanseAction cleanseAction;

					if (!ValidationJustForced)
					{
						AddressSuggestionControlHelper.CloseSuggestionForm(MainAddressDetailsGroupBox.Controls);
						cleanseAction = CleanseAction.QuickValidate;
					}
					else
					{
						cleanseAction = CleanseAction.ValidateAndSuggest;
						ValidationJustForced = false;
					}

					await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, AddressForValidation, this, this.ParentForm, MainAddressDetailsGroupBox.Controls, RefreshValidationStatus, MainAddressDetailsGroupBox.Invalidate, MaxWidth, cleanseAction, null, isFirstLoad);
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This is to help to fix Issue 01042023, please contact the ROPE team. The NullReferenceException was caught from ValidateAddressAsync().", e);
			}
		}

#if DEBUG
		public virtual
#endif
		bool IsOnCurrentSelectedTab()
		{
			var parentTabPage = Parent?.Parent?.Parent?.Parent?.Parent;
			if (parentTabPage != null && parentTabPage.Parent as ZTabControl != null)
			{
				return ((ZTabControl)parentTabPage.Parent).SelectedTab == parentTabPage;
			}
			return false;
		}

		async Task GetCityTownAsync()
		{
			if (IsOnCurrentSelectedTab() && (!string.IsNullOrEmpty(AddressForValidation.City) || !string.IsNullOrEmpty(AddressForValidation.Postcode)))
			{
				var maxWidth = Address1BoundTextBox.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, AddressForValidation, this, ParentForm, MainAddressDetailsGroupBox.Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		void MainAddress_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			if (AddressForValidation != null)
			{
				RefreshValidationStatus();
				if (AddressValidationService.IsAddressInValidStatus(AddressForValidation))
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
				}
			}
		}

		public void RefreshValidationStatus()
		{
			if (ShouldValidateAddress())
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, AddressForValidation.ValidationStatus, AddressForValidation.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1BoundTextBox, Address2BoundTextBox, CityBoundTextBox, PostCodeBoundTextBox, StateBoundDropEdit, CountryFindBox, AddressForValidation.ValidationStatus);
				MainAddressDetailsGroupBox.Invalidate();

				ValidateAddressButton.Visible = true;
				ClearFieldsButton.Visible = true;
			}
			else
			{
				AddressValidationUIHelper.ResetAddressFieldState(Address1BoundTextBox, Address2BoundTextBox, CityBoundTextBox, PostCodeBoundTextBox, StateBoundDropEdit, CountryFindBox);

				ValidateAddressButton.Visible = false;
				ClearFieldsButton.Visible = false;
			}
		}

		bool ShouldValidateAddress()
		{
			var countryCode = Organisation?.MainAddress?.Country;

			return
				countryCode != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(countryCode.PK.ToGuid(), Organisation.MainAddress.ValidationSection);
		}

		void AddressTextBox_TextChanged(object sender, EventArgs e)
		{
			if (SuggestionWindowParentControl != null)
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(SuggestionWindowParentControl.Controls);
			}
		}

		#region ISupportWebAddressValidationControl

		ZTextBox ISupportWebAddressValidationControl.CityControl
		{
			get { return CityBoundTextBox; }
		}

		ZTextBox ISupportWebAddressValidationControl.PostcodeControl
		{
			get { return PostCodeBoundTextBox; }
		}

		ZDropEdit ISupportWebAddressValidationControl.StateControl
		{
			get { return StateBoundDropEdit; }
		}

		CancellationTokenSource cancellationToken;

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ZTextBox Address1Control
		{
			get { return Address1BoundTextBox; }
		}

		public ZTextBox Address2Control
		{
			get { return Address2BoundTextBox; }
		}

		public ZCodeFindBox CountryControl
		{
			get { return CountryFindBox; }
		}

		public ZButton ValidateButton
		{
			get { return ValidateAddressButton; }
		}

		public bool ValidationJustForced { get; set; }

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return Organisation == null ? null : Organisation.MainAddress; }
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		public Control SuggestionWindowParentControl
		{
			get { return MainAddressDetailsGroupBox; }
		}

		#endregion

		#region ISupportDuplicationAlertControl

		public Point DuplicationAlertAnchorLocation => Point.Empty;

		public Control DuplicationAlertParentControl => MainAddressDetailsGroupBox;

		public Control DuplicationAlertReferenceControl => OH_FullNameBoundTextBox;
		#endregion

		#endregion
	}
}
