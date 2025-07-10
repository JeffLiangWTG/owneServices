using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AddressDetailsControlWithLanguage : ZUserControl, ISupportWebAddressValidationControl, IReadOnlyToggleControl
	{
		public AddressDetailsControlWithLanguage()
		{
			InitializeComponent();

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
				}
			}

			ValidationJustForced = false;
		}

		#region ISupportWebAddressValidationControl

		public ZTextBox Address1Control
		{
			get { return Address1TextBox; }
		}

		public ZTextBox Address2Control
		{
			get { return Address2TextBox; }
		}

		public ZTextBox CityControl
		{
			get { return CityTextBox; }
		}

		public ZCodeFindBox CountryControl
		{
			get { return CountryFindBox; }
		}

		public ZTextBox PostcodeControl
		{
			get { return PostcodeTextBox; }
		}

		public ZDropEdit StateControl
		{
			get { return StateDropEdit; }
		}

		public Control SuggestionWindowParentControl
		{
			get { return ParentForm; }
		}

		public ZButton ValidateButton
		{
			get { return ValidateAddressButton; }
		}

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public bool ValidationJustForced { get; set; }

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return Address; }
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		internal bool LanguageControlReadonly
		{
			get { return LanguageDropEdit.ReadOnly; }
			set { LanguageDropEdit.ReadOnly = value; }
		}

		#endregion

		#region Address Validation

		internal ISupportWebAddressValidation Address
		{
			get { return address; }
			set
			{
				if (address != value)
				{
					address = value;
					RefreshValidationStatus();
					HookValidationStatusChangeEvent();
				}
			}
		}
		ISupportWebAddressValidation address;

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
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, this.GetReadOnly(), this, Address))
			{
				return true;
			}
			return false;
		}
		#endregion

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(Address, this);
		}

		async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			ValidationJustForced = true;
			await ValidateAddress();
		}

		async Task ValidateAddress()
		{
			// due to race conditions, we need a non-getter provided reference
			var parentForm = ParentForm;

			try
			{
				if (AddressValidationService.IsAddressNeedValidation(Address) && parentForm != null)
				{
					CleanseAction cleanseAction;

					if (!ValidationJustForced)
					{
						AddressSuggestionControlHelper.CloseSuggestionForm(parentForm.Controls);
						cleanseAction = CleanseAction.QuickValidate;
					}
					else
					{
						cleanseAction = CleanseAction.ValidateAndSuggest;
						ValidationJustForced = false;
					}

					await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, Address, this, parentForm,
						parentForm.Controls, RefreshValidationStatus, null, 0, cleanseAction);
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00176409, please contact the ROPE team. The NullReferenceException was caught from ValidateAddress().", e);
			}
		}

		protected async Task GetCityTownAsync()
		{
			if (Address != null && Address1TextBox != null && ParentForm != null && (!string.IsNullOrEmpty(Address.City) || !string.IsNullOrEmpty(Address.Postcode)))
			{
				var maxWidth = Address1TextBox.Width;
				ValidationJustForced = true;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, Address, this, ParentForm, ParentForm.Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		void HookValidationStatusChangeEvent()
		{
			if (!DesignModeFinder.IsDesigning && Env.Instance.Registry.EnableAddressValidationWebService)
			{
				if (Address != null)
				{
					Address.AddressValidationStatusChanged += CurrentAddress_AddressValidationStatusChanged;
					CurrentAddress_AddressValidationStatusChanged(null, EventArgs.Empty);
					AddressSuggestionControlHelper.RegisterPropertyChangedEvent(Address, ValidateAddress);
					CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(Address, GetCityTownAsync);
				}
			}
		}

		void RefreshValidationStatus()
		{
			if (Address != null)
			{
				if (ShouldValidateAddress())
				{
					AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, Address.ValidationStatus, Address.IsErrorSuppressed);
					AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryFindBox, Address.ValidationStatus);

					ValidateAddressButton.Visible = true;
					ClearFieldsButton.Visible = true;
				}
				else
				{
					AddressValidationUIHelper.ResetAddressFieldState(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryFindBox);

					ValidateAddressButton.Visible = false;
					ClearFieldsButton.Visible = false;
				}
			}
		}

		void CurrentAddress_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			RefreshValidationStatus();
			if (AddressValidationService.IsAddressInValidStatus(Address))
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
			}
		}

		bool ShouldValidateAddress()
		{
			var orgAddress = Address as OrgAddress;

			var countryPK = orgAddress != null && orgAddress.Country != null
				? orgAddress.Country.PK.ToGuid()
				: Guid.Empty;

			return OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(countryPK, orgAddress?.ValidationSection ?? AddressValidationSection.OrganizationAddress);
		}

		#endregion

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

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (cancellationToken != null)
			{
				cancellationToken.Cancel();
				cancellationToken.Dispose();
				cancellationToken = null;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
