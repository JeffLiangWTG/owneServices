using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	public abstract class BaseEnvironmentCheckerTest : TestCaseWithFactory
	{
		public virtual void TestSMTPServerIsSet()
		{
			Env.Registry.SMTPServer = "";
			CheckFailureDescriptionMatches("The registry setting 'Physical Server -> Mail -> Outgoing -> SMTP -> SMTP Server' has not been configured.");
		}

		public virtual void TestSMTPPortIsSet()
		{
			Env.Registry.SMTPPort = 0;
			CheckFailureDescriptionMatches("The registry setting 'Physical Server -> Mail -> Outgoing -> SMTP -> SMTP Port' requires a value other than '0'.");
		}

		public virtual void TestMailServerIsSet()
		{
			Env.Registry.MailServer = "";
			CheckFailureDescriptionMatches("The registry setting 'Physical Server -> Mail -> Incoming -> Mail Server' has not been configured.");
		}

		public virtual void TestMailServerPortIsSet()
		{
			Env.Registry.MailServerPort = 0;
			CheckFailureDescriptionMatches("The registry setting 'Physical Server -> Mail -> Incoming -> Mail Server Port' requires a value other than '0'.");
		}

		#region Implementation
		protected virtual void SetupValidSettings()
		{
			Env.Registry.SMTPServer = "smtp.edi.com.au";
			Env.Registry.SMTPPort = 110;
			Env.Registry.MailServer = "pop3.edi.com.au";
			Env.Registry.MailServerPort = 25;
		}

		void MakeSureAllSetupsAreValid()
		{
			var environmentChecker = GetEnvironmentChecker();
			var failureDescriptions = environmentChecker.CheckEverythingRequiredToRunIsInPlace();
			AssertEquals("Base setup contained configuration errors: " + string.Join(", ", failureDescriptions), 0, failureDescriptions.Length);
		}

		protected abstract BaseEnvironmentChecker GetEnvironmentChecker();

		protected void CheckFailureDescriptionMatches(string expectedFailureDescription)
		{
			var environmentChecker = GetEnvironmentChecker();
			var failureDescriptions = environmentChecker.CheckEverythingRequiredToRunIsInPlace();
			AssertEquals(1, failureDescriptions.Length);
			AssertEquals(expectedFailureDescription, failureDescriptions[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupValidSettings();
			MakeSureAllSetupsAreValid();
		}
		#endregion
	}
}
