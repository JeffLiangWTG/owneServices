using System;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	static class RECR17Populator
	{
		public static RECR17 Populate(IReconciliation reconciliationData)
		{
			RECR17 r17 = new RECR17();

			r17.PaymentTypeIndicator = reconciliationData.PaymentTypeIndicator;
			r17.PreliminaryStatementPrintDate = reconciliationData.PreliminaryStatementPrintDate;
			r17.ClientBranchDesignation = reconciliationData.ClientBranchDesignation;

			r17.DutyPaymentAmount = Math.Max(0, reconciliationData.DutyPaymentAmount);
			r17.TaxPaymentAmount = Math.Max(0, reconciliationData.TaxPaymentAmount);
			r17.FeePaymentAmount = Math.Max(0, reconciliationData.FeePaymentAmount);
			r17.InterestPaymentAmount = Math.Max(0, reconciliationData.InterestPaymentAmount);

			return r17;
		}
	}
}
