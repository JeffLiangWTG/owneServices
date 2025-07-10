using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USDOTAgencyProgramCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), new Business.NHTSAProgramCodeList());
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new USDOTAgencyProgramCodeDescriptionPairProvider();
	}
}
