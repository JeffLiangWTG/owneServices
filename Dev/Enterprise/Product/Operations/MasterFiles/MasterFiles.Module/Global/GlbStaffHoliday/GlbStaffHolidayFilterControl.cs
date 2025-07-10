using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class GlbStaffHolidayFilterControl : ZFilterStripControl
	{
		public GlbStaffHolidayFilterControl()
		{
			InitializeComponent();
		}

		public GlbStaffHolidayFilterControl(IBusinessObjectCollection gridCollection, GlbStaffHolidayFilterBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
