using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferUnAssignAllLinesPickOnlyActionMethod : LinesUserAssignerActionMethod<WhsTransfer>
	{
		public TransferUnAssignAllLinesPickOnlyActionMethod()
			: base(new ZGuid("7e7fcd60-4bb6-4037-80a9-f4ea1c62ea42"))
		{ }

		#region Overrides

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			var applicator = new TransferUnAssignAllLinesActionMethodApplicator(factory);
			applicator.Option = AssignLineOptions.PickOnly;
			return applicator;
		}

		protected override string GetOperationalActionName()
			=> Res.GetString("b35e4722-459d-4d8f-9309-05ad6bde32a0", "Un-assign Transfer Lines (Pick only)");

		#endregion
	}
}
