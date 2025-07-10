using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

sealed class NctsDepartureMessageTypeCodeListTest : TestCaseWithFactory
{
	public void TestCodeDescriptionPairs() =>
		AssertCodeDescriptionPairList("Departure codes not yet provided by PM", new NctsDepartureMessageTypeCodeList(),
			("HLP", "Help me decide"));
}
