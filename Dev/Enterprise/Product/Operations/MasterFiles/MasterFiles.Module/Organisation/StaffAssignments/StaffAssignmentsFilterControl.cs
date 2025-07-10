using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class StaffAssignmentsFilterControl : ZFilterStripControl
	{
		public StaffAssignmentsFilterControl(IBusinessObjectCollection collection, StaffAssignmentsFilterBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
