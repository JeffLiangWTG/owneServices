using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(BillUserControl))]
sealed class BillUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		_ = control.AssertContainsControl<ZDropEdit>(nameof(control.ImportProcedureDropEdit),
				x => x.WithBindTo(nameof(AsycudaBill.ImportProcedure)));
		_ = control.AssertContainsControl<ZDropEdit>(nameof(control.ExportProcedureDropEdit),
				x => x.WithBindTo(nameof(AsycudaBill.ExportProcedure)));
		_ = control.AssertContainsControl<EmailAddressesUserControl>(nameof(control.EmailAddressControl));
		_ = control.AssertContainsControl<ZCodeFindBox>(nameof(control.PlaceOfLoadingCodeFindBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_RL_NKPortOfLoading))
			.WithShowDescriptionBox(false));
		_ = control.AssertContainsControl<ZTextBox>("PlaceOfLoadingTextBox",
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_CustomsLoadPort)));
		_ = control.AssertContainsControl<ZCodeFindBox>(nameof(control.PlaceOfUnloadingCodeFindBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_RL_NKPortOfDischarge))
			.WithShowDescriptionBox(false));
		_ = control.AssertContainsControl<ZTextBox>("PlaceOfUnloadingTextBox",
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_CustomsDischargePort)));
	});

	public void TestTransportDocumentTypeDropEdit() => CombineAssertions(() =>
		control.AssertContainsControl<ZDropEdit>("TransportDocumentTypeDropEdit", x => x
			.WithBindTo(nameof(AsycudaBill.TransportDocumentType))
			.WithFullDescription("Transport document type.")
		));

	public void TestPlaceOfAcceptancePanel() => CombineAssertions(() =>
	{
		var placeOfAcceptancePanel = control.AssertContainsControl<ZPanel>("PlaceOfAcceptancePanel", x => x
			.WithHeightUnScaled(20)
		);
		_ = placeOfAcceptancePanel.AssertContainsControl<ZCodeFindBox>("PlaceOfAcceptanceCodeFindBox", x => x
			.WithDock(DockStyle.Left)
		);
		_ = placeOfAcceptancePanel.AssertContainsControl<ZTextBox>("PlaceOfAcceptanceTextBox", x => x
			.WithDock(DockStyle.Fill)
		);
	});

	public void TestPlaceOfAcceptanceCodeFindBox() => CombineAssertions(() =>
		control.AssertContainsControl<ZCodeFindBox>("PlaceOfAcceptanceCodeFindBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_RL_NKOrigin))
			.WithCaption("Place of Acceptance")
			.WithFullDescription("Place of Acceptance code.")
			.WithShowDescriptionBox(false)
		));

	public void TestPlaceOfAcceptanceTextBox() => CombineAssertions(() =>
		control.AssertContainsControl<ZTextBox>("PlaceOfAcceptanceTextBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_CustomsOriginPort))
			.WithFullDescription("Place of Acceptance.")
		));

	public void TestPlaceOfDeliveryPanel() => CombineAssertions(() =>
	{
		var placeOfDeliveryPanel = control.AssertContainsControl<ZPanel>("PlaceOfDeliveryPanel", x => x
			.WithHeightUnScaled(20)
		);
		_ = placeOfDeliveryPanel.AssertContainsControl<ZCodeFindBox>("PlaceOfDeliveryCodeFindBox", x => x
			.WithDock(DockStyle.Left)
		);
		_ = placeOfDeliveryPanel.AssertContainsControl<ZTextBox>("PlaceOfDeliveryTextBox", x => x
			.WithDock(DockStyle.Fill)
		);
	});

	public void TestPlaceOfDeliveryCodeFindBox() => CombineAssertions(() =>
		control.AssertContainsControl<ZCodeFindBox>("PlaceOfDeliveryCodeFindBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_RL_NKFinalDestination))
			.WithCaption("Place of Delivery")
			.WithFullDescription("Place of Delivery code.")
			.WithShowDescriptionBox(false)
		));

	public void TestPlaceOfDeliveryTextBox() => CombineAssertions(() =>
		control.AssertContainsControl<ZTextBox>("PlaceOfDeliveryTextBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_CustomsFinalDestinationPort))
			.WithFullDescription("Place of Delivery.")
		));

	public void TestImportProcedureDropEdit() => CombineAssertions(() =>
		control.AssertContainsControl<ZDropEdit>(nameof(control.ImportProcedureDropEdit), x => x
			.WithBindTo(nameof(AsycudaBill.ImportProcedure))
		));

	public void TestExportProcedureDropEdit() => CombineAssertions(() =>
		control.AssertContainsControl<ZDropEdit>(nameof(control.ExportProcedureDropEdit), x => x
			.WithBindTo(nameof(AsycudaBill.ExportProcedure))
		));

	public void TestPlaceOfLoadingPanel() => CombineAssertions(() =>
	{
		var placeOfLoadingPanel = control.AssertContainsControl<ZPanel>("PlaceOfLoadingPanel", x => x
			.WithHeightUnScaled(20)
		);
		_ = placeOfLoadingPanel.AssertContainsControl<ZCodeFindBox>("PlaceOfLoadingCodeFindBox", x => x
			.WithDock(DockStyle.Left)
		);
		_ = placeOfLoadingPanel.AssertContainsControl<ZTextBox>("PlaceOfLoadingTextBox", x => x
			.WithDock(DockStyle.Fill)
		);
	});

	public void TestPlaceOfLoadingCodeFindBox() => CombineAssertions(() =>
	{
		control.AssertContainsControl<ZCodeFindBox>("PlaceOfLoadingCodeFindBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_RL_NKPortOfLoading))
			.WithCaption("Place of Loading")
			.WithFullDescription("Location of Loading.")
			.WithShowDescriptionBox(false)
		);
	});

	public void TestPlaceOfLoadingTextBox() => CombineAssertions(() =>
	{
		control.AssertContainsControl<ZTextBox>("PlaceOfLoadingTextBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_CustomsLoadPort))
			.WithFullDescription("Place of Loading.")
		);
	});

	public void TestPlaceOfUnloadingPanel() => CombineAssertions(() =>
	{
		var placeOfUnloadingPanel = control.AssertContainsControl<ZPanel>("PlaceOfUnloadingPanel", x => x
			.WithHeightUnScaled(20)
		);
		_ = placeOfUnloadingPanel.AssertContainsControl<ZCodeFindBox>("PlaceOfUnloadingCodeFindBox", x => x
			.WithDock(DockStyle.Left)
		);
		_ = placeOfUnloadingPanel.AssertContainsControl<ZTextBox>("PlaceOfUnloadingTextBox", x => x
			.WithDock(DockStyle.Fill)
		);
	});

	public void TestPlaceOfUnloadingCodeFindBox() => CombineAssertions(() =>
	{
		control.AssertContainsControl<ZCodeFindBox>("PlaceOfUnloadingCodeFindBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_RL_NKPortOfDischarge))
			.WithCaption("Place of Unloading")
			.WithFullDescription("Location of Unloading.")
			.WithShowDescriptionBox(false)
		);
	});

	public void TestPlaceOfUnloadingTextBox() => CombineAssertions(() =>
	{
		control.AssertContainsControl<ZTextBox>("PlaceOfUnloadingTextBox", x => x
			.WithBindTo(nameof(AsycudaBill.ABL_CustomsDischargePort))
			.WithFullDescription("Place of Unloading.")
		);
	});

	protected override void SetUp()
	{
		base.SetUp();
		control = new ();
	}

	protected override void TearDown()
	{
		control.Dispose();
		base.TearDown();
	}

	BillUserControl control;
}
