using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class MultipleEmailToContactSender : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MultipleEmailToContactSender(EmailToContactBusinessObjectCollection emailsToContacts)
		{
			fEmailsToContacts = emailsToContacts;
			RegisterEditableChildObject(emailsToContacts);
		}

		#region SendToAllIfNoErrors

		public bool SendToAllIfReady()
		{
			RunPreSaveValidation();

			bool result = !HasErrors;
			if (result)
			{
				foreach (EmailToContactBusinessObject emailToContact in EmailsToContacts)
				{
					emailToContact.SendEmail();
				}
				OnEmailsSent();
			}

			return result;
		}

		void OnEmailsSent()
		{
			if (EmailsSent != null)
			{
				EmailsSent(this, EventArgs.Empty);
			}
		}

		public event EventHandler EmailsSent;

		#endregion

		#region EmailsToContacts

		public EmailToContactBusinessObjectCollection EmailsToContacts
		{
			get { return fEmailsToContacts; }
		}

		readonly EmailToContactBusinessObjectCollection fEmailsToContacts;

		#endregion
	}
}
