using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class StocktakeAssignAllLinesToUserActionMethod : LinesUserAssignerActionMethod<WhsStocktake>
	{
		public StocktakeAssignAllLinesToUserActionMethod()
			: base(new ZGuid("274dc5be-d4ed-4820-b378-c6b76d627550"))
		{
		}

		#region Overrides

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new StocktakeAssignAllLinesToUserApplicator(factory);
		}

		protected override string GetOperationalActionName()
		{
			return Res.GetString("15816354-d5c0-48c9-b832-c7f77ec0de82", "Assign Stocktake Lines to User");
		}

		#endregion
	}
}
