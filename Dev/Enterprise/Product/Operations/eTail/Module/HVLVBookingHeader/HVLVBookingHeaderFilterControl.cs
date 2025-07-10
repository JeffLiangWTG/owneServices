using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.Module
{
	public partial class HVLVBookingHeaderFilterControl : ZFilterStripControl
	{
		public HVLVBookingHeaderFilterControl(IBusinessObjectCollection gridCollection, HVLVBookingHeaderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
