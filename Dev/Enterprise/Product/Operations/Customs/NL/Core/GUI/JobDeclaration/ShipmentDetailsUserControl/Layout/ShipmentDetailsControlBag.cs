using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class ShipmentDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

	public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

	[ThreadStatic]
	static ShipmentDetailsControlBag instance;

	ShipmentDetailsControlBag()
	{
		ShipmentDetailsIncoTermsUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsIncoTermsUserControl));
		AgreedPlaceCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.AgreedPlaceCodeFindBox));
	}

	public ControlReference ShipmentDetailsIncoTermsUserControl;
	public ControlReference AgreedPlaceCodeFindBox;
}
