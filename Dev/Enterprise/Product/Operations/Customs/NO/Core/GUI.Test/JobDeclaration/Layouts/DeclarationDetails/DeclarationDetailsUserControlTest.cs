using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(DeclarationDetailsUserControl))]
sealed class DeclarationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestSelfControl() => CombineAssertions(() =>
	{
		using var control = new DeclarationDetailsUserControl();
		_ = control.AssertThisControl(x => x
			.WithBindingSource<JobDeclaration>()
			.WithCaptionRenderingEnabled()
		);
	});

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new DeclarationDetailsUserControl();
		_ = control.AssertContainsControl<ZTextBox>("PhaseStatusTextBox", x => x
		.WithCaption("Phase Status")
		.WithBindTo(nameof(JobDeclaration.Schema.PhaseStatusDescription))
		.WithReadOnly());
		_ = control.AssertContainsControl<ZTextBox>("MessageStatusTextBox", x => x
		.WithCaption("Message Status")
		.WithBindTo(nameof(JobDeclaration.MessageStatus))
		.WithReadOnly());
	});
}
