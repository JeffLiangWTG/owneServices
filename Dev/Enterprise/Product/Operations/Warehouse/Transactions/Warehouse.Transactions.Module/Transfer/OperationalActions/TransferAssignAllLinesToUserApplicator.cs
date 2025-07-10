using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferAssignAllLinesToUserApplicator : AssignAllLinesOperationalActionMethodApplicator<WhsTransfer>
	{
		public TransferAssignAllLinesToUserApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("ea03d16c-6793-4eb3-bebf-7cef4f390183", "Assign Transfer Lines to User"), factory)
		{ }

		#region Overrides

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			foreach (WhsTransfer transfer in targets)
			{
				transfer.Option = Option;
			}

			AssignTargetsToUsers(log, targets.Cast<WhsTransfer>());
		}

		protected override string MessageHeader => Res.GetString("835f24c4-7911-4936-9404-c2b475e089cc", "Transfer");

		protected override string GetErrorMessage(WhsTransfer target)
		{
			var result = ZString.Empty;
			if (target.WD_DocketStatus == DocketStatus.Codes.Finalised)
			{
				result = Res.GetString("fa991a90-99ac-467f-8bbd-b8350284420c", "- No Lines were assigned because the Transfer is Finalized.");
			}
			else if (target.WD_DocketStatus == DocketStatus.Codes.Cancelled)
			{
				result = Res.GetString("c218ce7b-c991-49e9-87a4-201b2d04ef52", "- No Lines were assigned because the Transfer is Canceled.");
			}
			return result;
		}

		protected override string UnassignErrorMessage => Res.GetString("2b39f12f-2bb1-430d-87f6-dd32670e9fc3", "- No Lines were assigned because there are no unassigned lines, or Picking has already commenced.");

		protected override ControllerID ControllerID => ControllerIDs.WhsTransfer;

		protected override string GetJobNo(WhsTransfer target) => target.WD_DocketID;

		#endregion

		public AssignLineOptions Option { get; set; }
	}
}
