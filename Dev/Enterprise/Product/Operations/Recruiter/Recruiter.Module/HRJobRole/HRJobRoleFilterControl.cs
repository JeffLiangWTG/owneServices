using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HRJobRoleFilterControl : ZFilterStripControl
	{
		public HRJobRoleFilterControl()
		{
			InitializeComponent();
		}

		public HRJobRoleFilterControl(IBusinessObjectCollection gridCollection, HRJobRoleFilterBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

