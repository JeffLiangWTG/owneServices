using CargoWise.RefDbRepo.CAReferenceData.Business;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.Utils
{
	[TestFixture]
	internal class EmailSenderTest : TestWithApplicationTestConfig
	{
		[Test]
		public void TestSendEmail()
		{
			EmailSender.SentEmails.Clear();
			EmailSender.SendEmail("XXX", "Test message");
			Assert.That(EmailSender.SentEmails.Count, Is.EqualTo(1));

			var email = EmailSender.SentEmails[0];
			Assert.AreEqual("donotreply_refservice@wisetechglobal.com", email.From);
			Assert.AreEqual("ReplaceThisMailAddress@IfYouWantToTestRealMailSending.com", email.To);
			Assert.True(email.Subject.StartsWith("[CAReferenceData XXX] Notification"));
			Assert.AreEqual("Test message", email.Body);
		}

		//Becareful, confirm config relevant to email.
		[Explicit("Developer Test for local integration")]
		public void TestSendEmail_DeveloperTestOnly()
		{
			Assert.DoesNotThrow(() => EmailSender.SendEmail(Email.GenerateCommonEmail("XXX", "WARNING THIS IS NOT A DRILL")));
		}
	}
}
