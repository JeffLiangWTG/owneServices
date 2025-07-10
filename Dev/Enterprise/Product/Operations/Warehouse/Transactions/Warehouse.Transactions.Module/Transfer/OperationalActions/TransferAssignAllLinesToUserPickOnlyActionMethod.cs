using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferAssignAllLinesToUserPickOnlyActionMethod : LinesUserAssignerActionMethod<WhsTransfer>
	{
		public TransferAssignAllLinesToUserPickOnlyActionMethod()
			: base(new ZGuid("9249904d-f2fd-48f5-88dd-faf6e98762ae"))
		{
		}

		#region Overrides

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			var applicator = new TransferAssignAllLinesToUserApplicator(factory);
			applicator.Option = AssignLineOptions.PickOnly;
			return applicator;
		}

		#endregion

		protected override string GetOperationalActionName()
		{
			return Res.GetString("4161822b-a67a-4087-81e4-a441aa9089ec", "Assign Transfer Lines to User (Pick only)");
		}
	}
}
