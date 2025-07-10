using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class TransportDetailsControlBag : ControlBag
{
	public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

	[ThreadStatic]
	static TransportDetailsControlBag instance;

	TransportDetailsControlBag()
	{
		TransportInlandRailUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandRailUserControl));
		VesselUserControl = RegisterControl(nameof(TransportDetailsUserControl.VesselUserControl));
	}

	public ControlReference TransportInlandRailUserControl { get; }
	public ControlReference VesselUserControl { get; }

	protected override Control CreateTemplate() => new TransportDetailsUserControl();
}
