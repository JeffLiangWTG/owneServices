using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickUnAssignAllLinesActionMethodApplicator : UnAssignAllLinesActionMethodApplicator<WhsPick>
	{
		public PickUnAssignAllLinesActionMethodApplicator(BusinessObjectFactory factory)
			: this(Res.GetString("6e94da14-cfea-498b-8bc5-06eb2352a4bc", "Un-assign Pick Lines"), factory)
		{ }

		public PickUnAssignAllLinesActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{ }

		#region Overrides

		protected override string MessageHeader => Res.GetString("e18f6235-a84e-40a6-932c-5f9c57de7dfc", "Pick");

		protected override ControllerID ControllerID => ControllerIDs.WhsPicking;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
			=> AssignTargetsToUsers(log, targets.Cast<WhsPick>());

		protected override string GetErrorMessage(WhsPick target)
		{
			var result = ZString.Empty;
			if (target.WP_PickStatus == PickStatus.Codes.Cancelled)
			{
				result = Res.GetString("039d0afa-662d-45da-9e5b-26ecaedf408b", "- No Lines were un-assigned because the Pick is Canceled.");
			}
			else if (target.WP_PickStatus == PickStatus.Codes.Finalised)
			{
				result = Res.GetString("29a1387b-236d-4ccf-bb54-2b6ffe449eff", "- No Lines were un-assigned because the Pick is Finalized.");
			}
			return result;
		}

		protected override string GetJobNo(WhsPick target) => target.WP_PickNo;

		protected override string NoAssignLinesErrorMessage => Res.GetString("59938761-c25e-4845-b421-6e6a7d077dbb", "- No Lines were un-assigned because there are no assigned lines, or Picking has already commenced.");

		#endregion
	}
}
