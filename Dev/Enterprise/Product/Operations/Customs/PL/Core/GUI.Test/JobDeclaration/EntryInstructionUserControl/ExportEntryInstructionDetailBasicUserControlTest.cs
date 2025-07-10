using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ExportEntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new ExportEntryInstructionDetailBasicUserControl())
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
				AssertNotNull("DetailsLayoutControl", control.FindSingleOrDefault<EntryInstructionDetailsLayoutControl>("DetailsLayoutControl"));
			});
		}
	}

	public void TestIsVisibleForBindingExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.ZG_ExportManifest = false;
		declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;

		using (var form = new ZForm(declaration))
		{
			using (var control = new ExportEntryInstructionDetailBasicUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					Assert("OtherPartiesDetailsPanel", control.FindSingle<ZPanel>("OtherPartiesDetailsPanel").Visible);
					Assert("OtherPartiesGroupBox", control.FindSingle<ZGroupBox>("OtherPartiesGroupBox").Visible);
					Assert("FromWarehouseGroupBox", control.FindSingle<ZGroupBox>("FromWarehouseGroupBox").Visible);
					Assert("FromWarehouseCodeTextBox", control.FindSingle<ZTextBox>("FromWarehouseCodeTextBox").Visible);
					Assert("FromWarehouseAddressControl", control.FindSingle<ZAddressControl>("FromWarehouseAddressControl").Visible);
					Assert("ToWarehouseGroupBox", control.FindSingle<ZGroupBox>("ToWarehouseGroupBox").Visible);
					Assert("ToWarehouseCodeTextBox", control.FindSingle<ZTextBox>("ToWarehouseCodeTextBox").Visible);
					Assert("ToWarehouseAddressControl", control.FindSingle<ZAddressControl>("ToWarehouseAddressControl").Visible);
					Assert("DetailsPanel", control.FindSingle<ZPanel>("DetailsPanel").Visible);
					Assert("DetailsLayoutControl", control.FindSingle<EntryInstructionDetailsLayoutControl>("DetailsLayoutControl").Visible);
				});
			}
		}
	}
}
