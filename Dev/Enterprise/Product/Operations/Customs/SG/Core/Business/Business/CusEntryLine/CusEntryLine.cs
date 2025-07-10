using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusEntryLine : TypeSafeCusEntryLine, Integration.Customs.SG.ICusEntryLine, ICusInvoice, ICusItem, ICusCertItem
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString GetInvoiceLineTariffToSet(BaseJobComInvoiceLine invoiceLine)
		{
			return RandomLine?.JI_Tariff.Left(Schema.CL_AdValoremTariffMaxLength) ?? ZString.Empty;
		}

		public override ZString CL_CustomsPostedStatus
		{
			get => base.CL_CustomsPostedStatus;
			set
			{
				var oldValue = CL_CustomsPostedStatus;
				base.CL_CustomsPostedStatus = value;
				if (!IsCopying && oldValue != CL_CustomsPostedStatus && !IsValidationSuspended)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		#region Duty/Excise

		public TariffView Tariff => RandomLine?.UniversalTariff;

		public ZDecimal DutiableWGTVOLUNIT
		{
			get
			{
				ZDecimal result = 0m;

				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.SG_TotalDutiableWGTVOLQTY;
				}

				return result;
			}
		}

		public ZString DutiableWGTVOLUNITUQ
		{
			get { return RandomLine != null ? RandomLine.SG_TotalDutiableWGTVOLQTYUnit : ZString.Empty; }
		}

		public ZDecimal DutyUnitRate
		{
			get { return RandomLine != null ? RandomLine.SG_DutyUnitRate : ZDecimal.Zero; }
		}

		public ZDecimal DutyPercentageRate
		{
			get { return RandomLine != null ? RandomLine.SG_DutyPercentageRate : ZDecimal.Zero; }
		}

		public ZDecimal ExciseUnitRate
		{
			get { return RandomLine != null ? RandomLine.SG_ExciseUnitRate : ZDecimal.Zero; }
		}

		public ZString DutyRateUnit
		{
			get
			{
				ZString result = TotalDutiableQuantityUnitType;
				var tariff = Tariff;
				if (tariff != null)
				{
					var tariffUQ = tariff.ZZ1_ZZ8_UQ2;
					if (IsLiquor && tariffUQ == SGConstants.LPA)
					{
						result = SGConstants.LPA;
					}
					else if (IsMotorVehicle && (tariffUQ == UnitOfQuantityCodeList.Codes.NMB || tariffUQ.IsEmpty))
					{
						result = "PER";
					}
				}

				return result;
			}
		}

		public ZDecimal ExcisePercentageRate
		{
			get { return RandomLine != null ? RandomLine.SG_ExcisePercentageRate : ZDecimal.Zero; }
		}

		public ZDecimal OtherTaxUnitRate
		{
			get { return RandomLine?.SG_OtherTaxUnitRate ?? ZDecimal.Zero; }
		}

		public ZDecimal OtherTaxPercentageRate
		{
			get { return RandomLine?.SG_OtherTaxPercentageRate ?? ZDecimal.Zero; }
		}

		public ZInt TobaccoMultiplier
		{
			get { return RandomLine != null ? RandomLine.SG_TobaccoMultiplier : ZInt.Zero; }
		}

		public ZDecimal PercAlcohol
		{
			get { return RandomLine != null ? RandomLine.SG_PercAlcohol : ZDecimal.Zero; }
		}

		public ZDecimal LSP
		{
			get { return RandomLine != null ? RandomLine.SG_LastSellingPrice : ZDecimal.Zero; }
		}

		public bool PreferenceRateApplies
		{
			get { return RandomLine != null && RandomLine.PreferenceRateApplies; }
		}

		protected override ZDecimal GetGSTRate()
		{
			return RandomLine != null ? (ZDecimal)(((JobComInvoiceHeader)RandomLine.InvoiceHeader).SG_GSTRate / 100m)
				: RefCusTaxOrFeeLoader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, ZDateTime.Today)?.ZZF_Value ?? ZDecimal.Zero;
		}

		public decimal ExciseAmount
		{
			get { return Fees.GetAmount(Registry.EntryChargeTypeList.Codes.Excise); }
		}

		public decimal OtherTaxAmount
		{
			get { return Fees.GetAmount(Registry.EntryChargeTypeList.Codes.OtherTax); }
		}

		#endregion

		#region Properties

		public ZBool IsDG
		{
			get { return RandomLine != null && RandomLine.JI_HazMatCodeQualifier == DGIndicatorCodeList.Codes.Y; }
		}

		#endregion

		#region ICusInvoice Members

		ZDecimal ICusInvoice.InvoiceTotalAmount
		{
			get { return RandomLine != null ? RandomLine.InvoiceHeader.JZ_InvoiceAmount : ZDecimal.Zero; }
		}

		ZDecimal ICusInvoice.InvoiceCurrExchangeRate
		{
			get { return RandomLine != null ? RandomLine.InvoiceHeader.EffectiveExchangeRateForInvoiceCurr : ZDecimal.Zero; }
		}

		ZString ICusInvoice.InvoiceCurrency
		{
			get { return RandomLine != null && RandomLine.LinePriceRefCurrency != null ? RandomLine.LinePriceRefCurrency.RX_Code : ZString.Empty; }
		}

		ZDate ICusInvoice.InvoiceDate
		{
			get { return RandomLine != null ? (ZDate)RandomLine.InvoiceHeader.JZ_InvoiceDate : ZDate.Empty; }
		}

		ZGuid ICusInvoice.InvoicePK
		{
			get { return RandomLine != null ? RandomLine.InvoiceHeader.PK : ZGuid.Empty; }
		}

		ZString ICusInvoice.IncoTerm
		{
			get { return RandomLine != null ? RandomLine.InvoiceHeader.JZ_IncoTerm : ZString.Empty; }
		}

		IOrganisation ICusInvoice.Supplier
		{
			get { return RandomLine != null && RandomLine.InvoiceHeader.Supplier != null ? new EntryOrganisationsInfo(RandomLine.InvoiceHeader.Supplier) : null; }
		}

		ICusCharge ICusInvoice.FreightCharge
		{
			get { return GetAllInvoiceHeaderCharges(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight); }
		}

		ICusCharge ICusInvoice.InsuranceCharge
		{
			get { return GetAllInvoiceHeaderCharges(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance); }
		}

		ICusCharge ICusInvoice.OtherCharge
		{
			get { return GetAllInvoiceHeaderCharges(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges); }
		}

		#endregion

		#region ICusItem Members

		ZString ICusItem.SerialNumber
		{
			get { return CL_LineNumber.ToString(); }
		}

		ZString ICusItem.DGIndicator
		{
			get { return IsDG ? DGIndicatorCodeList.Codes.Y : DGIndicatorCodeList.Codes.N; }
		}

		ZString ICusItem.HSCode
		{
			get { return CL_AdValoremTariff; }
		}

		ZString ICusItem.GoodsDescription
		{
			get { return GoodsDescription; }
		}

		ZString ICusItem.BrandName => RandomLine?.JI_BrandName ?? ZString.Empty;

		ZString ICusItem.ModelDescription => RandomLine?.JI_Model ?? ZString.Empty;

		ZString ICusItem.CountryOfOriginCode
		{
			get { return CountryOfOrigin != null ? CountryOfOrigin.Code : ZString.Empty; }
		}

		ZString ICusItem.CurrentLotNumber
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_LotNo); }
		}

		ZString ICusItem.PreviousLotNumber
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_PreviousLotNo); }
		}

		public ZBool IsMotorVehicle
		{
			get { return RandomLine != null && RandomLine.SG_TariffCommodityType == CommodityTypeList.Codes.Vehicle; }
		}

		public ZBool IsLiquor
		{
			get { return RandomLine != null && RandomLine.SG_TariffCommodityType == CommodityTypeList.Codes.Alcohol; }
		}

		public ZBool IsTobacco
		{
			get { return RandomLine != null && RandomLine.SG_TariffCommodityType == CommodityTypeList.Codes.Tobacco; }
		}

		ZBool ICusItem.IsStrategic
		{
			get { return RandomLine != null ? RandomLine.SG_IsStrategic : ZBool.False; }
		}

		ZString ICusItem.CategoryCode
		{
			get { return RandomLine != null ? RandomLine.SG_CategoryCode : ZString.Empty; }
		}

		ZString ICusItem.EndUseCode1
		{
			get { return RandomLine != null ? RandomLine.SG_EndUseCode1 : ZString.Empty; }
		}

		ZString ICusItem.EndUseCode2
		{
			get { return RandomLine != null ? RandomLine.SG_EndUseCode2 : ZString.Empty; }
		}

		ZString ICusItem.EndUseCode3
		{
			get { return RandomLine != null ? RandomLine.SG_EndUseCode3 : ZString.Empty; }
		}

		ZDecimal ICusItem.PercentageOfAlcohol
		{
			get { return RandomLine != null ? RandomLine.SG_PercAlcohol : ZDecimal.Zero; }
		}

		ZDecimal ICusItem.TotalDutiableQuantity
		{
			get { return GetMergedDecimalSum(SGAddInfoSchema.SG_TotalDutiableWGTVOLQTY); }
		}

		public ZString TotalDutiableQuantityUnitType
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTYUnit); }
		}

		ZDecimal ICusItem.UnitDutiableQuantity
		{
			get { return RandomLine != "" ? RandomLine.SG_UnitDutiableWGTVOLQTY : ZDecimal.Zero; }
		}

		ZString ICusItem.UnitDutiableQuantityUnitType
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_UnitDutiableWGTVOLQTYUnit); }
		}

		ZDecimal ICusItem.HSQuantity
		{
			get { return CustomsQuantity; }
		}

		ZString ICusItem.HSQuantityUnitType
		{
			get { return CustomsUnitQty; }
		}

		ZString ICusItem.InvoiceUQ
		{
			get { return InvoiceUQ; }
		}

		#region Bills

		ZString ICusItem.InwardHAWB
		{
			get
			{
				ZString result = ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_InwardHAWB);
				if (result == "")
				{
					result = ((ISGCUSDEC)Header).InwardHouseBill;
				}
				return result;
			}
		}

		ZString ICusItem.InwardMAWB
		{
			get
			{
				ZString result = ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_InwardMAWB);
				if (result == "")
				{
					result = ((ISGCUSDEC)Header).InwardMasterBill;
				}
				return result;
			}
		}

		ZString ICusItem.OutwardHAWB
		{
			get
			{
				ZString result = ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_OutwardHAWB);
				if (result == "")
				{
					result = ((ISGCUSDEC)Header).OutwardHouseBill;
				}
				return result;
			}
		}

		ZString ICusItem.OutwardMAWB
		{
			get
			{
				ZString result = ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_OutwardMAWB);
				if (result == "")
				{
					result = ((ISGCUSDEC)Header).OutwardMasterBill;
				}
				return result;
			}
		}

		#endregion

		#region Packs

		ZInt ICusItem.PackOuterQuantity
		{
			get { return GetMergedIntSum(SGAddInfoSchema.SG_OuterPackQuantity); }
		}

		ZString ICusItem.PackOuterUnitType
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_OuterPackQuantityUnit); }
		}

		ZInt ICusItem.PackInQuantity
		{
			get { return GetMergedIntSum(SGAddInfoSchema.SG_InPackQuantity); }
		}

		ZString ICusItem.PackInUnitType
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_InPackQuantityUnit); }
		}

		ZInt ICusItem.PackInnerQuantity
		{
			get { return GetMergedIntSum(SGAddInfoSchema.SG_InnerPackQuantity); }
		}

		ZString ICusItem.PackInnerUnitType
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_InnerPackQuantityUnit); }
		}

		ZInt ICusItem.PackInmostQuantity
		{
			get { return GetMergedIntSum(SGAddInfoSchema.SG_InmostPackQuantity); }
		}

		ZString ICusItem.PackInmostUnitType
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_InmostPackQuantityUnit); }
		}

		#endregion

		ZString ICusItem.E_SDNPIndicator
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_ESNDPIndicator); }
		}

		ZDecimal ICusItem.CustomsValue
		{
			get
			{
				ZDecimal result = 0m;

				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.JI_CustomsValue;
				}

				return result;
			}
		}

		ZDecimal ICusItem.UnitPrice
		{
			get { return RandomLine != null ? RandomLine.UnitPrice : ZDecimal.Zero; }
		}

		ZDecimal ICusItem.LSPValue
		{
			get { return RandomLine != null ? RandomLine.SG_LastSellingPrice : ZDecimal.Zero; }
		}

		ZDecimal ICusItem.DutyAmount
		{
			get { return DutyAmount; }
		}

		ZDecimal ICusItem.DutyUnitRate
		{
			get { return DutyUnitRate; }
		}

		ZDecimal ICusItem.DutyPercentageRate
		{
			get { return DutyPercentageRate; }
		}

		ZDecimal ICusItem.ExciseAmount
		{
			get { return ExciseAmount; }
		}

		ZDecimal ICusItem.ExciseUnitRate
		{
			get { return ExciseUnitRate; }
		}

		ZDecimal ICusItem.ExcisePercentageRate
		{
			get { return ExcisePercentageRate; }
		}

		ZDecimal ICusItem.OtherTaxAmount
		{
			get { return OtherTaxAmount; }
		}

		ZDecimal ICusItem.OtherTaxUnitRate
		{
			get { return OtherTaxUnitRate; }
		}

		ZDecimal ICusItem.OtherTaxPercentageRate
		{
			get { return OtherTaxPercentageRate; }
		}

		ZDecimal ICusItem.GSTPayable
		{
			get { return GSTVATAmount; }
		}

		ZInt ICusItem.GSTRate
		{
			get { return RandomLine != null ? ((JobComInvoiceHeader)RandomLine.InvoiceHeader).SG_GSTRate : ZInt.Zero; }
		}

		ZString ICusItem.PreferenceIndicator => RandomLine != null ? RandomLine.JI_PrimaryPreference : ZString.Empty;
		ZString ICusItem.MarksAndNumbers
		{
			get { return RandomLine != null ? RandomLine.MarksAndNumbers : ZString.Empty; }
		}

		IEnumerable<ICusProductCode> ICusItem.ProductCodes
		{
			get
			{
				if (RandomLine != null)
				{
					foreach (ICusProductCode cusProductCode in RandomLine.ProductCodes)
					{
						yield return cusProductCode;
					}
				}
			}
		}

		ZString ICusItem.EndUseDescription
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_EndUseDescription); }
		}

		ICusCharge ICusItem.OptionalItemCharge
		{
			get { return GetInvoiceLineCharges(InvoiceLineCharge.ChargeTypes.OptionalItemCharges); }
		}

		ZDate ICusItem.DateOfFirstRegistration
		{
			get { return ReturnZDateFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_FirstRegistrationDate); }
		}

		ZDecimal ICusItem.EngineCapacity
		{
			get { return ReturnZDecimalFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_EngineCapacityPower); }
		}

		ZString ICusItem.EngineCapacityUnit
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_EngineCapacityPowerUnit); }
		}

		ZString ICusItem.RegistrationNumberSG
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_VehicleRegistrationNumber); }
		}

		CASCCode1Collection ICusItem.CASCCodes1
		{
			get { return RandomLine != null ? RandomLine.CASCCode1s : null; }
		}

		CASCCode2Collection ICusItem.CASCCodes2
		{
			get { return RandomLine != null ? RandomLine.CASCCode2s : null; }
		}

		CASCCode3Collection ICusItem.CASCCodes3
		{
			get { return RandomLine != null ? RandomLine.CASCCode3s : null; }
		}

		ZInt ICusItem.StrategicGoodsProductCodeQuantity
		{
			get { return GetMergedIntSum(SGAddInfoSchema.SG_StrategicGoodsProductCodeQuantity); }
		}

		ZString ICusItem.StrategicGoodsProductCodeUQ
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_StrategicGoodsProductCodeQuantityUnit); }
		}

		#region Refund

		ZDecimal ICusItem.ItemDutyRefund
		{
			get { return GetMergedDecimalSum(SGAddInfoSchema.SG_RefundForItemCustomsDutyAmount); }
		}

		ZDecimal ICusItem.ItemExciseRefund
		{
			get { return GetMergedDecimalSum(SGAddInfoSchema.SG_RefundForItemExciseAmount); }
		}

		ZDecimal ICusItem.ItemGSTRefund
		{
			get { return GetMergedDecimalSum(SGAddInfoSchema.SG_RefundForItemGSTAmount); }
		}

		#endregion

		#endregion

		#region ICusCertItem

		ZString ICusCertItem.TextileCategoryCode
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_TextileCatCode); }
		}

		ZDecimal ICusCertItem.TextileQuotaQty
		{
			get { return RandomLine != null ? RandomLine.SG_TextileQuotaQuantity : ZDecimal.Zero; }
		}

		ZString ICusCertItem.TextileQuotaUnitCode
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_TextileQuotaQuantityUnit); }
		}

		ZInt ICusCertItem.PercentageContent
		{
			get { return RandomLine != null ? RandomLine.SG_PercContent : ZInt.Zero; }
		}

		ZDate ICusCertItem.DateOfManufacturingCost
		{
			get { return RandomLine != null ? (ZDate)RandomLine.SG_ManufacturingCostStatementDate : ZDate.Empty; }
		}

		ZDecimal ICusCertItem.ItemValue
		{
			get { return RandomLine != null ? RandomLine.SG_CertItemValue : ZDecimal.Zero; }
		}

		ZDecimal ICusCertItem.ItemQuantity
		{
			get { return GetMergedDecimalSum(SGAddInfoSchema.SG_CertItemQuantity); }
		}

		ZString ICusCertItem.ItemQuantityUnitType
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_CertItemQuantityUnit); }
		}

		ZString ICusCertItem.ItemDescription
		{
			get { return RandomLine != null ? RandomLine.CertItemDescription : ZString.Empty; }
		}

		ZString ICusCertItem.OriginCriterion1
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_CertOriginCriterion1); }
		}

		ZString ICusCertItem.OriginCriterion2
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_CertOriginCriterion2); }
		}

		ZString ICusCertItem.OriginCriterion3
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_CertOriginCriterion3); }
		}

		ZString ICusCertItem.CertHSCode
		{
			get { return ReturnZStringFromItemAddInfoValue(SGAddInfoSchema.Constants.SG_CertHSCode); }
		}

		#endregion

		#region Implementation

		ZString ReturnZStringFromItemAddInfoValue(string propertyName)
		{
			return RandomLine != null ? (ZString)RandomLine[propertyName] : ZString.Empty;
		}

		ZDate ReturnZDateFromItemAddInfoValue(string propertyName)
		{
			//return RandomLine != null ? (ZDate)RandomLine[propertyName] : ZDate.Empty;

			ZDate value = ZDate.Empty;
			if (RandomLine != null)
			{
				ZDateTime lineValue = (ZDateTime)RandomLine[propertyName];
				if (!lineValue.IsEmpty)
				{
					value = (ZDate)lineValue;
				}
			}

			return value;
		}

		ZDecimal ReturnZDecimalFromItemAddInfoValue(string propertyName)
		{
			return RandomLine != null ? (ZDecimal)RandomLine[propertyName] : ZDecimal.Zero;
		}

		ZDecimal GetMergedDecimalSum(SchemaColumn decimalSchemaColumn)
		{
			ZDecimal result = 0;

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				result += (ZDecimal)invoiceLine[decimalSchemaColumn];
			}

			return result;
		}

		ZInt GetMergedIntSum(SchemaColumn intSchemaColumn)
		{
			ZInt result = 0;

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				result += (ZInt)invoiceLine[intSchemaColumn];
			}

			return result;
		}

		#region ICusCharge

		#region Invoice Header Charges

		ICusCharge GetAllInvoiceHeaderCharges(string chargeCode)
		{
			ICusCharge cusCharge = null;

			ChargeCollection chargesCollection = GetAllInvoiceHeaderChargesForChargeCode((JobComInvoiceHeader)RandomLine.InvoiceHeader, chargeCode);

			if (RandomLine != null)
			{
				cusCharge = GetCharge(chargesCollection);
			}

			return cusCharge ?? new CusChargeInfo(SGD, Header.EffectiveValuationDate);
		}

		ChargeCollection GetAllInvoiceHeaderChargesForChargeCode(JobComInvoiceHeader invoiceHeader, string chargeCode)
		{
			Common.JobComInvCharge[] charges = invoiceHeader.Charges.GetCharge(chargeCode);
			Common.JobComInvCharge[] groupCharges = invoiceHeader.GroupCharges.GetCharge(chargeCode);

			ChargeCollection result = new ChargeCollection(Factory);
			result.AddRange(charges);
			result.AddRange(groupCharges);

			return result;
		}

		#endregion

		#region Invoice Line Charges

		ICusCharge GetInvoiceLineCharges(string chargeCode)
		{
			ICusCharge cusCharge = null;

			ChargeCollection chargesCollection = new ChargeCollection(Factory);

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				chargesCollection.AddRange(invoiceLine.Charges.GetCharge(InvoiceLineCharge.ChargeTypes.OptionalItemCharges));
			}

			cusCharge = GetCharge(chargesCollection);

			return cusCharge;
		}

		#endregion

		ICusCharge GetCharge(ChargeCollection charges)
		{
			CusChargeInfo cusCharge = new CusChargeInfo(GetChargeCurrency(charges), Header.EffectiveValuationDate);

			if (charges.Count == 1)
			{
				cusCharge.Percentage = charges[0].J7_Percentage;
			}

			foreach (BaseJobComInvHeaderCharge charge in charges)
			{
				cusCharge.Add(charge.Money);
			}

			return cusCharge;
		}

		RefCurrency GetChargeCurrency(ChargeCollection freightCharges)
		{
			RefCurrency result = null;

			foreach (BaseJobComInvHeaderCharge freightCharge in freightCharges)
			{
				if (result == null)
				{
					result = freightCharge.Currency;
				}

				if (result != freightCharge.Currency)
				{
					result = SGD;
					break;
				}
			}
			return result ?? SGD;
		}

		RefCurrency SGD
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Singapore); }
		}

		#endregion

		#endregion

		RefCusTaxOrFee.Loader RefCusTaxOrFeeLoader => refCusTaxOrFeeLoader ?? (refCusTaxOrFeeLoader = new RefCusTaxOrFee.Loader(Factory));
		RefCusTaxOrFee.Loader refCusTaxOrFeeLoader;
	}
}
