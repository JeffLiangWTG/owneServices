using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public static class AddressSuggestionControlHelper
	{
		public const string AddressSuggestionControlName = "AddressSuggestionControl";

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static Tuple<string, List<ValidationResultItem>, ValidationResultItem> LastSuggestedResults { get; set; }

		public static async Task<WebAddressValidationResult> GetValidationResult(CancellationTokenSource cancellationToken,
			ISupportWebAddressValidation address, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			try
			{
				if (address != null && !address.IsValidatingAddress)
				{
					if (AddressSameAsInCache(address) && cleanseAction != CleanseAction.QuickValidate)
					{
						return new WebAddressValidationResult()
						{
							Message = "",
							ResultAddress = LastSuggestedResults.Item3,
							SuggestedResults = LastSuggestedResults.Item2
						};
					}
					else
					{
						address.ValidationStatus = AddressValidationStatus.ToBeVerified;
						var result = await address.ValidateAddressAsync(cancellationToken, cleanseAction);
						LastSuggestedResults = null;

						if (!string.IsNullOrEmpty(result.Message))
						{
							Globals.Message.ShowWarning(result.Message);
						}
						return result;
					}
				}
				return null;
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This is to help to fix Issue 01042023, please contact the ROPE team. The NullReferenceException was caught from ValidateAddressAsync().", e);
				return null;
			}
		}

		public static async Task ValidateAddressAsync(
			CancellationTokenSource cancellationToken,
			ISupportWebAddressValidation address,
			Control parentControl,
			Form applicationForm,
			Control.ControlCollection holderControlCollection,
			Action refreshAction,
			Action handleDestroyedAction,
			int maxWidth = 0,
			CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest,
			Control referenceControl = null,
			bool isFirstLoad = false,
			int maxHeight = 0)
		{
			if (address != null && !address.IsValidatingAddress)
			{
				if (AddressSameAsInCache(address) && cleanseAction != CleanseAction.QuickValidate)
				{
					ShowSuggestedAddresses(LastSuggestedResults.Item2, LastSuggestedResults.Item3, address, (ISupportWebAddressValidationControl)parentControl, applicationForm, holderControlCollection, referenceControl, refreshAction, handleDestroyedAction, maxWidth, false, maxHeight);
				}
				else if (!AddressValidationHasErrors(address, parentControl, isFirstLoad))
				{
					var result = await address.ValidateAddressAsync(cancellationToken, cleanseAction);
					LastSuggestedResults = null;

					if (cleanseAction != CleanseAction.QuickValidate)
					{
						ShowWarningOrSuggestedAddresses(address, parentControl, applicationForm, holderControlCollection, refreshAction, handleDestroyedAction, maxWidth, referenceControl, maxHeight, result);
					}
					else
					{
						PopulateClosestPort(address);
					}
				}
			}
		}

		static void PopulateClosestPort(ISupportWebAddressValidation address)
		{
			if (!address.IsRowDeletedOrDetachedOrNull &&
				(address.ValidationStatus == AddressValidationStatus.VerifiedToStreet || address.ValidationStatus == AddressValidationStatus.Verified))
			{
				AddressValidationService.SetClosestPort(address);
			}
		}

		static void ShowWarningOrSuggestedAddresses(ISupportWebAddressValidation address, Control parentControl, Form applicationForm, Control.ControlCollection holderControlCollection,
			Action refreshAction, Action handleDestroyedAction, int maxWidth, Control referenceControl, int maxHeight, WebAddressValidationResult result)
		{
			if (result != null && !string.IsNullOrEmpty(result.Message))
			{
				Globals.Message.ShowWarning(result.Message);
			}
			else if (!address.IsRowDeletedOrDetachedOrNull && address.ValidationStatus == AddressValidationStatus.Invalid && parentControl != null)
			{
				var validationControl = parentControl as ISupportWebAddressValidationControl;
				var suggestionControl = ShowSuggestedAddresses(result, address, validationControl, applicationForm,
					holderControlCollection, referenceControl, refreshAction, handleDestroyedAction, maxWidth, true, maxHeight);

				if (suggestionControl != null && validationControl != null)
				{
					HookTextControl(validationControl.Address1Control, suggestionControl, validationControl);
					HookTextControl(validationControl.Address2Control, suggestionControl, validationControl);
					HookTextControl(validationControl.CityControl, suggestionControl, validationControl);
					HookTextControl(validationControl.PostcodeControl, suggestionControl, validationControl);
					HookTextControl(validationControl.StateControl.CodeBox, suggestionControl, validationControl);
					HookTextControl(validationControl.CountryControl.CodeBox, suggestionControl, validationControl);
				}
			}
		}

		internal static bool AddressValidationHasErrors(ISupportWebAddressValidation address, Control parentControl, bool isFirstLoad = false)
		{
			var hasErrors = false;
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;

			// BO may has other validation errors not related to address
			var bizO = address as BusinessObject;
			var errorsBefore = bizO?.Notifications == null ? 0 : bizO.Notifications.Count();
			address.PreValidationForAddressValidationService();

			var errorsAfter = bizO?.Notifications == null ? 0 : bizO.Notifications.Count();
			if (errorsAfter > errorsBefore)
			{
				hasErrors = true;

				if (!isFirstLoad && parentControl != null)
				{
					TabPageNotificationsExposer.ExposeTabPageNotifications(parentControl, bizO);
					if (!bizO.Factory.IsInTransaction)
					{
						Globals.Message.Show(Res.GetString("b73b7a46-151f-4d38-a36b-a0d5f3339e5c", "Please fix errors on address before running validation."));
					}
				}
			}

			return hasErrors;
		}

		public static void HookTextControl(Control control, AddressSuggestionControl suggestionControl, ISupportWebAddressValidationControl parentControl)
		{
			if (control != null)
			{
				var textControl = control as TextBox;

				if (textControl != null)
				{
					textControl.SelectionStart = textControl.TextLength > 0 ? textControl.TextLength : 0;
					textControl.SelectionLength = 0;
#if !WINZOR
					if (suggestionControl != null && parentControl != null)
					{
						PreviewKeyDownEventHandler previewKeyDownHandler;
						KeyEventHandler keyDownHandler;

						previewKeyDownHandler = (x, y) =>
						{
							switch (y.KeyCode)
							{
								// Turn off automated return key handling so we can move in the suggestion control
								case Keys.Return:
								case Keys.Escape:
									y.IsInputKey = true;
									break;

								// Turn off automated up/down key handling so we can move the selection cursor in the suggestion control
								case Keys.Down:
								case Keys.Up:
									y.IsInputKey = true;
									break;
							}
						};
						textControl.PreviewKeyDown += previewKeyDownHandler;

						keyDownHandler = (x, y) =>
						{
							switch (y.KeyCode)
							{
								case Keys.Return:
									suggestionControl.SelectAddressAndClose();
									y.Handled = true;
									break;
								case Keys.Escape:
									suggestionControl.Close();
									y.Handled = true;
									break;
								case Keys.Down:
									suggestionControl.MoveSelection(1);
									y.Handled = true;
									break;
								case Keys.Up:
									suggestionControl.MoveSelection(-1);
									y.Handled = true;
									break;
							}
						};
						textControl.KeyDown += keyDownHandler;

						suggestionControl.Disposed += (x, y) =>
						{
							textControl.PreviewKeyDown -= previewKeyDownHandler;
							textControl.KeyDown -= keyDownHandler;
						};
					}
#endif
				}
			}
		}

		static string ConstructAddressCacheKey(ISupportWebAddressValidation address)
		{
			return string.Join("+", address.Address1, address.Address2, address.City, address.Postcode, address.State, address.Country);
		}

		static bool AddressSameAsInCache(ISupportWebAddressValidation address)
		{
			return LastSuggestedResults != null && ConstructAddressCacheKey(address) == LastSuggestedResults.Item1;
		}

		internal static AddressSuggestionControl ShowSuggestedAddresses(
			WebAddressValidationResult validationResult,
			ISupportWebAddressValidation address,
			ISupportWebAddressValidationControl validationControl,
			Form applicationForm,
			Control.ControlCollection holderControlCollection,
			Control referenceControl,
			Action refreshAction,
			Action handleDestroyedAction,
			int maxWidth,
			bool ifShouldReopen,
			int maxHeight = 0)
		{
			AddressSuggestionControl suggestionControl = null;

			var suggestionResult = validationResult;
			LastSuggestedResults = new Tuple<string, List<ValidationResultItem>, ValidationResultItem>(ConstructAddressCacheKey(address), suggestionResult?.SuggestedResults, validationResult?.TopRecommendedAddress);

			if (!string.IsNullOrEmpty(suggestionResult?.Message))
			{
				Globals.Message.Show(suggestionResult?.Message);
			}

			if (!string.IsNullOrEmpty(validationResult?.ResultAddress?.ErrorMessage))
			{
				Globals.Message.Show(validationResult?.ResultAddress.ErrorMessage);
			}
			else if (holderControlCollection != null)
			{
				suggestionControl = ShowSuggestedAddresses(suggestionResult?.SuggestedResults, validationResult?.TopRecommendedAddress, address, validationControl, applicationForm, holderControlCollection, referenceControl, refreshAction, handleDestroyedAction, maxWidth, ifShouldReopen, maxHeight);
			}

			return suggestionControl;
		}

		static AddressSuggestionControl ShowSuggestedAddresses(
			List<ValidationResultItem> suggestedAddresses,
			ValidationResultItem topRecommendedAddress,
			ISupportWebAddressValidation address,
			ISupportWebAddressValidationControl validationControl,
			Form applicationForm,
			Control.ControlCollection holderControlCollection,
			Control referenceControl,
			Action refreshAction,
			Action handleDestroyedAction,
			int maxWidth,
			bool ifShouldReopen,
			int maxHeight = 0)
		{
			if (IsSuggestionFormAlreadyShowing(holderControlCollection))
			{
				if (ifShouldReopen)
				{
					CloseSuggestionForm(holderControlCollection);
				}
				else
				{
					return null;
				}
			}

			if (CityTownSuggestionControlHelper.FindSuggestionForm(holderControlCollection) != null)
			{
				CityTownSuggestionControlHelper.CloseSuggestionForm(holderControlCollection);
			}

			if (validationControl.AddressForValidation != null)
			{
				if (!CheckNeedShowSuggestedAddresses(validationControl.AddressForValidation, address))
				{
					return null;
				}
			}

			var control = new AddressSuggestionControl(
				address,
				suggestedAddresses,
				topRecommendedAddress,
				applicationForm,
				holderControlCollection.Owner,
				referenceControl ?? validationControl.ValidateButton,
				maxWidth, maxHeight);

			if (refreshAction != null)
			{
				control.AddressSelected += (x, y) => refreshAction();
			}

			if (handleDestroyedAction != null)
			{
				control.HandleDestroyed += (x, y) => handleDestroyedAction();
			}

			if (((Control)validationControl).IsDisposed)
			{
				control.Dispose();
				return null;
			}

			AddOverlayControl(control, holderControlCollection);

			return control;
		}

#if DEBUG
		public
#endif
		static bool CheckNeedShowSuggestedAddresses(ISupportWebAddressValidation addressForValidation, ISupportWebAddressValidation validatedAddress)
		{
			return ZGuid.Equals(addressForValidation.EntityPK, validatedAddress.EntityPK);
		}

		static void AddOverlayControl(Control control, Control.ControlCollection holderControlCollection)
		{
			if (!control.IsDisposed)
			{
				holderControlCollection.Add(control);
				control.BringToFront();
			}
		}

		static bool IsSuggestionFormAlreadyShowing(Control.ControlCollection holderControlCollection)
		{
			return FindSuggestionForm(holderControlCollection) != null;
		}

		public static void CloseSuggestionForm(Control.ControlCollection holderControlCollection)
		{
			var suggestiongForm = FindSuggestionForm(holderControlCollection);
			if (suggestiongForm != null)
			{
				suggestiongForm.Close();
			}
		}

		public static AddressSuggestionControl FindSuggestionForm(Control.ControlCollection holderControlCollection)
		{
			var controls = holderControlCollection.Find(AddressSuggestionControlName, true);
			if (controls.Length > 0)
			{
				return controls[0] as AddressSuggestionControl;
			}
			return null;
		}

		public delegate Task CallValidationService();

		public static void RegisterPropertyChangedEvent(ISupportWebAddressValidation address, CallValidationService callValidationServiceMethod)
		{
			if (address != null)
			{
				address.ClearWebAddressValidationHandler();
				address.TriggerWebAddressValidation += (sender, e) =>
				{
					if (address.NeedValidation)
					{
						callValidationServiceMethod();
					}
				};
			}
		}
	}
}
