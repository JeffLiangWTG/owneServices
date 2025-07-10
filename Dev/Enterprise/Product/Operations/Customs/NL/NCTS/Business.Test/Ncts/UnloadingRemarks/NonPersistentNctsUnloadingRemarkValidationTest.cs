using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NonPersistentNctsUnloadingRemarkValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCode()
	{
		var unloadingRemark = new NonPersistentNctsUnloadingRemark(Factory);
		var codeInfo = unloadingRemark.CodeInfo;
		CombineAssertions(() =>
		{
			unloadingRemark.Code = "";
			AssertNoMessageErrorContaining("Empty", codeInfo, "list");
			unloadingRemark.Code = "O";
			AssertNoMessageErrorContaining("O", codeInfo, "list");
			unloadingRemark.Code = "I";
			AssertNoMessageErrorContaining("I", codeInfo, "list");
			unloadingRemark.Code = "X";
			AssertHasMessageErrorContaining("X", codeInfo, "list");
		});
	}
}
