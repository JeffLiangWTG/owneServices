using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class PreviousProcedureCodesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
			var factory = new BusinessObjectFactory();
			var procedureCodes = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountry(factory, "ZA", ZDateTime.Today);
			var provider = CreateCodeDescriptionPairListProvider() as DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider;
			int ppcTotalCount = 0;
			foreach (var cpc in procedureCodes.Select(x => x.ZZ6_ProcedureCode))
			{
				var list1 = provider.GetDependenceCodeDescriptionPairList(cpc);
				var list2 = provider.GetDependenceCodeDescriptionPairList(cpc);
				var ppcCount = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryProcedureCode(factory, "ZA", cpc, ZDateTime.Today).Count;
				ppcTotalCount += ppcCount;
				AssertEquals(ppcCount, list1.Count);
				AssertEquals(list1, list2);
			}

			var allList1 = provider.GetCodeDescriptionPairList();
			var allList2 = provider.GetDependenceCodeDescriptionPairList(string.Empty);
			AssertEquals(ppcTotalCount, allList1.Count);
			AssertEquals(allList1, allList2);
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new PreviousProcedureCodesCodeDescriptionPairProvider();
	}
}
