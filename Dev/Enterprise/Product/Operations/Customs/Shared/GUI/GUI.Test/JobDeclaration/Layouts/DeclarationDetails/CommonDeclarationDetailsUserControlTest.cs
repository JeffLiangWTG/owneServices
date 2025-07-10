using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonDeclarationDetailsUserControl))]
	sealed class CommonDeclarationDetailsUserControlTest : TestCase
	{
		public void TestSelfControl()
		{
			using var control = new CommonDeclarationDetailsUserControl();
			_ = control.AssertThisControl(x => x
				.WithBindingSource<BaseJobDeclaration>()
				.WithCaptionRenderingEnabled(true)
			);
		}

		public void TestControls() => CombineAssertions(() =>
		{
			using var control = new CommonDeclarationDetailsUserControl();
			_ = control.AssertContainsControl<ZTextBox>("DeclarationNumberTextBox", x => x
				.WithBindTo(nameof(BaseJobDeclaration.DeclarationNumber))
			);
			_ = control.AssertContainsControl<ZTextBox>("StatusTextBox", x => x
				.WithBindTo(nameof(BaseJobDeclaration.JE_EntryStatusDescription))
			);
		});
	}
}
