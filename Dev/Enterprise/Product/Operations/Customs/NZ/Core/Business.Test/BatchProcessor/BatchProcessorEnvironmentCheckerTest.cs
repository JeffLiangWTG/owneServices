using System;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Environment;

namespace Enterprise.Customs.NZ.Business.BatchProcessor.Testing
{
	public class BatchProcessorEnvironmentCheckerTest : Customs.Business.BatchProcessor.Testing.BaseEnvironmentCheckerTest
	{
		public void TestNZBrokerageIDIsSet()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			CheckFailureDescriptionMatches("The registry setting 'Customs -> Country or Region Specific -> New Zealand -> Brokerage ID' must be set to a value longer than '4' characters.");
		}

		public override void TestSMTPServerIsSet()
		{
			Env.Registry.SMTPServer = "";
			CheckNZEnvironmentFailure();
		}

		public override void TestSMTPPortIsSet()
		{
			Env.Registry.SMTPPort = 0;
			CheckNZEnvironmentFailure();
		}

		public override void TestMailServerIsSet()
		{
			Env.Registry.MailServer = "";
			CheckNZEnvironmentFailure();
		}

		public override void TestMailServerPortIsSet()
		{
			Env.Registry.MailServerPort = 0;
			CheckNZEnvironmentFailure();
		}

		protected override void SetupValidSettings()
		{
			Env.Registry.SMTPServer = "";
			Env.Registry.SMTPPort = 0;
			Env.Registry.MailServer = "";
			Env.Registry.MailServerPort = 0;
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}

		protected void CheckNZEnvironmentFailure()
		{
			var environmentChecker = GetEnvironmentChecker();
			var array = environmentChecker.CheckEverythingRequiredToRunIsInPlace();
			AssertEquals(0, array.Length);
		}

		protected override BaseEnvironmentChecker GetEnvironmentChecker()
		{
			return new BatchProcessorEnvironmentChecker();
		}
	}
}
