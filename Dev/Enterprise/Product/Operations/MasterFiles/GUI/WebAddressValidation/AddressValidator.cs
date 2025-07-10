using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public class AddressValidator
	{
		#region Fields

		readonly ISupportWebAddressValidationControl control;
		readonly Control parentControl;
		readonly CityTownSuggestionControlUtil cityTownSuggestionControlUtil;
		private protected readonly AddressValidatorConfiguration config;

		#endregion Fields

		#region Constructor

		public AddressValidator(Control parentControl, AddressValidatorConfiguration config)
		{
			this.config = config;
			this.parentControl = parentControl;

			control = parentControl as ISupportWebAddressValidationControl ?? throw new ArgumentException(null, nameof(parentControl));

			config.WithSelectCityTownAction(ForcedValidateAddress);

			cityTownSuggestionControlUtil = new CityTownSuggestionControlUtil(parentControl, ParentForm, CancellationToken, config);

			HookAddressValidationEvents();
		}

		public AddressValidator(Control parentControl)
			: this(parentControl, new AddressValidatorConfiguration())
		{
		}

		#endregion Constructor

		#region Properties

		private protected Form ParentForm
		{
			get
			{
				if (fParentForm is null)
				{
					if (parentControl is Form controlItself)
					{
						if (control.SuggestionWindowParentControl is Form || control.SuggestionWindowParentControl is TabPage)
						{
							fParentForm = controlItself;
						}
					}
					else
					{
						if (control.SuggestionWindowParentControl is Form suggestionWindowParentControl)
						{
							fParentForm = suggestionWindowParentControl;
						}

						if (parentControl is ContainerControl containerControl)
						{
							fParentForm = containerControl.ParentForm;
						}
					}
				}

				return fParentForm;
			}
		}

		Form fParentForm;

		private protected bool ValidationForced { get; set; }

		AddressSuggestionControl AddressSuggestionControl => AddressSuggestionControlHelper.FindSuggestionForm(control.SuggestionWindowParentControl.Controls);

		CityTownSuggestionControl CityTownSuggestionControl => CityTownSuggestionControlHelper.FindSuggestionForm(control.SuggestionWindowParentControl.Controls);

		CancellationTokenSource CancellationToken { get; } = new CancellationTokenSource();

		#endregion Properties

		#region Button Click

		void ValidateAddressButtonClick()
		{
			PerformActionIfSecuritiesCheckPassForCurrentOrgAddress(() =>
			{
				ForcedValidateAddress();

				if (config.ButtonClickWithStateAndPostcodeRequired
					&& !(control.AddressForValidation is null)
					&& (control.AddressForValidation.ValidationStatus == AddressValidationStatus.Unverifiable
						|| control.AddressForValidation.ValidationStatus == AddressValidationStatus.CountryNotAvailable))
				{
					control.AddressForValidation.ValidatePostcodeAndStateForAddress();
				}
			});
		}

		void ClearFieldsButtonClick()
		{
			PerformActionIfSecuritiesCheckPassForCurrentOrgAddress(() =>
			{
				control.AddressForValidation.IsUpdatingCityTown = true;
				control.AddressForValidation.Postcode = ZString.Empty;
				control.AddressForValidation.City = ZString.Empty;
				control.AddressForValidation.StateCode = ZString.Empty;
				control.AddressForValidation.Address1 = ZString.Empty;
				control.AddressForValidation.Address2 = ZString.Empty;
				control.AddressForValidation.IsUpdatingCityTown = false;
				control.AddressForValidation.GeoLocation = ZGeography.Empty;

				AddressSuggestionControlHelper.CloseSuggestionForm(control.SuggestionWindowParentControl.Controls);
				CityTownSuggestionControlHelper.CloseSuggestionForm(control.SuggestionWindowParentControl.Controls);

				control.Address1Control.Select();
			});
		}

		#endregion Button Click

		#region Validate Address

		private protected virtual void ForcedValidateAddress()
		{
			NestedForcedValidateAddress();

			async void NestedForcedValidateAddress()
			{
				ValidationForced = true;
				await ValidateAddress();
			}
		}

		Task ValidateAddress() => config.ValidateAddress.Invoke(ValidateAddressCore).Invoke();

		private protected async Task ValidateAddressCore()
		{
			await Task.Run(() => throw new NotImplementedException());
		}

		#endregion Validate Address

		#region Get CityTown

		private protected async Task GetCityTownAsyncCore()
		{
			await cityTownSuggestionControlUtil.GetCityTownAsync();
		}

		#endregion Get CityTown

		#region OrgAddress Security Check

		void PerformActionIfSecuritiesCheckPassForCurrentOrgAddress(Action buttonClickAction)
		{
			if (control.AddressForValidation is null)
			{
				return;
			}

			if (config.CurrentOrgAddressGetter is null)
			{
				buttonClickAction.Invoke();
			}
			else
			{
				var buttonClickActionIfSecurityCheckPass = ButtonClickValidIfSecurityCheckAction(buttonClickAction);

				var address = config.CurrentOrgAddressGetter.Invoke();
				var orgHeader = address?.Header;

				if (orgHeader is null)
				{
					return;
				}

				var modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities = orgHeader.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(address);
				if (modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities.Any())
				{
					Env.Security.ShowError(modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities);
				}
				else if (address.IsMainAddress)
				{
					buttonClickActionIfSecurityCheckPass.Invoke(Env.Security.OrgDetailsModifyNameAndAddress);
				}
				else if (address.IsInDatabase)
				{
					buttonClickActionIfSecurityCheckPass.Invoke(Env.Security.OrgAddressDetailsModify);
				}
				else
				{
					buttonClickActionIfSecurityCheckPass.Invoke(Env.Security.OrgAddressDetailsNew);
				}
			}
		}

		Action<SecurityCheckpoint> ButtonClickValidIfSecurityCheckAction(Action buttonClickAction)
		{
			return (SecurityCheckpoint checkpoint) =>
			{
				if (checkpoint.IsAllowed)
				{
					buttonClickAction.Invoke();
				}
				else
				{
					checkpoint.ShowError();
				}
			};
		}

		#endregion OrgAddress Security Check

		#region Got/Lost Focus

		void HookFocusChangedEvents()
		{
			control.Address1Control.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.Address2Control.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.CityControl.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.PostcodeControl.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.CountryControl.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.CountryControl.CodeBox.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.StateControl.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.StateControl.CodeBox.GotFocus += ShowSuggestionControlsUponGotFocus;
			control.ValidateButton.GotFocus += ShowSuggestionControlsUponGotFocus;

			control.Address1Control.LostFocus += HideSuggestionControlsUponLostFocus;
			control.Address2Control.LostFocus += HideSuggestionControlsUponLostFocus;
			control.CityControl.LostFocus += HideSuggestionControlsUponLostFocus;
			control.PostcodeControl.LostFocus += HideSuggestionControlsUponLostFocus;
			control.CountryControl.LostFocus += HideSuggestionControlsUponLostFocus;
			control.CountryControl.CodeBox.LostFocus += HideSuggestionControlsUponLostFocus;
			control.StateControl.LostFocus += HideSuggestionControlsUponLostFocus;
			control.StateControl.CodeBox.LostFocus += HideSuggestionControlsUponLostFocus;
			control.ValidateButton.LostFocus += HideSuggestionControlsUponLostFocus;
		}

		void ShowSuggestionControlsUponGotFocus(object sender, EventArgs e)
		{
			try
			{
				if (control.AddressForValidation is null)
				{
					return;
				}

				var addressUnverified = control.AddressForValidation.ValidationStatus != AddressValidationStatus.Verified &&
					control.AddressForValidation.ValidationStatus != AddressValidationStatus.VerifiedToStreet &&
					control.AddressForValidation.ValidationStatus != AddressValidationStatus.ManuallyVerified;

				if (addressUnverified)
				{
					var addressSuggestionControl = AddressSuggestionControl;
					ShowAddressSuggestionControl(addressSuggestionControl, isResizeControl: false);

					var addressControlsGotFocused = sender == control.Address1Control || sender == control.Address2Control;

					if ((addressSuggestionControl is null || !addressSuggestionControl.Visible) && !addressControlsGotFocused)
					{
						CityTownSuggestionControl?.Show();
					}
				}
			}
			catch (NullReferenceException ex)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00169865, please contact the MDM team. The NullReferenceException was caught from ShowSuggestionControlsUponGotFocus().", ex);
			}
		}

		void HideSuggestionControlsUponLostFocus(object sender, EventArgs e)
		{
			var cityTownFieldsFocused = control.CityControl.Focused ||
				control.PostcodeControl.Focused ||
				control.StateControl.Focused ||
				control.StateControl.CodeBox.Focused ||
				control.CountryControl.Focused ||
				control.CountryControl.CodeBox.Focused;

			var cityTownSuggestionControl = CityTownSuggestionControl;

			if (cityTownSuggestionControl != null &&
				!cityTownFieldsFocused &&
				!cityTownSuggestionControl.Bounds.Contains(control.SuggestionWindowParentControl.PointToClient(Control.MousePosition)))
			{
				cityTownSuggestionControl.Hide();
			}

			var addressFieldsFocused = cityTownFieldsFocused ||
				control.Address1Control.Focused ||
				control.Address2Control.Focused;

			var addressSuggestionControl = AddressSuggestionControl;

			if (addressSuggestionControl != null &&
				!addressFieldsFocused &&
				!addressSuggestionControl.Bounds.Contains(control.SuggestionWindowParentControl.PointToClient(Control.MousePosition)))
			{
				addressSuggestionControl.Hide();
			}
		}

		#endregion Got/Lost Focus

		#region RefreshValidationStatus

		protected void RefreshValidationStatus()
		{
			if (config.OverrideRefreshValidationStatus != null)
			{
				config.OverrideRefreshValidationStatus();
			}
			else
			{
				if (config.ShouldValidate?.Invoke() ?? true)
				{
					var validationStatus = config.ValidationStatus?.Invoke() ?? control.AddressForValidation?.ValidationStatus;
					AddressValidationUIHelper.SetButtonValidationStatus(control.ValidateButton, validationStatus, control.AddressForValidation != null && control.AddressForValidation.IsErrorSuppressed);
					AddressValidationUIHelper.SetAddressFieldValidationStatus(control.Address1Control, control.Address2Control, control.CityControl, control.PostcodeControl, control.StateControl, control.CountryControl, validationStatus);
					control.ValidateButton.Visible = true;
					if (control.ClearAddressFieldsButton != null)
					{
						control.ClearAddressFieldsButton.Visible = true;
					}

					config.RunWhenRefreshValidationStatus_ShouldValidate?.Invoke();
				}
				else
				{
					AddressValidationUIHelper.ResetAddressFieldState(control.Address1Control, control.Address2Control, control.CityControl, control.PostcodeControl, control.StateControl, control.CountryControl);
					control.ValidateButton.Visible = false;
					if (control.ClearAddressFieldsButton != null)
					{
						control.ClearAddressFieldsButton.Visible = false;
					}

					config.RunWhenRefreshValidationStatus_ShouldNotValidate?.Invoke();
				}
			}
		}

		#endregion

		#region AddressValidationStatusChanged

		protected void HookAddressValidationStatusChangedEvent()
		{
			if (control?.AddressForValidation != null)
			{
				control.AddressForValidation.AddressValidationStatusChanged += AddressForValidation_AddressValidationStatusChanged;
			}
		}

		protected void UnhookAddressValidationStatusChangedEvent()
		{
			if (control?.AddressForValidation != null)
			{
				control.AddressForValidation.AddressValidationStatusChanged -= AddressForValidation_AddressValidationStatusChanged;
			}
		}

		void AddressForValidation_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			if (control?.AddressForValidation != null)
			{
				RefreshValidationStatus();

				if (AddressValidationService.IsAddressInValidStatus(control.AddressForValidation))
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(parentControl.Controls);
				}
			}
		}

		#endregion

		#region Implementation

		void HookAddressValidationEvents()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					HookFocusChangedEvents();
				}
			}

			control.ValidateButton.Click += (object sender, EventArgs e) => config.ValidateButtonClick.Invoke(ValidateAddressButtonClick);

			if (control.ClearAddressFieldsButton != null)
			{
				control.ClearAddressFieldsButton.Click += (object sender, EventArgs e) => ClearFieldsButtonClick();
			}

			if (config.ShouldHookControlResize)
			{
				parentControl.Resize += (object sender, EventArgs e) => ShowAddressSuggestionControl(AddressSuggestionControl, isResizeControl: true);
			}
		}

		void ShowAddressSuggestionControl(AddressSuggestionControl addressSuggestionControl, bool isResizeControl)
		{
			if (addressSuggestionControl != null && addressSuggestionControl.Visible == isResizeControl)
			{
				addressSuggestionControl.UpdateSizeAndLocation(control.SuggestionWindowParentControl.FindForm(),
					control.SuggestionWindowParentControl,
					control.ValidateButton,
					config.MaxWidthGetter.Invoke(),
					config.MaxHeightGetter.Invoke());
				addressSuggestionControl.Show();
			}
		}

		#endregion Implementation
	}
}
