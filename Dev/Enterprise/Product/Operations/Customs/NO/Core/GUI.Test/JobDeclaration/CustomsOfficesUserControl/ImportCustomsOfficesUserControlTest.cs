using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class ImportCustomsOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var userControl = new ImportCustomsOfficesUserControl();
			CombineAssertions(() =>
			{
				var groupBox = userControl.AssertContainsControl<ZGroupBox>("CustomsDetailsGroupBox", x => x
					.WithCaption("Customs Details")
					.WithSizeScaled(455, 46)
				);
				groupBox.AssertContainsControl<ZTextBox>("PositionNumberTextBox", x => x
					.WithCaption("Position")
					.WithBindTo(nameof(JobDeclaration.JE_Position))
					.WithShouldEscapeAllSpecialCharacters()
					.WithCharacterCasing(CharacterCasing.Normal)
				);
				groupBox.AssertContainsControl<ZTextBox>("GoodsNumberTextBox", x => x
					.WithCaption("Goods Number")
					.WithBindTo(nameof(JobDeclaration.JE_GoodsNumber))
					.WithShouldEscapeAllSpecialCharacters()
					.WithCharacterCasing(CharacterCasing.Normal)
				);
			});
		}
	}
}
