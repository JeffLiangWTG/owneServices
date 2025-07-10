using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class GlbStaffChangeRequestFilterControl : ZFilterStripControl
	{
		public GlbStaffChangeRequestFilterControl()
		{
			InitializeComponent();
		}

		public GlbStaffChangeRequestFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
