using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class UpdateLastKnownTransitWarehouseActionMethod : OperationalActionMethod
	{
		public UpdateLastKnownTransitWarehouseActionMethod() : base(new Guid("9067D544-63F7-4D3F-8677-EBBE2987CC6E"))
		{
		}

		public override string Name => Res.GetString("794D0D1F-6728-471B-901E-C0A5DB827126", "Update Last Known TW Details");

		public override string Description => Res.GetString("323A0187-F887-4B11-8AB6-D75CD664DEAA", "Update last known TW details on shipments");

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new UpdateLastKnownTransitWarehouseUserControl();

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateLastKnownTransitWarehouseApplicator(factory);
		}
	}
}
