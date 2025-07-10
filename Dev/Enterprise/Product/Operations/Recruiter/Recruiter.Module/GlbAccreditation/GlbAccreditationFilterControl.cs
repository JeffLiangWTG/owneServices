using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class GlbAccreditationFilterControl : ZFilterStripControl
	{
		public GlbAccreditationFilterControl()
		{
			InitializeComponent();
		}

		public GlbAccreditationFilterControl(IBusinessObjectCollection gridCollection,
			FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
