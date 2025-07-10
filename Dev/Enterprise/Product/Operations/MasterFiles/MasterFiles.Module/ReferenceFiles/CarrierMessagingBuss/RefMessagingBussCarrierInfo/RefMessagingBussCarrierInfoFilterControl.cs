using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefMessagingBussCarrierInfoFilterControl : ZFilterStripControl
	{
		public RefMessagingBussCarrierInfoFilterControl()
		{
			InitializeComponent();
		}

		public RefMessagingBussCarrierInfoFilterControl(IBusinessObjectCollection gridCollection, RefMessagingBussCarrierInfoFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
