using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public class CityTownSuggestionControlUtil : SuggestionControlUtil<CityTownSuggestionControl, CandidateCityTown[]>
	{
		public CityTownSuggestionControlUtil(Control parentControl, Form parentForm, CancellationTokenSource cancellationTokenSource, AddressValidatorConfiguration config)
			: base(parentControl, parentForm, cancellationTokenSource, config, nameof(CityTownSuggestionControl))
		{
		}

		public async Task GetCityTownAsync()
		{
			if (validationControl.AddressForValidation is null ||
				!config.CurrentSelectedTabCheck.Invoke() ||
				(validationControl.AddressForValidation.City.IsEmpty && validationControl.AddressForValidation.Postcode.IsEmpty) ||
				validationControl.AddressForValidation.IsUpdatingCityTown ||
				(validationControl.AddressForValidation as BusinessObject).IsRowDeletedOrDetachedOrNull ||
				validationControl.AddressForValidation.ValidationStatus == AddressValidationStatus.Verified)
			{
				return;
			}

			var result = await GetCityTownCore();
			if (result.IsNullOrEmpty())
			{
				return;
			}

			validationControl.AddressForValidation.IsUpdatingCityTown = true;
			HandleSuggestionResult(result);
			validationControl.AddressForValidation.IsUpdatingCityTown = false;
		}

		public override void RegisterPropertyChangedEvent(Func<Task> callService)
		{
			if (validationControl.AddressForValidation is null)
			{
				return;
			}

			validationControl.AddressForValidation.ClearWebGetCityTownHandler();
			validationControl.AddressForValidation.TriggerWebGetCityTown += (sender, e) =>
		{
			callService();
		};
		}

		protected virtual async Task<CandidateCityTown[]> GetCityTownCore()
		{
			var result = await validationControl.AddressForValidation.GetCityTownAsync(cancellationTokenSource);

			foreach (var candidateCityTown in result ?? Array.Empty<CandidateCityTown>())
			{
				UpdateSuggestedState(candidateCityTown);
			}

			return result;
		}

		protected override CityTownSuggestionControl CreateNewSuggestionControl(CandidateCityTown[] candidateCityTowns)
		{
			var suggestionControl = new CityTownSuggestionControl(
				validationControl.AddressForValidation,
				candidateCityTowns,
				validationControl.CityControl.Text,
				validationControl.StateControl.Text,
				validationControl.PostcodeControl.Text,
				parentForm,
				parentControl,
				validationControl.StateControl);

			var handleCityTownSelected = new EventHandler<CityTownSuggestionControl.CityTownSelectedEventArgs>((sender, e) => config.SelectCityTownAction.Invoke());
			suggestionControl.CityTownSelected += handleCityTownSelected;
			suggestionControl.Disposed += (sender, e) => suggestionControl.CityTownSelected -= handleCityTownSelected;

			return suggestionControl;
		}

		#region Implementation

		void HandleSuggestionResult(CandidateCityTown[] candidateCityTowns)
		{
			var activeControlAtResponse = GetActiveControlAtResponse();
			if (activeControlAtResponse is null)
			{
				CloseSuggestionControlIfExists();
				return;
			}

			var cityTownNeedToFill = string.IsNullOrEmpty(validationControl.AddressForValidation.City) || string.IsNullOrEmpty(validationControl.AddressForValidation.Postcode);
			if (candidateCityTowns.Length == 1 && cityTownNeedToFill)
			{
				FillAddressWithSuggestion(candidateCityTowns.First());
				config.SelectCityTownAction?.Invoke();

				return;
			}

			if (HasExactMatch(candidateCityTowns))
			{
				validationControl.AddressForValidation.City = validationControl.CityControl.Text.ToUpper();
				validationControl.AddressForValidation.Postcode = validationControl.PostcodeControl.Text.ToUpper();
				validationControl.AddressForValidation.State = validationControl.StateControl.Text.ToUpper();
				config.SelectCityTownAction.Invoke();

				return;
			}

			AutoFillState(candidateCityTowns);
			CreateAndShowSuggestionControl(candidateCityTowns);
			HandleSuggestionControl(activeControlAtResponse);
		}

		void HandleSuggestionControl(Control activeControl)
		{
			var suggestionControl = FindSuggestionControl();
			if (suggestionControl is null)
			{
				return;
			}

			HookFocusChangedEvents(suggestionControl);

			validationControl.StateControl.SetDropButtonAvailability(false);
			suggestionControl.Disposed += (sender, args) => validationControl.StateControl.SetDropButtonAvailability(true);

			if (activeControl == validationControl.StateControl || activeControl == validationControl.StateControl.CodeBox)
			{
				activeControl = suggestionControl;
			}

			activeControl.Focus();
		}

		void HookFocusChangedEvents(CityTownSuggestionControl suggestionControl)
		{
			var handleStateGotFocus = new EventHandler((sender, args) =>
			{
				if (validationControl.AddressForValidation.IsRowDeletedOrDetachedOrNull ||
					(suggestionControl?.IsDisposed ?? true) ||
					validationControl.AddressForValidation.ValidationStatus == AddressValidationStatus.Verified)
				{
					return;
				}

				suggestionControl.TabIndex = validationControl.StateControl.TabIndex + 1;
				suggestionControl.Show();
				suggestionControl.Focus();
			});

			validationControl.StateControl.CodeBox.GotFocus += handleStateGotFocus;
			validationControl.StateControl.CodeBox.Disposed += (sender, e) => validationControl.StateControl.CodeBox.GotFocus -= handleStateGotFocus;

			var handleListViewLostFocus = new EventHandler((sender, args) => suggestionControl.Hide());

			suggestionControl.CityTownListView.LostFocus += handleListViewLostFocus;
			suggestionControl.CityTownListView.Disposed += (sender, e) => suggestionControl.CityTownListView.LostFocus -= handleListViewLostFocus;
		}

		void UpdateSuggestedState(CandidateCityTown candidateCityTown)
		{
			if (string.IsNullOrWhiteSpace(candidateCityTown?.State) ||
				validationControl.AddressForValidation.Country is null)
			{
				return;
			}

			if (validationControl.AddressForValidation.Country.States.Cast<RefCountryStates>().Any(s => s.RW_Code == candidateCityTown.State))
			{
				UpdateSuggestedStateCore(candidateCityTown, candidateCityTown.State);
				return;
			}

			var suggestedState = default(string);
			var normalizedState = NormalizePunctuations.Normalize(StripDiacritics.RemoveDiacritics(candidateCityTown.State, validationControl.AddressForValidation.Country.RN_Code));

			foreach (var (stateDescriptionGetter, candidateState) in new (Func<RefCountryStates, string>, string)[]
			{
				(state =>
					state.RW_DescriptionMultilingual,
					candidateCityTown.State),
				(state =>
					state.RW_DescriptionMultilingual.GetLocalizedValue(validationControl.AddressForValidation.Language).ToString(),
					candidateCityTown.State),
				(state =>
					NormalizePunctuations.Normalize(StripDiacritics.RemoveDiacritics(state.RW_DescriptionMultilingual, validationControl.AddressForValidation.Country.RN_Code)),
					normalizedState)
			})
			{
				suggestedState = validationControl.AddressForValidation.Country
					.States
					.Cast<RefCountryStates>()
					.Where(state => state.RW_IsActive)
					.FirstOrDefault(state => string.Equals(
						stateDescriptionGetter.Invoke(state),
						candidateState,
						StringComparison.OrdinalIgnoreCase))
					?.RW_Code;

				if (suggestedState != null)
				{
					UpdateSuggestedStateCore(candidateCityTown, suggestedState);
					return;
				}
			}

			suggestedState = Res.IsEnglish(validationControl.AddressForValidation.Language) ? normalizedState : candidateCityTown.State;
			UpdateSuggestedStateCore(candidateCityTown, suggestedState);
		}

		void UpdateSuggestedStateCore(CandidateCityTown candidateCityTown, string suggestedState)
		{
			if ((suggestedState?.Length ?? 0) > validationControl.AddressForValidation.State_MaxLength)
			{
				suggestedState = suggestedState.Substring(0, validationControl.AddressForValidation.State_MaxLength);
			}

			candidateCityTown.State = suggestedState;
		}

		bool HasExactMatch(CandidateCityTown[] result)
		{
			var hasExactMatch = false;

			foreach (var candidateCityTown in result)
			{
				if ((candidateCityTown.City?.Equals(validationControl.CityControl.Text, StringComparison.OrdinalIgnoreCase) ?? false) &&
					(candidateCityTown.State?.Equals(validationControl.StateControl.Text, StringComparison.OrdinalIgnoreCase) ?? false) &&
					(candidateCityTown.Postcode?.Equals(validationControl.PostcodeControl.Text, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					hasExactMatch = true;
				}
			}

			return hasExactMatch;
		}

		void FillAddressWithSuggestion(CandidateCityTown candidate)
		{
			validationControl.AddressForValidation.IsValidatingAddress = true;

			if (candidate.City != null)
			{
				validationControl.AddressForValidation.City = candidate.City;
			}
			if (candidate.Postcode != null)
			{
				validationControl.AddressForValidation.Postcode = candidate.Postcode.ToUpper(CultureInfo.InvariantCulture);
			}

			if (validationControl.AddressForValidation.Country?.IsStateMustNotBeEntered ?? false)
			{
				validationControl.AddressForValidation.State = string.Empty;
			}
			else if (candidate.State != null)
			{
				validationControl.AddressForValidation.State = candidate.State.ToUpper(CultureInfo.InvariantCulture);
			}

			validationControl.AddressForValidation.IsValidatingAddress = false;
		}

		void AutoFillState(CandidateCityTown[] cityTowns)
		{
			if ((validationControl.AddressForValidation.Country?.IsStateMustNotBeEntered ?? false) ||
				!validationControl.AddressForValidation.State.IsEmpty)
			{
				return;
			}

			var state = cityTowns.First().State;
			if (!string.IsNullOrEmpty(state) && cityTowns.All(cityTown => cityTown.State == state))
			{
				validationControl.AddressForValidation.State = state.ToUpper(CultureInfo.InvariantCulture);
			}
		}

		Control GetActiveControlAtResponse()
		{
			var control = parentControl;
			while (control is IContainerControl containerControl)
			{
				control = containerControl.ActiveControl;
			}

			return control;
		}

		#endregion Implementation

#if !WINZOR
		#region HookTextControl

		protected override TextBox[] TextBoxControls => new TextBox[]
		{
			validationControl.CityControl,
			validationControl.PostcodeControl,
			validationControl.StateControl.CodeBox,
			validationControl.CountryControl.CodeBox
		};

		protected override (Action HookAction, Action UnHookAction) GetHookActions(TextBox textControl, CityTownSuggestionControl suggestionControl)
		{
			var (baseHookAction, baseUnHookAction) = base.GetHookActions(textControl, suggestionControl);

			var textChangedHandler = new EventHandler((x, y) =>
			{
				if ((validationControl.AddressForValidation as BusinessObject).IsRowDeletedOrDetachedOrNull)
				{
					suggestionControl.Close();
					return;
				}

				if (validationControl.AddressForValidation.IsUpdatingCityTown || (suggestionControl?.IsDisposed ?? true))
				{
					return;
				}

				suggestionControl.UpdateFilter(validationControl.CityControl.Text, validationControl.StateControl.Text, validationControl.PostcodeControl.Text);
				suggestionControl.UpdateSize();
			});

			var hookAction = new Action(() =>
			{
				baseHookAction.Invoke();
				textControl.TextChanged += textChangedHandler;
			});

			var unHookAction = new Action(() =>
			{
				baseUnHookAction.Invoke();
				textControl.TextChanged -= textChangedHandler;
			});

			return (hookAction, unHookAction);
		}

		#endregion HookTextControl
#endif
	}
}
