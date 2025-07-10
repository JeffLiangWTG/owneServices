using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusCodeForMatchingTest : TestCase
	{
		public void TestOK_CustomsRegNoHasChanges()
		{
			Assert("should be true", new OrgCusCodeForMatching().OK_CustomsRegNoHasChanges);
		}

		public void TestOK_CodeTypeHasChanges()
		{
			Assert("should be true", new OrgCusCodeForMatching().OK_CodeTypeHasChanges);
		}
	}
}
