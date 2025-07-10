using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NctsHeaderLookupsTest : TestCaseWithFactory
{
	public void TestCommunicationLanguageList()
	{
		CombineAssertions(() =>
		{
			var communicationLanguageList = lookups.CommunicationLanguageList;
			AssertEquals("CodesAsString", "EN, NL", communicationLanguageList.CodesAsString);
			AssertEquals("EN Description", "English", communicationLanguageList.GetDescriptionFromCode(SharedConstants.Languages.English));
			AssertEquals("NL Description", "Dutch", communicationLanguageList.GetDescriptionFromCode(Constants.CountryCodes.Netherlands));

			AssertSame("Cached", communicationLanguageList, lookups.CommunicationLanguageList);
		});
	}

	public void TestCalculationMethodList()
	{
		var list = lookups.CalculationMethodList;

		CombineAssertions(() =>
		{
			AssertEquals("List for Arrival - incidents", "DUT, WGT, DEF", list.CodesAsString);
			AssertSame("Cached", list, lookups.CalculationMethodList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		lookups = Factory.New<NctsHeader>().Lookups;
	}

	NctsHeaderLookups lookups;
}
