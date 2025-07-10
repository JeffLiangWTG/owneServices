using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	static class RECR10Populator
	{
		public static RECR10 Populate(string actionCode, IReconciliation reconciliationData)
		{
			var r10 = new RECR10();
			r10.ActionCode = actionCode;
			r10.ReconciliationEntryNumber = MQEDIMessage.USEntryFilerEntryNumberPlaceHolder;
			r10.ReconciliationPort = reconciliationData.ProcessingDistrictPort;
			if (actionCode != "D")
			{
				r10.ImporterID = reconciliationData.ImporterID;
				r10.SuretyCode = reconciliationData.SuretyCode;
				r10.EstimatedReconciliationEntrySummaryDate = reconciliationData.EstimatedReconciliationEntrySummaryDate;
				r10.ReconciliationTeam = reconciliationData.TeamNumber;
				var issueCode = reconciliationData.IssueCode;
				if (issueCode != ReconIssueCodeList.Codes.NotApplicable)
				{
					r10.IssueCode = issueCode;
				}
				r10.AggregateReconciliationIndicator = reconciliationData.AggregateReconciliationIndicator ? "Y" : "N";
				r10.IncreaseRefundIndicator = reconciliationData.IncreaseRefundIndicator;
				r10.EarliestImportDate = reconciliationData.EarliestImportDate;
				r10.EarliestEntrySummaryDate = reconciliationData.EarliestEntrySummaryDate;
				r10.AgentBroker4811ReferenceID = reconciliationData.AgentBrokerReferenceID;
				r10.BrokerReferenceNumber = reconciliationData.BrokerReferenceNumber.Right(9);
			}

			return r10;
		}
	}
}
