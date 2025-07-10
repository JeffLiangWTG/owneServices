using CargoWise.EntityFramework;
using Enterprise.MailManager.Module;

namespace Enterprise.Recruiter.Module
{
	public partial class HREmailsFilterControl : MailItemFilterControl
	{
		public HREmailsFilterControl(IBusinessObjectCollection gridCollection, HREmailsFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
