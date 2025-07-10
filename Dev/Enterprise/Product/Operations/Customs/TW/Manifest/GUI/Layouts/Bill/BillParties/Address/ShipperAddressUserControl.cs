using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public partial class ShipperAddressUserControl : ZUserControl
	{
		public ShipperAddressUserControl()
		{
			InitializeComponent();
		}

		public override ResourceStringData CaptionResourceString
		{
			get { return MainGroupBox.CaptionResourceString; }
			set { MainGroupBox.CaptionResourceString = value; }
		}
	}
}
