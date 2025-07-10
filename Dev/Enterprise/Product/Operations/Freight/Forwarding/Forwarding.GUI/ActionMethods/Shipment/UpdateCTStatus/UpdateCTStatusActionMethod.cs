using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class UpdateCTStatusActionMethod : OperationalActionMethod
	{
		public UpdateCTStatusActionMethod() : base(new Guid("4809430e-5509-43cd-8a2d-84368832950e"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("357198ee-5d77-4237-a637-fbab222c334d", "Updates CT status on shipments"); }
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateCTStatusUserControl();
		}

		public override string Name
		{
			get { return Res.GetString("38710ee7-e3e2-4d53-8edd-8c6d682250e4", "Update CT Status"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateCTStatusApplicator();
		}
	}
}
