using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
{
	public EntryCreationStrategy(JobDeclaration declaration) : base(declaration)
	{
	}

	public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
	{
		return GetMergeBy() == OrgConstants.MergeInvoiceLines.NotMerge || baseInvoiceLine is not JobComInvoiceLine invoiceLine
			? base.GetKeyForLine(baseInvoiceLine)
			: GetMergeKeyCore(invoiceLine);
	}

	protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
	{
		var mergeKey = base.GetKeyForHeaderCore(invoiceLine);
		if (invoiceLine?.InvoiceHeader is JobComInvoiceHeader invoiceHeader)
		{
			mergeKey.Add(invoiceHeader.JZ_ValuationMethod);
		}
		return mergeKey;
	}

	static MergeKey GetMergeKeyCore(JobComInvoiceLine invoiceLine)
	{
		var mergeKey = new MergeKey();
		mergeKey.Add(invoiceLine.JI_ValuationCode);
		mergeKey.Add(invoiceLine.JI_Procedure);
		mergeKey.Add(invoiceLine.JI_Tariff);
		mergeKey.Add(invoiceLine.JI_CountryOfOrigin);
		mergeKey.Add(invoiceLine.JI_StateOrRegionOfOrigin);
		mergeKey.Add(invoiceLine.JI_PrimaryPreference);
		mergeKey.Add(invoiceLine.JI_ReducedCustomsFlag);
		mergeKey.Add(invoiceLine.JI_CustomsRateOverrideValue);
		mergeKey.Add(invoiceLine.JI_CustomsRateOverrideType);
		mergeKey.Add(invoiceLine.JI_ZZF_NKTaxType);
		mergeKey.Add(invoiceLine.JI_MergeOverride);
		mergeKey.Add(GetAlcoholStrengthOrDefault(invoiceLine));
		ProcessSupportingDocsForLineMergeKey(mergeKey, invoiceLine);
		return mergeKey;
	}

	static ZDecimal GetAlcoholStrengthOrDefault(JobComInvoiceLine invoiceLine)
	{
		return invoiceLine switch
		{
			_ when invoiceLine.JI_CustomsUnitQty == UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength => invoiceLine.JI_CustomsQuantity,
			_ when invoiceLine.JI_CustomsSecondUnitQty == UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength => invoiceLine.JI_CustomsSecondQuantity,
			_ when invoiceLine.JI_CustomsThirdUnitQty == UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength => invoiceLine.JI_CustomsThirdQuantity,
			_ when invoiceLine.JI_CustomsFourthUnitQty == UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength => invoiceLine.JI_CustomsFourthQuantity,
			_ when invoiceLine.JI_CustomsFifthUnitQty == UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength => invoiceLine.JI_CustomsFifthQuantity,
			_ => ZDecimal.Zero
		};
	}

	static void ProcessSupportingDocsForLineMergeKey(MergeKey mergeKey, JobComInvoiceLine invoiceLine)
	{
		foreach (var doc in invoiceLine.SupportingDocuments.Cast<SupportingDocument>())
		{
			mergeKey.Add(doc.CSI_Code);
			mergeKey.Add(doc.CSI_ReferenceNumber);
		}
	}
}
