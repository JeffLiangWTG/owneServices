using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class ExportCustomsOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var userControl = new ExportCustomsOfficesUserControl();
			CombineAssertions(() =>
			{
				var groupBox = userControl.AssertContainsControl<ZGroupBox>("CustomsDetailsGroupBox", x => x
					.WithCaption("Customs Details")
					.WithSizeScaled(455, 75)
				);
				groupBox.AssertContainsControl<ZDropEdit>("CustomsOfficeDropEdit", x => x
					.WithCaption("Exit Customs office")
					.WithFullDescription("The physical customs office of exit. Example: when road, on the border.")
					.WithBindTo(nameof(JobDeclaration.JE_CustomsOffice))
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

		public void TestNorwegianTranslations()
		{
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Norwegian))
			{
				using var userControl = new ExportCustomsOfficesUserControl();
				CombineAssertions(() =>
				{
					var groupBox = userControl.AssertContainsControl<ZGroupBox>("CustomsDetailsGroupBox");
					groupBox.AssertContainsControl<ZDropEdit>("CustomsOfficeDropEdit", x => x
						.WithCaption("Utpasseringstollsted")
						.WithFullDescription("Fysisk utpasseringstollsted. Eksempel: ved landtransport på grensen.")
					);
				});
			}
		}
	}
}
