using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

sealed class CUSDECDocumentMessageSummaryWrapper(CusEntryHeader entryHeader, JobComInvoiceHeader invoiceHeader) : ICUSDECMessageDataProvider.IDocumentMessageSummary
{
	CusEntryHeader EntryHeader { get; } = Argument.NotNull(entryHeader, nameof(entryHeader));

	JobComInvoiceHeader InvoiceHeader { get; } = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));

	public IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetails> ItemDetailsCollection => itemDetailsCollection ??= GetItemDetailsCollection();
	IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetails> itemDetailsCollection;

	IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetails> GetItemDetailsCollection()
	{
		return EntryHeader.AllEntryLines
			.Select(x => new CUSDECItemDetailsWrapper(x))
			.ToImmutableArray();
	}

	public ZString InvoiceNumber => InvoiceHeader.JZ_InvoiceNumber;

	public ZString InvoiceDate => InvoiceHeader.JZ_InvoiceDate.ToShortCustomsFormatDateString(4);
}
