using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgMatchApprovalFilterControl : ZFilterStripControl
	{
		public OrgMatchApprovalFilterControl(BusinessObjectCollection gridCollection, OrgMatchApprovalFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
