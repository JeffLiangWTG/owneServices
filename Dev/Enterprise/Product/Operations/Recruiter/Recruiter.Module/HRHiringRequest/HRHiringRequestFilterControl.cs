using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HRHiringRequestFilterControl : ZFilterStripControl
	{
		public HRHiringRequestFilterControl()
		{
			InitializeComponent();
		}

		public HRHiringRequestFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
