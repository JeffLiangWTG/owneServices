using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USFDAProcessingCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var provider = CreateCodeDescriptionPairListProvider() as DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider;
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.BIO), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.BIO));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.COS), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.COS));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.DEV), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.DEV));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.DRU), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.DRU));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.FOO), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.FOO));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.RAD), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.RAD));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.TOB), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.TOB));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(FDAProgramCodeList.Codes.VME), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.VME));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList("bla"), USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList("bla"));
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new USFDAProcessingCodeDescriptionPairProvider();
	}
}
