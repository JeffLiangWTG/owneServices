using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

public sealed class ShipmentTypeControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentTypeUserControl();

	public static ShipmentTypeControlBag Instance => instance ??= new ShipmentTypeControlBag();

	[ThreadStatic]
	static ShipmentTypeControlBag instance;

	ShipmentTypeControlBag()
	{
		CustomsTransportModeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.CustomsTransportModeDropEdit));
	}

	public ControlReference CustomsTransportModeDropEdit { get; }
}
