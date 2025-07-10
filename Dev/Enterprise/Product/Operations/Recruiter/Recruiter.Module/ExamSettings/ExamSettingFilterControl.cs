using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class ExamSettingFilterControl : ZFilterStripControl
	{
		public ExamSettingFilterControl()
		{
			InitializeComponent();
		}

		public ExamSettingFilterControl(IBusinessObjectCollection gridCollection,
			FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
