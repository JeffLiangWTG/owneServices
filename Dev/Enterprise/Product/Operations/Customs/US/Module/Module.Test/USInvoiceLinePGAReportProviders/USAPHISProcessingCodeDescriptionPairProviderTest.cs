using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USAPHISProcessingCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			var provider = CreateCodeDescriptionPairListProvider() as DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider;
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(APHISProgramCodeList.Codes.ABS), (CodeDescriptionPairList)APHISGovernmentAgencyProcessingCodeList.GetListForProgram(factory, APHISProgramCodeList.Codes.ABS));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(APHISProgramCodeList.Codes.APQ), (CodeDescriptionPairList)APHISGovernmentAgencyProcessingCodeList.GetListForProgram(factory, APHISProgramCodeList.Codes.APQ));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(APHISProgramCodeList.Codes.AAC), (CodeDescriptionPairList)APHISGovernmentAgencyProcessingCodeList.GetListForProgram(factory, APHISProgramCodeList.Codes.AAC));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(APHISProgramCodeList.Codes.AVS), (CodeDescriptionPairList)APHISGovernmentAgencyProcessingCodeList.GetListForProgram(factory, APHISProgramCodeList.Codes.AVS));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList("bla"), (CodeDescriptionPairList)APHISGovernmentAgencyProcessingCodeList.GetListForProgram(factory, "bla"));
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new USAPHISProcessingCodeDescriptionPairProvider();
	}
}
