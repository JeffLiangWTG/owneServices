using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CombineTranslatedAddressUserControl : ZUserControl, IReadOnlyToggleControl, ISupportWebAddressValidationControl
	{
		public CombineTranslatedAddressUserControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetCharacterCasing();

				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					ValidateAddressButton.Visible = ValidateAddressButtonVisibility;
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
					ValidateAddressButton.ReadOnly = !Env.Security.OrgAddressModify.IsAllowed;
				}
				else
				{
					ValidateAddressButton.Visible = false;
				}
			}
			ValidationJustForced = false;
		}
		#region Properties
		#region IReadOnlyToggleControl Members
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				ValidateAddressButton.ReadOnly = value;
			}
		}
		bool readOnly;

		#endregion

		OrgAddress fOrgAddress;
		internal OrgAddress CurrentAddress
		{
			get
			{
				if (fOrgAddress == null)
				{
					fOrgAddress = this.CurrentDataItem as OrgAddress;
				}
				return fOrgAddress;
			}
		}

		public bool ValidateAddressButtonVisibility { get; set; }
		#endregion

		#region GUI Setup

		void SetCharacterCasing()
		{
			OA_AdditionalAddressInformationBoundTextBox.CharacterCasing = RequiredCasing;
			OA_Address1BoundTextBox.CharacterCasing = RequiredCasing;
			OA_Address2BoundTextBox.CharacterCasing = RequiredCasing;
			OA_CityBoundTextBox.CharacterCasing = RequiredCasing;
			OA_StateBoundDropEdit.CharacterCasing = RequiredCasing;
			OA_CompanyNameOverrideBoundTextBox.CharacterCasing = RequiredCasing;
		}

		CharacterCasing RequiredCasing
		{
			get { return Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper; }
		}

		#endregion

		#region Address Validation

		CancellationTokenSource cancellationToken;

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
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, AddressForValidation, this);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			if (CurrentAddress.IsMainAddress)
			{
				if (Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed)
				{
					ValidationJustForced = true;
					await ValidateAddress();
				}
				else
				{
					Env.Security.OrgDetailsModifyNameAndAddress.ShowError();
				}
			}
			else
			{
				if (CurrentAddress.IsInDatabase)
				{
					if (Env.Security.OrgAddressDetailsModify.IsAllowed)
					{
						ValidationJustForced = true;
						await ValidateAddress();
					}
					else
					{
						Env.Security.OrgAddressDetailsModify.ShowError();
					}
				}
				else
				{
					if (Env.Security.OrgAddressDetailsNew.IsAllowed)
					{
						ValidationJustForced = true;
						await ValidateAddress();
					}
					else
					{
						Env.Security.OrgAddressDetailsNew.ShowError();
					}
				}
			}
		}

		async Task ValidateAddress()
		{
			if (IsOnCurrentSelectedTab() && AddressValidationService.IsAddressNeedValidation(AddressForValidation))
			{
				CleanseAction cleanseAction;

				if (!ValidationJustForced)
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(ParentForm.Controls);
					cleanseAction = CleanseAction.QuickValidate;
				}
				else
				{
					cleanseAction = CleanseAction.ValidateAndSuggest;
					ValidationJustForced = false;
				}

				await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, AddressForValidation, this, ParentForm, ParentForm.Controls, RefreshValidationStatus, null, 0, cleanseAction);
			}
		}

		bool IsOnCurrentSelectedTab()
		{
			if (this.Parent != null && this.Parent.Parent as ZTabControl != null)
			{
				return ((ZTabControl)this.Parent.Parent).SelectedTab == this.Parent;
			}
			return false;
		}

#if DEBUG
		internal
#endif
	void RefreshValidationStatus()
		{
			if (ShouldValidateAddress())
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, AddressForValidation.ValidationStatus, AddressForValidation.IsErrorSuppressed);
				ValidateAddressButton.ReadOnly = CurrentAddress.ReadOnly;
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryFindBox, AddressForValidation.ValidationStatus);
				AddressDetailsPanel.Invalidate();

				ValidateAddressButton.Visible = true;
			}
			else
			{
				AddressValidationUIHelper.ResetAddressFieldState(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryFindBox);

				ValidateAddressButton.Visible = false;
			}
		}

		void AddressForValidation_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			RefreshValidationStatus();

			if (AddressValidationService.IsAddressInValidStatus(AddressForValidation))
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
			}
		}

		bool ShouldValidateAddress()
		{
			return
				CurrentAddress != null &&
				CurrentAddress.Country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(CurrentAddress.Country.PK.ToGuid(), CurrentAddress.ValidationSection);
		}

		public ZTextBox Address1Control => OA_Address1BoundTextBox;

		public ZTextBox Address2Control => OA_Address2BoundTextBox;

		public ZTextBox CityControl => OA_CityBoundTextBox;

		public ZTextBox PostcodeControl => OA_PostCodeBoundTextBox;

		public ZDropEdit StateControl => OA_StateBoundDropEdit;

		public ZCodeFindBox CountryControl => CountryFindBox;

		public ZButton ValidateButton => ValidateAddressButton;

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public bool ValidationJustForced { get; set; }

		public Control SuggestionWindowParentControl
		{
			get { return ParentForm; }
		}
		public ISupportWebAddressValidation AddressForValidation
		{
			get
			{
				if (CurrentAddress != null)
				{
					return CurrentAddress.SelectedTranslatedAddress;
				}
				return null;
			}
		}

		public ZButton ClearAddressFieldsButton { get; }
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		#endregion
	}
}
