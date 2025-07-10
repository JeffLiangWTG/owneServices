using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class AssignWhsPutawayGroupMethod : OperationalActionMethod
	{
		public AssignWhsPutawayGroupMethod()
			: base(new ZGuid("fac863bd-c586-47eb-959d-02ca4edd81e9"))
		{
		}

		#region NewApplicator

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AssignWhsPutawayGroupMethodApplicator(Name, factory);
		}

		#endregion

		#region NewGuiControl

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new AssignWhsPutawayGroupControl();
		}

		#endregion

		#region Name

		public override string Name => Res.GetString("659da318-9c6f-456a-9c3a-b2aef7f735ec", "Assign Warehouse Putaway Group to Product/Warehouse parameters");

		#endregion

		#region Description

		public override string Description => Res.GetString("659da318-9c6f-456a-9c3a-b2aef7f735ec", "Assign Warehouse Putaway Group to Product/Warehouse parameters");

		#endregion
	}
}
