using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HREmailToContactBusinessObject : EmailToContactBusinessObject
	{
		public HREmailToContactBusinessObject(BusinessObject businessObjectSendingEmail)
			: base(businessObjectSendingEmail)
		{
		}

		protected override void PostSendEmail()
		{
			base.PostSendEmail();
			OnEmailSent();
		}

		public event EventHandler EmailSent;

		void OnEmailSent()
		{
			EmailSent?.Invoke(this, EventArgs.Empty);
		}
	}
}
