using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NL.Module.Testing;

sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
{
	public void TestEntryStatusList()
	{
		var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
		var list = lookups.EntryStatusList();
		var expectedCodes = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Now);
		AssertContainsExactElementsInAnyOrder(expectedCodes, list);
	}

	public void TestMessageStatusList()
	{
		var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
		var actualType = lookups.MessageStatusList().GetType();
		AssertEquals("Expected MessageStatusList to be of type CustomsEntryMessageStatusList",typeof(CustomsEntryMessageStatusList), actualType);
	}
}

