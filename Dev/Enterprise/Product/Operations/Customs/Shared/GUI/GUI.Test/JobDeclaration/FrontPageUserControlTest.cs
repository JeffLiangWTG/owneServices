using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(FrontPageUserControl))]
	sealed class FrontPageUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var control = new FrontPageUserControl();
			var groupBox = control.AssertContainsControl<ZGroupBox>("DeclarationDetailsGroupBox");
			_ = groupBox.AssertContainsControl<ZTextBox>("ExportDeclarationNumberBoundTextBox", x => x
				.WithBindTo(nameof(BaseJobDeclaration.DeclarationNumber))
				.WithCaption("Entry Number")
			);
			_ = groupBox.AssertContainsControl<ZTextBox>("StatusTextBox", x => x
				.WithBindTo(nameof(BaseJobDeclaration.JE_EntryStatusDescription))
				.WithCaption("Status")
			);
		}
	}
}
