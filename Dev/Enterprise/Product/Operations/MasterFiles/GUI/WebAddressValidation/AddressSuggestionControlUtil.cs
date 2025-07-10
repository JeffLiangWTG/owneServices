using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public class AddressSuggestionControlUtil : SuggestionControlUtil<AddressSuggestionControl, WebAddressValidationResult>
	{
		public AddressSuggestionControlUtil(Control parentControl, Form parentForm, CancellationTokenSource cancellationTokenSource, AddressValidatorConfiguration config)
			: base(parentControl, parentForm, cancellationTokenSource, config, nameof(AddressSuggestionControl))
		{
		}

		public async Task ValidateAddressAsync(CleanseAction cleanseAction)
		{
			if (validationControl.AddressForValidation is null ||
				validationControl.AddressForValidation.IsValidatingAddress ||
				TryShowSuggestionControlFromCachedResult(cleanseAction) ||
				CheckIfAddressHasAnyPreValidationError())
			{
				return;
			}

			var validationResult = await ValidateAddressCore(cleanseAction);

			if (cleanseAction == CleanseAction.QuickValidate)
			{
				SetClosestPort();
			}
			else if (cleanseAction == CleanseAction.ValidateAndSuggest)
			{
				ShowValidationResult(validationResult);
			}
		}

		public override void RegisterPropertyChangedEvent(Func<Task> callService)
		{
			if (validationControl.AddressForValidation is null)
			{
				return;
			}

			validationControl.AddressForValidation.ClearWebAddressValidationHandler();
			validationControl.AddressForValidation.TriggerWebAddressValidation += (sender, e) =>
			{
				if (validationControl.AddressForValidation.NeedValidation)
				{
					callService();
				}
			};
		}

		protected virtual async Task<WebAddressValidationResult> ValidateAddressCore(CleanseAction cleanseAction)
		{
			return await validationControl.AddressForValidation.ValidateAddressAsync(cancellationTokenSource, cleanseAction);
		}

		protected virtual void SetClosestPortCore()
		{
			AddressValidationService.SetClosestPort(validationControl.AddressForValidation);
		}

		protected override AddressSuggestionControl CreateNewSuggestionControl(WebAddressValidationResult validationResult)
		{
			var control = new AddressSuggestionControl(
				validationControl.AddressForValidation,
				validationResult.SuggestedResults,
				validationResult.TopRecommendedAddress,
				parentForm,
				parentControl,
				config.ReferenceControlGetter.Invoke(validationControl),
				config.MaxWidthGetter.Invoke(),
				config.MaxHeightGetter.Invoke());

			if (config.AddressSelectedAction != null)
			{
				var handleAddressSelected = new EventHandler<AddressSuggestionControl.AddressSelectedEventArgs>((sender, e) => config.AddressSelectedAction.Invoke());
				control.AddressSelected += handleAddressSelected;
				control.Disposed += (sender, e) => control.AddressSelected -= handleAddressSelected;
			}

			if (config.HandleDestroyedAction != null)
			{
				var handleDestroyed = new EventHandler((sender, e) => config.HandleDestroyedAction.Invoke());
				control.HandleDestroyed += handleDestroyed;
				control.Disposed += (sender, e) => control.HandleDestroyed -= handleDestroyed;
			}

			return control;
		}

#if !WINZOR
		#region HookTextControl
		protected override TextBox[] TextBoxControls => new TextBox[]
		{
			validationControl.Address1Control,
			validationControl.Address2Control,
			validationControl.CityControl,
			validationControl.PostcodeControl,
			validationControl.StateControl.CodeBox,
			validationControl.CountryControl.CodeBox
		};
		#endregion HookTextControl
#endif

		#region Implementation

		string ConstructedCacheKey => validationControl.AddressForValidation is null ? string.Empty : string.Join("+",
			validationControl.AddressForValidation.Address1,
			validationControl.AddressForValidation.Address2,
			validationControl.AddressForValidation.City,
			validationControl.AddressForValidation.Postcode,
			validationControl.AddressForValidation.State,
			validationControl.AddressForValidation.Country?.Code ?? string.Empty);

		void SetClosestPort()
		{
			var addressUnverified = validationControl.AddressForValidation.ValidationStatus != AddressValidationStatus.VerifiedToStreet && validationControl.AddressForValidation.ValidationStatus != AddressValidationStatus.Verified;

			if (addressUnverified || (validationControl.AddressForValidation as BusinessObject).IsRowDeletedOrDetachedOrNull)
			{
				return;
			}

			SetClosestPortCore();
		}

		bool TryShowSuggestionControlFromCachedResult(CleanseAction cleanseAction)
		{
			if (cleanseAction == CleanseAction.ValidateAndSuggest &&
				CachedValidationResult.HasValue &&
				ConstructedCacheKey == CachedValidationResult.Value.CacheKey)
			{
				if (FindSuggestionControl() is null)
				{
					CreateAndShowSuggestionControl(CachedValidationResult.Value.Result);
				}

				return true;
			}

			return false;
		}

		/* The logic of checking if any error is just keep the same with the old (AddressSuggestionControlHelper)
		 * But the logic here will conflict with checking postcode. (see countrySpecificRulesApplying related logic)
		 * Will investigate and fix this in later WI.
		 */
		bool CheckIfAddressHasAnyPreValidationError()
		{
			validationControl.AddressForValidation.ValidationStatus = AddressValidationStatus.ToBeVerified;

			var bizO = validationControl.AddressForValidation as BusinessObject;

			var errorsBefore = bizO.Notifications?.Count() ?? 0;
			validationControl.AddressForValidation.PreValidationForAddressValidationService();
			var errorsAfter = bizO.Notifications?.Count() ?? 0;

			if (errorsAfter > errorsBefore && !config.CheckIfControlFirstLoad())
			{
				TabPageNotificationsExposer.ExposeTabPageNotifications(parentControl, bizO);

				if (!bizO.Factory.IsInTransaction)
				{
					Globals.Message.Show(Res.GetString("ea34a71f-98c5-4e6d-9672-59ee293db9ab", "Please fix errors on address before running validation."));
				}
			}

			return errorsAfter > errorsBefore;
		}

		void ShowValidationResult(WebAddressValidationResult result)
		{
			var message = result?.Message ?? result?.ResultAddress?.ErrorMessage;

			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.ShowWarning(message);
				return;
			}

			CachedValidationResult = (ConstructedCacheKey, result);

			if ((validationControl.AddressForValidation as BusinessObject).IsRowDeletedOrDetachedOrNull ||
				validationControl.AddressForValidation.ValidationStatus != AddressValidationStatus.Invalid)
			{
				return;
			}

			CloseSuggestionControlIfExists();
			CreateAndShowSuggestionControl(result);
		}

		(string CacheKey, WebAddressValidationResult Result)? CachedValidationResult { get; set; }

		#endregion Implementation
	}
}
