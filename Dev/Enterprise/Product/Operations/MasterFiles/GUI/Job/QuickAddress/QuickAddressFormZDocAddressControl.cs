using System.Threading.Tasks;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI.WebAddressValidation;

namespace Enterprise.MasterFiles.GUI
{
	class QuickAddressFormZDocAddressControl : ZDocAddressControl
	{
		const int LocationX = 263;
		const int LocationY = 48;
		const int SizeExtensionX = 273;
		const int SizeExtentionY = 87;

		protected override async Task ValidateAddress()
		{
			await base.ValidateAddress();

			if (this?.SuggestionWindowParentControl != null)
			{
				var addressSuggestionControl = SupportWebAddressValidationControlHelper.FindAddressSuggestionControl(this);

				if (addressSuggestionControl != null)
				{
					addressSuggestionControl.Location = ControlDpiScalingHelper.NewScaledPoint(LocationX, LocationY);
					ParentForm.Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(addressSuggestionControl.Width) + SizeExtensionX, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(addressSuggestionControl.Height) + SizeExtentionY);
				}
			}
		}

		protected override async Task GetCityTownAsync()
		{
			await base.GetCityTownAsync();

			if (ParentForm?.Controls != null)
			{
				var cityTownSuggestionControl = CityTownSuggestionControlHelper.FindSuggestionForm(ParentForm.Controls);

				if (cityTownSuggestionControl != null)
				{
					cityTownSuggestionControl.Location = ControlDpiScalingHelper.NewScaledPoint(LocationX, LocationY);
					ParentForm.Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(cityTownSuggestionControl.Width) + SizeExtensionX, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(cityTownSuggestionControl.Height) + SizeExtentionY);
				}
			}
		}

		protected override void CloseSuggestionForms()
		{
			base.CloseSuggestionForms();
			ParentForm.Size = ControlDpiScalingHelper.NewScaledSize(273, 253);
		}
	}
}
