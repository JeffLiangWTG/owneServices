using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbBranchForm : ZForm, ISupportWebAddressValidationControl
	{
		public GlbBranchForm(GlbBranch branch) : base(branch)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			this.Branch = branch;

			if (branch.Lookups.AccountingGroupCodes.Count > 0 || !branch.GB_AccountingGroupCode.IsEmpty)
			{
				this.GB_AccountingGroupCodeZDropEdit.Visible = true;
			}

			if (!DesignModeFinder.IsDesigning)
			{
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
			}

			if (EInvoicingCredentialTaxCoreTabPage != null)
			{
				EInvoicingCredentialTaxCoreTabPage.TabVisible = branch.CertificateCredentialsTaxCore.IsEditAllowed;
			}
			if (BranchCredentialIndiaTabPage != null)
			{
				BranchCredentialIndiaTabPage.TabVisible = GlbBranchCredentialsForIndia.IsAllowed(branch);
			}
			if (EInvoicingCredentialTabPage != null)
			{
				EInvoicingCredentialTabPage.TabVisible = branch.EInvoicingPasswordCredentials.IsEditAllowed || branch.EInvoicingCertificateCredentials.IsEditAllowed;
				EInvoicingCertificateCredentialUserControl.Visible = branch.EInvoicingCertificateCredentials.CredentialSettings.IsCertificate();
			}

			if (!ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().IsBranchLevelTaxSystemConfigured(Branch))
			{
				if (taxConfigurationTabPage != null)
				{
					taxConfigurationTabPage.Controls.Remove(taxConfigurationTabPage);
					taxConfigurationTabPage.Dispose();
				}
			}

			ValidationJustForced = false;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();
		}

#if DEBUG
		public EInvoicingCertificateUserControl CertificateCredentialControl_TestOnly => EInvoicingCertificateCredentialUserControl;
		public ZTemplateTabControl BranchTabControl_TestOnly => BranchTabControl;
#endif

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ISupportWebAddressValidationControlInitialise();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var boundBranch = (GlbBranch)BusinessEntity;
			if (boundBranch != null)
			{
				boundBranch.GB_IsActiveInfo.ValueChanged -= OnBranchIsActiveChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			boundBranch = (GlbBranch)BusinessEntity;
			if (boundBranch != null)
			{
				boundBranch.GB_IsActiveInfo.ValueChanged += OnBranchIsActiveChanged;
			}
		}

		void OnBranchIsActiveChanged(object sender, EventArgs e)
		{
			if (Branch == null || !Branch.IsInDatabase)
			{
				return;
			}

			// Check that it comes from actual value change and not from RefreshBinding
			if (e is ValueChangedEventArgs valueChangedEventArgs && !valueChangedEventArgs.NewValue.Equals(valueChangedEventArgs.OldValue))
			{
				if (!Branch.GB_IsActive)
				{
					if (!inBranchDeactivation)
					{
						inBranchDeactivation = true;
						try
						{
							var switcher = BranchSwitcher;
							if (switcher.HasActiveSchedules || switcher.HasActiveStaff)
							{
								switcher.RefreshValidationOnBranches();
								ZFormModaliser.ShowDialogAndDispose(new ChangingServiceTaskBranchForm(switcher), this);
								Branch.GB_IsActiveInfo.RefreshBinding();
							}
						}
						finally
						{
							inBranchDeactivation = false;
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
		}
		bool inBranchDeactivation;

		BranchSwitcherBusinessObject BranchSwitcher => branchSwitcher ?? (branchSwitcher = new BranchSwitcherBusinessObject(Branch));
		BranchSwitcherBusinessObject branchSwitcher;

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			if (Branch.PK == GlbBranch.CurrentBranch.PK)
			{
				Globals.Message.ShowError(Res.GetString("50057530-e3b1-4c85-8b34-713a38a48184", "Cannot delete Branch. You are currently logged in to this Branch!"));
				return ContinueWithDelete.No;
			}
			return base.ShowPreDeleteDialogs();
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

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

		#endregion

		#region ISupportWebAddressValidationControl

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ZTextBox Address1Control
		{
			get
			{
				return GB_Address1BoundTextEdit;
			}
		}

		public ZTextBox Address2Control
		{
			get
			{
				return GB_Address2BoundTextEdit;
			}
		}

		public ZTextBox CityControl
		{
			get
			{
				return GB_CityBoundTextEdit;
			}
		}

		public ZTextBox PostcodeControl
		{
			get
			{
				return GB_PostCodeBoundTextEdit;
			}
		}

		public ZDropEdit StateControl
		{
			get
			{
				return GB_StateBoundDropEdit;
			}
		}

		public ZCodeFindBox CountryControl
		{
			get
			{
				return GB_CountryFindBox;
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

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return Branch; }
		}

		public Control SuggestionWindowParentControl
		{
			get
			{
				return BranchTabPage;
			}
		}

		void RefreshValidationStatus()
		{
			if (Address != null)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, Address.ValidationStatus, Address.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, Address.ValidationStatus);
				BranchTabPage.Invalidate();
			}
		}

		async void ISupportWebAddressValidationControlInitialise()
		{
			if (Env.Instance.Registry.EnableAddressValidationWebService && Address != null)
			{
				Branch.PreValidationForAddressValidationService();
				var topLevelZForm = this.TopLevelControl as ZForm;
				if (topLevelZForm != null && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly && !string.IsNullOrEmpty(Address.Address1) && Address.ValidationStatus != AddressValidationStatus.ManuallyVerified && !Address.Address1Info.ReadOnly)
				{
					using (Branch.SuspendSettingHasChanges())
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

		#endregion

		#region Dispose

		IContainer components;
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
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
