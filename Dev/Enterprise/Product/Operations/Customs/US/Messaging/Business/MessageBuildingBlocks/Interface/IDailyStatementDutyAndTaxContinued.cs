using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IDailyStatementDutyAndTaxContinued
	{
		ZString TeamNumber { get; }
		ZString CensusWarningIndicator { get; }
		ZString ACEIndicator { get; }
		ZString PaperlessElectronicIndicator { get; }
		ZString ElectronicInvoiceIndicator { get; }
		ZString PaymentTypeIndicator { get; }
		ZDecimal CountervailingDutyAmount { get; }
		ZDecimal AntidumpingDutyAmount { get; }
		ZString CountervailingIndicator { get; }
		ZString AntidumpingIndicator { get; }
	}
}
