using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Edifact.D96B.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class CUSDECMessageSG30Helper : NonPersistentBusinessObject, ILineLevelInformation
	{
		public CUSDECMessageSG30Helper(SegmentGroup30 sg30, ZDateTime dateOfAssessment, BusinessObjectFactory factory) : base(factory)
		{
			this.group30 = sg30;
			this.dateOfAssessment = dateOfAssessment;
		}

		public SegmentGroup30 Group30 => group30;

		readonly SegmentGroup30 group30;
		readonly ZDateTime dateOfAssessment;

		public ZDateTime DateOfAssessment => dateOfAssessment;

		#region caching

		CSTSegment CST => cstSegment ?? (cstSegment = Group30.CST[0]);
		CSTSegment cstSegment;

		IEnumerable<FTXSegment> FTXSegments => ftxSegments ?? (ftxSegments = Group30.FTX.Cast<FTXSegment>());
		IEnumerable<FTXSegment> ftxSegments;

		FTXSegment FTXForProcedureCode => ftxForProcedureCode ?? (ftxForProcedureCode = FTXSegments.FirstOrDefault(ftx => ftx.TextSubjectQualifier == TextSubjectQualifierList.CustomsClearanceInstructions));
		FTXSegment ftxForProcedureCode;

		FTXSegment FTXForVDN => ftxForVDN ?? (ftxForVDN = FTXSegments.FirstOrDefault(ftx => ftx.TextSubjectQualifier == TextSubjectQualifierList.AdditionalInformation));
		FTXSegment ftxForVDN;

		RFFSegment RFFForPreviousProcedure => rffForPreviousProcedure ?? (rffForPreviousProcedure = Group30.Group35.Cast<SegmentGroup35>().FirstOrDefault(sg35 => sg35.RFF[0].Reference.ReferenceQualifier == ReferenceQualifierList.WarehouseEntryNumber)?.RFF[0]);
		RFFSegment rffForPreviousProcedure;

		IEnumerable<MEASegment> MEASegments => meaSegments ?? (meaSegments = Group30.MEA.Cast<MEASegment>());
		IEnumerable<MEASegment> meaSegments;

		#endregion

		#region CST

		ZString ILineLevelInformation.LineNumber => CST.GoodsItemNumber;

		ZString ILineLevelInformation.TariffCode => CST.CustomsIdentityCodes1.CustomsCodeIdentification;

		ZString ILineLevelInformation.PreferenceCode => CST.CustomsIdentityCodes2.CustomsCodeIdentification;

		#endregion

		#region FTX

		public ZString ValueDeterminationNumber
		{
			get
			{
				if (FTXForVDN != null)
				{
					var elements = new List<string>() {
						FTXForVDN.TextLiteral.FreeText1,
						FTXForVDN.TextLiteral.FreeText2,
						FTXForVDN.TextLiteral.FreeText3,
						FTXForVDN.TextLiteral.FreeText4,
						FTXForVDN.TextLiteral.FreeText5
					};
					var vdnSnippet = elements.FirstOrDefault(e => e.StartsWith(UniversalReferenceConstants.AdditionalInformation.ValueDeterminationNumber, StringComparison.Ordinal));
					if (!string.IsNullOrEmpty(vdnSnippet) && (vdnSnippet.Length > 3))
					{
						return vdnSnippet.Substring(3);
					}
				}
				return ZString.Empty;
			}
		}

		ZString ILineLevelInformation.CustomsProcedureCode => FTXForProcedureCode?.TextLiteral.FreeText1 ?? ZString.Empty;

		ZString ILineLevelInformation.PreviousProcedureCode => FTXForProcedureCode?.TextLiteral.FreeText2 ?? ZString.Empty;

		ZString ILineLevelInformation.ProcedureMeasure => FTXForProcedureCode?.TextLiteral.FreeText3 ?? ZString.Empty;

		ZString ILineLevelInformation.TradeStatisticsIndicator => FTXForProcedureCode?.TextLiteral.FreeText5 ?? ZString.Empty;

		ZString ILineLevelInformation.GoodsDescription
		{
			get
			{
				var result = ZString.Empty;
				var textLiteral = FTXSegments.FirstOrDefault(ftx => ftx.TextSubjectQualifier == TextSubjectQualifierList.GoodsDescription)?.TextLiteral;
				if (textLiteral != null)
				{
					result = ZString.Format("{0}{1}{2}{3}{4}", textLiteral.FreeText1, textLiteral.FreeText2, textLiteral.FreeText3, textLiteral.FreeText4, textLiteral.FreeText5);
				}
				return result;
			}
		}

		IEnumerable<IAdditionalInformation> ILineLevelInformation.AdditionalInformations
		{
			get
			{
				if (additionalInformations == null)
				{
					additionalInformations = new List<IAdditionalInformation>();
					foreach (var addinfoFTX in FTXSegments.Where(ftx => ftx.TextSubjectQualifier == TextSubjectQualifierList.AdditionalInformation || ftx.TextSubjectQualifier == TextSubjectQualifierList.GeneralInformation))
					{
						var textLiteral = addinfoFTX.TextLiteral;
						if (!string.IsNullOrEmpty(textLiteral.FreeText1))
						{
							additionalInformations.Add(new AdditionalInformationDocWrapper(textLiteral.FreeText1));
						}

						if (!string.IsNullOrEmpty(textLiteral.FreeText2))
						{
							additionalInformations.Add(new AdditionalInformationDocWrapper(textLiteral.FreeText2));
						}

						if (!string.IsNullOrEmpty(textLiteral.FreeText3))
						{
							additionalInformations.Add(new AdditionalInformationDocWrapper(textLiteral.FreeText3));
						}

						if (!string.IsNullOrEmpty(textLiteral.FreeText4))
						{
							additionalInformations.Add(new AdditionalInformationDocWrapper(textLiteral.FreeText4));
						}

						if (!string.IsNullOrEmpty(textLiteral.FreeText5))
						{
							additionalInformations.Add(new AdditionalInformationDocWrapper(textLiteral.FreeText5));
						}
					}
				}
				return additionalInformations;
			}
		}
		List<IAdditionalInformation> additionalInformations;

		#endregion

		#region LOC

		ZString ILineLevelInformation.CountryOfOrigin => Group30.LOC.Cast<LOCSegment>()?.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfOrigin)?.LocationIdentification.PlaceLocationIdentification ?? ZString.Empty;

		#endregion

		#region NAD

		ZString ILineLevelInformation.RebateUserCode => Group30.NAD.Cast<NADSegment>().FirstOrDefault(nad => nad.PartyQualifier == PartyQualifierList.WarehouseKeeper)?.PartyIdentificationDetails?.PartyIdIdentification ?? ZString.Empty;

		#endregion

		#region RFF

		ZString ILineLevelInformation.PreviousProcedureMRN => RFFForPreviousProcedure?.Reference.ReferenceNumber ?? ZString.Empty;

		ZString ILineLevelInformation.WarehousingMRNLineNumber => RFFForPreviousProcedure?.Reference.LineNumber ?? ZString.Empty;

		#endregion

		#region MEA

		ZDecimal ILineLevelInformation.CustomsQuantity => ZDecimal.ParseSafe(MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._1stSpecifiedTariffQuantity)?.ValueRange.MeasurementValue, ZDecimal.Zero);

		ZString ILineLevelInformation.CustomsUnitQty => MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._1stSpecifiedTariffQuantity)?.ValueRange.MeasureUnitQualifier ?? ZString.Empty;

		ZDecimal ILineLevelInformation.AdditionalQuantity => ZDecimal.ParseSafe(MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._2ndSpecifiedTariffQuantity)?.ValueRange.MeasurementValue, ZDecimal.Zero);

		ZString ILineLevelInformation.AdditionalUnitQty => MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._2ndSpecifiedTariffQuantity)?.ValueRange.MeasureUnitQualifier ?? ZString.Empty;

		ZDecimal ILineLevelInformation.ClassificationQuantity => ZDecimal.ParseSafe(MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._3rdSpecifiedTariffQuantity)?.ValueRange.MeasurementValue, ZDecimal.Zero);

		ZString ILineLevelInformation.ClassificationUnitQty => MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._3rdSpecifiedTariffQuantity)?.ValueRange.MeasureUnitQualifier ?? ZString.Empty;

		ZDecimal ILineLevelInformation.WarehouseCountableQuantity => ZDecimal.ParseSafe(MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList.CustomsLineItemMeasurement)?.ValueRange.MeasurementValue, ZDecimal.Zero);

		ZString ILineLevelInformation.WarehouseCountableUnitQty => MEASegments.FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList.CustomsLineItemMeasurement)?.ValueRange.MeasureUnitQualifier ?? ZString.Empty;

		#endregion

		#region MOA

		ZDecimal ILineLevelInformation.ActualPrice => ZDecimal.ParseSafe(Group30.Group33.Cast<SegmentGroup33>()?.FirstOrDefault(sg33 => sg33.MOA[0].MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.InvoiceItemAmount)?.MOA[0].MonetaryAmount.MonetaryAmount ?? ZString.Empty, ZDecimal.Zero);
		ZDecimal ILineLevelInformation.CustomsValue => ZDecimal.ParseSafe(Group30.Group33.Cast<SegmentGroup33>()?.FirstOrDefault(sg33 => sg33.MOA[0].MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.CustomsValue)?.MOA[0].MonetaryAmount.MonetaryAmount ?? ZString.Empty, ZDecimal.Zero);

		#endregion

		#region SG41

		internal CalcFeeValues FeeValues
		{
			get
			{
				if (!cachedFeeValues.HasValue)
				{
					var result = new CalcFeeValues();
					var rateCodes = Factory.GetRateCodesWithEX1();
					var sched1and2Codes = ChargeTypeHelper.GetCustomsDutiesSchedule1P1And2Codes();

					var customsDutiesExcluding12B = new List<IDutyFeeInformation>();
					foreach (var item in DutiesAndFees)
					{
						var feeCode = item.Code;
						if (feeCode == UniversalReferenceConstants.TaxOrFeeTypeCode.VAT)
						{
							result.ValueAddedTax += item.Value;
						}
						else if (rateCodes.Any(x => x.ZY1_RateCode == feeCode))
						{
							result.S1P2BDuty += item.Value;
						}
						else
						{
							result.CustomsDutyExcluding12B += item.Value;
							customsDutiesExcluding12B.Add(new DutyFeeInformationDocWrapper(item.Code, item.Value));
						}

						if (sched1and2Codes.Contains(feeCode))
						{
							result.CustomsDutiesSchedule1P1andSchedule2 += item.Value;
						}
					}

					result.CustomsDutiesExcluding12B = customsDutiesExcluding12B;

					var penaltyList = new List<IDutyFeeInformation>();
					var provisionalPaymentList = new List<IDutyFeeInformation>();
					var penaltyCodes = ProvisionalPaymentTypesHelper.GetTypesForRateType(Universal.Constants.RateTypes.Penalty);
					var ppCodes = ProvisionalPaymentTypesHelper.GetTypesForRateType(Universal.Constants.RateTypes.ProvisionalPayment);
					foreach (var item in ProvisionalPayments)
					{
						var feeCode = item.Code;
						if (ppCodes.Contains(feeCode))
						{
							result.ProvisionalPayment += item.Value;
							provisionalPaymentList.Add(new DutyFeeInformationDocWrapper(item.Code, item.Value));
						}
						else if (penaltyCodes.Contains(feeCode))
						{
							result.Penalty += item.Value;
							penaltyList.Add(new DutyFeeInformationDocWrapper(item.Code, item.Value));
						}
					}

					result.Penalties = penaltyList;
					result.ProvisionalPayments = provisionalPaymentList;
					cachedFeeValues = result;
				}
				return cachedFeeValues.Value;
			}
		}
		CalcFeeValues? cachedFeeValues;

		public IEnumerable<IDutyFeeInformation> DutiesAndFees
		{
			get
			{
				if (dutiesAndFees == null)
				{
					dutiesAndFees = new List<IDutyFeeInformation>();
					var ppList = Factory.GetAllProvisionalPaymentTypes();
					foreach (var sg41 in Group30?.Group41?.Cast<SegmentGroup41>() ?? Array.Empty<SegmentGroup41>())
					{
						var code = sg41.TAX[0].DutyTaxFeeType.DutyTaxFeeTypeCoded.ToString();
						if (!ppList.Contains(code))
						{
							dutiesAndFees.Add(new DutyFeeInformationDocWrapper(code, sg41.MOA[0].MonetaryAmount.MonetaryAmount));
						}
					}
				}
				return dutiesAndFees;
			}
		}
		List<IDutyFeeInformation> dutiesAndFees;

		public IEnumerable<IDutyFeeInformation> ProvisionalPayments
		{
			get
			{
				if (provisionalPayments == null)
				{
					provisionalPayments = new List<IDutyFeeInformation>();
					var ppList = Factory.GetAllProvisionalPaymentTypes();
					foreach (var sg41 in Group30?.Group41?.Cast<SegmentGroup41>() ?? Array.Empty<SegmentGroup41>())
					{
						var code = sg41.TAX[0].DutyTaxFeeType.DutyTaxFeeTypeCoded.ToString();
						if (ppList.Contains(code))
						{
							provisionalPayments.Add(new DutyFeeInformationDocWrapper(code, sg41.MOA[0].MonetaryAmount.MonetaryAmount));
						}
					}
				}
				return provisionalPayments;
			}
		}
		List<IDutyFeeInformation> provisionalPayments;

		#endregion
	}
}
