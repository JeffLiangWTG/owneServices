using NUnit.Framework;
#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionAgreementApprovalExceptionTest : TestCase
	{
		public void TestSerialization()
		{
#if NETFRAMEWORK
			var original = new CommissionAgreementApprovalException("log-message", "user-message");
			var copy = SerializationTestWithAppDomainHelper.PassBetweenAppDomains(original) as CommissionAgreementApprovalException;
			Assert("precondition", copy != original);
			AssertEquals("log-message", copy.Message);
			AssertEquals("user-message", copy.UserFriendlyMessage);
#else
			Fail("This test has not been upgraded to support .NET Core yet");
#endif
		}
	}
}
