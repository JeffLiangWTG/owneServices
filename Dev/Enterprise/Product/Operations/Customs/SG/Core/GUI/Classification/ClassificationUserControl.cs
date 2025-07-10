namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class ClassificationUserControl : Customs.GUI.BaseClassificationUserControl
	{
		public ClassificationUserControl()
		{
			InitializeComponent();
			cC_TariffNumCodeFindBox.GetCountryCode = () => Core.Constants.CountryCodes.Singapore;
			cC_TariffNumCodeFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.Singapore;
		}
	}
}
