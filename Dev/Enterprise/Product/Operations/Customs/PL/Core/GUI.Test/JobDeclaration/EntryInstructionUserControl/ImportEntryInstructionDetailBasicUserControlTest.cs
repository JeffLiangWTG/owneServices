using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ImportEntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new ImportEntryInstructionDetailBasicUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNotNull("OtherPartiesDetailsPanel", control.FindSingleOrDefault<ZPanel>("OtherPartiesDetailsPanel"));
				AssertNotNull("OtherPartiesGroupBox", control.FindSingleOrDefault<ZGroupBox>("OtherPartiesGroupBox"));
				AssertNotNull("FromWarehouseGroupBox", control.FindSingleOrDefault<ZGroupBox>("FromWarehouseGroupBox"));
				AssertNotNull("FromWarehouseCodeTextBox", control.FindSingleOrDefault<ZTextBox>("FromWarehouseCodeTextBox"));
				AssertNotNull("FromWarehouseAddressControl", control.FindSingleOrDefault<ZAddressControl>("FromWarehouseAddressControl"));
				AssertNotNull("ToWarehouseGroupBox", control.FindSingleOrDefault<ZGroupBox>("ToWarehouseGroupBox"));
				AssertNotNull("ToWarehouseCodeTextBox", control.FindSingleOrDefault<ZTextBox>("ToWarehouseCodeTextBox"));
				AssertNotNull("ToWarehouseAddressControl", control.FindSingleOrDefault<ZAddressControl>("ToWarehouseAddressControl"));
				AssertNotNull("DetailsPanel", control.FindSingleOrDefault<ZPanel>("DetailsPanel"));
				AssertNotNull("DetailsLayoutControl", control.FindSingle<EntryInstructionDetailsLayoutControl>("DetailsLayoutControl"));

				AssertNotNull("GuaranteesUserControl", control.FindSingleOrDefault<EntryInstructionDetailGuaranteesUserControl>("GuaranteesUserControl"));
				AssertNotNull("GuaranteesGroupBox", control.FindSingleOrDefault<ZGroupBox>("GuaranteesGroupBox"));
			});
		}
	}
}
