using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MultipleEmailToContactSender))]
	public class MultipleEmailToContactSenderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckAllContactsAreReadyToSendEmail()
		{
			DummyBusinessObject businessObjectSendingEmail = Factory.New<DummyBusinessObject>();

			EmailToContactBusinessObjectCollection emailsToContacts = new EmailToContactBusinessObjectCollection(businessObjectSendingEmail);

			DummyEmailToContactBusinessObject emailContactObject1 = new DummyEmailToContactBusinessObject(businessObjectSendingEmail);
			DummyEmailToContactBusinessObject emailContactObject2 = new DummyEmailToContactBusinessObject(businessObjectSendingEmail);

			emailsToContacts.Add(emailContactObject1);
			emailsToContacts.Add(emailContactObject2);

			emailContactObject1.FillWithValidData();
			emailContactObject2.FillWithValidData();

			Assert("Email contact object 1 should not have been sent", !emailContactObject1.IsSendEmailCalled);
			Assert("Email contact object 2 should not have been sent", !emailContactObject2.IsSendEmailCalled);

			MultipleEmailToContactSender sender = new MultipleEmailToContactSender(emailsToContacts);

			Assert("All contacts should be able to send email", sender.SendToAllIfReady());
			Assert("Email contact object 1 should have been sent", emailContactObject1.IsSendEmailCalled);
			Assert("Email contact object 2 should have been sent", emailContactObject2.IsSendEmailCalled);

			emailContactObject1.IsSendEmailCalled = false; //reset
			emailContactObject2.IsSendEmailCalled = false; //reset

			Assert("Email contact object 1 should not have been sent", !emailContactObject1.IsSendEmailCalled);
			Assert("Email contact object 2 should not have been sent", !emailContactObject2.IsSendEmailCalled);

			emailContactObject1.FillWithInvalidData();
			emailContactObject2.FillWithValidData();

			Assert("Contacts should NOT be able to send email", !sender.SendToAllIfReady());
			Assert("Email contact object 1 should not have been sent", !emailContactObject1.IsSendEmailCalled);
			Assert("Email contact object 2 should not have been sent", !emailContactObject2.IsSendEmailCalled);

			emailContactObject1.FillWithValidData();

			Assert("All contacts should be able to send email", sender.SendToAllIfReady());
			Assert("Email contact object 1 should have been sent", emailContactObject1.IsSendEmailCalled);
			Assert("Email contact object 2 should have been sent", emailContactObject2.IsSendEmailCalled);

			emailContactObject1.IsSendEmailCalled = false; //reset
			emailContactObject2.IsSendEmailCalled = false; //reset

			DummyEmailToContactBusinessObject emailContactObject3 = new DummyEmailToContactBusinessObject(businessObjectSendingEmail);
			emailsToContacts.Add(emailContactObject3);

			Assert("All contacts should NOT be able to send email", !sender.SendToAllIfReady());

			Assert("Missing fields on Email Contact Object, All contacts should NOT be able to send email", !sender.SendToAllIfReady());

			emailContactObject3.FillWithValidData();
			Assert("All contacts should be able to send email", sender.SendToAllIfReady());
			Assert("Email contact object 1 should have been sent", emailContactObject1.IsSendEmailCalled);
			Assert("Email contact object 2 should have been sent", emailContactObject2.IsSendEmailCalled);
			Assert("Email contact object 3 should have been sent", emailContactObject3.IsSendEmailCalled);
		}

		public void TestEmailsSent()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			EmailToContactBusinessObjectCollection collection = new EmailToContactBusinessObjectCollection(dummy);
			MultipleEmailToContactSender sender = new MultipleEmailToContactSender(collection);
			bool emailsSentFired = false;
			sender.EmailsSent += delegate
			{ emailsSentFired = true; };
			Assert("Precondition", !emailsSentFired);

			sender.SendToAllIfReady();
			Assert(emailsSentFired);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			EmailToContactBusinessObjectCollection emailsToContacts = new EmailToContactBusinessObjectCollection(bizO);
			return new MultipleEmailToContactSender(emailsToContacts);
		}

		#endregion
	}
}
