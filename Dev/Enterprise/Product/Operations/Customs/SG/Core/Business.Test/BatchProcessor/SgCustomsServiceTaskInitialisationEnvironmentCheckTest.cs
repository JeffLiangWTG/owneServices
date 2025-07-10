using System;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Business.BatchProcessor.Testing;
using Enterprise.Environment;
using Enterprise.Integration;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SgCustomsServiceTaskInitialisationEnvironmentCheckTest : BaseEnvironmentCheckerTest
	{
		protected override BaseEnvironmentChecker GetEnvironmentChecker()
		{
			return SgCustomsServiceTaskInitialisationEnvironmentCheckForTesting.GetNewEnvironmentChecker();
		}

		public void TestEnvironmentNotValidUntilItIsFirstValidated()
		{
			AssertEquals("Pre-condition: Environment is valid for service task", 0, SgCustomsServiceTaskInitialisationEnvironmentCheck.Validate().Length);

			using (Env.Registry.RawRegistry.MailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var errorMessages = SgCustomsServiceTaskInitialisationEnvironmentCheck.Validate();
				CombineAssertions("Environment is not valid for service task", () =>
				{
					AssertEquals(1, errorMessages.Length);
					AssertContains("The registry setting 'Physical Server -> Mail -> Incoming -> Mail Server' has not been configured.", errorMessages[0]);
				});
			}
		}

		class SgCustomsServiceTaskInitialisationEnvironmentCheckForTesting : SgCustomsServiceTaskInitialisationEnvironmentCheck
		{
			public static BaseEnvironmentChecker GetNewEnvironmentChecker()
			{
				return new BatchProcessorEnvironmentChecker();
			}
		}
	}
}
