using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AuthCertNotFoundExceptionTest : TestCaseWithFactory
	{
		public void TestAuthCertNotFoundException_Should_PopulateCorrectExceptionMessage()
		{
			AssertEquals(new AuthCertNotFoundException().Message, "System to system trust certificate not found.");
		}
	}
}
