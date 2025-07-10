using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusSupplyChainActorReferenceProviderTest : TestCaseWithFactory
{
	public void TestGetByDataGroupingCode()
	{
		var provider = EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Poland);
		CombineAssertions(() =>
		{
			AssertType<CusSupplyChainActorReferenceProvider>("Type", provider);
			AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Poland, provider.DataGroupingCode);
		});
	}

	public void TestOverwrittenReferenceColumnCaption()
	{
		AssertEquals("Identification (TCUI/EORI)", EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Poland).OverwrittenReferenceColumnCaption);
	}
}
