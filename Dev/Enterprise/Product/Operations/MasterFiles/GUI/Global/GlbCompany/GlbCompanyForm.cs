using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCompanyForm : ZForm, ISupportWebAddressValidationControl
	{
		public GlbCompanyForm(GlbCompany company)
			: base(company)
		{
			InitializeComponent();
			this.Company = company;
			migrateReciprocalExchangeRates = !company.IsInDatabase;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);

			company.GC_RN_NKCountryCodeInfo.ValueChanged += GC_RN_NKCountryCodeInfo_ValueChanged;
			company.GC_IsReciprocalInfo.ValueChanged += GC_IsReciprocalInfo_ValueChanged;
			company.GC_RX_NKLocalCurrencyInfo.ValueChanged += GC_RX_NKLocalCurrencyInfo_ValueChanged;
			company.Branches.OnAttemptedToDeleteCurrentBranch += new EventHandler(OnAttemptedToDeleteCurrentBranch);
			company.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(CompanyFactory_Saved);
			company.GC_CodeInfo.ValueChanged += GC_CodeInfo_ValueChanged;

			if (!DesignModeFinder.IsDesigning)
			{
				AddCredentialsTabPage();
			}

			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			if (!DesignModeFinder.IsDesigning)
			{
				GlbBranchModuleButtonGrid.InnerGrid.DataSourceChanged += new EventHandler(GlbBranchModuleButtonGrid_DataSourceChanged);

				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					cancellationToken = new CancellationTokenSource();
					this.HandleCreated += (o, e) =>
					{
						if (this.ParentForm != null)
						{
							this.ParentForm.FormClosed += (x, y) =>
							{
								if (cancellationToken != null)
								{
									cancellationToken.Cancel();
								}
							};
						}
					};
					HookISupportWebAddressValidationControlChangeFocusEvents();
				}
				else
				{
					ValidateAddressButton.Visible = false;
					ClearFieldsButton.Visible = false;
				}
				var provider = Company?.CustomsNumberProvider;
				NumberRangesTabPage.TabVisible = provider != null;
				provider?.SetupRelatedDataAndNotification();
			}
			ValidationJustForced = false;

			if (!Env.Security.BranchModify.IsAllowed)
			{
				foreach (Control control in BranchesGroupBox.Controls)
				{
					control.SetReadOnly(true);
				}
			}

			if (!ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().IsCompanyLevelTaxSystemConfigured(Company))
			{
				if (taxConfigurationTabPage != null)
				{
					AccConfigTabControl.Controls.Remove(taxConfigurationTabPage);
					taxConfigurationTabPage.Dispose();
				}
			}

			if (!Env.Security.CompaniesModifySurchargeConfiguration.IsAllowed)
			{
				foreach (Control control in accSurchargeConfiguration.Controls)
				{
					control.SetReadOnly(true);
				}
				foreach (Control control in accSurchargeBasis.Controls)
				{
					control.SetReadOnly(true);
				}
			}

			if (!Env.Security.CompaniesModifySurchargeApplication.IsAllowed)
			{
				foreach (Control control in accSurchargeApplicationGrid.Controls)
				{
					control.SetReadOnly(true);
				}
			}

			if (!DesignModeFinder.IsDesigning)
			{
				EInvoiceCredentialsForTurkeyTabPage.TabVisible = GlbCompanySignatureCredentialCollection.IsEditAllowedForCompany(Company);
				EInvoiceCredentialsForHungaryTabPage.TabVisible = GlbCompanyExternalPasswordHUI.IsAllowed(Company);
				EInvoiceCredentialsForPhilippinesTabPage.TabVisible = GlbCompanyExternalPasswordForPhilippines.IsAllowed(Company);
				EInvoiceOAuthAuthorizationTabPage.TabVisible = IsEInvoiceCredentialsTabPageVisible();
				EInvoiceCertificatesTabPage.TabVisible = IsEInvoiceCertificatesTabPageVisible();
				PlaceOfSupplyConfigurationTabPage.TabVisible = PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(Company);
				ARInvTemplateTabPage.TabVisible = Company.IsTemplateFileConfigurationsEnabled;
				CashAdvanceTabPage.TabVisible = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled;
			}
		}

		readonly GlbCompany Company;
		bool migrateReciprocalExchangeRates;

		#region ISupportWebAddressValidationControl
		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ZTextBox Address1Control
		{
			get
			{
				return GC_Address1TextBox;
			}
		}

		public ZTextBox Address2Control
		{
			get
			{
				return GC_Address2TextBox;
			}
		}

		public ZTextBox CityControl
		{
			get
			{
				return GC_CityTextBox;
			}
		}

		public ZTextBox PostcodeControl
		{
			get
			{
				return GC_PostCodeTextBox;
			}
		}

		public ZDropEdit StateControl
		{
			get
			{
				return GC_StateDropEdit;
			}
		}

		public ZCodeFindBox CountryControl
		{
			get
			{
				return GC_RN_NKCountryCodeCodeFindBox;
			}
		}

		public ZButton ValidateButton
		{
			get
			{
				return ValidateAddressButton;
			}
		}

		public bool ValidationJustForced { get; set; }

		public Control SuggestionWindowParentControl
		{
			get
			{
				return CompanyInfoTabPage;
			}
		}

		void RefreshValidationStatus()
		{
			if (Address != null)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, Address.ValidationStatus, Address.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, Address.ValidationStatus);
				CompanyInfoTabPage.Invalidate();
			}
		}

		async void ISupportWebAddressValidationControlInitialise()
		{
			if (Env.Instance.Registry.EnableAddressValidationWebService && Address != null)
			{
				Company.PreValidationForAddressValidationService();
				var topLevelZForm = this.TopLevelControl as ZForm;
				if (topLevelZForm != null && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly && !string.IsNullOrEmpty(Address.Address1) && Address.ValidationStatus != AddressValidationStatus.ManuallyVerified && !Address.Address1Info.ReadOnly)
				{
					using (Company.SuspendSettingHasChanges())
					{
						await ValidateAddress();
					}
				}

				RefreshValidationStatus();

				//The form may have closed or the binding may have changed by the time the address has been validated, nothing to do if null.
				if (Address != null)
				{
					Address.AddressValidationStatusChanged += Address_AddressValidationStatusChanged;
					AddressSuggestionControlHelper.RegisterPropertyChangedEvent(Address, ValidateAddress);
					CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(Address, GetCityTownAsync);

					ValidateAddressButton.ReadOnly = ClearFieldsButton.ReadOnly = Address.Address1Info.ReadOnly;
				}
			}
		}

		void Address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			if (Address != null)
			{
				RefreshValidationStatus();
				if (AddressValidationService.IsAddressInValidStatus(Address))
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
				}
			}
		}

		async Task ValidateAddress()
		{
			if (AddressValidationService.IsAddressNeedValidation(Address))
			{
				CleanseAction cleanseAction;

				if (!ValidationJustForced)
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(SuggestionWindowParentControl.Controls);
					cleanseAction = CleanseAction.QuickValidate;
				}
				else
				{
					cleanseAction = CleanseAction.ValidateAndSuggest;
					ValidationJustForced = false;
				}

				await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, Address, this, this, SuggestionWindowParentControl.Controls, RefreshValidationStatus, null, 0, cleanseAction);
			}
		}

		async Task GetCityTownAsync()
		{
			if ((!string.IsNullOrEmpty(Address.City) || !string.IsNullOrEmpty(Address.Postcode)))
			{
				var maxWidth = StateControl.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, Address, this, this, SuggestionWindowParentControl.Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		CancellationTokenSource cancellationToken;

		void HookISupportWebAddressValidationControlChangeFocusEvents()
		{
			SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(this, ISupportWebAddressValidationControl_GotFocus, ISupportWebAddressValidationControl_LostFocus);
		}

		void ISupportWebAddressValidationControl_GotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, Address, this);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			ValidationJustForced = true;
			await ValidateAddress();
		}

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(Address, this);
		}

		public ISupportWebAddressValidation Address
		{
			get
			{
				return CurrentDataItem as ISupportWebAddressValidation;
			}
		}

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return Company; }
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;

		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		#endregion

		#region Implementation

		IEInvoiceCredentialsProvider eInvoiceCredentialsProvider
			=> ObjectFactory.Get<ICountryComplianceFactory>()?.GetICountryComplianceInfoBase(Company.GC_RN_NKCountryCode) as IEInvoiceCredentialsProvider;

		bool IsEInvoiceCredentialsTabPageVisible()
			=> eInvoiceCredentialsProvider?.ShouldShowCredentialsTab(Company) ?? false;

		bool IsEInvoiceCertificatesTabPageVisible()
			=> eInvoiceCredentialsProvider?.ShouldShowCertificatesTab(Company) ?? Company.EInvoicingCertificateCredentials.IsEditAllowed;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var boundCompany = (GlbCompany)BusinessEntity;
			if (boundCompany != null)
			{
				boundCompany.GC_IsActiveInfo.ValueChanged -= OnCompanyIsActiveChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			boundCompany = (GlbCompany)BusinessEntity;
			if (boundCompany != null)
			{
				boundCompany.GC_IsActiveInfo.ValueChanged += OnCompanyIsActiveChanged;
			}
		}

		void NewOrgProxyButton_Click(object sender, EventArgs e)
		{
			var newFactory = new BusinessObjectFactory();
			var org = Company.GetNewOrgProxy(newFactory);

			using (var form = new ZOrganisationsForm(org))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form, this);

				if (org.IsInDatabase)
				{
					Company.GC_OH_OrgProxy = org.PK;
					foreach (var branch in Company.Branches)
					{
						branch.GB_OH_OrgProxy = org.PK;
					}
				}
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var continueWithSave = ContinueWithSave.Yes;

			string confirmationMessage;
			if (Company.HasGSTRegisteredChangedToUnregistered)
			{
				if (Company.GSTRateIsAlreadyUsedByTransactions)
				{
					confirmationMessage = Res.GetString("69b38c14-af42-4d9b-a240-cdb34c7ed702", "You have changed the {0} Registration status of this company. There are already transactions with {0} posted and these transactions should be canceled and re-posted. Are you sure you want to do this?", Company.ConsumptionTaxDescriptionForCompanyForm);
				}
				else
				{
					confirmationMessage = Res.GetString("311e00ed-0387-4704-885a-1dfa8f7d939c", "You have changed the {0} Registration status of this company, which can have dire consequences if it was not intended. Are you sure you want to do this?", Company.ConsumptionTaxDescriptionForCompanyForm);
				}

				DialogResult result = Globals.Message.ShowConfirmation(confirmationMessage, Res.GetString("a2fdde16-2468-4976-b5f6-f75d18d1fcad", "Continue To Save?"), Res.GetString("532fad25-bb64-4264-bcc9-e438c2fa39de", "To continue, type:") + " ", Res.GetString("58898c08-2a8f-48ef-9100-717f82d6f368", "Yes"), MessageBoxIcon.Question);
				continueWithSave = (result == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No);
			}

			if (continueWithSave == ContinueWithSave.Yes)
			{
				continueWithSave = base.ShowPreSaveDialogs();
			}

			return continueWithSave;
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			if (((BusinessObject)BusinessEntity).PK == GlbCompany.CurrentCompany.PK)
			{
				Globals.Message.ShowError(Res.GetString("acad01b3-e732-4b79-8c63-f2b0eeafeb70", "Cannot delete Company. You are currently logged in to this Company."));
				return ContinueWithDelete.No;
			}

			return base.ShowPreDeleteDialogs();
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			var allFactories = new ArrayList(factories);
			if (migrateReciprocalExchangeRates)
			{
				var reciprocalExchangeRatesMigrationTransaction = new ReciprocalExchangeRatesMigrationTransactionParticipant(Company);
				allFactories.Add(reciprocalExchangeRatesMigrationTransaction);
			}
			base.Save((ITransactionParticipant[])allFactories.ToArray(typeof(ITransactionParticipant)));
		}

		#region Event Handlers

		void GC_RN_NKCountryCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupCountryRelatedControls();
		}

		void GlbCompanyForm_Load(object sender, EventArgs e)
		{
			if (!Company.IsInDatabase)
			{
				var prevCountryCode = Company.GC_RN_NKCountryCode;
				Company.AccountingCountry = "";
				Company.AccountingCountry = prevCountryCode;
			}

			SetupCountryRelatedControls();
			ISupportWebAddressValidationControlInitialise();
		}

		void CompanyFactory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				migrateReciprocalExchangeRates = false;
			}
		}

		void GC_RX_NKLocalCurrencyInfo_ValueChanged(object sender, EventArgs e)
		{
			Company.Validation.ValidateGC_RX_NKLocalCurrency();
			if (Company != null && Company.IsInDatabase && !Company.GC_RX_NKLocalCurrencyInfo.HasErrors() && Company.GC_RX_NKLocalCurrencyInfo.HasChanges)
			{
				var warningText = Res.GetString("2af34609-3be4-4058-b923-e2b3d6146078", "You have changed the local currency of this company, which can have dire consequences if it was not intended. Are you sure you want to do this?");
				DialogResult result = Globals.Message.Show(warningText, Res.GetString("9d349f95-8df7-4cf0-b8a4-7acf4ac2190e", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result != DialogResult.Yes)
				{
					Company.GC_RX_NKLocalCurrency = (ZString)Company.GC_RX_NKLocalCurrencyInfo.OriginalValue;
					this.GC_RX_NKLocalCurrencyCodeFindBox.CodeBox.Text = Company.GC_RX_NKLocalCurrency;
				}
			}
		}

		void GC_CodeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!Company.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("28d7548d-ffcd-484a-9a30-f0761a024abf", "Company code cannot be changed once saved, except by support. Please ensure the code is a suitable value to identify this company."),
					Res.GetString("72d4eda9-4122-44cc-a373-27fe17da28a5", "Confirm Company Code"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		void OnAttemptedToDeleteCurrentBranch(object sender, EventArgs args)
		{
			Globals.Message.Show(Res.GetString("8e4b57a8-a9c4-4a27-9f4d-bf8a461eceb8", "Cannot delete Branch. You are currently logged in to this Branch!"),
				Res.GetString("ec7d39dc-51c2-4932-bff9-74f9507e1bd4", "Cannot Delete Current Branch"), MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		void SetupCountryRelatedControls()
		{
			GC_BusinessRegNoTextBox.GetExtension<LabelCaptionRenderer>().Caption = Company.BusinessRegistrationNumberOneCaption;
			GC_BusinessRegNo2TextBox.GetExtension<LabelCaptionRenderer>().Caption = Company.BusinessRegistrationNumberTwoCaption;

			var isGSTRegisteredCaption = Res.GetString("f5e4c3b9-7981-40dd-b671-5de5621dd9b7", "{0} Registered", Company.ConsumptionTaxDescriptionForCompanyForm);
			GC_IsGSTRegisteredCheckBox.GetExtension<LabelCaptionRenderer>().Caption = isGSTRegisteredCaption;
			GC_IsGSTRegisteredCheckBox.GetExtension<HintExtension>().Caption = isGSTRegisteredCaption;
			GC_IsGSTRegisteredCheckBox.GetExtension<HintExtension>().Description = isGSTRegisteredCaption;

			var isGSTCashBasisCaption = Res.GetString("d1eff870-c72d-46f0-937f-d241814820a5", "Cash Basis {0} Enabled", Company.ConsumptionTaxDescriptionForCompanyForm);
			GC_IsGSTCashBasisCheckBox.GetExtension<LabelCaptionRenderer>().Caption = isGSTCashBasisCaption;
			GC_IsGSTCashBasisCheckBox.GetExtension<HintExtension>().Caption = isGSTCashBasisCaption;
			GC_IsGSTCashBasisCheckBox.GetExtension<HintExtension>().Description = Res.GetString("051a000e-00fa-4090-91d1-a1d6ef74cbcd", "By default {0} is posted and reported on an Accruals (Invoice) Basis.\r\nWhen this check box is ticked, additional Cash (Payment) Basis {0} posting and reporting features will be enabled for the Login Company.", Company.ConsumptionTaxDescriptionForCompanyForm);

			GC_StateDropEdit.Visible = Company.StateListHasMembers;
			GC_StateTextBox.Visible = !Company.StateListHasMembers;
			var numberRangesVisible = Company.CustomsNumberProvider != null;
			var numberRangesTabPageTabVisible = NumberRangesTabPage.TabVisible;
			if (numberRangesTabPageTabVisible)
			{
				if (numberRangesVisible)
				{
					customsNumberViewStmNumsTabPageUserControl.LoadOrReloadUserControl();
				}
				else
				{
					NumberRangesTabPage.TabVisible = false;
				}
			}
			else
			{
				if (numberRangesVisible)
				{
					NumberRangesTabPage.TabVisible = true;
				}
			}
		}

		void GC_IsReciprocalInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Company != null && Company.IsInDatabase)
			{
				if (Company.GC_IsReciprocalInfo.HasChanges)
				{
					string reciprocalText = string.Empty;
					string decimalPlacesText = string.Empty;

					if (Company.GC_IsReciprocal)
					{
						reciprocalText = Res.GetString("435cfad9-736c-4853-ac5d-cc45a6ae31a3", "RECIPROCAL");
						decimalPlacesText = Res.GetString("2f23ad8a-1b01-4bf8-8f91-d2f483887a79", "six");
					}
					else
					{
						reciprocalText = Res.GetString("d97dc0ed-14cc-4be3-bf86-fe22a3ad7c8d", "NON RECIPROCAL");
						decimalPlacesText = Res.GetString("32364f24-5b67-46f6-b551-f683ed8ff794", "four");
					}

					string warningText = Res.GetString("27f79215-aa2c-4a84-92a3-327961ea58ad",
@"Changing 'Is Reciprocal' will change the way EXCHANGE RATES are recorded and used in this company.

This will flag this company as {0}.

All Exchange Rates (excluding Customs Entry) will be inverted and converted to {1} decimal places.

Do you want to continue?", reciprocalText, decimalPlacesText);

					DialogResult result = Globals.Message.Show(warningText, Res.GetString("9d349f95-8df7-4cf0-b8a4-7acf4ac2190e", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
					{
						migrateReciprocalExchangeRates = true;
					}
					else
					{
						Company.GC_IsReciprocal = !Company.GC_IsReciprocal;
					}
				}
				else
				{
					migrateReciprocalExchangeRates = false;
				}
			}
		}

		void OnCompanyIsActiveChanged(object sender, EventArgs e)
		{
			if (Company == null || !Company.IsInDatabase)
			{
				return;
			}

			// Check that it comes from actual value change and not from RefreshBinding
			if (e is ValueChangedEventArgs valueChangedEventArgs && !valueChangedEventArgs.NewValue.Equals(valueChangedEventArgs.OldValue))
			{
				if (!Company.GC_IsActive)
				{
					if (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.Value)
					{
						PopUpDeactivateCompanyMessageBox();
					}
					else
					{
						SwitchCompanyBranch();
					}
				}
				else
				{
					if (companyBranchSwitcher != null) // Do not create new switcher if it is null - no need to load unchanged data
					{
						companyBranchSwitcher.CancelBranchChanges();
					}

					if (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.Value && Company.HasInactiveBranch)
					{
						ZFormModaliser.ShowDialogAndDispose(new ReactivateBranchesOrAddressesForm(new ReactivateBranchOrAddressModel(Company.Branches)));
					}
				}
			}
		}

		void PopUpDeactivateCompanyMessageBox()
		{
			if (Company.HasActiveBranch)
			{
				using (var messageBox = new DeactivateCompanyMessageBox())
				{
					var result = ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
					if (result != DialogResult.Yes)
					{
						Company.GC_IsActiveInfo.ValueChanged -= OnCompanyIsActiveChanged;
						Company.GC_IsActive = true;
						Company.GC_IsActiveInfo.ValueChanged += OnCompanyIsActiveChanged;
					}
					else
					{
						SwitchCompanyBranch();
					}
				}
			}
		}

		protected void SwitchCompanyBranch()
		{
			if (!inBranchDeactivation)
			{
				inBranchDeactivation = true;
				try
				{
					var switcher = CompanyBranchSwitcher.HasChanges ? new BranchSwitcherBusinessObject(Company) : CompanyBranchSwitcher;
					if (switcher.HasActiveSchedules || switcher.HasActiveStaff)
					{
						Company.GC_IsActiveInfo.ValueChanged -= OnCompanyIsActiveChanged;
						Company.GC_IsActive = true;
						Company.GC_IsActiveInfo.ValueChanged += OnCompanyIsActiveChanged;

						switcher.RefreshValidationOnBranches();
						ZFormModaliser.ShowDialogAndDispose(new ChangingServiceTaskBranchForm(switcher), this);
						Company.GC_IsActiveInfo.RefreshBinding();
					}
					else if (Company.Branches?.Any() ?? false)
					{
						foreach (var branch in Company.Branches)
						{
							branch.GB_IsActive = false;
							branch.GB_IsActiveInfo.RefreshBinding();
						}
					}
				}
				finally
				{
					inBranchDeactivation = false;
				}
			}
		}
		bool inBranchDeactivation;

		BranchSwitcherBusinessObject CompanyBranchSwitcher => companyBranchSwitcher ?? (companyBranchSwitcher = new BranchSwitcherBusinessObject(Company));
		BranchSwitcherBusinessObject companyBranchSwitcher;

		#endregion

		#region GlbBranchModuleButtonGrid

		void GlbBranchModuleButtonGrid_DataSourceChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated && !IsDisposed)
			{
				BeginInvoke(new MethodInvoker(HookIsActiveColumn));
			}
		}

		void HookIsActiveColumn()
		{
			var isActiveColumn = GlbBranchModuleButtonGrid.InnerGrid.Columns[GlbBranchSchema.GB_IsActive.Name];
			if (isActiveColumn != null)
			{
				((CheckBox)((ZCheckBoxColumnStyle)isActiveColumn.ColumnStyle).EditControl).CheckedChanged -= OnBranchIsActiveChanged;
				((CheckBox)((ZCheckBoxColumnStyle)isActiveColumn.ColumnStyle).EditControl).CheckedChanged += OnBranchIsActiveChanged;
			}
		}

		Dictionary<ZGuid, BranchSwitcherBusinessObject> BranchSwitchers => branchSwitchers ?? (branchSwitchers = new Dictionary<ZGuid, BranchSwitcherBusinessObject>());
		Dictionary<ZGuid, BranchSwitcherBusinessObject> branchSwitchers;

		bool inBranchDeactivationForGrid;

		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "It is disposed by the ZFormModaliser")]
		public void OnBranchIsActiveChanged(object sender, EventArgs e)
		{
			var branch = (GlbBranch)GlbBranchModuleButtonGrid.InnerGrid.GetCurrent();

			if (branch == null || !branch.IsInDatabase)
			{
				return;
			}

			BranchSwitcherBusinessObject branchSwitcher;
			if (!BranchSwitchers.TryGetValue(branch.PK, out branchSwitcher) && !branch.GB_IsActive)
			{
				branchSwitcher = new BranchSwitcherBusinessObject(branch);
				BranchSwitchers.Add(branch.PK, branchSwitcher);
			}

			if (!branch.GB_IsActive)
			{
				if (!inBranchDeactivationForGrid)
				{
					inBranchDeactivationForGrid = true;
					try
					{
						if (branchSwitcher.HasActiveSchedules || branchSwitcher.HasActiveStaff)
						{
							branchSwitcher.RefreshValidationOnBranches();
							ZFormModaliser.ShowDialogAndDispose(new ChangingServiceTaskBranchForm(branchSwitcher), this);
							branch.GB_IsActiveInfo.RefreshBinding();
						}
					}
					finally
					{
						inBranchDeactivationForGrid = false;
					}
				}
			}
			else
			{
				if (branchSwitcher != null) // Do not create new switcher if it is null - no need to load unchanged data
				{
					branchSwitcher.CancelBranchChanges();
				}
			}
		}

		#endregion

		#region ProcessCmdKey
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ProcessManuallyVerifyShortCutKey(ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, this.GetReadOnly(), this, Address))
			{
				return true;
			}
			return false;
		}
		#endregion

		void AddCredentialsTabPage()
		{
			CompanyTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.CompanyCredentialsPlugIn, CompanyTabControl.TabPages.IndexOf(CompanyInfoTabPage) + 1);
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnsubscribeHandlers();

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
			}
			base.Dispose(disposing);
		}

		void UnsubscribeHandlers()
		{
			Company.GC_RN_NKCountryCodeInfo.ValueChanged -= GC_RN_NKCountryCodeInfo_ValueChanged;
			Company.GC_IsReciprocalInfo.ValueChanged -= GC_IsReciprocalInfo_ValueChanged;
			Company.GC_RX_NKLocalCurrencyInfo.ValueChanged -= GC_RX_NKLocalCurrencyInfo_ValueChanged;
			Company.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(CompanyFactory_Saved);
			Company.Branches.OnAttemptedToDeleteCurrentBranch -= new EventHandler(OnAttemptedToDeleteCurrentBranch);
			Company.GC_CodeInfo.ValueChanged -= GC_CodeInfo_ValueChanged;

			if (!Company.IsDeleted)
			{
				Company.CustomsNumberProvider?.ClearRelatedDataAndNotification();
			}
		}

		#endregion
	}
}
