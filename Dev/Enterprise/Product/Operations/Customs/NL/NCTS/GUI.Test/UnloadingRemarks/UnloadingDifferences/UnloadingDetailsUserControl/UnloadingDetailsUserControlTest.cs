using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

sealed class UnloadingDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
	}

	public void TestUnloadingRemarksFreeText()
	{
		var unloadingRemarks = userControl.UnloadingRemarksFreeTextTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>(unloadingRemarks);
			AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, unloadingRemarks.CharacterCasing);
		});
	}

	public void TestUnloadingRemarksGrid()
	{
		var unloadingRemarksGrid = userControl.UnloadingRemarksGrid;
		CombineAssertions(() =>
		{
			AssertType<ZGrid>(unloadingRemarksGrid);
			AssertEquals("UnloadingRemarksGrid: Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.UnloadingRemarkCollection), unloadingRemarksGrid.BindTo);
			AssertEquals("UnloadingRemarksGrid: ItemNumber column width", 80, unloadingRemarksGrid.GetColumnWidth(nameof(NonPersistentNctsUnloadingRemark.ItemNumber)));
			AssertEquals("UnloadingRemarksGrid: EoriNumber column width", 80, unloadingRemarksGrid.GetColumnWidth(nameof(NonPersistentNctsUnloadingRemark.EoriNumber)));
			AssertEquals("UnloadingRemarksGrid: Code column width", 80, unloadingRemarksGrid.GetColumnWidth(nameof(NonPersistentNctsUnloadingRemark.Code)));
			AssertEquals("UnloadingRemarksGrid: Number column width", 80, unloadingRemarksGrid.GetColumnWidth(nameof(NonPersistentNctsUnloadingRemark.Number)));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new UnloadingDetailsUserControl();
	}
	UnloadingDetailsUserControl userControl;

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}
}
