using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Recruiter.GUI
{
	public class HREmailsForm : EmailContactForm
	{
		public HREmailsForm(EmailToContactBusinessObject emailToContactBusinessObject)
			: base(emailToContactBusinessObject)
		{
		}
	}
}
