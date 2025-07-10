using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public static class PhoneNumberValidationOverrideControlHelper
	{
		public const string PhoneNumberValidationOverrideControlName = "PhoneNumberValidationOverrideControl";

		public static void ShowConfirmationMessage(ZPropertyInfo phoneManuallyVerifiedPropertyInfo, ZUserControl parentControl, Point location, Control.ControlCollection holderControlCollection, Action refreshAction, string notificationMessage, ZString phoneNumber)
		{
			if (IsConfirmationFormAlreadyShowing(holderControlCollection))
			{
				return;
			}

			var control = new PhoneNumberValidationOverrideControl(phoneManuallyVerifiedPropertyInfo, notificationMessage);
			control.Name = PhoneNumberValidationOverrideControlName;
			control.PhoneNumberConfirmed += (x, y) => refreshAction();
			if (!IsPhoneNumberAcceptable(phoneNumber))
			{
				control.NotAcceptedReason = Res.GetString("8935BE40-8CD3-480C-93DC-9D46BE1951F9", "This number contains non-numeric characters which are not accepted. System Administrators have disallowed non-numeric values to be manually verified.");
			}
			AddOverlayControl(control, location, parentControl, holderControlCollection);
		}

		static void AddOverlayControl(Control validationOverrideControl, Point relativeStartingLocation, ZUserControl parentControl, Control.ControlCollection holderControlCollection)
		{
			var location = parentControl.ParentForm.PointToClient(relativeStartingLocation);

			validationOverrideControl.Location = location;
			if (!validationOverrideControl.IsDisposed)
			{
				holderControlCollection.Add(validationOverrideControl);
				validationOverrideControl.BringToFront();
			}
		}

		static bool IsConfirmationFormAlreadyShowing(Control.ControlCollection holderControlCollection)
		{
			return FindConfirmationForm(holderControlCollection) != null;
		}

		static bool IsPhoneNumberAcceptable(ZString phoneNumber)
		{
			bool isPhoneNumberAcceptable = true;
			if (DataRegistry.Instance.NumericValuesOnlyForPhoneNumberFields)
			{
				var phoneNumbersRegex = new Regex(@"^[\d\-\+\s]+$");
				isPhoneNumberAcceptable = phoneNumbersRegex.IsMatch(phoneNumber);
			}

			return isPhoneNumberAcceptable;
		}

		public static bool IsConfirmationFormFocused(Control.ControlCollection holderControlCollection)
		{
			var confirmationForm = FindConfirmationForm(holderControlCollection);
			return confirmationForm?.IsEntered ?? false;
		}

		public static void CloseConfirmationForm(Control.ControlCollection holderControlCollection)
		{
			var confirmationForm = FindConfirmationForm(holderControlCollection);
			if (confirmationForm != null)
			{
				confirmationForm.Close();
			}
		}

		public static PhoneNumberValidationOverrideControl FindConfirmationForm(Control.ControlCollection holderControlCollection)
		{
			var controls = holderControlCollection.Find(PhoneNumberValidationOverrideControlName, true);
			if (controls.Length > 0)
			{
				return controls[0] as PhoneNumberValidationOverrideControl;
			}
			return null;
		}
	}
}
