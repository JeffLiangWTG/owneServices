using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccPOSChargeCodeGroupFilterControl : ZFilterStripControl
	{
		public AccPOSChargeCodeGroupFilterControl(IBusinessObjectCollection gridCollection, AccPOSChargeCodeGroupFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
