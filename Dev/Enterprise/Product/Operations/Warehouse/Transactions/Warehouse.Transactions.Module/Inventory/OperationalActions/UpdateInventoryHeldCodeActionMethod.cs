using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class UpdateInventoryHeldCodeActionMethod : OperationalActionMethod
	{
		public UpdateInventoryHeldCodeActionMethod()
			: base(new ZGuid("70dcee23-5386-414e-910a-2f3fa89eea57"))
		{
		}

		#region GUI Control

		public override IComponent NewGuiControl()
		{
			return new FindInventoryHeldCode();
		}

		public override bool HasControl => true;

		#endregion

		#region Name

		public override string Name => Res.GetString("UpdateInventoryStatusActionMethod|Name", "Update Inventory Hold Code");

		#endregion

		#region Description

		public override string Description => Res.GetString("UpdateInventoryStatusActionMethod|Description", "Update Inventory Hold Code");

		#endregion

		#region NewApplicator

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateInventoryHeldCodeActionMethodApplicator(factory);
		}

		#endregion
	}
}
