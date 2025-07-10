using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module.Testing
{
	class ProcedureCodesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
			var procedureCodes = new CodeDescriptionPairList();
			foreach (var cpc in RefCusProcedureCollection.LoadCustomsProcedureCodesForCountry(new BusinessObjectFactory(), GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today))
			{
				procedureCodes.AddPairIfNotExist(cpc.ZZ6_ProcedureCode, cpc.ZZ6_Description);
			}

			procedureCodes.Sort();
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), procedureCodes);
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new ProcedureCodesCodeDescriptionPairProvider();
	}
}
