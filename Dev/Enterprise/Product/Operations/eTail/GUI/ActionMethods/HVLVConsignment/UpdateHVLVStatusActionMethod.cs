using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.eTail.GUI
{
	public sealed class UpdateHVLVStatusActionMethod : OperationalActionMethod
	{
		public UpdateHVLVStatusActionMethod()
			: base(new Guid("f81995d0-d896-41f2-a843-925d9cd26c29"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("7dba8524-47d6-419b-a229-7ddb63cebcd6", "Updates status on shipment HVLV consignments"); }
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateHVLVStatusUserControl();
		}

		public override string Name
		{
			get { return Res.GetString("a876acc1-690f-400f-9f21-9e1a270d226e", "Update HVLV Status"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateHVLVStatusApplicator();
		}
	}
}
