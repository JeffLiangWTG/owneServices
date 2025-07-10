using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	/// <summary>
	/// Filter control for HRJobApplicant.
	/// </summary>
	public partial class HRJobApplicantFilterControl : ZFilterStripControl
	{
		public HRJobApplicantFilterControl()
		{
			InitializeComponent();
		}

		public HRJobApplicantFilterControl(IBusinessObjectCollection gridCollection, HRJobApplicantFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

