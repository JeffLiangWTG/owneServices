using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickAssignAllLinesToUserApplicator : AssignAllLinesOperationalActionMethodApplicator<WhsPick>
	{
		public PickAssignAllLinesToUserApplicator(BusinessObjectFactory factory)
			: this(Res.GetString("a2a3d28e-a39e-4b84-83cb-348639db3724", "Assign Pick Lines to User"), factory)
		{ }

		public PickAssignAllLinesToUserApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{ }

		#region Overrides

		protected override string MessageHeader => Res.GetString("8c1e7495-2f9b-491a-92c2-ea43fa67a3f6", "Pick");

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			AssignTargetsToUsers(log, targets.Cast<WhsPick>());
		}

		protected override string GetErrorMessage(WhsPick target)
		{
			var result = ZString.Empty;
			if (target.WP_PickStatus == PickStatus.Codes.Cancelled)
			{
				result = Res.GetString("2af3eea3-4764-4866-bc4f-c9e3926f2c2d", "- No Lines were assigned because the Pick is Canceled.");
			}
			else if (target.WP_PickStatus == PickStatus.Codes.Finalised)
			{
				result = Res.GetString("a8d8dc8b-e668-4a1c-a7d5-afd35c8e7eb7", "- No Lines were assigned because the Pick is Finalized.");
			}
			return result;
		}

		protected override string UnassignErrorMessage
			=> Res.GetString("2b39f12f-2bb1-430d-87f6-dd32670e9fc3", "- No Lines were assigned because there are no unassigned lines, or Picking has already commenced.");

		protected override ControllerID ControllerID => ControllerIDs.WhsPicking;

		protected override string GetJobNo(WhsPick target)
		{
			return target.WP_PickNo;
		}

		#endregion
	}
}
