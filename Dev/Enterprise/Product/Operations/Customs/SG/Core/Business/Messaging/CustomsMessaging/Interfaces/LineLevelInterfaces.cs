using System.Collections.Generic;

using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public interface ICusInvoice
	{
		ZGuid InvoicePK { get; }
		ZDate InvoiceDate { get; }

		ZDecimal InvoiceTotalAmount { get; }
		ZDecimal InvoiceCurrExchangeRate { get; }

		ZString IncoTerm { get; }
		ZString InvoiceCurrency { get; }
		ZString InvoiceNumber { get; }

		ICusCharge FreightCharge { get; }
		ICusCharge InsuranceCharge { get; }
		ICusCharge OtherCharge { get; }

		IOrganisation Supplier { get; }
	}

	public interface ICusItem : ICusInvoice
	{
		ZBool IsMotorVehicle { get; }
		ZBool IsLiquor { get; }
		ZBool IsTobacco { get; }
		ZBool IsStrategic { get; }

		ZDecimal DutyAmount { get; }
		ZDecimal DutyPercentageRate { get; }
		ZDecimal DutyUnitRate { get; }
		ZDecimal CustomsValue { get; }
		ZDecimal EngineCapacity { get; }
		ZDecimal ExciseAmount { get; }
		ZDecimal ExciseUnitRate { get; }
		ZDecimal ExcisePercentageRate { get; }
		ZDecimal OtherTaxAmount { get; }
		ZDecimal OtherTaxUnitRate { get; }
		ZDecimal OtherTaxPercentageRate { get; }
		ZDecimal GSTPayable { get; }
		ZInt GSTRate { get; }
		ZDecimal HSQuantity { get; }
		ZDecimal ItemDutyRefund { get; }
		ZDecimal ItemExciseRefund { get; }
		ZDecimal ItemGSTRefund { get; }
		ZDecimal LSPValue { get; }
		ZDecimal PercentageOfAlcohol { get; }
		ZDecimal TotalDutiableQuantity { get; }
		ZDecimal UnitDutiableQuantity { get; }
		ZDecimal UnitPrice { get; }

		ZDate DateOfFirstRegistration { get; }

		ZInt PackInmostQuantity { get; }
		ZInt PackInnerQuantity { get; }
		ZInt PackInQuantity { get; }
		ZInt PackOuterQuantity { get; }
		ZInt StrategicGoodsProductCodeQuantity { get; }

		ZString BrandName { get; }
		ZString CategoryCode { get; }
		ZString CountryOfOriginCode { get; }
		ZString CurrentLotNumber { get; }
		ZString DGIndicator { get; }
		ZString E_SDNPIndicator { get; }
		ZString EndUseCode1 { get; }
		ZString EndUseCode2 { get; }
		ZString EndUseCode3 { get; }
		ZString EndUseDescription { get; }
		ZString EngineCapacityUnit { get; }
		ZString DutyRateUnit { get; }
		ZString GoodsDescription { get; }
		ZString HSCode { get; }
		ZString HSQuantityUnitType { get; }
		ZString InwardHAWB { get; }
		ZString InwardMAWB { get; }
		ZString MarksAndNumbers { get; }
		ZString ModelDescription { get; }
		ZString InvoiceUQ { get; }
		ZString OutwardHAWB { get; }
		ZString OutwardMAWB { get; }
		ZString PackOuterUnitType { get; }
		ZString PackInUnitType { get; }
		ZString PackInnerUnitType { get; }
		ZString PackInmostUnitType { get; }
		ZString PreferenceIndicator { get; }
		ZString PreviousLotNumber { get; }
		ZString RegistrationNumberSG { get; }
		ZString SerialNumber { get; }
		ZString StrategicGoodsProductCodeUQ { get; }
		ZString TotalDutiableQuantityUnitType { get; }
		ZString UnitDutiableQuantityUnitType { get; }

		CASCCode1Collection CASCCodes1 { get; }
		CASCCode2Collection CASCCodes2 { get; }
		CASCCode3Collection CASCCodes3 { get; }

		IEnumerable<ICusProductCode> ProductCodes { get; }

		ICusCharge OptionalItemCharge { get; }
	}

	public interface ICusCertItem : ICusItem
	{
		ZDate DateOfManufacturingCost { get; }

		ZDecimal ItemValue { get; }
		ZDecimal ItemQuantity { get; }

		ZInt PercentageContent { get; }
		ZDecimal TextileQuotaQty { get; }

		ZString ItemDescription { get; }
		ZString ItemQuantityUnitType { get; }
		ZString OriginCriterion1 { get; }
		ZString OriginCriterion2 { get; }
		ZString OriginCriterion3 { get; }
		ZString CertHSCode { get; }
		ZString TextileCategoryCode { get; }
		ZString TextileQuotaUnitCode { get; }
	}

	public interface ICusCharge
	{
		ZDecimal ExchangeRate { get; }
		ZDecimal Amount { get; }
		ZDecimal Percentage { get; }

		ZString CurrencyCode { get; }
	}

	public interface ITariffData
	{
		ProductCodeCollection ProductCodes { get; }
		ZString TariffNum { get; }
		ZDecimal PercAlcohol { get; }
	}
}
