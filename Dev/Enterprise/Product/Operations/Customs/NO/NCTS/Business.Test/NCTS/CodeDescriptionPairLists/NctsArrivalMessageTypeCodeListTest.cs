using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

sealed class NctsArrivalMessageTypeCodeListTest : TestCaseWithFactory
{
	public void TestCodeDescriptionPairs() =>
		AssertCodeDescriptionPairList("Expecting 007+044", new NctsArrivalMessageTypeCodeList(),
			("007", "Arrival notification"),
			("044", "Unloading Remarks"));
}
