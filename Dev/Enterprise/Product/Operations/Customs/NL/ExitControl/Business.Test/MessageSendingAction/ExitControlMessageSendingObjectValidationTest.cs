using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.NL.ExitControl.Business.Testing;

sealed class ExitControlMessageSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckEntryType()
	{
		var exitReport = Factory.New<CusExitReport>();
		var control = new ExitControlMessageSendingObject(exitReport);
		var expectedMessageError = ListValidation.InvalidCodeError;
		var entryTypeInfo = control.EntryTypeInfo;

		CombineAssertions(() =>
		{
			control.EntryType = ZString.Empty;
			AssertHasErrors("error when empty", entryTypeInfo);

			control.EntryType = EntryTypeList.Codes.OriginalDeclaration;
			AssertNoErrorContaining("valid", entryTypeInfo, expectedMessageError);

			control.EntryType = "x";
			AssertHasErrorContaining("invalid", entryTypeInfo, expectedMessageError);
		});
	}
}
