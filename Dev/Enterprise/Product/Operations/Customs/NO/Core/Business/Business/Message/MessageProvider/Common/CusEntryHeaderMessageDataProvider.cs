using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

sealed class CusEntryHeaderMessageDataProvider
{
	public CusEntryHeaderMessageDataProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		invoiceHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(entryHeader.RandomHeader));
	}
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly JobComInvoiceHeader invoiceHeader;

	public ZDecimal GetTotalInvoiceAmount()
	{
		if (InvoiceLines.Length <= 1)
		{
			return invoiceHeader.JZ_InvoiceAmount;
		}
		return InvoiceLines.Sum(HasMultipleCurrencies
			? x => x.JI_LinePriceInLocalCurrency
			: x => x.JI_LinePrice);
	}

	public ZString GetGoodsNumberPosition()
	{
		return entryHeader.EntryInstruction is not { CEI_SubPosition: { IsEmpty: false } subPosition }
			? declaration.JE_Position
			: $"{declaration.JE_Position}/{subPosition}";
	}

	public bool HasMultipleCurrencies => hasMultipleCurrencies ??=
#if NETFRAMEWORK
		InvoiceLines.DistinctBy(x => x.InvoiceHeader.Invoice_Currency)
#else
		Enumerable.DistinctBy(InvoiceLines, x => x.InvoiceHeader.Invoice_Currency)
#endif
			.Count() > 1;
	bool? hasMultipleCurrencies;

	ImmutableArray<JobComInvoiceLine> InvoiceLines => invoiceLines ??= entryHeader
		.MergedLines
		.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>())
		.ToImmutableArray();
	ImmutableArray<JobComInvoiceLine>? invoiceLines;
}
