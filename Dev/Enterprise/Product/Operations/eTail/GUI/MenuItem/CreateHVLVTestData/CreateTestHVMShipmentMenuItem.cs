using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI;

public class CreateTestHVMShipmentMenuItem : ZMenuItem
{
	public CreateTestHVMShipmentMenuItem(ForwardingConsol consol)
		: base(ResString.GetMultilingualString("7b9e6a58-4b6f-47ad-ba4f-298dd1800298", "Create Test HVM Shipment"))
	{
		this.consol = consol;
	}

	readonly ForwardingConsol consol;

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);

		var shipment = consol.Shipments.AddNew();
		shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueMaster;

		Globals.Message.ShowInformation(Res.GetString("19288acb-1939-4710-964d-feab0bf414e6", "Test HVM Shipment created successfully"));
	}
}
