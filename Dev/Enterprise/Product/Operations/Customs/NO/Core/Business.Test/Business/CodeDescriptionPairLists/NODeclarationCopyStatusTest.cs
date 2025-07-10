using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing;

sealed class NODeclarationCopyStatusTest : TestCaseWithFactory
{
	public void TestNODeclarationCopyStatus() => CombineAssertions(() =>
		AssertCodeDescriptionPairList(new NODeclarationCopyStatus(),
			("REC", "Recalculation"),
			("FIN", "Final Import"),
			("REX", "Re Export")
		));
}
