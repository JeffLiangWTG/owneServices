using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsMeasureProvider : IGoodsMeasure
{
	public AESGoodsMeasureProvider(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		this.invoiceLines = entryLine.InvoiceLines;
		this.randomInvoiceLine = entryLine.RandomLine;
		this.entryInstruction = Argument.NotNull(randomInvoiceLine.EntryInstruction, $"{nameof(CusEntryLine)}.{nameof(entryLine.RandomLine)}.{nameof(JobComInvoiceLine.EntryInstruction)}");
	}

	readonly CusEntryLine entryLine;
	readonly Customs.Business.InvoiceLinesForEntryLineCollection invoiceLines;
	readonly JobComInvoiceLine randomInvoiceLine;
	readonly CusEntryInstruction entryInstruction;

	public decimal? GrossMassValue => CachedValueHelper.GetValue(ref grossMassValue,
		() => entryInstruction.JobDeclaration.Packages.Count > 0 && InvoiceLineMatchedRuleC0060AndRuleR0222(invoiceLines.Cast<JobComInvoiceLine>()).Any()
		? decimal.Zero
		: entryInstruction.EntryHeader.RandomEntryLine == entryLine
			? InvoiceLineMatchedRuleC0060AndRuleR0222(entryInstruction.InvoiceLines).Union(invoiceLines.Cast<JobComInvoiceLine>()).Sum(invoiceLine => invoiceLine.GrossWeightInKG)
			: invoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.GrossWeightInKG));
	CachedValue<decimal?> grossMassValue;

	public decimal NetMass => CachedValueHelper.GetValue(ref netMass,
		() => invoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.CustomsFirstQuantityInKG));
	CachedValue<decimal> netMass;

	public decimal? SupplementaryUnitsValue => CachedValueHelper.GetValue(ref supplementaryUnitsValue, GetSupplementaryUnitsValue);
	CachedValue<decimal?> supplementaryUnitsValue;

	decimal? GetSupplementaryUnitsValue() => randomInvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly
		? invoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.JI_CustomsSecondQuantity)
		: null;

	bool RuleC0060(Customs.Business.BasePackage basePackage) => basePackage is not Package { IsBulk: true };

	bool RuleR0222(Customs.Business.BaseCusLinkPackage linkPackage) => linkPackage.PackQty.IsEmpty;

	IEnumerable<JobComInvoiceLine> InvoiceLineMatchedRuleC0060AndRuleR0222(IEnumerable<JobComInvoiceLine> linesToCheck)
		=> linesToCheck.Where(invoiceLine => invoiceLine.PackagesForInvoiceLinesForBindingOnly
												.Cast<Customs.Business.BaseCusLinkPackage>()
												.All(linkPackage => RuleC0060(linkPackage.Package) && RuleR0222(linkPackage)));
}
