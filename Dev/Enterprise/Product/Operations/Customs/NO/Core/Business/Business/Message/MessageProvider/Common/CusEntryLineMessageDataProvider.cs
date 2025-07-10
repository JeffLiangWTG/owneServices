using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NO.Business;

sealed class CusEntryLineMessageDataProvider(CusEntryLine entryLine)
{
	readonly CusEntryLine entryLine = Argument.NotNull(entryLine, nameof(entryLine));

	public IEnumerable<ZString> GetGoodsDescriptions()
		=> MergedInvoiceLines
			.OrderBy(x => x.JI_LineNo)
			.Select(x => x.JI_Description.Left(FreeTextMaxLength))
			.Distinct()
			.Take(5)
			.ToArray();

	public ZWeight GetTotalGrossWeight()
		=> MergedInvoiceLines.Aggregate(ZWeight.Empty, (sum, line) => sum + new ZWeight(line.JI_Weight, line.JI_WeightUQ));

	public ZWeight GetTotalNetWeight()
		=> MergedInvoiceLines.Aggregate(ZWeight.Empty, (sum, line) => sum + new ZWeight(line.JI_NetWeight, line.JI_NetWeightUQ));

	public ZString GetValuationCodeOrMethod() => valuationCodeOrMethod ??= InvoiceLine switch
	{
		{ JI_ValuationCode: { IsEmpty: false } valuation } => valuation,
		{ InvoiceHeader.JZ_ValuationMethod: { IsEmpty: false } valuation } => valuation,
		_ => ZString.Empty,
	};
	ZString? valuationCodeOrMethod;

	public ZString GetProcedureCode() => procedureCode ??= InvoiceLine switch
	{
		{ JI_Procedure: { IsEmpty: false } procedure } => procedure,
		{ EntryInstruction.CEI_Procedure: { IsEmpty: false } procedure } => procedure,
		_ => ZString.Empty,
	};
	ZString? procedureCode;

	IEnumerable<JobComInvoiceLine> MergedInvoiceLines => entryLine.InvoiceLines.Cast<JobComInvoiceLine>();

	JobComInvoiceLine InvoiceLine => entryLine.RandomLine;

	const int FreeTextMaxLength = 31;
}
