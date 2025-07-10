using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(EntryHeaderFilterLookups))]
sealed class EntryHeaderFilterLookupsTest : TestCaseWithFactory
{
	public void TestMessageStatusList()
	{
		var actualCodes = Lookups.MessageStatusList().GetAllCodes();

		AssertContainsExactElementsInAnyOrder("MessageStatusList", new[] { "ACC", "CAN", "ERR", "INV", "REM", "SNT" }, Lookups.MessageStatusList().GetAllCodes());
	}

	EntryHeaderFilterLookups Lookups => lookups ??= new EntryHeaderFilterLookups(new EntryHeaderFilterBusinessObject());
	EntryHeaderFilterLookups lookups;
}
