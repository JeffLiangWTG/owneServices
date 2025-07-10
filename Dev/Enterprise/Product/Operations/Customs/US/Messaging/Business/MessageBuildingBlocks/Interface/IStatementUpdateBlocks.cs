using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IStatementUpdateInputHBlock
	{
		ZString DistrictPortOfEntrySummary { get; set; }
		ZString EntryFilerCode { get; set; }
		ZString EntryNumber { get; set; }
		ZString PaymentTypeIndicator { get; set; }
		ZDate PreliminaryStatementPrintDate { get; set; }
		ZString ClientBranchDesignation { get; set; }
		ZString PeriodicStatementMonth { get; set; }
	}

	public interface IStatementUpdateOutputH1Block
	{
		ZString EntryFilerCode { get; }
		ZString EntryNumber { get; }
		ZString PaymentTypeIndicator { get; }
		ZDate PreliminaryStatementPrintDate { get; }
		ZString ClientBranchDesignation { get; }
	}

	public interface IStatementUpdateAdditionalOutput
	{
		ZString StatementNumber { get; }
		ZDecimal TotalAmountDue { get; }
		ZString PeriodicMonthlyStatementNumber { get; }
		ZDecimal PeriodicMonthlyStatementTotalAmountDue { get; }
	}
}
