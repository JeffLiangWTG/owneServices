using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TestCombineTranslatedAddressesControl : CombineTranslatedAddressUserControl
	{
		public TestCombineTranslatedAddressesControl()
				: base()
		{ }

		public bool OA_StateDropDownVisible
		{
			get { return OA_StateBoundDropEdit.Visible; }
		}

		public ZButton ValidateAddressButton_Exposed
		{
			get { return this.ValidateAddressButton; }
		}

		public ZTextBox OA_AdditionalAddressInformationBoundTextBox_Exposed
		{
			get { return this.OA_AdditionalAddressInformationBoundTextBox; }
		}

		public ZTextBox OA_Address1BoundTextBox_Exposed
		{
			get { return this.OA_Address1BoundTextBox; }
		}

		public ZTextBox OA_Address2BoundTextBox_Exposed
		{
			get { return this.OA_Address2BoundTextBox; }
		}

		public ZTextBox OA_CityBoundTextBox_Exposed
		{
			get { return this.OA_CityBoundTextBox; }
		}

		public ZDropEdit OA_StateBoundDropEdit_Exposed
		{
			get { return this.OA_StateBoundDropEdit; }
		}

		public ZTextBox OA_PostCodeBoundTextBox_Exposed
		{
			get { return this.OA_PostCodeBoundTextBox; }
		}
	}
}
