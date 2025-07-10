using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefComplianceCommodityAlertFilterControl : ZFilterStripControl
	{
		public RefComplianceCommodityAlertFilterControl()
		{
			InitializeComponent();
		}

		public RefComplianceCommodityAlertFilterControl(IBusinessObjectCollection gridCollection, RefComplianceCommodityAlertFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
