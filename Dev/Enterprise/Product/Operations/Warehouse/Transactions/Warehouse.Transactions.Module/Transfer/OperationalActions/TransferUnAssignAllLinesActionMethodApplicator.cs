using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferUnAssignAllLinesActionMethodApplicator : UnAssignAllLinesActionMethodApplicator<WhsTransfer>
	{
		public TransferUnAssignAllLinesActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("9129f61a-075d-4842-969d-d5430d2efd4e", "Un-assign Transfer Lines"), factory)
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

		protected override string MessageHeader => Res.GetString("bde7e5e1-2d56-4a1b-ac73-96c8a64308d3", "Transfer");

		protected override string GetErrorMessage(WhsTransfer target)
		{
			var result = ZString.Empty;
			if (target.IsFinalised)
			{
				result = Res.GetString("0aa6d2e0-6d35-4300-a895-f3131ba85acc", "- No Lines were un-assigned because the Transfer is Finalized.");
			}
			else if (target.WD_DocketStatus == DocketStatus.Codes.Cancelled)
			{
				result = Res.GetString("8dc3ce38-8a44-4318-a75a-c54f7a6821d4", "- No Lines were un-assigned because the Transfer is Canceled.");
			}
			return result;
		}

		protected override string NoAssignLinesErrorMessage => Res.GetString("828c3e50-7c26-42f1-bc3a-8b87b494931b", "- No Lines were un-assigned because there are no assigned lines, or Picking has already commenced.");

		protected override ControllerID ControllerID => ControllerIDs.WhsTransfer;

		protected override string GetJobNo(WhsTransfer target) => target.WD_DocketID;

		#endregion

		public AssignLineOptions Option { get; set; }
	}
}
