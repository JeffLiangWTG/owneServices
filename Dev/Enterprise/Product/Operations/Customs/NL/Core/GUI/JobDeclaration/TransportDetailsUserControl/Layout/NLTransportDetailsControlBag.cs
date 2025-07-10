using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

sealed class NLTransportDetailsControlBag : ControlBag
{
	NLTransportDetailsControlBag()
	{
		TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl = RegisterControl(nameof(NLTransportDetailsUserControl.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl));
		ImportTransportInlandRoadUserControl = RegisterControl(nameof(NLTransportDetailsUserControl.ImportTransportInlandRoadUserControl));
		FlightAndNationalityUserControl = RegisterControl(nameof(NLTransportDetailsUserControl.FlightAndNationalityUserControl));
	}

	public static NLTransportDetailsControlBag Instance => instance ??= new NLTransportDetailsControlBag();

	[ThreadStatic]
	static NLTransportDetailsControlBag instance;

	protected override Control CreateTemplate() => new NLTransportDetailsUserControl();

	public ControlReference TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl { get; }
	public ControlReference ImportTransportInlandRoadUserControl { get; }
	public ControlReference FlightAndNationalityUserControl { get; }
}
