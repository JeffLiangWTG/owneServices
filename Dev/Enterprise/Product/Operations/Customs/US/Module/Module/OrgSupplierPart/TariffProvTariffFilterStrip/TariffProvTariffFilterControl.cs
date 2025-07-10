using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public partial class TariffProvTariffFilterControl : ZUserControl
	{
		public TariffProvTariffFilterControl()
		{
			InitializeComponent();
		}

		public static TariffProvTariffFilterControl New(bool hideProvTariffField)
		{
			var result = new TariffProvTariffFilterControl();
			if (hideProvTariffField)
			{
				result.ProvTariffTextBox.Visible = false;
				result.TariffTextBox.CaptionResourceString = new ResourceStringData("", "");
			}
			ControlDpiScalingHelper.SetHeight(ref result, result.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			return result;
		}
	}
}
