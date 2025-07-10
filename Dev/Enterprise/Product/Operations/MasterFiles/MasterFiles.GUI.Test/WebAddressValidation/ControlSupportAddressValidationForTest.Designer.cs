namespace Enterprise.MasterFiles.GUI.Test
{
	public partial class ControlSupportAddressValidationForTest
	{
		void InitializeComponent()
		{
			SuspendLayout();

			Controls.Add(ValidateButton);
			Controls.Add(ClearAddressFieldsButton);
			Controls.Add(AddressCodeControl);
			Controls.Add(AdditionalAddressInformationControl);
			Controls.Add(Address1Control);
			Controls.Add(Address2Control);
			Controls.Add(CityControl);
			Controls.Add(PostcodeControl);
			Controls.Add(StateControl);
			Controls.Add(CountryControl);

			ResumeLayout(false);
			PerformLayout();
		}
	}
}
