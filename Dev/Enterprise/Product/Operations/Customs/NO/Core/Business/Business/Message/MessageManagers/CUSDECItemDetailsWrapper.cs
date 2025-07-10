using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

sealed class CUSDECItemDetailsWrapper(CusEntryLine entryLine) : ICUSDECMessageDataProvider.IItemDetails
{
	JobComInvoiceLine InvoiceLine => EntryLine.RandomLine;

	CusEntryLine EntryLine { get; } = Argument.NotNull(entryLine, nameof(entryLine));

	InvoiceLinesForEntryLineCollection MergedInvoiceLines => EntryLine.InvoiceLines;

	JobDeclaration Declaration => entryLine.Declaration;

	public IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetailsFee> ItemDetailsFeeCollection => itemDetailsFeeCollection ??= GetItemDetailsFeeCollection();
	IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetailsFee> itemDetailsFeeCollection;

	IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetailsFee> GetItemDetailsFeeCollection()
	{
		return EntryLine.Fees
			.Where(x => !x.CF_IsLandedCostOnly)
			.Select(x => new CUSDECItemDetailsFeeWrapper(x))
			.ToImmutableArray();
	}

	public ZString CountryOfOrigin => CountryOfOriginCore();

	ZString CountryOfOriginCore() 
	{
		if (Declaration.IsImport || InvoiceLine.JI_CountryOfOrigin != Constants.CountryCodes.Norway)
		{
			return InvoiceLine.JI_CountryOfOrigin;
		}
		return ZString.Empty;
	}

	public ZString RegionOfOrigin => Declaration.IsImport ? ZString.Empty : InvoiceLine.JI_StateOrRegionOfOrigin;

	public ZString GeneralGoodsMarks => InvoiceLine.JI_GoodsMarks switch
	{
		{ IsEmpty: false } goodsMarks => goodsMarks,
		_ => "ADR",
	};

	public ZString ChassisNumber => Declaration.IsImport ? ChassisNumberCore() : ZString.Empty;

	ZString ChassisNumberCore()
	{
		return InvoiceLine.PackagesPivot
			.Select(x => x.Package)
			.WhereNotNull()
			.Where(x => x.CW_PackType == UniversalReferenceConstants.PackageTypes.VehicleChassisNumber)
			.Select(x => x.CW_MarksAndNos)
			.FirstOrDefault();
	}

	public IReadOnlyCollection<ZString> ContainerNumbers => containerNumbers ??=
		InvoiceLine.ContainersPivot
			.Select(x => x.ContainerNumber)
			.ToImmutableArray();
	IReadOnlyCollection<ZString> containerNumbers;

	public ZString NumberOfPackages => InvoiceLine.JI_InvoiceQuantity switch
	{
		{ IsEmpty: true } => "1",
		var quantity => quantity.ToNorwegianAmountString(),
	};

	public ZString PackageType => UniversalReferenceConstants.PackageTypes.PackageInGeneral;

	public ZString DeclarationLineNumber => InvoiceLine.JI_LineNo.ToString();

	public ZString TariffNumber => InvoiceLine switch
	{
		{ CusEntryLine.CL_AdValoremTariff: { IsEmpty: false } tariff } => tariff,
		{ JI_Tariff: { IsEmpty: false } tariff } => tariff,
		_ => ZString.Empty,
	};

	public ZString ProcedureCode => EntryLineMessageDataProvider.GetProcedureCode();

	public ZString PreferenceCode => InvoiceLine.JI_PrimaryPreference;

	public ZString ValuationMethod => EntryLineMessageDataProvider.GetValuationCodeOrMethod();

	public ZString ReducedCustomsFlag => Declaration.IsImport ? InvoiceLine.JI_ReducedCustomsFlag : ZString.Empty;

	public ZString AdjustedValue => Declaration.IsImport ? AdjustedValueCore.Round(0).ToString() : "0";

	ZDecimal AdjustedValueCore => MergedInvoiceLines
		.SelectMany(x => x.ApportionedCharges)
		.Where(x => x.ChargeCode.Code != "VGE")
		.Sum(x => GetAdjustedValueForCharge(x));

	static ZDecimal GetAdjustedValueForCharge(BaseInvoiceLineApportionedCharge charge)
	{
		var money = charge.Money;
		if (money.Currency is { Code: not Constants.CurrencyCodes.Norway })
		{
			money = charge.MoneyInLocalCurrency;
		}
		var isDeduction = charge.J7_ChargeType == Common.CustomsChargeTypeList.Codes.DeductionCharge || charge.IsDiscount;
		return isDeduction ? -1m * money.Amount : money.Amount;
	}

	public ZBool AdjustedValueIsDiscount => AdjustedValueCore < 0m;

	public ZString StatisticValue => EntryLine.CL_StatisticalValue.ToNorwegianAmountString();

	public ZString AddedValueDueToProcessingAbroad => AddedValueDueToProcessingAbroadCore.ToNorwegianAmountString();

	ZDecimal AddedValueDueToProcessingAbroadCore => Declaration.IsImport &&
		InvoiceLine.ApportionedCharges.Any(x => x.J7_ChargeType == NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported)
			? EntryLine.CL_CustomsValue
			: ZDecimal.Zero;

	public ZString GrossWeight => EntryLineMessageDataProvider.GetTotalGrossWeight().Amount.ToNorwegianAmountString();

	public ZString NetWeight => EntryLineMessageDataProvider.GetTotalNetWeight().Amount.ToNorwegianAmountString();

	public ZString CustomsQtyAmount
	{
		get
		{
			var invoiceLines = MergedInvoiceLines.Where(HasSecondUnitQtyOfTypeCU2).ToArray();
			return invoiceLines.Length > 0
				? new ZDecimal(invoiceLines.Sum(i => i.JI_CustomsSecondQuantity)).ToNorwegianAmountString()
				: ZString.Empty;
		}
	}

	public ZString CustomsQtyUnitOfMeasurement => HasSecondUnitQtyOfTypeCU2(InvoiceLine)
		? InvoiceLine.JI_CustomsSecondUnitQty
		: ZString.Empty;

	public ZString[] GoodsDescriptions => EntryLineMessageDataProvider.GetGoodsDescriptions().ToArray();

	public IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments> ItemDetailsSupportingDocumentsCollection => itemDetailsSupportingDocumentsCollection ??= GetItemDetailsSupportingDocumentsCollection();
	IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments> itemDetailsSupportingDocumentsCollection;

	IReadOnlyCollection<ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments> GetItemDetailsSupportingDocumentsCollection()
	{
		return EntryLine.RandomLine.SupportingDocuments
			.Select(x => new CUSDECItemDetailsDocumentsWrapper(x))
			.ToImmutableArray();
	}

	static bool HasSecondUnitQtyOfTypeCU2(BaseJobComInvoiceLine line)
	{
		if (line is not {
			JI_CustomsSecondUnitQty: { IsEmpty: false } qty,
			UniversalTariff.UnitsOfMeasure: { } unitsOfMeasure })
		{
			return false;
		}
		return unitsOfMeasure.Any(u => u.ZZ8_Type == UnitTypeCU2 && u.ZZ8_UOM == qty);
	}

	CusEntryLineMessageDataProvider EntryLineMessageDataProvider => entryLineMessageDataProvider ??= new(entryLine);
	CusEntryLineMessageDataProvider entryLineMessageDataProvider;

	const string UnitTypeCU2 = "CU2";
}
