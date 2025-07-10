using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(NOManifestUserControl))]
sealed class NOManifestUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new NOManifestUserControl();
		_ = control.AssertContainsControl<ZTextBox>("DriverCommunicationIdTextBox", x => x
			.WithBindTo(nameof(AsycudaManifestHeader.AMA_DriverCommunicationId))
			.WithCaption("Phone NO./E-Mail")
			.WithFullDescription("Telephone number (or mail address) of operator/driver of Means of transport at Border Crossing.")
		);
		_ = control.AssertContainsControl<ZTextBox>("DriverNameTextBox", x => x
			.WithBindTo(nameof(AsycudaManifestHeader.AMA_DriverName))
			.WithCaption("Operator/Driver")
			.WithFullDescription("Name of operator/driver of Means of transport at Border Crossing.")
		);
		_ = control.AssertContainsControl<ZDateEdit>("ScheduledDateOfArrCustOfficeDateEdit", x => x
			.WithBindTo(nameof(AsycudaManifestHeader.AMA_ScheduledDateOfAddCustOff))
			.WithCaption("Sched. Arr Cust.Off")
			.WithFullDescription("Scheduled Time of Arrival at Customs Office/Border Crossing (set at first submission).")
		);
		_ = control.AssertContainsControl<ZCodeFindBox>("TransportMeansCodeFindBox", x => x
			.WithBindTo(nameof(AsycudaManifestHeader.AMA_TransportMeans))
			.WithCaption("Type of Means")
			.WithFullDescription("Type of Means of transport at Border Crossing.")
		);
		_ = control.AssertContainsControl<VehicleRegistrationAndNationalityUserControl>("VehicleRegistrationAndNationalityUserControl", x => x
			.WithBindTo(".")
		);
		_ = control.AssertContainsControl<ZAddressControl>("RepresentativeAddressControl", x => x
			.WithBindTo(nameof(AsycudaManifestHeader.MasterBill) +  "." + nameof(ASYCUDA.Business.AsycudaBill.ABL_OA_Forwarder))
			.WithCaption("Representative")
			.WithFullDescription("Customs representative on behalf of carrier (operator/driver).")
		);

		var masterBillGroupBox = control.AssertContainsControl<ZGroupBox>("MasterBillGroupBox",
			g => g.WithCaption("Master Bill")
				.WithValue(c => c.MaximumSize.Width, 600)
				.WithValue(c => c.MaximumSize.Height, 550)
		);

		_ = masterBillGroupBox.AssertContainsControl<DynamicLayoutPanel>("MasterBillDynamicLayoutPanel",
			d => d.WithBindTo("MasterBill"));
	});

	public void TestGetMasterBillLayout()
	{
		using var control = new NOManifestUserControl();
		AssertType<MasterBillLayout>(control.GetMasterBillLayout());
	}

	public void TestMasterBillLayoutControls() => CombineAssertions(() =>
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.Bills.AddNew();

		using var form = new ZForm(header);
		using var control = new NOManifestUserControl();
		form.Controls.Add(control);
		form.Show();

		_ = control.AssertContainsControl<ZTextBox>("BillNumberTextBox");
		_ = control.AssertContainsControl<ZDropEdit>("TransportDocumentTypeDropEdit");
		_ = control.AssertContainsControl<ZAddressControl>("ShipperAddressControl");
		_ = control.AssertContainsControl<ZAddressControl>("ConsigneeAddressControl");
		_ = control.AssertContainsControl<ZCodeFindBox>("PlaceOfLoadingCodeFindBox");
		_ = control.AssertContainsControl<ZTextBox>("PlaceOfLoadingTextBox");
		_ = control.AssertContainsControl<ZCodeFindBox>("PlaceOfUnloadingCodeFindBox");
		_ = control.AssertContainsControl<ZTextBox>("PlaceOfUnloadingTextBox");
		_ = control.AssertContainsControl<ZCodeFindBox>("PlaceOfDeliveryCodeFindBox");
		_ = control.AssertContainsControl<ZTextBox>("PlaceOfDeliveryTextBox");
		_ = control.AssertContainsControl<ZTextBox>("Email1TextBox");
		_ = control.AssertContainsControl<ZTextBox>("Email2TextBox");
		_ = control.AssertContainsControl<ZTextBox>("Email3TextBox");
	});
}
