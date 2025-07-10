using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.GUI
{
	public partial class CarrierContractUnitPairControl : ZUserControl,
		IResCaptionedControl
	{
		public CarrierContractUnitPairControl()
		{
			InitializeComponent();
			CaptionRenderingEnabled = true;
		}

		public void SetMainLabel(ResourceStringData label)
		{
			MainLabel.CaptionResourceString = label;
		}
	}
}
