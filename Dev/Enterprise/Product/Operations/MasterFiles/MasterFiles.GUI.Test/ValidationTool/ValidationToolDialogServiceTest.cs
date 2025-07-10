using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing;

sealed class ValidationToolDialogServiceTest : TestCaseWithFactory
{
	public void TestProceed() => CombineAssertions(() =>
	{
		using var suspendDispose = ZFormModaliser.SuspendDispose();
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
		var proceed = new ValidationToolDialogService().Proceed(failure);
		AssertType<ValidationToolFailurePopup>("Form type", ZFormModaliser.LastFormShownDialogForTest);
		AssertEquals("Proceed?", true, proceed);
		ZFormModaliser.LastFormShownDialogForTest.Dispose();
	});

	public void TestProceed_IfNullInput() => CombineAssertions(() =>
	{
		using var suspendDispose = ZFormModaliser.SuspendDispose();
		var proceed = new ValidationToolDialogService().Proceed(null);
		AssertEquals("Proceed?", false, proceed);
		AssertNull("No form shown", ZFormModaliser.LastFormShownDialogForTest);
	});
}
