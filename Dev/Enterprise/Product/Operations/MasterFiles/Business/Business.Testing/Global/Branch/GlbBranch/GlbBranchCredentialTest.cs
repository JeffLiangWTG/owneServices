using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class GlbBranchCredentialTest<T> : GlbExternalPasswordWithPasswordTypeTest<T> where T : GlbBranchCredential
	{
		public void TestSetDefaultValues()
		{
			var credential = Factory.NewWithValidTestData<T>();

			AssertEquals(nameof(credential.GP_GS), ZGuid.Empty, credential.GP_GS);
			AssertNotEquals(nameof(credential.GP_GC), ZGuid.Empty, credential.GP_GC);
			AssertNotEquals(nameof(credential.GP_GB), ZGuid.Empty, credential.GP_GB);
		}
	}
}
