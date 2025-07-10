using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryLine : EU.Business.Declaration.CusEntryLine, Integration.Customs.PL.ICusEntryLine
{
	public CusEntryLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceLine RandomLine => (JobComInvoiceLine)base.RandomLine;

	public new JobComInvoiceLine FirstLine => (JobComInvoiceLine)base.FirstLine;

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryLineFeeCollection Fees => (CusEntryLineFeeCollection)base.Fees;

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new CusEntryLineValidation Validation => (CusEntryLineValidation)base.Validation;

	public new IEnumerable<SupportingDocument> SupportingDocuments => base.SupportingDocuments.Cast<SupportingDocument>();

	protected override Customs.Business.CusEntryLineValidation GetNewValidation()
		=> Declaration?.IsExport ?? false
			? new ExportCusEntryLineValidation(this)
			: (Declaration?.IsImport ?? false
				? new ImportCusEntryLineValidation(this)
				: new CusEntryLineValidation(this));

	protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new CusEntryLineFeeCollection(this, Factory);

	protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

	public int ContainerCount => Factory.GetValue(ref containerCount, () => Containers.Count());
	CachedProperty<int> containerCount;

	public int AdditionalInfoCount => Factory.GetValue(ref additionalInfoCount, () => AdditionalInfos.Count() + (Header?.AdditionalInfos.Count() ?? 0));
	CachedProperty<int> additionalInfoCount;

	public int PreviousDocumentCount => Factory.GetValue(ref previousDocumentCount, () => PreviousDocuments.Count());
	CachedProperty<int> previousDocumentCount;

	public int SupportingDocumentCount => Factory.GetValue(ref supportingDocumentCount, () => SupportingDocuments.Count() + (Header?.SupportingDocuments.Count() ?? 0));
	CachedProperty<int> supportingDocumentCount;

	protected override void DoMergeInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
	{
		base.DoMergeInvoiceLine(baseInvoiceLine);
		CL_StatisticalValue = Utilities.Round(CL_StatisticalValue < 1 ? 1 : CL_StatisticalValue, 0);
	}

	public CurrencyConverter CUDCurrencyConverter => new RefCurrencyCurrencyConverter(Factory, roundToTargetCurrencyDecimals: false)
	{
		DateForRate = RandomLine.GetCurrencyConverterDateForRate,
		RateType = ExchangeRateType.CustomsMeasureEURExRate
	};

	public RefCurrency LocalCurrency => RandomLine.LocalCurrency;

	public RefCurrency EURCurrency => RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.EuropeanUnion);

	public IEnumerable<CusFiscalReference> FiscalReferencesCombined
	{
		get
		{
			var result = InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.FiscalReferences);
			return Header?.EntryInstruction is CusEntryInstruction cusEntryInstruction
				? cusEntryInstruction.FiscalReferences.Concat(result)
				: result;
		}
	}

	public CurrencyConverter ApportionedCostsCurrencyConverter => new RefCurrencyCurrencyConverter(Factory, roundToTargetCurrencyDecimals: false)
	{
		DateForRate = RandomLine.GetCurrencyConverterDateForRate,
		RateType = ExchangeRateType.Customs
	};
}
