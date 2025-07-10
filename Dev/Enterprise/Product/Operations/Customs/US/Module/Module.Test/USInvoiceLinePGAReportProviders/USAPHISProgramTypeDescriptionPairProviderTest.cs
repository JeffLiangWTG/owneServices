using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USAPHISProgramTypeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), (CodeDescriptionPairList)Business.APHISProgramCodeList.GetActiveList(new CargoWise.EntityFramework.BusinessObjectFactory()));
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new USAPHISProgramTypeDescriptionPairProvider();
	}
}
