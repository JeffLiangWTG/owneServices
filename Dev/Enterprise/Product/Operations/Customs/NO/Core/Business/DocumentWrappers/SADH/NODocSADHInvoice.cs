using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSADHInvoice : DocBaseWrapper
{
	public static NODocSADHInvoice New(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory) => new (invoiceHeader, factory);

	NODocSADHInvoice(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory) : base(invoiceHeader, factory)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
	}

	readonly JobComInvoiceHeader invoiceHeader;

	public ZString CurrencyCode => invoiceHeader.JZ_RX_NKInvoice_Currency;

	public ZString CurrencyExchangeRate => GetCurrencyExchangeRate();

	public ZString IncoTerm => invoiceHeader.JZ_IncoTerm;

	public ZString IncoTermPlace => invoiceHeader.JZ_IncoTermPlace;

	public ZString InvoiceAmount => invoiceHeader.JZ_InvoiceAmount.ToStringRounded(2);

	public ZString InvoiceAmountNOK => invoiceHeader.JZ_InvoiceAmountInLocalCurrency.ToStringRounded(2);

	public ZInt NumberOfEntryHeaders => NumberOfEntryHeadersCore();

	public ZString InvoiceDate => invoiceHeader.JZ_InvoiceDate.ToCustomsFormatString("yyyy.MM.dd");

	public ZString InvoiceNumber => invoiceHeader.JZ_InvoiceNumber;

	public ZString SupplierName => invoiceHeader.Supplier?.OH_FullName ?? ZString.Empty;

	public ZString ValuationCode => invoiceHeader.JZ_ValuationCode;

	ZString GetCurrencyExchangeRate()
	{
		var invoiceLine = invoiceHeader.InvoiceLines.FirstOrDefault() as JobComInvoiceLine;
		var exchangeRate = invoiceLine?.InvoiceLineCurrencyExchangeRateForCustoms ?? ZDecimal.Zero;
		exchangeRate = exchangeRate == ZDecimal.Zero ? 1m : exchangeRate;
		return exchangeRate.ToStringRounded(3);
	}

	ZInt NumberOfEntryHeadersCore()
	{
		var noOfEntryInstructions = invoiceHeader.InvoiceLines
			.Select(ji => ji.EntryInstruction)
			.Distinct()
			.Take(2)
			.Count();
		return noOfEntryInstructions;
	}
}
