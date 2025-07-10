using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class CertificateTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var actualList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var expectedList = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value.GetActiveCodeDescriptionPairList();

			AssertEquals(expectedList.Count, actualList.Count);
			for (var i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actualList[i].Code);
				AssertEquals(expectedList[i].Description, actualList[i].Description);
			}
		}

		#endregion

		#region Implementation

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new CertificateTypeCodeDescriptionPairListProvider();
		}

		#endregion
	}
}
