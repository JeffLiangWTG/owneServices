using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HRJobApplicationFilterControl : ZFilterStripControl
	{
		public HRJobApplicationFilterControl()
		{
			InitializeComponent();
		}

		public HRJobApplicationFilterControl(IBusinessObjectCollection gridCollection, HRJobApplicationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

