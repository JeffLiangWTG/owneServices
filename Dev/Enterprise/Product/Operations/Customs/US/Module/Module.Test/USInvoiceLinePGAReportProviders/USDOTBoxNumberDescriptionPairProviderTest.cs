using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USDOTBoxNumberDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var provider = CreateCodeDescriptionPairListProvider() as DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider;
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(NHTSAProgramCodeList.Codes.MVS), USNHTSAAddInfoLookups.GetProgramCodeRelatedBoxNumberList(NHTSAProgramCodeList.Codes.MVS));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(NHTSAProgramCodeList.Codes.OEI), USNHTSAAddInfoLookups.GetProgramCodeRelatedBoxNumberList(NHTSAProgramCodeList.Codes.OEI));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(NHTSAProgramCodeList.Codes.OFF), USNHTSAAddInfoLookups.GetProgramCodeRelatedBoxNumberList(NHTSAProgramCodeList.Codes.OFF));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(NHTSAProgramCodeList.Codes.REI), USNHTSAAddInfoLookups.GetProgramCodeRelatedBoxNumberList(NHTSAProgramCodeList.Codes.REI));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(NHTSAProgramCodeList.Codes.TPE), USNHTSAAddInfoLookups.GetProgramCodeRelatedBoxNumberList(NHTSAProgramCodeList.Codes.TPE));
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList("bla"), USNHTSAAddInfoLookups.GetProgramCodeRelatedBoxNumberList("bla"));
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new USDOTBoxNumberDescriptionPairProvider();
	}
}
