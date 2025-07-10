using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public static class SupportWebAddressValidationControlHelper
	{
		public static bool ProcessCommandKey(Keys keyData, bool isControlReadOnly, ISupportWebAddressValidationControl supportWebAddressValidationControl, ISupportWebAddressValidation addressForValidation)
		{
			if (addressForValidation != null)
			{
				if (keyData == Keys.F4 && !isControlReadOnly)
				{
					if (supportWebAddressValidationControl.Address1Control.Focused ||
						supportWebAddressValidationControl.Address2Control.Focused ||
						supportWebAddressValidationControl.CityControl.Focused ||
						supportWebAddressValidationControl.StateControl.Focused || supportWebAddressValidationControl.StateControl.CodeBox.Focused ||
						supportWebAddressValidationControl.PostcodeControl.Focused ||
						supportWebAddressValidationControl.CountryControl.Focused || supportWebAddressValidationControl.CountryControl.CodeBox.Focused ||
						IsFocused(supportWebAddressValidationControl.AddressCodeControl) ||
						IsFocused(supportWebAddressValidationControl.AdditionalAddressInformationControl))
					{
						ForceCommitCurrentChanges(supportWebAddressValidationControl, addressForValidation);
						return true;
					}
				}
				else if (keyData == (Keys.Control | Keys.M) && !isControlReadOnly)
				{
					if (Env.Security.OrgAddressesAllowedManualVerification.IsAllowed)
					{
						addressForValidation.IsValidatingAddress = true;
						ForceCommitCurrentChanges(supportWebAddressValidationControl, addressForValidation);
						addressForValidation.IsValidatingAddress = false;
						addressForValidation.ValidationStatus = AddressValidationStatus.ManuallyVerified;
						AddressSuggestionControlHelper.CloseSuggestionForm(supportWebAddressValidationControl.SuggestionWindowParentControl.Controls);
						CityTownSuggestionControlHelper.CloseSuggestionForm(supportWebAddressValidationControl.SuggestionWindowParentControl.Controls);
						addressForValidation.ValidatePostcodeAndStateForAddress();
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("9132b33a-18ad-4780-98e3-261a44de9391", @"You do not have the appropriate security rights to manually verify an address.

If you require access to manually verify an address, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", Env.Security.OrgAddressesAllowedManualVerification.DisplayTextPathToSecurityRight), ResString.GetMultilingualString("dce9a287-e7cb-48a6-9b77-a0b838bc17ed", "Access Denied: {0}", Env.Security.OrgAddressesAllowedManualVerification.DisplayText));
					}
					return true;
				}
			}
			return false;
		}

		static void ForceCommitCurrentChanges(ISupportWebAddressValidationControl supportWebAddressValidationControl, ISupportWebAddressValidation addressForValidation)
		{
			// Force changes to commit so that current values are used when F4 pressed
			// Only commit the focused control so we don't fire unintended events
			if (supportWebAddressValidationControl.Address1Control.Focused)
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.Address1 = supportWebAddressValidationControl.Address1Control.Text;
			}
			else if (supportWebAddressValidationControl.Address2Control.Focused)
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.Address2 = supportWebAddressValidationControl.Address2Control.Text;
			}
			else if (supportWebAddressValidationControl.CityControl.Focused)
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.City = supportWebAddressValidationControl.CityControl.Text;
			}
			else if (supportWebAddressValidationControl.StateControl.Focused || supportWebAddressValidationControl.StateControl.CodeBox.Focused)
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.State = supportWebAddressValidationControl.StateControl.CodeBox.Text;
			}
			else if (supportWebAddressValidationControl.PostcodeControl.Focused)
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.Postcode = supportWebAddressValidationControl.PostcodeControl.Text;
			}
			else if (supportWebAddressValidationControl.CountryControl.CodeBox.Focused)
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.CountryCodeISO2 = supportWebAddressValidationControl.CountryControl.CurrentCode;
			}
			else if (IsFocused(supportWebAddressValidationControl.AddressCodeControl))
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.AddressCode = supportWebAddressValidationControl.AddressCodeControl.Text;
			}
			else if (IsFocused(supportWebAddressValidationControl.AdditionalAddressInformationControl))
			{
				supportWebAddressValidationControl.ValidationJustForced = true;
				addressForValidation.UnrestrictedAdditionalAddressInformation = supportWebAddressValidationControl.AdditionalAddressInformationControl.Text;
			}
		}

		static bool IsFocused(Control control)
		{
			return control != null && control.Focused;
		}

		public static void ClearFields(ISupportWebAddressValidation addressForValidation, ISupportWebAddressValidationControl supportWebAddressValidationControl)
		{
			addressForValidation.IsUpdatingCityTown = true;
			addressForValidation.Postcode = ZString.Empty;
			addressForValidation.City = ZString.Empty;
			addressForValidation.StateCode = ZString.Empty;
			addressForValidation.Address1 = ZString.Empty;
			addressForValidation.Address2 = ZString.Empty;
			addressForValidation.IsUpdatingCityTown = false;
			addressForValidation.GeoLocation = ZGeography.Empty;
			AddressSuggestionControlHelper.CloseSuggestionForm(supportWebAddressValidationControl.SuggestionWindowParentControl.Controls);
			CityTownSuggestionControlHelper.CloseSuggestionForm(supportWebAddressValidationControl.SuggestionWindowParentControl.Controls);
			supportWebAddressValidationControl.Address1Control.Focus();
		}

		public static void HookISupportWebAddressValidationControlChangeFocusEvents(ISupportWebAddressValidationControl supportWebAddressValidationControl, EventHandler gotFocusAction, EventHandler lostFocusAction)
		{
			supportWebAddressValidationControl.Address1Control.GotFocus += gotFocusAction;
			supportWebAddressValidationControl.Address2Control.GotFocus += gotFocusAction;
			supportWebAddressValidationControl.CityControl.GotFocus += gotFocusAction;
			supportWebAddressValidationControl.PostcodeControl.GotFocus += gotFocusAction;
			supportWebAddressValidationControl.CountryControl.GotFocus += gotFocusAction;
			supportWebAddressValidationControl.CountryControl.CodeBox.GotFocus += gotFocusAction;
			supportWebAddressValidationControl.ValidateButton.GotFocus += gotFocusAction;

			supportWebAddressValidationControl.Address1Control.LostFocus += lostFocusAction;
			supportWebAddressValidationControl.Address2Control.LostFocus += lostFocusAction;
			supportWebAddressValidationControl.CityControl.LostFocus += lostFocusAction;
			supportWebAddressValidationControl.PostcodeControl.LostFocus += lostFocusAction;
			supportWebAddressValidationControl.CountryControl.LostFocus += lostFocusAction;
			supportWebAddressValidationControl.CountryControl.CodeBox.LostFocus += lostFocusAction;
			supportWebAddressValidationControl.ValidateButton.LostFocus += lostFocusAction;
		}

		public static void ShowSuggestionControlsUponGotFocus(object sender, ISupportWebAddressValidation addressForValidation, ISupportWebAddressValidationControl supportWebAddressValidationControl, int maxWidth = 0, int maxHeiht = 0)
		{
			try
			{
				if (addressForValidation != null)
				{
					var addressIsNotTverified = addressForValidation.ValidationStatus != AddressValidationStatus.Verified &&
						addressForValidation.ValidationStatus != AddressValidationStatus.VerifiedToStreet &&
						addressForValidation.ValidationStatus != AddressValidationStatus.ManuallyVerified;

					// Only show for invalid or unverified addresses
					if (addressIsNotTverified)
					{
						var addressSuggestionControlIsVisible = ShowAddressSuggestionControl(supportWebAddressValidationControl, maxWidth, maxHeiht);

						var cityTownSuggestionControls = supportWebAddressValidationControl.SuggestionWindowParentControl.Controls.Find(CityTownSuggestionControlHelper.CityTownSuggestionControlName, true);

						if (cityTownSuggestionControls.Length == 1 && cityTownSuggestionControls[0] != null &&
							sender != supportWebAddressValidationControl.Address1Control && sender != supportWebAddressValidationControl.Address2Control && !addressSuggestionControlIsVisible)
						{
							cityTownSuggestionControls[0].Show();
						}
					}
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00169865, please contact the ROPE team. The NullReferenceException was caught from ShowSuggestionControlsUponGotFocus().", e);
			}
		}

		public static bool ShowAddressSuggestionControl(ISupportWebAddressValidationControl supportWebAddressValidationControl, int maxWidth, int maxHeiht, bool isResizeControl = false)
		{
			var addressSuggestionControl = FindAddressSuggestionControl(supportWebAddressValidationControl);
			var addressSuggestionControlIsVisible = false;
			if (addressSuggestionControl != null && (!addressSuggestionControl.Visible || addressSuggestionControl.Visible && isResizeControl))
			{
				addressSuggestionControl.UpdateSizeAndLocation(supportWebAddressValidationControl.SuggestionWindowParentControl.FindForm(),
					supportWebAddressValidationControl.SuggestionWindowParentControl,
					supportWebAddressValidationControl.ValidateButton,
					maxWidth,
					maxHeiht);
				addressSuggestionControl.Show();
				addressSuggestionControlIsVisible = true;
			}
			return addressSuggestionControlIsVisible;
		}

		internal static AddressSuggestionControl FindAddressSuggestionControl(ISupportWebAddressValidationControl supportWebAddressValidationControl)
		{
			AddressSuggestionControl addressSuggestionControl = null;
			var addressSuggestionControls = supportWebAddressValidationControl.SuggestionWindowParentControl.Controls.Find(AddressSuggestionControlHelper.AddressSuggestionControlName, true);
			if (addressSuggestionControls.Length == 1 && addressSuggestionControls[0] != null)
			{
				addressSuggestionControl = addressSuggestionControls[0] as AddressSuggestionControl;
			}
			return addressSuggestionControl;
		}

		public static void HideSuggestionControlsUponLostFocus(ISupportWebAddressValidationControl supportWebAddressValidationControl)
		{
			var containerControl = supportWebAddressValidationControl as ContainerControl;

			var cityStatePostcodeActiveOrFocussed =
				(supportWebAddressValidationControl.CityControl.Focused || supportWebAddressValidationControl.PostcodeControl.Focused ||
				supportWebAddressValidationControl.StateControl.Focused ||
				supportWebAddressValidationControl.StateControl.CodeBox.Focused ||
				supportWebAddressValidationControl.CountryControl.Focused ||
				supportWebAddressValidationControl.CountryControl.CodeBox.Focused);

			var addressFieldActiveOrFocussed =
				(cityStatePostcodeActiveOrFocussed ||
				supportWebAddressValidationControl.Address1Control.Focused || supportWebAddressValidationControl.Address2Control.Focused);

			var cityTownSuggestionControls = supportWebAddressValidationControl.SuggestionWindowParentControl.Controls.Find(CityTownSuggestionControlHelper.CityTownSuggestionControlName, false);

			if (cityTownSuggestionControls.Length == 1 && cityTownSuggestionControls[0] != null)
			{
				// Don't hide if city/state/postcode are the active or focused controls or mouse is within bounds of city town suggestion control
				if (containerControl.ActiveControl != cityTownSuggestionControls[0] && !cityStatePostcodeActiveOrFocussed && !cityTownSuggestionControls[0].Bounds.Contains(supportWebAddressValidationControl.SuggestionWindowParentControl.PointToClient(Control.MousePosition)))
				{
					cityTownSuggestionControls[0].Hide();
				}
			}

			var addressSuggestionControls = supportWebAddressValidationControl.SuggestionWindowParentControl.Controls.Find(AddressSuggestionControlHelper.AddressSuggestionControlName, true);

			if (addressSuggestionControls.Length == 1 && addressSuggestionControls[0] != null)
			{
				// Don't hide if address fields are the active or focused controls or mouse is within bounds of address suggestion control
				if (containerControl.ActiveControl != addressSuggestionControls[0] && !addressFieldActiveOrFocussed && !addressSuggestionControls[0].Bounds.Contains(supportWebAddressValidationControl.SuggestionWindowParentControl.PointToClient(Control.MousePosition)))
				{
					addressSuggestionControls[0].Hide();
				}
			}
		}
	}
}
