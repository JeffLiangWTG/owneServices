using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USFDAAgencyProgramCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), new Business.FDAProgramCodeList());
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new USFDAAgencyProgramCodeDescriptionPairProvider();
	}
}
