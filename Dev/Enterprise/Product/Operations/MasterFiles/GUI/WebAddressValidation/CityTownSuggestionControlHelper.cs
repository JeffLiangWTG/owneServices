using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public static class CityTownSuggestionControlHelper
	{
		public const string CityTownSuggestionControlName = "CityTownSuggestionControl";

#if DEBUG
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static Action<ISupportWebAddressValidation> AddressHandlerForTest;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static CandidateCityTown[] FakeResult;
#endif

		static async Task<CandidateCityTown[]> GetCityTownAsync(ISupportWebAddressValidation address, CancellationTokenSource cancellationToken)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				AddressHandlerForTest?.Invoke(address);
				if (FakeResult != null)
				{
					return FakeResult;
				}
			}
#endif
			return await address.GetCityTownAsync(cancellationToken);
		}

		public static async Task GetCityTownAsync(
			CancellationTokenSource cancellationToken,
			ISupportWebAddressValidation address,
			Control parentControl,
			Form applicationForm,
			Control.ControlCollection holderControlCollection,
			Action handleSelectAction,
			int maxWidth = 0)
		{
			try
			{
				var shouldPopulateSuggestion =
					address != null &&
					!address.IsUpdatingCityTown &&
					!address.IsRowDeletedOrDetachedOrNull &&
					address.ValidationStatus != AddressValidationStatus.Verified;

				if (shouldPopulateSuggestion)
				{
					CityTownSuggestionControl suggestionControl = null;

					var result = await GetCityTownAsync(address, cancellationToken);

					if (result != null)
					{
						foreach (var candidateCityTown in result)
						{
							UpdateSuggestedState(candidateCityTown, address);
						}
					}

					// Get the control that had focus when the call returned so we can show the suggestion dropdown
					// This allows us to reactivate the control once the suggestion dropdown has been shown
					// When changing tab, the result could be null
					var activeControlAtResponse = FindFocusedControl(parentControl);

					if (activeControlAtResponse != null && result != null && result.Length > 0)
					{
						address.IsUpdatingCityTown = true;

						var validationControl = parentControl as ISupportWebAddressValidationControl;
						var activeControlWasStateDropdown = (activeControlAtResponse == validationControl.StateControl || activeControlAtResponse == validationControl.StateControl.CodeBox);

						var exactMatchIndex = GetIndexOfExactMatch(result, validationControl);

						if (address.IsRowDeletedOrDetachedOrNull)
						{
							return;
						}

						if (result.Length == 1 && (address.City == string.Empty || address.Postcode == string.Empty))
						{
							FillAddressWithExactMatchSuggestion(address, result);
							handleSelectAction();
						}
						else if (exactMatchIndex == -1)
						{
							suggestionControl = ShowSuggestedCityTowns(
								result,
								address,
								parentControl,
								applicationForm,
								holderControlCollection,
								handleSelectAction,
								maxWidth,
								true);

							if (suggestionControl == null)
							{
								address.IsUpdatingCityTown = false;
								return;
							}

							HookTextControl(address, validationControl.CityControl, suggestionControl, validationControl);
							HookTextControl(address, validationControl.PostcodeControl, suggestionControl, validationControl);
							HookTextControl(address, validationControl.StateControl.CodeBox, suggestionControl, validationControl);
							HookTextControl(address, validationControl.CountryControl.CodeBox, suggestionControl, validationControl);

							// Reactivate the control now the suggestion control has been displayed
							// NOTE: We don't allow focus on the state dropdown, but instead force focus to the suggestion control
							//       so that the user's up/down arrows don't open the state dropdown
							if (activeControlWasStateDropdown)
							{
								suggestionControl.Focus();
							}
							else
							{
								activeControlAtResponse.Focus();
							}

							// If the user tries to tab or click back into the state dropdown force focus to the suggestion control
							// again so that the user's up/down arrows don't open the state dropdown
							EventHandler onGotFocus;
							onGotFocus = (sender, args) =>
							{
								if (address.IsRowDeletedOrDetachedOrNull)
								{
									return;
								}

								if (suggestionControl != null && !suggestionControl.IsDisposed && address.ValidationStatus != AddressValidationStatus.Verified)
								{
									suggestionControl.TabIndex = validationControl.StateControl.TabIndex + 1;
									suggestionControl.Show();
									suggestionControl.Focus();
								}
							};

							validationControl.StateControl.CodeBox.GotFocus += onGotFocus;
							validationControl.StateControl.CodeBox.Disposed += (sender, args) =>
							{
								validationControl.StateControl.CodeBox.GotFocus -= onGotFocus;
							};

							EventHandler onLostFocus;
							onLostFocus = (sender, args) =>
							{
								if (suggestionControl != null && !suggestionControl.IsDisposed)
								{
									suggestionControl.Hide();
								}
							};

							suggestionControl.CityTownListView.LostFocus += onLostFocus;
							suggestionControl.CityTownListView.Disposed += (x, y) =>
							{
								suggestionControl.CityTownListView.LostFocus -= onLostFocus;
							};
						}
						else
						{
							if (address.IsRowDeletedOrDetachedOrNull)
							{
								return;
							}

							address.City = validationControl.CityControl.Text.ToUpper();
							address.Postcode = validationControl.PostcodeControl.Text.ToUpper();
							address.State = validationControl.StateControl.Text.ToUpper();
							handleSelectAction();
						}

						address.IsUpdatingCityTown = false;
					}
					else
					{
						CloseSuggestionForm(holderControlCollection);
					}
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00167351, please contact the ROPE team. The NullReferenceException was caught from GetCityTownAsync().", e);
			}
		}

#if DEBUG
		internal
#endif
		static void FillAddressWithExactMatchSuggestion(ISupportWebAddressValidation address, CandidateCityTown[] result)
		{
			address.IsValidatingAddress = true;
			if (result[0].City != null)
			{
				address.City = result[0].City;
			}
			if (result[0].Postcode != null)
			{
				address.Postcode = result[0].Postcode.ToUpper(CultureInfo.InvariantCulture);
			}
			if (address.Country != null && address.Country.IsStateMustNotBeEntered)
			{
				address.State = String.Empty;
			}
			else if (result[0].State != null)
			{
				address.State = result[0].State.ToUpper(CultureInfo.InvariantCulture);
			}

			address.IsValidatingAddress = false;
		}

#if DEBUG
		internal
#endif
		static void UpdateSuggestedState(CandidateCityTown candidateCityTown, ISupportWebAddressValidation addressForValidation)
		{
			string suggestedState;

			if (string.IsNullOrWhiteSpace(candidateCityTown.State) || addressForValidation.IsRowDeletedOrDetachedOrNull)
			{
				return;
			}

			var country = addressForValidation.Country;

			if (candidateCityTown == null || country == null)
			{
				return;
			}

			if (country.States.Cast<RefCountryStates>().Any(s => s.RW_Code == candidateCityTown.State))
			{
				suggestedState = candidateCityTown.State;
			}
			else
			{
				var matchedRefState = country
					.States
					.Cast<RefCountryStates>()
					.Where(state => state.RW_IsActive)
					.FirstOrDefault(state => string.Equals(state.RW_DescriptionMultilingual, candidateCityTown.State, StringComparison.OrdinalIgnoreCase));

				if (matchedRefState != null)
				{
					suggestedState = matchedRefState.RW_Code;
				}
				else
				{
					matchedRefState = country
						.States
						.Cast<RefCountryStates>()
						.Where(state => state.RW_IsActive)
						.FirstOrDefault(state => string.Equals(state.RW_DescriptionMultilingual.GetLocalizedValue(addressForValidation.Language).ToString(), candidateCityTown.State, StringComparison.OrdinalIgnoreCase));
					if (matchedRefState != null)
					{
						suggestedState = matchedRefState.RW_Code;
					}
					else
					{
						string normalisedState = NormalizePunctuations.Normalize(StripDiacritics.RemoveDiacritics(candidateCityTown.State, country.RN_Code));

						matchedRefState = country
							.States
							.Cast<RefCountryStates>()
							.Where(state => state.RW_IsActive)
							.FirstOrDefault(state => string.Equals(NormalizePunctuations.Normalize(StripDiacritics.RemoveDiacritics(state.RW_DescriptionMultilingual, country.RN_Code)), normalisedState, StringComparison.OrdinalIgnoreCase));

						if (matchedRefState != null)
						{
							suggestedState = matchedRefState.RW_Code;
						}
						else if (Res.IsEnglish(addressForValidation.Language))
						{
							suggestedState = normalisedState;
						}
						else
						{
							suggestedState = candidateCityTown.State;
						}
					}
				}
			}

			candidateCityTown.State = !string.IsNullOrEmpty(suggestedState) && suggestedState.Length > addressForValidation.State_MaxLength
				? suggestedState.Substring(0, addressForValidation.State_MaxLength)
				: suggestedState;
		}

		static int GetIndexOfExactMatch(CandidateCityTown[] results, ISupportWebAddressValidationControl validationControl)
		{
			var index = -1;

			for (int i = 0; i < results.Length; i++)
			{
				if (results[i].City != null && results[i].State != null && results[i].Postcode != null)
				{
					if (results[i].City.Equals(validationControl.CityControl.Text, StringComparison.OrdinalIgnoreCase) &&
					results[i].State.Equals(validationControl.StateControl.Text, StringComparison.OrdinalIgnoreCase) &&
					results[i].Postcode.Equals(validationControl.PostcodeControl.Text, StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			return index;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
#if DEBUG
		internal
#endif
		static void HookTextControl(ISupportWebAddressValidation address, Control control, CityTownSuggestionControl suggestionControl, ISupportWebAddressValidationControl parentControl)
		{
			if (control != null)
			{
				var textControl = control as TextBox;

				if (textControl != null)
				{
					textControl.SelectionStart = textControl.TextLength;
					textControl.SelectionLength = 0;
#if !WINZOR
					if (suggestionControl != null && parentControl != null)
					{
						EventHandler textChangedHandler;
						PreviewKeyDownEventHandler previewKeyDownHandler;
						KeyEventHandler keyDownHandler;
						textChangedHandler = (x, y) =>
						{
							if (!address.IsRowDeletedOrDetachedOrNull)
							{
								if (!address.IsUpdatingCityTown && suggestionControl != null && !suggestionControl.IsDisposed)
								{
									suggestionControl.UpdateFilter(parentControl.CityControl.Text, parentControl.StateControl.Text, parentControl.PostcodeControl.Text);
									suggestionControl.UpdateSize();
								}
							}
							else
							{
								suggestionControl.Close();
							}
						};
						textControl.TextChanged += textChangedHandler;

						previewKeyDownHandler = (x, y) =>
						{
							switch (y.KeyCode)
							{
								// Turn off automated return key handling so we can  in the suggestion control
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
							if (suggestionControl != null && !suggestionControl.IsDisposed)
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
							}
						};
						textControl.KeyDown += keyDownHandler;

						suggestionControl.Disposed += (x, y) =>
							{
								textControl.TextChanged -= textChangedHandler;
								textControl.PreviewKeyDown -= previewKeyDownHandler;
								textControl.KeyDown -= keyDownHandler;
							};
					}
#endif
				}
			}
		}

		static CityTownSuggestionControl ShowSuggestedCityTowns(
			CandidateCityTown[] cityTowns,
			ISupportWebAddressValidation address,
			Control parentControl,
			Form applicationForm,
			Control.ControlCollection holderControlCollection,
			Action handleSelectAction,
			int maxWidth,
			bool ifShouldReopen)
		{
			if (AddressSuggestionControlHelper.FindSuggestionForm(holderControlCollection) != null)
			{
				return null;
			}

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

			var interfaceOnControl = parentControl as ISupportWebAddressValidationControl;
			if (interfaceOnControl == null)
			{
				ErrorReporter.ReportOnce("ShowSuggestedCityTowns_ISupportWebAddressValidationControlNotImplemented", "Parent control does not implement ISupportWebAddressValidationControl interface");
				return null;
			}

			var control = new CityTownSuggestionControl(
				address,
				cityTowns,
				interfaceOnControl.CityControl.Text,
				interfaceOnControl.StateControl.Text,
				interfaceOnControl.PostcodeControl.Text,
				applicationForm,
				holderControlCollection.Owner,
				interfaceOnControl.StateControl,
				maxWidth);

			control.Name = CityTownSuggestionControlName;
			control.CityTownSelected += (x, y) => handleSelectAction();

			if (parentControl.IsDisposed)
			{
				control.Dispose();
				return null;
			}

			AddOverlayControl(control, parentControl, holderControlCollection);
			AutoFillStateIfPossible(cityTowns, address);

			interfaceOnControl.StateControl.SetDropButtonAvailability(false);
			control.Disposed += (sender, args) =>
			{
				interfaceOnControl.StateControl.SetDropButtonAvailability(true);
			};

			return control;
		}

		static Control FindFocusedControl(Control control)
		{
			var container = control as IContainerControl;
			while (container != null)
			{
				control = container.ActiveControl;
				container = control as IContainerControl;
			}
			return control;
		}

#if DEBUG
		internal
#endif
		static void AutoFillStateIfPossible(CandidateCityTown[] cityTowns, ISupportWebAddressValidation address)
		{
			if (address.Country == null || !address.Country.IsStateMustNotBeEntered)
			{
				if (cityTowns.Length > 0)
				{
					var state = cityTowns[0].State;
					if (!string.IsNullOrEmpty(state) && cityTowns.All(cityTown => cityTown.State == state) && address.State.IsEmpty)
					{
						address.State = state.ToUpper(CultureInfo.InvariantCulture);
					}
				}
			}
		}

		static void AddOverlayControl(Control control, Control parentControl, Control.ControlCollection holderControlCollection)
		{
			if (!control.IsDisposed)
			{
				holderControlCollection.Add(control);
				control.BringToFront();
			}
			parentControl.Disposed += (x, y) => { control.Dispose(); };
		}

		public delegate Task CallGetCityTownService();

		public static void RegisterPropertyChangedEvent(ISupportWebAddressValidation address, CallGetCityTownService callGetCityTownServiceMethod)
		{
			if (address != null)
			{
				address.ClearWebGetCityTownHandler();
				address.TriggerWebGetCityTown += (sender, e) =>
				{
					callGetCityTownServiceMethod();
				};
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

		public static CityTownSuggestionControl FindSuggestionForm(Control.ControlCollection holderControlCollection)
		{
			var controls = holderControlCollection.Find(CityTownSuggestionControlName, true);
			if (controls.Length > 0)
			{
				return controls[0] as CityTownSuggestionControl;
			}
			return null;
		}
	}
}
