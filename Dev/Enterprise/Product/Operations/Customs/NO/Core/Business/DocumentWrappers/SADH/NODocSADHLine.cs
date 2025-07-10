using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSADHLine : DocBaseWrapper
{
	NODocSADHLine(CusEntryLine entryLine, BusinessObjectFactory factory) : base(entryLine, factory)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		entryHeader = Argument.NotNull(entryLine.Header, nameof(entryLine.Header));
	}

	readonly CusEntryLine entryLine;
	readonly CusEntryHeader entryHeader;

	public static NODocSADHLine New(CusEntryLine entryLine, BusinessObjectFactory factory) => new(entryLine, factory);

	JobComInvoiceLine[] InvoiceLines => invoiceLines ??= entryLine.InvoiceLines.ToArray<JobComInvoiceLine>();
	JobComInvoiceLine[] invoiceLines;

	internal ZDecimal TotalFreightCostInLocalCurrency => InvoiceLines.Sum(l => l.JI_Calc_FreightInLocalCurrency);

	public NODocSADHMarksAndNumbersCollection Box31MarksAndNumbers => box31MarksAndNumbers ??= GetBox31MarksAndNumbers();
	NODocSADHMarksAndNumbersCollection box31MarksAndNumbers;

	public NODocSADHMarksAndNumbersCollection Box31MarksAndNumbersFromLine27 => box31MarksAndNumbersFromLine27 ??= GetBox31MarksAndNumbersFromLine27();
	NODocSADHMarksAndNumbersCollection box31MarksAndNumbersFromLine27;

	public NODocSupportingDocumentCollection Box44SupportingDocuments => box44SupportingDocuments ??= GetBox44SupportingDocumentsCore();
	NODocSupportingDocumentCollection box44SupportingDocuments;

	public NODocSADHLineTaxCollection Box47Taxes => box47Taxes ??= GetBox47TaxesCore();
	NODocSADHLineTaxCollection box47Taxes;

	public ZString Box31MarksAndNumbers_NoInfo => Box31MarksAndNumbers.Count == 0 ? Res.GetString("e8eeb817-5c42-4d16-8f4f-dd138edef0cd", "ADR") : ZString.Empty;

	public ZString Box31GoodsDescription => entryLine.RandomLine.JI_Description;

	public ZShort Box32LineNumber => entryLine.CL_LineNumber;

	public ZString Box33Tariff => entryLine.CL_AdValoremTariff;

	public ZString Box33ReducedCustoms => entryLine.ReducedCustomsFlag;

	public ZString Box34CountryOfOrigin => entryLine.CtryOfOrigin;

	public ZString Box35GrossWeight => GetGrossWeight();

	public ZString Box36Preference => entryLine.Preference;

	public ZString Box37Procedure => !entryLine.Procedure.IsEmpty ? entryLine.Procedure : entryHeader.Procedure;

	public ZString Box38NetWeight => GetNetWeight();

	public ZString Box41CusQuantityAndType => GetCusQuantityAndType();

	ZString Box41CusQuantityType => entryLine.RandomLine.JI_CustomsSecondUnitQty;

	public ZString Box43ValuationCode => entryLine.ValuationCode;

	public ZString Adjustments
	{
		get
		{
			var ajustments = entryLine.AdjustmentsRounded;
			if (ajustments.IsEmpty)
			{
				return ZString.Empty;
			}
			return ajustments.ToString();
		}
	}

	public ZString StatisticalValue
	{
		get
		{
			var statisticalValue = entryLine.CL_StatisticalValue;
			if (statisticalValue.IsEmpty)
			{
				return ZString.Empty;
			}
			return statisticalValue.ToStringRounded(0);
		}
	}

	NODocSADHMarksAndNumbersCollection GetBox31MarksAndNumbers()
	{
		var marksCollection = new NODocSADHMarksAndNumbersCollection(Factory);

		foreach (var line in InvoiceLines)
		{
			if (!line.JI_GoodsMarks.IsEmpty)
			{
				foreach (var singleLineMark in line.JI_GoodsMarks.Split(System.Environment.NewLine))
				{
					marksCollection.AddIfValueIsDistinct(singleLineMark);
				}
			}

			line.ContainersForInvoiceLinesForBindingOnly
				.Cast<Customs.Business.NonPersistentCusContainer>()
				.Where(x => x.IsForInvoiceLine)
				.ForEach(x => marksCollection.AddIfValueIsDistinct(x.ContainerNumber));

			line.PackagesForInvoiceLinesForBindingOnly
				.Cast<Customs.Business.BaseCusLinkPackage>()
				.Where(bclp => bclp.IsLinked)
				.Select(bclp => bclp.Package)
				.Where(bp => bp.CW_PackType == "VN")
				.ForEach(bp => marksCollection.AddIfValueIsDistinct(bp.CW_MarksAndNos));
		}
		return marksCollection;
	}

	NODocSADHMarksAndNumbersCollection GetBox31MarksAndNumbersFromLine27()
	{
		var startIndex = 26;
		var marksCollection = new NODocSADHMarksAndNumbersCollection(Factory);

		for (int i = startIndex; i < Box31MarksAndNumbers.Count; i++)
		{
			marksCollection.Add(Box31MarksAndNumbers[i]);
		}
		return marksCollection;
	}

	NODocSupportingDocumentCollection GetBox44SupportingDocumentsCore() => new(entryLine.RandomLine.SupportingDocuments.Cast<SupportingDocument>(), Factory);

	NODocSADHLineTaxCollection GetBox47TaxesCore()
	{
		var lineTaxCollection = new NODocSADHLineTaxCollection(Factory);
		entryLine.Fees
			.Where(fee => !fee.CF_IsLandedCostOnly)
			.ForEach(fee => lineTaxCollection.Add(NODocSADHLineTax.New(fee, Factory)));
		lineTaxCollection.Sort<NODocSADHLineTax>(DutyComparer.Comparison);
		return lineTaxCollection;
	}

	ZString GetGrossWeight()
	{
		return entryLine.EffectiveGrossWeight.InKilogramsSafe.ToString("g", NOCultureInfo) + "K";
	}

	ZString GetNetWeight()
	{
		return entryLine.EffectiveNetWeight.InKilogramsSafe.ToString("g", NOCultureInfo) + "K";
	}

	ZString GetCusQuantityAndType()
	{
		var result = ZString.Empty;
		var cu2 = entryLine.RandomLine.UniversalTariff?.UnitsOfMeasure?.FirstOrDefault(a => a.ZZ8_Type == UOMTypeList.Codes.CU2)?.ZZ8_UOM ?? ZString.Empty;
		if (!cu2.IsEmpty && Box41CusQuantityType == cu2)
		{
			var value = InvoiceLines.Where(i => i.JI_CustomsSecondUnitQty == cu2).Sum(i => i.JI_CustomsSecondQuantity);
			var number = value.ToString("g", NOCultureInfo).TrimEnd('0').TrimEnd(',');
			result = number + " " + cu2;
		}

		return result;
	}

	static CultureInfo NOCultureInfo => new("nb-NO");
}
