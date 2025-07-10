using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefAccessorialFilterControl : ZFilterStripControl
	{
		public RefAccessorialFilterControl()
		{
			InitializeComponent();
		}

		public RefAccessorialFilterControl(IBusinessObjectCollection gridCollection, RefAccessorialFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
