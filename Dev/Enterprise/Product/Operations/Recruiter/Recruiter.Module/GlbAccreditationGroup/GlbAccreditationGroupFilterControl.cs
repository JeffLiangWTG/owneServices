using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class GlbAccreditationGroupFilterControl : ZFilterStripControl
	{
		public GlbAccreditationGroupFilterControl()
		{
			InitializeComponent();
		}

		public GlbAccreditationGroupFilterControl(IBusinessObjectCollection gridCollection,
			FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
