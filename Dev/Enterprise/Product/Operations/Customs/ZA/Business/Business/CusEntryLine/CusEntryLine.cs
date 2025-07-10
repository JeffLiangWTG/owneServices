using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using static Enterprise.Integration.Customs;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusEntryLine : AutoZACusEntryLine, ICusEntryLine, ICusCodeDataTypeSupporter
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override ECB.CusEntryLineLookups GetNewLookups()
		{
			return new CusEntryLineLookups(this);
		}

		protected override ECB.CusEntryLineValidation GetNewValidation()
		{
			return new CusEntryLineValidation(this);
		}

		#region New Properties

		public bool IsLine1
		{
			get { return CL_LineNumber == 1; }
		}

		public ZString EntryNumber
		{
			get
			{
				CusEntryHeader header = this.Header;
				return header != null ? header.EntryNumber : ZString.Empty;
			}
		}

		public ZDecimal BND => BNDCore;

		protected virtual ZDecimal BNDCore => ZDecimal.ParseSafe(AdditionalInformationCodes.GetFirstElementHaving(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount)?.CY_Data ?? "0.0", 0.0);

		#region AdditionalInformations

		[ChildEditable(true)]
		public AdditionalInformationCollection AdditionalInformationCodes
		{
			get
			{
				if (additionalInformationCodes == null)
				{
					additionalInformationCodes = new AdditionalInformationCollection(this);
					additionalInformationCodes.Load();
					RegisterEditableChildObject(additionalInformationCodes);
				}
				return additionalInformationCodes;
			}
		}
		AdditionalInformationCollection additionalInformationCodes;

		#endregion

		#region ProvisionalPayments

		[ChildEditable]
		public ProvisionalPaymentAmountCodeDataCollection ProvisionalPayments
		{
			get
			{
				if (provisionalPayments == null)
				{
					provisionalPayments = new ProvisionalPaymentAmountCodeDataCollection(this);
					provisionalPayments.Load();
					RegisterEditableChildObject(provisionalPayments);
				}
				return provisionalPayments;
			}
		}
		ProvisionalPaymentAmountCodeDataCollection provisionalPayments;

		#endregion

		public ZString ValuationCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (Header.ValueDeterminationNumber.IsEmpty)
				{
					if (RandomLine != null && RandomLine.InvoiceHeader != null)
					{
						result = RandomLine.InvoiceHeader.JZ_RelatedIndicator + RandomLine.InvoiceHeader.JZ_ValuationCode;
					}
				}
				return result;
			}
		}

		public ZDecimal CustomsDuty
		{
			get { return DutyAmount; }
		}

		public ZDecimal ProvisionalPaymentAmount => GetProvisionalPaymentAmountCore().AmountIncludingPPValueToLiquidate;

		(ZDecimal AmountIncludingPPValueToLiquidate, IEnumerable<IDutyFeeInformation> Details) GetProvisionalPaymentAmountCore()
		{
			var (amountResult, detailsResult) = (ZDecimal.Zero, Enumerable.Empty<IDutyFeeInformation>());

			if (MessageDataProviderInstruction.ShouldOutputProvisionalPayments(MessageKeyFactor))
			{
				var (amount, details) = ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.ProvisionalPayment);

				if (IsLine1)
				{
					var (isPPTypeLiquidated, ppValueToLiquidate) = HasPPTypeBeenLiquidated();
					if (!isPPTypeLiquidated)
					{
						amount += ppValueToLiquidate;
					}
				}

				(amountResult, detailsResult) = (amount, details);
			}
			return (amountResult, detailsResult);
		}

		(ZBool IsLiquidated, ZDecimal PPValueToLiquidate) HasPPTypeBeenLiquidated()
		{
			var result = (false, ZDecimal.Zero);

			var instruction = EntryInstruction;
			var headerPPType = instruction?.CEI_ProvisionalPaymentType ?? ZString.Empty;
			if (!headerPPType.IsEmpty)
			{
				var isLiquidated = Header?.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(headerPPType, 1) ?? false;
				var headerPPValue = instruction?.CEI_ProvisionalPaymentAmount ?? ZDecimal.Zero;
				result = (isLiquidated, headerPPValue);
			}

			return result;
		}

		public ZDecimal PenaltyAmount => GetPenaltyAmountCore().Amount;

		(ZDecimal Amount, IEnumerable<IDutyFeeInformation> Details) GetPenaltyAmountCore()
		{
			var (amountResult, detailsResult) = (ZDecimal.Zero, Enumerable.Empty<IDutyFeeInformation>());

			if (MessageDataProviderInstruction.ShouldOutputProvisionalPayments(MessageKeyFactor))
			{
				(amountResult, detailsResult) = ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.Penalty);
			}

			return (amountResult, detailsResult);
		}

		public ZDecimal DutySch1P2B
		{
			get { return GetDutySch1P2B(false); }
		}

		internal ZDecimal GetDutySch1P2B(bool? landCostOnly = null)
		{
			ZDecimal result = 0m;

			foreach (CusEntryLineFee fee in Fees)
			{
				if ((!landCostOnly.HasValue || fee.CF_IsLandedCostOnly == landCostOnly.Value) && RateCodesWithEX1.Any(x => x.ZY1_RateCode == fee.CF_ChargeType))
				{
					result += fee.CF_ChargeAmount;
				}
			}
			return result;
		}

		public ZDecimal OtherDA63Duties
		{
			get { return GetOtherDA63Duties(); }
		}

		ZDecimal GetOtherDA63Duties()
		{
			ZDecimal result = 0m;
			result = Fees.OfType<CusEntryLineFee>().Where(f => ChargeTypeHelper.GetOtherDA63DutiesCodes().Contains(f.CF_ChargeType)).Select(x => (decimal)x.CF_ChargeAmount).Sum();
			return result;
		}

		ZDecimal GetCustomsDutiesSchedule1P1And2()
		{
			var result = Fees.OfType<CusEntryLineFee>().Where(f => ChargeTypeHelper.GetCustomsDutiesSchedule1P1And2Codes().Contains(f.CF_ChargeType)).Select(x => (decimal)x.CF_ChargeAmount).Sum();
			return result;
		}

		public ZDecimal ConversionFactor => RandomLine?.InvoiceHeader?.JZ_Calc_ConversionFactor ?? ZDecimal.Zero;

		public ZDecimal VAT
		{
			get { return ZArchitecture.Core.Utilities.Round(GSTVATAmount, 2); }
		}

		/// <summary> Rounded to two decimals as it is used in EntryHeader and Documents </summary>
		public virtual ZDecimal ImportDutyPaid
		{
			get
			{
				ZDecimal result = 0;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					result += line.JI_ImportDutyPaid;
				}
				return ZArchitecture.Core.Utilities.Round(result, 2);
			}
		}

		/// <summary> Rounded to two decimals as it is used in EntryHeader and Documents </summary>
		public virtual ZDecimal ImportDutySch1P2BPaid
		{
			get
			{
				ZDecimal result = 0;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					result += line.JI_ImportSch1P2BPaid;
				}
				return ZArchitecture.Core.Utilities.Round(result, 2);
			}
		}

		/// <summary> Rounded to two decimals as it is used in EntryHeader and Documents </summary>
		public virtual ZDecimal ImportVATPaid
		{
			get
			{
				ZDecimal result = 0;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					result += line.JI_ImportVATPaid;
				}
				return ZArchitecture.Core.Utilities.Round(result, 2);
			}
		}

		public virtual ZDecimal ImportCustomsValue
		{
			get
			{
				ZDecimal result = 0;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					result += line.JI_ImportCustomsValue;
				}
				return result.RoundUsingCustomsValueRule();
			}
		}

		public ZDateTime EffectiveAssessmentDate => RandomLine?.EffectiveAssessmentDate ?? ZDateTime.Today;

		internal CalcFeeValues GetCalcFeeValues()
			=> Factory.GetValue(ref calcFeeValuesCached, () =>
				{
					var result = new CalcFeeValues();
					result.CustomsDutiesExcluding12B = CustomsDutiesExcluding12B;
					result.CustomsDutyExcluding12B = CustomsDuty - DutySch1P2B;
					result.S1P2BDuty = DutySch1P2B;
					result.ValueAddedTax = VAT;
					(result.ProvisionalPayment, result.ProvisionalPayments) = GetProvisionalPaymentAmountCore();
					(result.Penalty, result.Penalties) = GetPenaltyAmountCore();
					result.CustomsDutiesSchedule1P1andSchedule2 = GetCustomsDutiesSchedule1P1And2();
					return result;
				});
		CachedProperty<CalcFeeValues> calcFeeValuesCached;

		IEnumerable<IDutyFeeInformation> CustomsDutiesExcluding12B
		{
			get
			{
				if (!IsDeclarationIntegrated)
				{
					foreach (CusEntryLineFee duty in Fees)
					{
						if (!duty.CF_IsLandedCostOnly && duty.IsPayableDuty && RateCodesWithEX1.All(x => x.ZY1_RateCode != duty.CF_ChargeType))
						{
							yield return new DutyFeeInformationDocWrapper(duty.CF_ChargeType, duty.CF_ChargeAmount);
						}
					}
				}
			}
		}

		IEnumerable<CusRefRateCodeView> RateCodesWithEX1 => Factory.GetRateCodesWithEX1();

		#endregion

		#region Overrides

		protected override ZString GetFallbackInvoiceLineDescription()
		{
			ZString lastDesc = ZString.Empty;
			foreach (BaseJobComInvoiceLine line in InvoiceLines)
			{
				if (lastDesc.IsEmpty)
				{
					lastDesc = line.JI_Description;
				}

				if (!string.Equals(line.JI_Description, lastDesc, StringComparison.OrdinalIgnoreCase))
				{
					lastDesc = ZString.Empty;
					break;
				}
			}
			return lastDesc;
		}

		public ZString RebateDescription
		{
			get
			{
				var rebateAmount = ZDecimal.Zero;
				var rateCodes = Universal.CusRefRateCodeView.Loader.LoadByRateType(Factory, Core.Constants.CountryCodes.SouthAfrica, Universal.Constants.RateTypes.Rebate);
				if (rateCodes != null)
				{
					foreach (var rateCode in rateCodes)
					{
						var fee = Fees.GetElementWithThisCode(rateCode.ZY1_RateCode);
						if (fee != null)
						{
							rebateAmount += fee.CF_ChargeAmount;
						}
					}
				}
				return (rebateAmount != ZDecimal.Zero) ? ZString.Format("Rebate Amount; {0:F2}", rebateAmount) : ZString.Empty;
			}
		}

		protected override ICusEntryLineFeeCollection<ECB.CusEntryLineFee, ECB.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);
		}

		public override void RoundCustomsValue()
		{
			var customsValue = Math.Max(CL_CustomsValue, 1m);
			CL_CustomsValue = (new ZDecimal(customsValue)).RoundUsingCustomsValueRule();
		}

		protected override ZDecimal GetGSTVATAmountCore()
		{
			if (IsDeclarationIntegrated)
			{
				return Fees.GetAmount(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT);
			}
			else
			{
				return Fees.GetAmount(RandomLine?.JI_ZZF_NKTaxType ?? ZString.Empty);
			}
		}

		protected override ZDecimal GetDutyAmountCore()
		{
			return GetDutyAmount();
		}

		internal ZDecimal GetDutyAmount()
		{
			var result = 0m;
			if (IsDeclarationIntegrated)
			{
				result = Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount);
			}
			else
			{
				foreach (CusEntryLineFee duty in Fees)
				{
					if (!duty.CF_IsLandedCostOnly && duty.IsPayableDuty)
					{
						result += duty.CF_ChargeAmount;
					}
				}
			}
			return result;
		}

		public ZBool IsDeclarationIntegrated => Header?.Declaration?.IsDeclarationIntegrated ?? ZBool.True;

		internal ZDecimal GetDutyAmountForLandedCosting()
		{
			var result = 0m;
			if (IsDeclarationIntegrated)
			{
				result = Fees.GetAmountIncludingLCOnly(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount);
			}
			else
			{
				foreach (CusEntryLineFee duty in Fees)
				{
					if (duty.IsPayableDuty)
					{
						result += duty.CF_ChargeAmount;
					}
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		public override void ResetTotalsAndCachedValues()
		{
			base.ResetTotalsAndCachedValues();
			messageKeyFactor = null;
		}

		protected override void AddCustomsQuantitiesForInvoiceLine(BaseJobComInvoiceLine line)
		{
			JobComInvoiceLine invoiceLine = line as JobComInvoiceLine;
			CustomsQuantities.AddQuantity(new InvoiceLineQuantity(invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty));

			if (!invoiceLine.JI_CustomsSecondQuantity.IsEmpty && !invoiceLine.JI_CustomsSecondUnitQty.IsEmpty)
			{
				CustomsQuantities.AddQuantity(new InvoiceLineQuantity(invoiceLine.JI_CustomsSecondQuantity, invoiceLine.JI_CustomsSecondUnitQty));
			}
			if (!invoiceLine.JI_CustomsThirdQuantity.IsEmpty && !invoiceLine.JI_CustomsThirdUnitQty.IsEmpty)
			{
				CustomsQuantities.AddQuantity(new InvoiceLineQuantity(invoiceLine.JI_CustomsThirdQuantity, invoiceLine.JI_CustomsThirdUnitQty));
			}
			if (!invoiceLine.JI_BondedWhsQuantity.IsEmpty && !invoiceLine.JI_BondedWhsUnitQty.IsEmpty)
			{
				CustomsQuantities.AddQuantity(new InvoiceLineQuantity(Convert.ToDecimal(invoiceLine.JI_BondedWhsQuantity), invoiceLine.JI_BondedWhsUnitQty));
			}
		}

		#endregion

		protected override ECB.BondedWarehouseTransactionLine GetNewBondedWarehouseTransactionLine()
		{
			return new BondedWarehouseTransactionLine(this);
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new ECB.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.AdditionalInformation, typeof(AdditionalInformation));
			result.Add(CusCodeDataTypeList.Codes.PPAmount, typeof(ProvisionalPaymentAmountCodeData));
			return result;
		}

		#region ICusEntryLine Members

		public ZInt PreviousEntryLineNumber
		{
			get
			{
				var randomLine = RandomLine;
				return randomLine != null ? randomLine.JI_PreviousEntryLineNumber : ZInt.Zero;
			}
		}

		public ZString PrimaryPreference
		{
			get
			{
				var randomLine = RandomLine;
				return randomLine == null ? ZString.Empty :
					randomLine.HasIntoWarehouseProcedure ? new ZString(UniversalReferenceConstants.PrimaryPreference.Standard) : randomLine.JI_PrimaryPreference;
			}
		}

		public ZBool JI_TakeUpInTradeStatistics
		{
			get
			{
				var randomLine = RandomLine;
				return randomLine != null ? randomLine.JI_TakeUpInTradeStatistics : ZBool.False;
			}
		}

		public bool IsImport
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsImport;
			}
		}

		public ZString ValueDeterminationNumber
		{
			get
			{
				ZString result = ZString.Empty;
				var randomLine = RandomLine;
				if (randomLine != null && randomLine.InvoiceHeader != null)
				{
					result = RandomLine.InvoiceHeader.JZ_VDN;
				}
				return result;
			}
		}
		protected override ZDecimal GetCustomsQuantity()
		{
			var customQty = base.GetCustomsQuantity();
			return customQty >= 0.01m ? customQty : new ZDecimal(0.01);
		}

		#endregion

		public CusEntryInstruction EntryInstruction => EntryInstructionCore;

		protected virtual CusEntryInstruction EntryInstructionCore => RandomLine?.EntryInstruction;

		public ZString ProcedureCategory => RandomLine?.ProcedureCategory ?? ZString.Empty;

		public ZString CustomsProcedureCode => EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZString PreviousProcedureCode => RandomLine?.JI_Calc_PreviousProcedure ?? ZString.Empty;

		public RefCusProcedure CusProcedure => RandomLine?.CusProcedure;

		public ZString ProcedureMeasure
		{
			get { return RandomLine?.ProcedureMeasureTariff?.GetTariffCodeWithCheckDigit() ?? ZString.Empty; }
		}

		[CargoWise.ComponentModel.List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.CountryOfOrigins))]
		public ZString CalcGoodsOrigin => (this as ILineLevelInformation).CountryOfOrigin;

		public ZString CalcPreviousMRN => (this as ILineLevelInformation).PreviousProcedureMRN;

		public ZInt CalcPreviousLineNumber => MessageDataProviderInstruction.ShouldOutputPreviousMRN(MessageKeyFactor) ? RandomLine?.JI_PreviousEntryLineNumber ?? ZInt.Zero : ZInt.Zero;

		public ZDecimal CalcCustomsQuantity
		{
			get
			{
				var customQty = (this as ILineLevelInformation).CustomsQuantity;
				return customQty > 0.01m ? customQty : new ZDecimal(0.01);
			}
		}

		public ZString CalcCustomsUnitQty => (this as ILineLevelInformation).CustomsUnitQty;

		public ZDecimal CalcAdditionalQuantity => (this as ILineLevelInformation).AdditionalQuantity;

		public ZString CalcAdditionalUnitQty => (this as ILineLevelInformation).AdditionalUnitQty;

		public ZDecimal CalcClassificationQuantity => (this as ILineLevelInformation).ClassificationQuantity;

		public ZString CalcClassificationUnitQty => (this as ILineLevelInformation).ClassificationUnitQty;

		public ZDecimal CalcWarehouseCountableQuantity => (this as ILineLevelInformation).WarehouseCountableQuantity;

		public ZString CalcWarehouseCountableUnitQty => (this as ILineLevelInformation).WarehouseCountableUnitQty;

		public ZString CalcTradeAgreement => RandomLine?.TradeAgreementCode ?? ZString.Empty;

		public ZString CalcTradeAgreementLabel => RandomLine?.TradeAgreementLabel ?? ZString.Empty;

		public ZString CalcPreference => RandomLine?.JI_PrimaryPreference ?? ZString.Empty;

		public ZString CalcROOCert => RandomLine?.JI_ROOCert ?? ZString.Empty;

		public ZDecimal CustomsDutyExcluding12B => GetCalcFeeValues().CustomsDutyExcluding12B;

		public ZBool IsSpecifiedMotorVehicle => IsSpecifiedMotorVehicleCore;

		protected virtual ZBool IsSpecifiedMotorVehicleCore => RandomLine.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.SpecifiedMotorVehicle) ?? ZBool.False;

		public List<Action<ZShort>> AdditionalInformationCodesActions => additionalInformationCodesActions ?? (additionalInformationCodesActions = new List<Action<ZShort>>());

		List<Action<ZShort>> additionalInformationCodesActions;

		#region Concurrency Handling

		protected override void OnConcurrencyExceptionCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			if (!IsDeleted)
			{
				base.OnConcurrencyExceptionCore(propertyRecords);
			}
		}

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			if (!IsDeleted)
			{
				base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);
			}
		}

		#endregion
	}
}
