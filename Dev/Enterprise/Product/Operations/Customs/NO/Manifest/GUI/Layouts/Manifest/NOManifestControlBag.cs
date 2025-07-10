using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class NOManifestControlBag : ControlBag
{
	NOManifestControlBag()
	{
		DriverCommunicationIdTextBox = RegisterControl(nameof(NOManifestUserControl.DriverCommunicationIdTextBox));
		DriverNameTextBox = RegisterControl(nameof(NOManifestUserControl.DriverNameTextBox));
		RepresentativeAddressControl = RegisterControl(nameof(NOManifestUserControl.RepresentativeAddressControl));
		ScheduledDateOfArrCustOfficeDateEdit = RegisterControl(nameof(NOManifestUserControl.ScheduledDateOfArrCustOfficeDateEdit));
		TransportMeansCodeFindBox = RegisterControl(nameof(NOManifestUserControl.TransportMeansCodeFindBox));
		VehicleRegistrationAndNationalityUserControl = RegisterControl(nameof(NOManifestUserControl.VehicleRegistrationAndNationalityUserControl));
		MasterBillGroupBox = RegisterControl(nameof(NOManifestUserControl.MasterBillGroupBox));
	}

	public static NOManifestControlBag Instance => instance ??= new NOManifestControlBag();

	[ThreadStatic]
	static NOManifestControlBag instance;

	protected override Control CreateTemplate() => new NOManifestUserControl();

	public ControlReference DriverCommunicationIdTextBox { get; }
	public ControlReference DriverNameTextBox { get; }
	public ControlReference RepresentativeAddressControl { get; }
	public ControlReference ScheduledDateOfArrCustOfficeDateEdit { get; }
	public ControlReference TransportMeansCodeFindBox { get; }
	public ControlReference VehicleRegistrationAndNationalityUserControl { get; }
	public ControlReference MasterBillGroupBox { get; }
}
