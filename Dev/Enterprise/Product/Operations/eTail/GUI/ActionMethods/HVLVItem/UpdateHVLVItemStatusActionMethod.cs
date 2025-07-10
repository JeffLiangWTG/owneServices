using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.eTail.GUI
{
	public sealed class UpdateHVLVItemStatusActionMethod : OperationalActionMethod
	{
		public UpdateHVLVItemStatusActionMethod()
			: base(new Guid("85a517c7-766b-4aff-b438-7129f8c25985"))
		{
		}

		public override string Name => Res.GetString("6b4ac6fb-be41-4ed2-81b8-d92f69e59633", "Update HVLV Item Status");

		public override string Description => Res.GetString("bd983f0a-e3c2-4507-bac7-3f35f267e6f1", "Updates HVLV item statuses on HVLV consignments");

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new UpdateHVLVItemStatusUserControl();

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateHVLVItemStatusApplicator(factory);
		}
	}
}
