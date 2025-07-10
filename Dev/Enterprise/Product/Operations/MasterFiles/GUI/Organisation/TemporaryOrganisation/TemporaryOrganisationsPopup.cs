using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressFormDesignerAnalysis]
	public partial class TemporaryOrganisationsPopup
		: ZChildForm, IFindBoxPopup, ISupportWebAddressValidationControl
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TemporaryOrganisationsPopup()
		{
			if (!DesignMode)
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object");
			}

			InitialiseTimer();
			InitializeComponent();
			InitialiseGridContextMenu();
		}

		void InitialiseGridContextMenu()
		{
			var menuItem = new ZMenuItem(ResString.GetMultilingualString("9F7FE978-7DC9-4F87-8FAB-3D41AF0A5925", "View Organization"));
			menuItem.Click += (o, e) => ShowOrganisation(SimilarOrgMatchesBoundGrid.GetCurrent() as IOrgPatternMatch);

			SimilarOrgMatchesBoundGrid.ContextMenu.MenuItems.Add(menuItem);
			SimilarOrgMatchesBoundGrid.ContextMenu.Popup += (o, e) => menuItem.Enabled = SimilarOrgMatchesBoundGrid.GetCurrent() is IOrgPatternMatch;
		}

		public TemporaryOrganisationsPopup(IOrgHeaderCollection orgCollection, IOrgHeader businessEntity)
			: base(businessEntity)
		{
			InitialiseTimer();
			InitializeComponent();
			InitialiseGridContextMenu();
			SetCharacterCasing();

			this.orgCollection = orgCollection;
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveButton, CancelBtn);

			if (!DesignModeFinder.IsDesigning)
			{
				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					cancellationToken = new System.Threading.CancellationTokenSource();
					HandleCreated += (o, e) =>
					{
						FormClosed += (x, y) =>
						{
							cancellationToken?.Cancel();
						};
					};

					HookISupportWebAddressValidationControlChangeFocusEvents();
					HookResizeEvent();
					RefreshValidationStatus();

					if (MainAddress != null)
					{
						MainAddress.IsTemporaryOrgAddress = true;
						MainAddress.AddressValidationStatusChanged += MainAddress_AddressValidationStatusChanged;
						AddressSuggestionControlHelper.RegisterPropertyChangedEvent(MainAddress, ValidateAddress);
						CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(MainAddress, GetCityTownAsync);
						ValidateAddressButton.ReadOnly = MainAddress.OA_Address1Info.ReadOnly;
					}
				}
				else
				{
					ValidateAddressButton.Visible = false;
				}

				SearchResultsStatusLabel.AllowOverlap(SimilarOrgMatchesBoundGrid);
			}

			ValidationJustForced = false;
		}

		void SetCharacterCasing()
		{
			NameControl.CharacterCasing = DefaultCasing;
			Address1Control.CharacterCasing = DefaultCasing;
			Address2Control.CharacterCasing = DefaultCasing;
			CityControl.CharacterCasing = DefaultCasing;
			StateControl.CharacterCasing = DefaultCasing;
			EmailControl.CharacterCasing = CharacterCasing.Normal;
		}

		CharacterCasing DefaultCasing
		{
			get { return Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper; }
		}

		public ZGuid SelectedGuid { get; private set; }
		internal OrganisationEmdeddedModulePopup ParentFindBoxPopUp;
		readonly IOrgHeaderCollection orgCollection;

		public new IOrgHeader BusinessEntity { get { return (IOrgHeader)base.BusinessEntity; } }
		ISimilarOrganisationsFinder SimilarOrgFinder { get { return BusinessEntity != null ? BusinessEntity.SimilarOrgFinder : null; } }

		#region Web Address Validation

		void AddressTextBox_TextChanged(object sender, EventArgs e)
		{
			AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
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

		async Task GetCityTownAsync()
		{
			if (!string.IsNullOrEmpty(AddressForValidation?.City) || !string.IsNullOrEmpty(AddressForValidation?.Postcode))
			{
				var maxWidth = Address1Control.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, AddressForValidation, this, this, Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		protected async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			if (AddressForValidation == null)
			{
				return; //No validation required.
			}

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

		async Task ValidateAddress()
		{
			try
			{
				if (AddressValidationService.IsAddressNeedValidation(AddressForValidation))
				{
					CleanseAction cleanseAction;

					if (!ValidationJustForced)
					{
						AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
						cleanseAction = CleanseAction.QuickValidate;
					}
					else
					{
						cleanseAction = CleanseAction.ValidateAndSuggest;
						ValidationJustForced = false;
					}

					await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, AddressForValidation, this, this, Controls, RefreshValidationStatus, Invalidate, MaxWidth, cleanseAction);
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This is to help to fix Issue 01042023, please contact the ROPE team. The NullReferenceException was caught from ValidateAddressAsync().", e);
			}
		}

		public void RefreshValidationStatus()
		{
			if (ShouldValidateAddress())
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, AddressForValidation.ValidationStatus, AddressForValidation.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, AddressForValidation.ValidationStatus);
				Invalidate();

				ValidateAddressButton.Visible = true;
			}
			else
			{
				AddressValidationUIHelper.ResetAddressFieldState(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl);

				ValidateAddressButton.Visible = false;
			}
		}

		void HookResizeEvent()
		{
			Resize += TemporaryOrganisationsPopupForm_Resize;
		}

		void TemporaryOrganisationsPopupForm_Resize(object sender, EventArgs e)
		{
			var suggestionControl = SupportWebAddressValidationControlHelper.FindAddressSuggestionControl(this);
			if (suggestionControl != null && suggestionControl.Visible)
			{
				SupportWebAddressValidationControlHelper.ShowAddressSuggestionControl(this, MaxWidth, 0, true);
			}
		}

		int MaxWidth
		{
			get
			{
				var location = ControlDpiScalingHelper.NewScaledPoint(ValidateAddressButton.Left + ValidateAddressButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), ValidateAddressButton.Top, false);
				return Width - location.X - ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
			}
		}

		System.Threading.CancellationTokenSource cancellationToken;

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

		public ZTextBox NameControl => NameTextBox;

		public ZTextBox EmailControl => EmailTextBox;

		public ZTextBox AddressCodeControl => null;

		public ZTextBox Address1Control => Address1TextBox;

		public ZTextBox Address2Control => Address2TextBox;

		public ZTextBox CityControl => CityTextBox;

		public ZTextBox PostcodeControl => PostCodeTextBox;

		public ZDropEdit StateControl => StateDropDownEdit;

		public ZCodeFindBox CountryControl => CountryFindBox;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ZButton ValidateButton => ValidateAddressButton;

		public bool ValidationJustForced { get; set; }

		public Control SuggestionWindowParentControl => this;

		OrgAddress mainAddress;
		public OrgAddress MainAddress => mainAddress ?? (mainAddress = BusinessEntity?.MainAddress as OrgAddress);
		public ISupportWebAddressValidation AddressForValidation => MainAddress;
		public ZButton ClearAddressFieldsButton { get; }
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

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
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, Address1TextBox.ReadOnly, this, AddressForValidation))
			{
				return true;
			}

			return false;
		}
		#endregion

		bool ShouldValidateAddress()
		{
			var countryCode = AddressForValidation?.Country;

			return
				countryCode != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(countryCode.PK.ToGuid(), MainAddress.ValidationSection);
		}

		#endregion

		protected virtual void ShowOrganisation(IOrgPatternMatch orgMatch)
		{
			if (orgMatch != null)
			{
				var factory = new BusinessObjectFactory();
				var org = factory.Load<OrgHeader>(orgMatch.OS_OH);

				if (org != null)
				{
					using (var form = new ZOrganisationsForm(org))
					{
						form.DisplayMode = ODisplayMode.ReadOnly;

						ZFormModaliser.ShowDialogWithoutDispose(form, this);
					}
				}
			}
		}

		#region Searching

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			NameTextBox.TextChanged += ControlTextChanged;
			Address1TextBox.TextChanged += ControlTextChanged;
			Address2TextBox.TextChanged += ControlTextChanged;
			CityTextBox.TextChanged += ControlTextChanged;
			StateDropDownEdit.CodeBox.TextChanged += ControlTextChanged;
			PostCodeTextBox.TextChanged += ControlTextChanged;
			PortFindBox.CodeBox.TextChanged += ControlTextChanged;
			PhoneTextBox.TextChanged += ControlTextChanged;
			FaxTextBox.TextChanged += ControlTextChanged;
			EmailTextBox.TextChanged += ControlTextChanged;
			BusinessRegNZTextBox.TextChanged += ControlTextChanged;
			ConsigneeCheckBox.CheckedChanged += ControlTextChanged;
			ConsignorCheckBox.CheckedChanged += ControlTextChanged;
			ReceivablesCheckBox.CheckedChanged += ControlTextChanged;
			PayablesCheckBox.CheckedChanged += ControlTextChanged;
		}

		protected override bool ProcessTabKeyCore(bool forward)
		{
			if (searchTimer.Enabled)
			{
				searchTimer.Stop();
				searchTimer.Start();
			}

			return base.ProcessTabKeyCore(forward);
		}

		void InitialiseTimer()
		{
			searchTimer.Interval = SearchTimerInterval;
			searchTimer.Enabled = false;
			searchTimer.Tick += SearchTimer_Tick;
		}

		readonly Timer searchTimer = new Timer();

		const int SearchTimerInterval = 1500;

		void ControlTextChanged(object sender, EventArgs e)
		{
			if (!IsSearching)
			{
				ResetTimer();
			}
		}

		protected virtual void ResetTimer()
		{
			searchTimer.Stop();
			searchTimer.Start();
		}

		protected void SearchTimer_Tick(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				try
				{
					IsSearching = true;

					searchTimer.Stop();
					PushValuesToBusinessObjectAndPreserveTextboxState();
					SimilarOrgFinder.ForceRegeneratePatternMatch();
					SimilarOrgFinder.FindSimilarOrganisations(false);

					if (SimilarOrgFinder.HasTooManyMatches)
					{
						SearchResultsStatusLabel.Text = Res.GetString("6fd02d03-9bd6-4dd7-bee4-f0f262b8d534", "Too many matches were found, please enter more information.");
						SimilarOrganisationsLabel.Text = Res.GetString("92dad02d-94dd-41a4-b882-2332a9903e6b", "Similar Organizations ({0} found):", SimilarOrgFinder.SimilarOrganisationMatchesFound);
						SearchResultsStatusLabel.Visible = true;
					}
					else if (SimilarOrgFinder.SimilarOrganisationMatchesShown == 0)
					{
						SearchResultsStatusLabel.Text = Res.GetString("64daae8b-b6a1-48d4-9bca-5f9b37d8a49c", "None found.");
						SimilarOrganisationsLabel.Text = Res.GetString("257a4fb3-a181-4c62-9fd0-41ee7f6219fa", "Similar Organizations:");
						SearchResultsStatusLabel.Visible = true;
					}
					else
					{
						SimilarOrganisationsLabel.Text = Res.GetString("dbe3c4a9-0ed3-40d4-8a48-62be76d1742d", "Similar Organizations (Top {0} of {1}):", SimilarOrgFinder.SimilarOrganisationMatchesShown, SimilarOrgFinder.SimilarOrganisationMatchesFound);
						SearchResultsStatusLabel.Visible = false;
					}
				}
				finally
				{
					IsSearching = false;
				}
			}
		}

		bool IsSearching;

		TextBox GetActiveTextBox()
		{
			var result = ActiveControl;
			while (result is IContainerControl)
			{
				result = ((IContainerControl)result).ActiveControl;
			}

			return result as TextBox;
		}

		void PushValuesToBusinessObjectAndPreserveTextboxState()
		{
			var activeTextBox = GetActiveTextBox();
			int currentSelectionStart = 0, currentSelectionLength = 0;
			var currentControlText = string.Empty;

			if (activeTextBox != null)
			{
				currentSelectionStart = activeTextBox.SelectionStart;
				currentSelectionLength = activeTextBox.SelectionLength;
				currentControlText = activeTextBox.Text;
			}

			var manager = ((CurrencyManager)BindingContext[BusinessEntity, ""]);
			manager.EndCurrentEdit();
			manager.Refresh();

			if (activeTextBox != null)
			{
				var selectionStart = Math.Min(currentSelectionStart, currentControlText.Length);
				var selectionLength = Math.Min(currentSelectionLength + selectionStart, currentControlText.Length) - selectionStart;

				activeTextBox.Text = currentControlText;
				activeTextBox.SelectionStart = selectionStart;
				activeTextBox.SelectionLength = selectionLength;
			}
		}

		#endregion

		#region Form Closing & Selection Accepting

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//Overridden to prevent the "You have made changes" when closing the form by selecting an existing organisation
			if (!ExistingOrganisationAccepted)
			{
				base.ZForm_Closing(sender, e);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Enter)
			{
				if (SimilarOrgMatchesBoundGrid.Focused && (SimilarOrgMatchesBoundGrid.SelectedElements.Length == 1))
				{
					e.Handled = true;
					AcceptSelection();
				}
				else
				{
					SearchTimer_Tick(this, EventArgs.Empty);
				}
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			// If the searchtimer has been enabled by a textchange event, and the user clicks save before
			// the timer runs, then the list of similar orgmatches may be wrong.
			if (searchTimer.Enabled)
			{
				SearchTimer_Tick(this, EventArgs.Empty);
			}

			if (SimilarOrgFinder.LikelyMatchFound)
			{
				var ignoreSimilarOrgs = Globals.Message.Show(Res.GetString("e4ae50c7-b3e8-4c4a-a051-5a04818b6b47", "Very similar organizations were found. Are you sure you wish to save a new organization?"), Res.GetString("365b73db-31ad-4a07-ba71-e4e1424ab914", "Similar Organizations Found"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				return (ignoreSimilarOrgs == DialogResult.Yes) ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			return base.ShowPreSaveDialogs();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			if (result == ContinueWithSave.Yes)
			{
				SelectedOrganisation = BusinessEntity as ICodeDescription;
				var org = (BusinessObject)BusinessEntity;

				if (!orgCollection.Contains(org))
				{
					if (MainAddress != null)
					{
						MainAddress.IsTemporaryOrgAddress = false;
					}

					orgCollection.Add(org);
				}
				CommitOrgToFindBox();
			}

			return result;
		}

		ICodeDescription SelectedOrganisation;

		void OKButton_Click(object sender, EventArgs e)
		{
			if (SimilarOrgMatchesBoundGrid.SelectedElements.Length == 1)
			{
				AcceptSelection();
			}
		}

		void SimilarOrgMatchesBoundGrid_DoubleClick(object sender, EventArgs e)
		{
			if (SimilarOrgMatchesBoundGrid.SelectedElements.Length == 1)
			{
				AcceptSelection();
			}
		}

		void AcceptSelection()
		{
			ExistingOrganisationAccepted = true;
			SelectedOrganisation = SimilarOrgMatchesBoundGrid.SelectedElements[0];
			CommitOrgToFindBox();
		}

		bool ExistingOrganisationAccepted;

		protected virtual void CommitOrgToFindBox()
		{
			if (SelectedOrganisation != null)
			{
				FindBox.Code = SelectedOrganisation.Code;
				FindBox.Description = SelectedOrganisation.Description;
			}

			Close();

			if (ParentFindBoxPopUp != null)
			{
				// If this form was shown by clicking the "Temporary Account" button on the findbox popup, close that form on 
				// accepting the selection as well.
				ParentFindBoxPopUp.Close();
			}
		}

		#endregion

		#region IFindBoxPopup Members

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.FindBox = findBox;
			ZFormModaliser.Show(this, parentForm);
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		IFindBox FindBox;

		void IFindBoxPopup.SelectRowByPK(ZGuid pK)
		{
		}

		#endregion

		#region Security

		void BusinessEntity_OnSecurityAccessDenied(object sender, EventArgs e)
		{
			Globals.Message.ShowError(Res.GetString("c10bd587-6a59-4c27-8993-bea2c184f5db", "You don't have the security permissions to change this value. Please see your system administrator if you require access."));
		}

		#endregion

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			UnsubscribeHandlers();
			base.SetDataBinding(dataSource, dataMember);
			SubsribeHandlers();
		}

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				UnsubscribeHandlers();

				searchTimer?.Dispose();
				components?.Dispose();

				if (cancellationToken != null)
				{
					cancellationToken.Cancel();
					cancellationToken.Dispose();
					cancellationToken = null;
				}
			}

			base.Dispose(isDisposing);
		}

		void SubsribeHandlers()
		{
			var similarOrgFinder = SimilarOrgFinder;
			if (similarOrgFinder != null)
			{
				similarOrgFinder.OnSecurityAccessDenied += BusinessEntity_OnSecurityAccessDenied;
			}
		}

		void UnsubscribeHandlers()
		{
			var similarOrgFinder = SimilarOrgFinder;
			if (similarOrgFinder != null)
			{
				similarOrgFinder.OnSecurityAccessDenied -= BusinessEntity_OnSecurityAccessDenied;
			}
		}

		#endregion
	}
}
