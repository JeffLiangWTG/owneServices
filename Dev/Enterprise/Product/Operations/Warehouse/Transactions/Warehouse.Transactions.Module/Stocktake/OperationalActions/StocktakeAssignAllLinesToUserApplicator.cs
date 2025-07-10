using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class StocktakeAssignAllLinesToUserApplicator : AssignAllLinesOperationalActionMethodApplicator<WhsStocktake>
	{
		public StocktakeAssignAllLinesToUserApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("0f15043a-1b94-4a43-ad22-66e9cbe09274", "Stocktake assign all lines to user"), factory)
		{ }

		#region Overrides

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			AssignTargetsToUsers(log, targets.Cast<WhsStocktake>());
		}

		protected override string GetErrorMessage(WhsStocktake target)
		{
			var result = ZString.Empty;
			if (target.WS_StocktakeStatus == StocktakeStatus.Codes.New)
			{
				result = Res.GetString("cf1ef42a-2717-4517-8782-00af362b8f0a", "- No Lines were assigned because the Stocktake is not Loaded.");
			}
			else if (target.WS_StocktakeStatus == StocktakeStatus.Codes.Finalised)
			{
				result = Res.GetString("b79b5a77-7036-46d8-85e6-e82c2c5ef842", "- No Lines were assigned because the Stocktake is Finalized.");
			}
			return result;
		}

		protected override string MessageHeader => Res.GetString("f068ef9d-39b0-4204-872a-ffe571c1ee36", "Stocktake");

		protected override ControllerID ControllerID => ControllerIDs.WhsStocktake;

		protected override string GetJobNo(WhsStocktake target)
		{
			return target.WS_StocktakeNumber;
		}

		#endregion
	}
}
