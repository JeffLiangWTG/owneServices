using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public partial class ShipperAddressUserControl : ZUserControl, IExtendedControl
	{
		public ShipperAddressUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this.MainGroupBox);
		}

		Control IExtendedControl.Host => this;

		public IControlExtensionCollection Extensions { get; }

		public override ResourceStringData CaptionResourceString
		{
			get { return MainGroupBox.CaptionResourceString; }
			set { MainGroupBox.CaptionResourceString = value; }
		}
	}
}
