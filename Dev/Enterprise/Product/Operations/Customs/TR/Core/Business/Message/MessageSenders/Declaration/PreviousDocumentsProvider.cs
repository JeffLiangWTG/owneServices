using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class PreviousDocumentsProvider : IDeclarationOpeningAndClosingInfo
	{
		public PreviousDocumentsProvider(JobComInvoiceLine invoiceLine, ZDecimal totalPackQty, ZString allDescriptions)
		{
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			TotalPackQty = totalPackQty;
			AllDescriptions = allDescriptions;
		}
		JobComInvoiceLine InvoiceLine { get; }
		ZDecimal TotalPackQty { get; }
		ZString AllDescriptions { get; }

		public string ClosedDeclarationNo => InvoiceLine.JI_PreviousEntryNumber;
		public int ClosedDeclarationLineNo => InvoiceLine.JI_PreviousEntryLineNumber;
		public decimal ClosedQuantity => TotalPackQty.RoundAmount();
		public string Description => AllDescriptions.Trim();
	}
}
