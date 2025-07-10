using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;

	public class ItemsTestClass : ICusItem, ICusCertItem
	{
		#region ICusItem Members

		public ZString SerialNumber
		{
			get { return fSerialNumber; }
			set { fSerialNumber = value; }
		}
		ZString fSerialNumber;

		public ZString DGIndicator
		{
			get { return fDGIndicator; }
			set { fDGIndicator = value; }
		}
		ZString fDGIndicator;

		public ZString InvoiceUQ
		{
			get { return fInvoiceUQ; }
			set { fInvoiceUQ = value; }
		}
		ZString fInvoiceUQ;

		public ZString HSCode
		{
			get { return fHSCode; }
			set { fHSCode = value; }
		}
		ZString fHSCode;

		public ZString GoodsDescription
		{
			get { return fGoodsDescription; }
			set { fGoodsDescription = value; }
		}
		ZString fGoodsDescription;

		public ZString BrandName
		{
			get { return fBrandName; }
			set { fBrandName = value; }
		}
		ZString fBrandName;

		public ZString ModelDescription
		{
			get { return fModelDescription; }
			set { fModelDescription = value; }
		}
		ZString fModelDescription;

		public ZString CountryOfOriginCode
		{
			get { return fCountryOfOriginCode; }
			set { fCountryOfOriginCode = value; }
		}
		ZString fCountryOfOriginCode;

		public ZString CurrentLotNumber
		{
			get { return fCurrentLotNumber; }
			set { fCurrentLotNumber = value; }
		}
		ZString fCurrentLotNumber;

		public ZString PreviousLotNumber
		{
			get { return fPreviousLotNumber; }
			set { fPreviousLotNumber = value; }
		}
		ZString fPreviousLotNumber;

		public ZString InwardHAWB
		{
			get { return fInwardHAWB; }
			set { fInwardHAWB = value; }
		}
		ZString fInwardHAWB;

		public ZString InwardMAWB
		{
			get { return fInwardMAWB; }
			set { fInwardMAWB = value; }
		}
		ZString fInwardMAWB;

		public ZString OutwardHAWB
		{
			get { return fOutwardHAWB; }
			set { fOutwardHAWB = value; }
		}
		ZString fOutwardHAWB;

		public ZString OutwardMAWB
		{
			get { return fOutwardMAWB; }
			set { fOutwardMAWB = value; }
		}
		ZString fOutwardMAWB;

		public ZBool IsDangerous
		{
			get { return fIsDangerous; }
			set { fIsDangerous = value; }
		}
		ZBool fIsDangerous;

		public ZBool IsMotorVehicle
		{
			get { return fIsMotorVehicle; }
			set { fIsMotorVehicle = value; }
		}
		ZBool fIsMotorVehicle;

		public ZBool IsLiquor
		{
			get { return fIsLiquor; }
			set { fIsLiquor = value; }
		}
		ZBool fIsLiquor;

		public ZBool IsTobacco
		{
			get { return fIsTobacco; }
			set { fIsTobacco = value; }
		}
		ZBool fIsTobacco;

		public ZBool IsStrategic
		{
			get { return isStrategic; }
			set { isStrategic = value; }
		}
		ZBool isStrategic;

		public ZString CategoryCode
		{
			get { return categoryCode; }
			set { categoryCode = value; }
		}
		ZString categoryCode;

		public ZString EndUseCode1
		{
			get { return endUseCode1; }
			set { endUseCode1 = value; }
		}
		ZString endUseCode1;

		public ZString EndUseCode2
		{
			get { return endUseCode2; }
			set { endUseCode2 = value; }
		}
		ZString endUseCode2;

		public ZString EndUseCode3
		{
			get { return endUseCode3; }
			set { endUseCode3 = value; }
		}
		ZString endUseCode3;

		public ZBool IsInBondedWarehouse
		{
			get { return fIsInBondedWarehouse; }
			set { fIsInBondedWarehouse = value; }
		}
		ZBool fIsInBondedWarehouse;

		public ZDecimal PercentageOfAlcohol
		{
			get { return fPercentageOfAlcohol; }
			set { fPercentageOfAlcohol = value; }
		}
		ZDecimal fPercentageOfAlcohol;

		public ZString PercentageOfAlcoholUnitType
		{
			get { return fPercentageOfAlcoholUnitType; }
			set { fPercentageOfAlcoholUnitType = value; }
		}
		ZString fPercentageOfAlcoholUnitType;

		public ZString PreferenceIndicator
		{
			get { return fPreferenceIndicator; }
			set { fPreferenceIndicator = value; }
		}
		ZString fPreferenceIndicator;

		public ZString CASCProductCode
		{
			get { return fCASCProductCode; }
			set { fCASCProductCode = value; }
		}
		ZString fCASCProductCode;

		public ZBool IsBasedOnRates
		{
			get { return fIsBasedOnRates; }
			set { fIsBasedOnRates = value; }
		}
		ZBool fIsBasedOnRates;

		public ZDecimal TotalDutiableQuantity
		{
			get { return fTotalDutiableQuantity; }
			set { fTotalDutiableQuantity = value; }
		}
		ZDecimal fTotalDutiableQuantity;

		public ZString TotalDutiableQuantityUnitType
		{
			get { return fTotalDutiableQuantityUnitType; }
			set { fTotalDutiableQuantityUnitType = value; }
		}
		ZString fTotalDutiableQuantityUnitType;

		public ZDecimal HSQuantity
		{
			get { return fHSQuantity; }
			set { fHSQuantity = value; }
		}
		ZDecimal fHSQuantity;

		public ZString HSQuantityUnitType
		{
			get { return fHSQuantityUnitType; }
			set { fHSQuantityUnitType = value; }
		}
		ZString fHSQuantityUnitType;

		public ZDecimal UnitPriceQuantity
		{
			get { return fUnitPriceQuantity; }
			set { fUnitPriceQuantity = value; }
		}
		ZDecimal fUnitPriceQuantity;

		public ZDecimal UnitDutiableQuantity
		{
			get { return fUnitDutiableQuantity; }
			set { fUnitDutiableQuantity = value; }
		}
		ZDecimal fUnitDutiableQuantity;

		public ZString UnitDutiableQuantityUnitType
		{
			get { return fUnitDutiableQuantityUnitType; }
			set { fUnitDutiableQuantityUnitType = value; }
		}
		ZString fUnitDutiableQuantityUnitType;

		public ZInt PackOuterQuantity
		{
			get { return fPackOuterQuantity; }
			set { fPackOuterQuantity = value; }
		}
		ZInt fPackOuterQuantity;

		public ZString PackOuterUnitType
		{
			get { return fPackOuterUnitType; }
			set { fPackOuterUnitType = value; }
		}
		ZString fPackOuterUnitType;

		public ZInt PackInQuantity
		{
			get { return fPackInQuantity; }
			set { fPackInQuantity = value; }
		}
		ZInt fPackInQuantity;

		public ZString PackInUnitType
		{
			get { return fPackInUnitType; }
			set { fPackInUnitType = value; }
		}
		ZString fPackInUnitType;

		public ZInt PackInnerQuantity
		{
			get { return fPackInnerQuantity; }
			set { fPackInnerQuantity = value; }
		}
		ZInt fPackInnerQuantity;

		public ZString PackInnerUnitType
		{
			get { return fPackInnerUnitType; }
			set { fPackInnerUnitType = value; }
		}
		ZString fPackInnerUnitType;

		public ZInt PackInmostQuantity
		{
			get { return fPackInmostQuantity; }
			set { fPackInmostQuantity = value; }
		}
		ZInt fPackInmostQuantity;

		public ZString PackInmostUnitType
		{
			get { return fPackInmostUnitType; }
			set { fPackInmostUnitType = value; }
		}
		ZString fPackInmostUnitType;

		public ZString PackingUnitType
		{
			get { return fPackingUnitType; }
			set { fPackingUnitType = value; }
		}
		ZString fPackingUnitType;

		public ZString E_SDNPIndicator
		{
			get { return fE_SDNPIndicator; }
			set { fE_SDNPIndicator = value; }
		}
		ZString fE_SDNPIndicator;

		public ZDecimal CustomsValue
		{
			get { return fCustomsValue; }
			set { fCustomsValue = value; }
		}
		ZDecimal fCustomsValue;

		public ZDecimal UnitPrice
		{
			get { return fUnitPrice; }
			set { fUnitPrice = value; }
		}
		ZDecimal fUnitPrice;

		public ZDecimal LSPValue
		{
			get { return fLSPValue; }
			set { fLSPValue = value; }
		}
		ZDecimal fLSPValue;

		public ZDecimal DutyAmount
		{
			get { return fDutyAmount; }
			set { fDutyAmount = value; }
		}
		ZDecimal fDutyAmount;

		public ZDecimal DutyUnitRate
		{
			get { return fDutyUnitRate; }
			set { fDutyUnitRate = value; }
		}
		ZDecimal fDutyUnitRate;

		public ZString DutyUnitRateUnit
		{
			get { return fDutyUnitRateUnit; }
			set { fDutyUnitRateUnit = value; }
		}
		ZString fDutyUnitRateUnit;

		public ZDecimal DutyPercentageRate
		{
			get { return fDutyPercentageRate; }
			set { fDutyPercentageRate = value; }
		}
		ZDecimal fDutyPercentageRate;

		public ZDecimal OtherTaxAmount
		{
			get; set;
		}

		public ZDecimal OtherTaxUnitRate
		{
			get; set;
		}

		public ZDecimal OtherTaxPercentageRate
		{
			get; set;
		}

		public ZDecimal ExciseAmount
		{
			get { return fExciseAmount; }
			set { fExciseAmount = value; }
		}
		ZDecimal fExciseAmount;

		public ZDecimal ExciseUnitRate
		{
			get { return fExciseUnitRate; }
			set { fExciseUnitRate = value; }
		}
		ZDecimal fExciseUnitRate;

		public ZString DutyRateUnit
		{
			get { return dutyRateUnit; }
			set { dutyRateUnit = value; }
		}
		ZString dutyRateUnit;

		public ZDecimal ExcisePercentageRate
		{
			get { return fExcisePercentageRate; }
			set { fExcisePercentageRate = value; }
		}
		ZDecimal fExcisePercentageRate;

		public ZDecimal GSTPayable
		{
			get { return fGSTPayable; }
			set { fGSTPayable = value; }
		}
		ZDecimal fGSTPayable;

		public ZInt GSTRate
		{
			get { return fGSTRate; }
			set { fGSTRate = value; }
		}
		ZInt fGSTRate;

		public ZString MarksAndNumbers
		{
			get { return fMarksAndNumbers; }
			set { fMarksAndNumbers = value; }
		}
		ZString fMarksAndNumbers;

		public IEnumerable<ICusProductCode> ProductCodes
		{
			get
			{
				if (fProductCodes == null)
				{
					fProductCodes = System.Array.Empty<ICusProductCode>();
				}
				return fProductCodes;
			}
			set { fProductCodes = value; }
		}
		IEnumerable<ICusProductCode> fProductCodes;

		public ZDecimal ItemQuantity
		{
			get { return fItemQuantity; }
			set { fItemQuantity = value; }
		}
		ZDecimal fItemQuantity;

		public ZString ItemQuantityUnitType
		{
			get { return fItemQuantityUnitType; }
			set { fItemQuantityUnitType = value; }
		}
		ZString fItemQuantityUnitType;

		public ZDate DateOfManufacturingCost
		{
			get { return fDateOfManufacturingCost; }
			set { fDateOfManufacturingCost = value; }
		}
		ZDate fDateOfManufacturingCost;

		public ZString ItemDescription
		{
			get { return fItemDescription; }
			set { fItemDescription = value; }
		}
		ZString fItemDescription;

		public ZInt PercentageContent
		{
			get { return fPercentageContent; }
			set { fPercentageContent = value; }
		}
		ZInt fPercentageContent;

		public ZString OriginCriterion1
		{
			get { return fOriginCriterion1; }
			set { fOriginCriterion1 = value; }
		}
		ZString fOriginCriterion1;

		public ZString OriginCriterion2
		{
			get { return fOriginCriterion2; }
			set { fOriginCriterion2 = value; }
		}
		ZString fOriginCriterion2;

		public ZString OriginCriterion3
		{
			get { return fOriginCriterion3; }
			set { fOriginCriterion3 = value; }
		}
		ZString fOriginCriterion3;

		public ZString CertHSCode
		{
			get { return certHSCode; }
			set { certHSCode = value; }
		}
		ZString certHSCode;

		public ZString TextileCategoryCode
		{
			get { return fTextileCategoryCode; }
			set { fTextileCategoryCode = value; }
		}
		ZString fTextileCategoryCode;

		public ZDecimal TextileQuotaQty
		{
			get { return fTextileQuotaQty; }
			set { fTextileQuotaQty = value; }
		}
		ZDecimal fTextileQuotaQty;

		public ZString TextileQuotaUnitCode
		{
			get { return fTextileQuotaUnitCode; }
			set { fTextileQuotaUnitCode = value; }
		}
		ZString fTextileQuotaUnitCode;

		public ZString EndUserInformation
		{
			get { return fEndUserInformation; }
			set { fEndUserInformation = value; }
		}
		ZString fEndUserInformation;

		public ZDecimal ItemValue
		{
			get { return fItemValue; }
			set { fItemValue = value; }
		}
		ZDecimal fItemValue;

		public ZString EndUseDescription
		{
			get { return fEndUseDescription; }
			set { fEndUseDescription = value; }
		}
		ZString fEndUseDescription;

		public ZDecimal ItemDutyRefund
		{
			get { return fItemDutyRefund; }
			set { fItemDutyRefund = value; }
		}
		ZDecimal fItemDutyRefund;

		public ZDecimal ItemExciseRefund
		{
			get { return fItemExciseRefund; }
			set { fItemExciseRefund = value; }
		}
		ZDecimal fItemExciseRefund;

		public ZDecimal ItemGSTRefund
		{
			get { return fItemGSTRefund; }
			set { fItemGSTRefund = value; }
		}
		ZDecimal fItemGSTRefund;

		public ICusCharge OptionalItemCharge
		{
			get { return fOptionalItemCharge; }
			set { fOptionalItemCharge = value; }
		}
		ICusCharge fOptionalItemCharge;

		public ZDecimal EngineCapacity
		{
			get { return fEngineCapacity; }
			set { fEngineCapacity = value; }
		}
		ZDecimal fEngineCapacity;

		public ZString EngineCapacityUnit
		{
			get { return fEngineCapacityUnit; }
			set { fEngineCapacityUnit = value; }
		}
		ZString fEngineCapacityUnit;

		public ZDate DateOfFirstRegistration
		{
			get { return fDateOfFirstRegistration; }
			set { fDateOfFirstRegistration = value; }
		}
		ZDate fDateOfFirstRegistration;

		public ZString RegistrationNumberSG
		{
			get { return fRegistrationNumberSG; }
			set { fRegistrationNumberSG = value; }
		}
		ZString fRegistrationNumberSG;

		public CASCCode1Collection CASCCodes1
		{
			get { return fCASCCodes1; }
			set { fCASCCodes1 = value; }
		}
		CASCCode1Collection fCASCCodes1;

		public CASCCode2Collection CASCCodes2
		{
			get { return fCASCCodes2; }
			set { fCASCCodes2 = value; }
		}
		CASCCode2Collection fCASCCodes2;

		public CASCCode3Collection CASCCodes3
		{
			get { return fCASCCodes3; }
			set { fCASCCodes3 = value; }
		}
		CASCCode3Collection fCASCCodes3;

		public ZInt StrategicGoodsProductCodeQuantity
		{
			get { return fStrategicGoodsProductCodeQuantity; }
			set { fStrategicGoodsProductCodeQuantity = value; }
		}
		ZInt fStrategicGoodsProductCodeQuantity;

		public ZString StrategicGoodsProductCodeUQ
		{
			get { return fStrategicGoodsProductCodeUQ; }
			set { fStrategicGoodsProductCodeUQ = value; }
		}
		ZString fStrategicGoodsProductCodeUQ;

		#endregion

		#region ICusInvoice Members

		public ZString InvoiceCurrency
		{
			get { return fCurrencyOfCharge; }
			set { fCurrencyOfCharge = value; }
		}
		ZString fCurrencyOfCharge;

		public ZDecimal InvoiceTotalAmount
		{
			get { return fInvoiceTotalAmount; }
			set { fInvoiceTotalAmount = value; }
		}
		ZDecimal fInvoiceTotalAmount;

		public ZDecimal InvoiceCurrExchangeRate
		{
			get { return fInvoiceCurrExchangeRate; }
			set { fInvoiceCurrExchangeRate = value; }
		}
		ZDecimal fInvoiceCurrExchangeRate;

		public ZString IncoTerm
		{
			get { return fUnitPriceTermType; }
			set { fUnitPriceTermType = value; }
		}
		ZString fUnitPriceTermType;

		public ZString InvoiceNumber
		{
			get { return fInvoiceNumber; }
			set { fInvoiceNumber = value; }
		}
		ZString fInvoiceNumber;

		public ZDate InvoiceDate
		{
			get { return fInvoiceDate; }
			set { fInvoiceDate = value; }
		}
		ZDate fInvoiceDate;

		public ZGuid InvoicePK
		{
			get { return fInvoicePK; }
			set { fInvoicePK = value; }
		}
		ZGuid fInvoicePK;

		public IOrganisation Supplier
		{
			get { return fSupplier; }
			set { fSupplier = value; }
		}
		IOrganisation fSupplier;

		public ICusCharge FreightCharge
		{
			get { return fFreightCharge ?? (fFreightCharge = new CusChargeInfo(SGD, ZDateTime.Now)); }
			set { fFreightCharge = value; }
		}
		ICusCharge fFreightCharge;

		public ICusCharge InsuranceCharge
		{
			get { return fInsuranceCharge ?? (fInsuranceCharge = new CusChargeInfo(SGD, ZDateTime.Now)); }
			set { fInsuranceCharge = value; }
		}
		ICusCharge fInsuranceCharge;

		public ICusCharge OtherCharge
		{
			get { return fOtherCharge ?? (fOtherCharge = new CusChargeInfo(SGD, ZDateTime.Now)); }
			set { fOtherCharge = value; }
		}
		ICusCharge fOtherCharge;

		RefCurrency SGD
		{
			get { return RefCurrency.LoadFromCurrencyCode(new BusinessObjectFactory(), Core.Constants.CurrencyCodes.Singapore); }
		}

		#endregion
	}
}
