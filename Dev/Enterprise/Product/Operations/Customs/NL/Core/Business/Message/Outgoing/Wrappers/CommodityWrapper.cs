using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class CommodityWrapper : ICommodity
{
	public CommodityWrapper(EU.Business.Declaration.CusEntryLine cusEntryLine)
	{
		this.cusEntryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
		this.invoiceLines = this.cusEntryLine.InvoiceLines;
		this.invoiceLine = (JobComInvoiceLine)invoiceLines.FirstOrDefault();
	}

	readonly EU.Business.Declaration.CusEntryLine cusEntryLine;
	readonly Customs.Business.InvoiceLinesForEntryLineCollection invoiceLines;
	readonly JobComInvoiceLine invoiceLine;

	public IGoodsMeasure GoodsMeasure => new GoodsMeasureWrapper(cusEntryLine);

	public decimal ItemChargeAmount => cusEntryLine.TotalLinePrice.Amount;

	public ITaxCalculation TaxCalculation => new TaxCalculationWrapper(invoiceLine);

	public IReadOnlyCollection<IClassification> Classifications => classifications ??= GetClassifications().ToList();
	IReadOnlyCollection<IClassification> classifications;

	public string Description => cusEntryLine.CL_Description;

	public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ??=
		invoiceLine.UNDGs.Select((x, index) => new DangerousGoodsWrapper(x, index + 1)).ToList();
	IReadOnlyCollection<IDangerousGoods> dangerousGoods;

	IEnumerable<IClassification> GetClassifications()
	{
		if (!invoiceLine.JI_Tariff.IsEmpty)
		{
			yield return new ClassificationWrapper(1, invoiceLine.JI_Tariff.SubstringSafe(0, 8), NLConstants.Classification.IdentificationTypeCodes.TSP);
			var trc = invoiceLine.JI_Tariff.SubstringSafe(8, 2);
			if (cusEntryLine.Declaration.IsImport && !trc.IsEmpty)
			{
				yield return new ClassificationWrapper(1, trc, NLConstants.Classification.IdentificationTypeCodes.TRC);
			}
		}

		var sequenceGN = 1;
		var sequenceTRA = 1;
		foreach (var supplementaryCode in invoiceLine.SupplementaryCodes)
		{
			if (supplementaryCode.CY_Data.EqualsIgnoringCase(cusEntryLine.CountryCode))
			{
				yield return new ClassificationWrapper(sequenceGN++, supplementaryCode.CY_Code, NLConstants.Classification.IdentificationTypeCodes.GN);
			}
			else
			{
				yield return new ClassificationWrapper(sequenceTRA++, supplementaryCode.CY_Code, NLConstants.Classification.IdentificationTypeCodes.TRA);
			}
		}

		if (!invoiceLine.ZG_CusNumber.IsEmpty)
		{
			yield return new ClassificationWrapper(1, invoiceLine.ZG_CusNumber, NLConstants.Classification.IdentificationTypeCodes.CV);
		}

		if (!invoiceLine.JI_PartNo.IsEmpty)
		{
			yield return new ClassificationWrapper(1, invoiceLine.JI_PartNo, NLConstants.Classification.IdentificationTypeCodes.UIN);
		}
	}
}
