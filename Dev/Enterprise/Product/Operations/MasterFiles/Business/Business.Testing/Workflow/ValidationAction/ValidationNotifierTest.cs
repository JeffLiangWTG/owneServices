using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing
{
	sealed class ValidationNotifierTest : TestCaseWithFactory
	{
		public void TestResultingEmail()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.HumanReadableNameForTest = "Dummy Job D00001101";

			var errors = new[]
				{
					"Error - Those shoes don't go with those pants.",
					"Message Error - There's a leter in your mailbox.",
					"Warning - Time to change your socks.",
				};

			var notifier = new ValidationNotifier("johnny@therockets.nom", dummyBO, Factory);
			notifier.SendValidationFailureNotification(errors, "http://a.com");

			AssertEquals("Emails created during processing", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "Validation Failure on Dummy Job D00001101.", email.Subject);
				AssertEquals("email.Recipients", "johnny@therockets.nom", email.Recipients.RecipientsAsDelimitedString());
				AssertContains("email.Body", @"
Validation was run on <a href=""http://a.com"">Dummy Job D00001101</a>, and the following failures occurred:-<br><br>
Error - Those shoes don't go with those pants.<br>
Message Error - There's a leter in your mailbox.<br>
Warning - Time to change your socks.
					".Trim(), email.Body);
			});
		}
	}
}
