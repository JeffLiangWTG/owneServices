using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ExportAWBHeaderUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public ExportAWBHeaderUniqueIndexFailureHandler(ExportAWBHeader awbHeader)
		{
			this.awbHeader = awbHeader;
		}

		ExportAWBHeader awbHeader;

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return ExportAWBHeaderSchema.Constants.Indexes.NR_UC__EH_ParentID; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var message = Res.GetString("79a64cd5-d883-4789-9339-0ae5886248a9", "'{0}' has created an Air Waybill form for '{1}' while this form was open. Your conflicting changes have been discarded and the created Air Waybill form has been loaded. Review changes to the Air Waybill form and save again.",
					 GetTheUserWhoLastEditedTheParentObject(), awbHeader.EH_ReferenceNumber);

			awbHeader.Factory.ClearQueryCache();

			var query = new ZDBOnlyQuery(typeof(ExportAWBHeader));
			query.AddToFilter(ExportAWBHeaderSchema.EH_ParentID, awbHeader.EH_ParentID);

			awbHeader = awbHeader.Factory.LoadTop1<ExportAWBHeader>(query);
			if (awbHeader != null)
			{
				var parent = awbHeader.Parent;
				parent?.NotifyConcurrencyHandled();
			}

			notifier.ReportError(message, Res.GetString("da6a8abd-c228-49f6-9cf3-fb8558efd06e", "Error"));
		}

		ZString GetTheUserWhoLastEditedTheParentObject()
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, awbHeader.EH_ParentID);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " DESC";

			var log = awbHeader.Factory.LoadTop1<StmALog>(query);

			return (log != null) ? log.SL_UserNameAndInitials : ZString.Empty;
		}
	}
}
