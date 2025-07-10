using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccInvMsgFilterControl : ZFilterStripControl
	{
		public AccInvMsgFilterControl()
		{
			InitializeComponent();
		}

		public AccInvMsgFilterControl(IBusinessObjectCollection gridCollection, AccInvMsgFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
