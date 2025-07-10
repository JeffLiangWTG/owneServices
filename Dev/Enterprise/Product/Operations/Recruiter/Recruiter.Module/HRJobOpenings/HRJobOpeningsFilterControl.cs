
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HRJobOpeningsFilterControl : ZFilterStripControl
	{
		public HRJobOpeningsFilterControl()
		{
			InitializeComponent();
		}

		public HRJobOpeningsFilterControl(IBusinessObjectCollection gridCollection, HRJobOpeningsFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

