using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(GoodsNumberAndPositionUserControl))]
	sealed class GoodsNumberAndPositionUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using var control = new GoodsNumberAndPositionUserControl();
			AssertEquals("GoodsNumberAndPositionUserControl test data source", typeof(CusEntryInstruction), control.BindingSource.DataSourceType);
		}

		public void TestControls() => CombineAssertions(() =>
		{
			using var control = new GoodsNumberAndPositionUserControl();
			_ = control.AssertContainsControl<ZTextBox>("CEI_GoodsNumberTextBox", x => x
				.WithBindTo(nameof(CusEntryInstruction.CEI_GoodsNumber))
				.WithCaption("Goods Number")
				.WithCharacterCasing(CharacterCasing.Normal)
			);
			_ = control.AssertContainsControl<ZTextBox>("CEI_PositionNumberTextBox", x => x
				.WithBindTo(nameof(CusEntryInstruction.CEI_Position))
				.WithCaption("Position")
				.WithCharacterCasing(CharacterCasing.Normal)
			);
			_ = control.AssertContainsControl<ZTextBox>("CEI_SubPositionNumberTextBox", x => x
				.WithBindTo(nameof(CusEntryInstruction.CEI_SubPosition))
				.WithCaption("Sub Position")
				.WithCharacterCasing(CharacterCasing.Normal)
			);
		});
	}
}
