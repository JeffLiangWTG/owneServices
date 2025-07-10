using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public class AddressValidatorConfiguration
	{
		public AddressValidatorConfiguration WithCustomSuggestionWindowMaxWidthGetter(Func<int> maxWidthGetter)
		{
			MaxWidthGetter = maxWidthGetter;
			ShouldHookControlResize = true;
			return this;
		}

		public AddressValidatorConfiguration WithCustomSuggestionWindowMaxHeightGetter(Func<int> maxHeightGetter)
		{
			MaxHeightGetter = maxHeightGetter;
			ShouldHookControlResize = true;
			return this;
		}

		public AddressValidatorConfiguration WithCurrentSelectedTabCheck(Func<bool> currentSelectedTabCheck)
		{
			CurrentSelectedTabCheck = currentSelectedTabCheck;
			return this;
		}

		public AddressValidatorConfiguration WithCustomValidateAddress(Func<Func<Task>, Func<Task>> validateAddress)
		{
			ValidateAddress = validateAddress;
			return this;
		}

		public AddressValidatorConfiguration WithCustomGetCityTownAsync(Func<Func<Task>, Func<Task>> getCityTownAsync)
		{
			GetCityTownAsync = getCityTownAsync;
			return this;
		}

		public AddressValidatorConfiguration WithCustomValidateButtonClick(Action<Action> validateButtonClick)
		{
			ValidateButtonClick = validateButtonClick;
			return this;
		}

		public AddressValidatorConfiguration WithOrgAddressSecuritiesCheckWhenButtonClick(Func<OrgAddress> currentOrgAddressGetter)
		{
			CurrentOrgAddressGetter = currentOrgAddressGetter;
			return this;
		}

		public AddressValidatorConfiguration WithStateAndPostcodeRequiredWhenButtonClick()
		{
			ButtonClickWithStateAndPostcodeRequired = true;
			return this;
		}

		public AddressValidatorConfiguration WithRefreshValidationStatus(
			Action overrideRefreshValidationStatus,
			Func<bool> shouldValidate,
			Func<string> validationStatus,
			Action runWhenRefreshValidationStatus_ShouldValidate,
			Action runWhenRefreshValidationStatus_ShouldNotValidate)
		{
			OverrideRefreshValidationStatus = overrideRefreshValidationStatus;
			ShouldValidate = shouldValidate;
			ValidationStatus = validationStatus;
			RunWhenRefreshValidationStatus_ShouldValidate = runWhenRefreshValidationStatus_ShouldValidate;
			RunWhenRefreshValidationStatus_ShouldNotValidate = runWhenRefreshValidationStatus_ShouldNotValidate;
			return this;
		}

		public AddressValidatorConfiguration WithCheckIfControlFirstLoad(Func<bool> checkIfControlFirstLoad)
		{
			CheckIfControlFirstLoad = checkIfControlFirstLoad;
			return this;
		}

		public AddressValidatorConfiguration WithReferenceControlGetter(Func<ISupportWebAddressValidationControl, Control> referenceControlGetter)
		{
			ReferenceControlGetter = referenceControlGetter;
			return this;
		}

		public AddressValidatorConfiguration WithAddressSelectedAction(Action addressSelectedAction)
		{
			AddressSelectedAction = addressSelectedAction;
			return this;
		}

		public AddressValidatorConfiguration WithHandleDestroyedAction(Action handleDestroyedAction)
		{
			HandleDestroyedAction = handleDestroyedAction;
			return this;
		}

		public AddressValidatorConfiguration WithSelectCityTownAction(Action selectCityTownAction)
		{
			SelectCityTownAction = selectCityTownAction;
			return this;
		}

		public bool ShouldHookControlResize { get; private set; }
		public Action OverrideRefreshValidationStatus { get; private set; }
		public Func<bool> ShouldValidate { get; private set; }
		public Func<string> ValidationStatus { get; private set; }
		public Action RunWhenRefreshValidationStatus_ShouldValidate { get; private set; }
		public Action RunWhenRefreshValidationStatus_ShouldNotValidate { get; private set; }
		public Func<int> MaxWidthGetter { get; private set; } = () => 0;
		public Func<int> MaxHeightGetter { get; private set; } = () => 0;
		public Func<bool> CurrentSelectedTabCheck { get; private set; } = () => true;
		public Func<Func<Task>, Func<Task>> ValidateAddress { get; private set; } = core => core;
		public Func<Func<Task>, Func<Task>> GetCityTownAsync { get; private set; } = core => core;
		public Action<Action> ValidateButtonClick { get; private set; } = (clickCore) => clickCore.Invoke();
		public Func<OrgAddress> CurrentOrgAddressGetter { get; private set; }
		public bool ButtonClickWithStateAndPostcodeRequired { get; private set; }
		public Func<bool> CheckIfControlFirstLoad { get; private set; } = () => false;
		public Func<ISupportWebAddressValidationControl, Control> ReferenceControlGetter { get; private set; } = (validationControl) => validationControl.ValidateButton;
		public Action AddressSelectedAction { get; private set; }
		public Action SelectCityTownAction { get; private set; }
		public Action HandleDestroyedAction { get; private set; }
	}
}
