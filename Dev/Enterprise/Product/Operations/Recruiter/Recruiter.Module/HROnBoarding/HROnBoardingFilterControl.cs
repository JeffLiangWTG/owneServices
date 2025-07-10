using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HROnBoardingFilterControl : ZFilterStripControl
	{
		public HROnBoardingFilterControl()
		{
			InitializeComponent();
		}

		public HROnBoardingFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
