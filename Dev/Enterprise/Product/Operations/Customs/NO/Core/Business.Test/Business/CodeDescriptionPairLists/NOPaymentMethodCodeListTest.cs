using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing;

sealed class NOPaymentMethodCodeListTest : TestCaseWithFactory
{
	public void TestNOPaymentMethodCodeListPairs() => CombineAssertions(() =>
		AssertCodeDescriptionPairList(new NOPaymentMethodCodeList(),
			("N", "No Duties/VAT payable"),
			("M", "Importers Deferred"),
			("D", "Forwarders Day Credit"),
			("K", "Cash")
		));
}
