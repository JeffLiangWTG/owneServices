using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.ZArchitecture.Schema;
using Inst = Enterprise.Customs.ZA.Business.MessageDataProviderInstruction;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusEntryLine : ILineLevelInformation
	{
		#region Cached Value for MessageDataProviderInstruction

		internal MessageDataProviderKeyFactor MessageKeyFactor
		{
			get
			{
				if (messageKeyFactor == null)
				{
					var invoiceLine = RandomLine;
					messageKeyFactor = invoiceLine == null ? CreateMessageKeyFactor(Declaration, Header) : CreateMessageKeyFactor(Declaration, invoiceLine);
				}
				return messageKeyFactor;
			}
		}
		MessageDataProviderKeyFactor messageKeyFactor;

		static MessageDataProviderKeyFactor CreateMessageKeyFactor(JobDeclaration declaration, CusEntryHeader entry)
		{
			var cpc = ZString.Empty;
			var procedureCategory = ZString.Empty;
			var procedureConcession = ZString.Empty;
			if (entry != null)
			{
				var entryInstruction = entry.EntryInstruction;
				if (entryInstruction != null)
				{
					cpc = entryInstruction.CEI_Style;
					var procedure = entryInstruction.CusProcedure;
					if (procedure != null)
					{
						procedureCategory = procedure.ZZ6_Category;
						procedureConcession = procedure.Concessions?.FirstNonSpecificTariffType(entry.Factory) ?? ZString.Empty;
					}
				}
			}
			return CreateMessageKeyFactor(declaration, cpc, ZString.Empty, procedureCategory, procedureConcession);
		}

		internal static MessageDataProviderKeyFactor CreateMessageKeyFactor(JobDeclaration declaration, JobComInvoiceLine invoiceLine)
		{
			var cpc = ZString.Empty;
			var ppc = ZString.Empty;
			var procedureCategory = ZString.Empty;
			var procedureConcession = ZString.Empty;
			if (invoiceLine != null)
			{
				var procedure = invoiceLine.CusProcedure;
				if (procedure == null)
				{
					ppc = invoiceLine.JI_Calc_PreviousProcedure;
					var entryInstruction = invoiceLine.EntryInstruction;
					if (entryInstruction != null)
					{
						cpc = entryInstruction.CEI_Style;
						procedure = entryInstruction.CusProcedure;
						if (procedure != null)
						{
							procedureCategory = procedure.ZZ6_Category;
							procedureConcession = procedure.Concessions?.FirstNonSpecificTariffType(invoiceLine.Factory) ?? ZString.Empty;
						}
					}
				}
				else
				{
					cpc = procedure.ZZ6_ProcedureCode;
					ppc = procedure.ZZ6_PreviousProcedureCode;
					procedureCategory = procedure.ZZ6_Category;
					procedureConcession = procedure.Concessions?.FirstNonSpecificTariffType(declaration?.Factory ?? new BusinessObjectFactory()) ?? ZString.Empty;
				}
			}
			return CreateMessageKeyFactor(declaration, cpc, ppc, procedureCategory, procedureConcession);
		}

		static MessageDataProviderKeyFactor CreateMessageKeyFactor(JobDeclaration declaration, ZString procedureCode, ZString previousProcedure, ZString procedureCategory, ZString procedureConcession)
		{
			var result = new MessageDataProviderKeyFactor()
			{
				CPC = procedureCode,
				PPC = previousProcedure,
				ProcedureCategory = procedureCategory,
				FirstNonSpecificTariffTypeConcession = procedureConcession
			};
			if (declaration != null)
			{
				result.ShipmentType = declaration.JE_MessageType;
				result.TransportMode = declaration.JE_TransportMode;
				result.RemovalTransportMode = declaration.JE_RemovalTransportCode;
				result.CountryOfDestination = declaration.FinalDestination?.Country;
				result.CountryOfOrigin = declaration.Origin?.Country;
			}
			return result;
		}
		#endregion

		#region ILineLevelInfomation

		public ZDateTime DateOfAssessment => CusEntryInstruction.GetEffectiveAssessmentDate(Header?.EntryInstruction, Factory);

		BusinessObjectFactory ILineLevelInformation.Factory => Factory;

		ZString ILineLevelInformation.LineNumber => Inst.ShouldOutputLineNumber(MessageKeyFactor) ? CL_LineNumber.ToString().PadLeft(4, '0') : string.Empty;

		ZString ILineLevelInformation.TariffCode
		{
			get
			{
				var result = ZString.Empty;

				if (Inst.ShouldOutputTariffCode(MessageKeyFactor))
				{
					var randomLine = RandomLine;
					if (randomLine.IsExcise)
					{
						result = randomLine.Schedule1Part2ATariff?.ZZ1_TariffCode.PadRight(9, '0') ?? ZString.Empty;
					}
					else
					{
						result = randomLine.UniversalTariff?.GetTariffCodeWithCheckDigit() ?? ZString.Empty;
					}
				}

				return result;
			}
		}

		ZString ILineLevelInformation.PreferenceCode => Inst.ShouldOutputPreferenceCode(MessageKeyFactor)
			? PrimaryPreference
			: ZString.Empty;

		ZString ILineLevelInformation.GoodsDescription => Inst.ShouldOutputGoodsDescription(MessageKeyFactor) ? EffectiveDescription : ZString.Empty;

		IEnumerable<IAdditionalInformation> ILineLevelInformation.AdditionalInformations
		{
			get
			{
				foreach (var order in AdditionalInformationCodes.OfType<AdditionalInformation>().GroupBy(x => x.CY_Order))
				{
					foreach (var additionalInformation in order.OrderBy(x => x.CY_Code))
					{
						yield return additionalInformation;
					}
				}
			}
		}

		ZString ILineLevelInformation.CustomsProcedureCode => Inst.ShouldOutputCustomsProcedureCode(MessageKeyFactor) ? CustomsProcedureCode : ZString.Empty;

		ZString ILineLevelInformation.PreviousProcedureCode => PreviousProcedureCode;

		ZString ILineLevelInformation.ProcedureMeasure => ShouldOutputProcedureMeasure ? ProcedureMeasure : ZString.Empty;

		bool ShouldOutputProcedureMeasure
		{
			get
			{
				var result = false;
				var tariff = RandomLine?.ProcedureMeasureTariff;
				if (tariff != null)
				{
					var fee = Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == tariff.ZZ1_ZZI_TariffTypeCode);
					result = fee == null || !fee.CF_IsLandedCostOnly;
				}
				return result;
			}
		}

		ZString ILineLevelInformation.TradeStatisticsIndicator => Inst.ShouldOutputTradeStatisticsIndicator(MessageKeyFactor) ? (RandomLine.JI_TakeUpInTradeStatistics ? MessageBuilders.Constants.TradeStatisticsIndicator.Yes : MessageBuilders.Constants.TradeStatisticsIndicator.No) : string.Empty;

		ZString ILineLevelInformation.CountryOfOrigin => Inst.ShouldOutputCountryOfOrigin(MessageKeyFactor) ? RandomLine.JI_CountryOfOrigin : ZString.Empty;

		ZDecimal ILineLevelInformation.CustomsQuantity => Inst.ShouldOutputCustomsQuantity(MessageKeyFactor) ? SumAndRoundInvoiceLineProperty(line => line.JI_CustomsQuantity, 2) : ZDecimal.Zero;

		ZString ILineLevelInformation.CustomsUnitQty => Inst.ShouldOutputCustomsUnitQty(MessageKeyFactor) ? RandomLine.JI_CustomsUnitQty : ZString.Empty;

		ZDecimal ILineLevelInformation.AdditionalQuantity => SumAndRoundInvoiceLineProperty(line => line.JI_CustomsSecondQuantity, 2);

		ZString ILineLevelInformation.AdditionalUnitQty => RandomLine.JI_CustomsSecondUnitQty;

		ZDecimal ILineLevelInformation.ClassificationQuantity => Inst.ShouldOutputClassificationQuantity(MessageKeyFactor) ? SumAndRoundInvoiceLineProperty(line => line.JI_CustomsThirdQuantity, 2) : ZDecimal.Zero;

		ZString ILineLevelInformation.ClassificationUnitQty => Inst.ShouldOutputClassificationUnitQty(MessageKeyFactor) ? RandomLine.JI_CustomsThirdUnitQty : ZString.Empty;

		ZDecimal ILineLevelInformation.WarehouseCountableQuantity => Inst.ShouldOutputWarehouseCountableQuantity(MessageKeyFactor) ? SumAndRoundInvoiceLineProperty(line => line.JI_BondedWhsQuantity, 0) : ZDecimal.Zero;

		ZString ILineLevelInformation.WarehouseCountableUnitQty => Inst.ShouldOutputWarehouseCountableUnitQty(MessageKeyFactor) ? RandomLine.JI_BondedWhsUnitQty : ZString.Empty;

		ZDecimal SumAndRoundInvoiceLineProperty(Func<JobComInvoiceLine, decimal> propertySelectorFunc, int numberOfDecimalPlaces)
		{
			ZDecimal summedValue = InvoiceLines.Cast<JobComInvoiceLine>().Sum(propertySelectorFunc);
			return summedValue.Round(numberOfDecimalPlaces);
		}

		ZString ILineLevelInformation.RebateUserCode
		{
			get
			{
				var result = ZString.Empty;
				var factor = MessageKeyFactor;
				var sourceInstruction = RandomLine?.EntryInstruction;
				if (sourceInstruction != null && Inst.ShouldOutputRebateUserCode(factor) && !ProcedureMeasure.IsEmpty)
				{
					result = sourceInstruction.RebateUserCode;
					if (result.IsEmpty)
					{
						result = sourceInstruction.ImporterCode;
					}
				}
				return result;
			}
		}

		ZDecimal ILineLevelInformation.ActualPrice
		{
			get
			{
				var result = ZDecimal.Zero;
				if (Inst.ShouldOutputActualPrice(MessageKeyFactor))
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						result += invoiceLine.JI_Calc_ActualPrice;
					}
				}
				result = result.Round(CusEntryLineSchema.CL_CustomsValue.Scale);
				return result.RoundUsingCustomsValueRule();
			}
		}

		ZDecimal ILineLevelInformation.CustomsValue => Inst.ShouldOutputCustomsValue(MessageKeyFactor) ? CL_CustomsValue : ZDecimal.Zero;

		ZString ILineLevelInformation.PreviousProcedureMRN => Inst.ShouldOutputPreviousMRN(MessageKeyFactor) ? RandomLine.JI_PreviousEntryNumber : ZString.Empty;

		ZString ILineLevelInformation.WarehousingMRNLineNumber => Inst.ShouldOutputPreviousMRN(MessageKeyFactor) && RandomLine.JI_PreviousEntryLineNumber != 0 ? RandomLine.JI_PreviousEntryLineNumber.ToString().PadLeft(4, '0') : string.Empty;

		IEnumerable<IDutyFeeInformation> ILineLevelInformation.DutiesAndFees
		{
			get
			{
				if (Inst.ShouldOutputDutiesAndFees(MessageKeyFactor))
				{
					foreach (var dutyFeeInformation in Fees.Cast<CusEntryLineFee>().Where(x => !x.CF_IsLandedCostOnly).OrderBy(x => GetOrderKey(x.CF_ChargeType)))
					{
						if (dutyFeeInformation.IsPayableDuty)
						{
							yield return dutyFeeInformation;
						}
						else if (dutyFeeInformation.CF_ChargeType == UniversalReferenceConstants.TaxOrFeeTypeCode.VAT)
						{
							yield return dutyFeeInformation;
						}
					}
				}
			}
		}

		ZString GetOrderKey(ZString code)
		{
			var prefix = code.Left(1) == "V" ? "0" : (code == UniversalReferenceConstants.CusTariffCode.Schedule1Part1 ? "1" : "2");
			return prefix + code;
		}

		IEnumerable<IDutyFeeInformation> ILineLevelInformation.ProvisionalPayments
		{
			get
			{
				if (Inst.ShouldOutputProvisionalPayments(MessageKeyFactor))
				{
					var instruction = EntryInstruction;
					if (IsLine1 && instruction != null)
					{
						var headerPPtype = instruction.CEI_ProvisionalPaymentType;
						var headerPPAmount = instruction.CEI_ProvisionalPaymentAmount;
						if (!headerPPtype.IsEmpty && !headerPPAmount.IsEmpty && (!Header?.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(headerPPtype, 1) ?? true))
						{
							yield return new DocumentWrappers.DutyFeeInformationDocWrapper(headerPPtype, headerPPAmount);
						}
					}
					var lineNumber = this.CL_LineNumber;
					foreach (var provisionalPayment in ProvisionalPayments.OfType<ProvisionalPaymentAmountCodeData>().OrderBy(x => x.CY_Code))
					{
						var ppType = provisionalPayment.CY_Code;
						if (!ppType.IsEmpty && !provisionalPayment.CY_Value.IsEmpty && (!Header?.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(ppType, lineNumber) ?? true))
						{
							yield return provisionalPayment;
						}
					}
				}
				else if (Inst.ShouldOutputProvisionalPaymentsForDiamondLevy(MessageKeyFactor))
				{
					foreach (var provisionalPayment in ProvisionalPayments.OfType<ProvisionalPaymentAmountCodeData>().OrderBy(x => x.CY_Code))
					{
						var ppType = provisionalPayment.CY_Code;
						if (!ppType.IsEmpty && !provisionalPayment.CY_Value.IsEmpty)
						{
							yield return provisionalPayment;
						}
					}
				}
			}
		}

		#endregion
	}
}
