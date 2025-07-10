using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefComplianceListFilterControl : ZFilterStripControl
	{
		public RefComplianceListFilterControl()
		{
			InitializeComponent();
		}

		public RefComplianceListFilterControl(IBusinessObjectCollection gridCollection, RefComplianceListFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
