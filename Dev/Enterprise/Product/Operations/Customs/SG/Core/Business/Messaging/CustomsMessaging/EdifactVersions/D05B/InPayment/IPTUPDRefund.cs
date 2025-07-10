
using CargoWise.Types;
using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class IPTUPDRefund : IPTUPD
	{
		public IPTUPDRefund(IIPTUPD customsDec)
			: base(customsDec)
		{
		}

		protected override DocumentNameCodeList DocumentNameCode
		{
			get { return DocumentNameCodeList.RelatedDocument; }
		}

		public override string MessageSubType
		{
			get { return CUSDECEDIMessage.Refund; }
		}

		#region Header Section

		protected override ZString UpdateIndicator
		{
			get { return CustomsDec.AdditionalMessageInformation != null ? CustomsDec.AdditionalMessageInformation.UpdateIndicator : ZString.Empty; }
		}

		protected override void GenerateLOCSegments(LOCSegmentMessageSection locSection)
		{
		}

		protected override void GenerateDTMSegments(DTMSegmentMessageSection dtmSection)
		{
		}

		protected override void GenerateMEASegments(MEASegmentMessageSection meaSection)
		{
		}

		protected override void GenerateEQDAndSELSegments(EQDSegmentMessageSection eqdSection, SELSegmentMessageSection selSection)
		{
		}

		protected override void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			GenerateMessageSenderMailboxSegment(sg1Section);
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT  *
			PopulateReplacementPermitNoSegment(sg1Section); // AAE
			PopulateAdditionalRecipientsSegments(sg1Section); // MR  *
		}

		protected override void GenerateFTX(FTXSegmentMessageSection ftxSegment)
		{
			if (CustomsDec.AdditionalMessageInformation != null)
			{
				//note 3a
				if (!CustomsDec.AdditionalMessageInformation.RefundCode.IsEmpty)
				{
					var fTX = ftxSegment.InstantiateAChildAndAddItToChildrenCollection();
					fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.Reason;
					fTX.TextLiteral.FreeText1 = CustomsDec.AdditionalMessageInformation.RefundCode;

					//note 3b
					if (!CustomsDec.AdditionalMessageInformation.ReasonForRefund.IsEmpty)
					{
						var refundReasonFTX = ftxSegment.InstantiateAChildAndAddItToChildrenCollection();
						refundReasonFTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.DiscrepancyInformation;
						TextSplitElegantly refundremark = new TextSplitElegantly(70, 4);
						refundremark.Text = CustomsDec.AdditionalMessageInformation.ReasonForRefund;
						refundReasonFTX.TextLiteral.FreeText1 = refundremark[0];
						refundReasonFTX.TextLiteral.FreeText2 = refundremark[1];
						refundReasonFTX.TextLiteral.FreeText3 = refundremark[2];
						refundReasonFTX.TextLiteral.FreeText4 = refundremark[3];
					}
				}
			}
		}

		protected override void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
		}

		protected override void GenerateSegmentGroup5(SegmentGroup5MessageSection sg5Section)
		{
			PopulateSupportingDocs(sg5Section);
		}

		protected override void GenerateSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			PopulateDeclarantSegments(sg6Section);
		}

		#endregion

		#region Detail Section

		public override void GenerateSegmentGroup10(SegmentGroup10MessageSection sg10Section)
		{
		}

		public override void GenerateSegmentGroup30(SegmentGroup30MessageSection sg30Section)
		{
			if (CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.FRF || CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.PRG)
			{
				// no group 30 required
			}
			else
			{
				base.GenerateSegmentGroup30(sg30Section);
			}
		}

		protected override void GenerateGroup30_FTX(SegmentGroup30 sG30, ICusItem item)
		{
		}

		protected override void GenerateGroup30_LOC(SegmentGroup30 sG30, ICusItem item)
		{
		}

		protected override void GenerateGroup30_MEA(SegmentGroup30 sG30, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroup31(SegmentGroup30 sG30, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroup33(SegmentGroup30 sG30, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroup35(SegmentGroup30 sG30, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroup37(SegmentGroup30 sG30, ICusItem item)
		{
		}

		#endregion

		#region Summary Section

		protected override void GenerateCNT(CUSDECMessage cusdecEdifactMsg)
		{
			var cNT = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cNT.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}

		protected override void GenerateSegmentGroup49(SegmentGroup49MessageSection sg49Section)
		{
			GenerateSummaryRefundSegments(sg49Section);
		}

		void GenerateSummaryRefundSegments(SegmentGroup49MessageSection sg49Section)
		{
			if (CustomsDec.AdditionalMessageInformation != null)
			{
				if (CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.FRF || CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.PRG)
				{
					if (CustomsDec.AdditionalMessageInformation.DutyRefundAmount > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.CustomsDuty, CustomsDec.AdditionalMessageInformation.DutyRefundAmount);
					}

					if (CustomsDec.AdditionalMessageInformation.ExciseRefundAmount > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.ExciseDuty, CustomsDec.AdditionalMessageInformation.ExciseRefundAmount);
					}

					if (CustomsDec.AdditionalMessageInformation.GSTRefundAmount > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, DutyOrTaxOrFeeTypeNameCodeList.GoodsAndServicesTax, CustomsDec.AdditionalMessageInformation.GSTRefundAmount);
					}
				}

				if (CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.PRS)
				{
					ZDecimal gST = 0;
					ZDecimal duty = 0;
					ZDecimal excise = 0;

					foreach (ICusItem item in CustomsDec.Items)
					{
						gST += item.ItemGSTRefund;
						duty += item.ItemDutyRefund;
						excise += item.ItemExciseRefund;
					}

					if (duty > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.CustomsDuty, duty);
					}

					if (excise > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.ExciseDuty, excise);
					}

					if (gST > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, DutyOrTaxOrFeeTypeNameCodeList.GoodsAndServicesTax, gST);
					}
				}
			}
		}

		void GenerateUPDSummaryTAXandMOASegments(SegmentGroup49MessageSection sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList taxQualifier, DutyOrTaxOrFeeTypeNameCodeList refundTypeQualifier, decimal monetaryValue)
		{
			var sG49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateUPDSummaryTAXSegment(sG49, taxQualifier, refundTypeQualifier);
			GenerateUPDSummaryMOASegment(sG49, monetaryValue);
		}

		void GenerateUPDSummaryTAXSegment(SegmentGroup49 sG49, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier, DutyOrTaxOrFeeTypeNameCodeList refundType)
		{
			TAXSegment tAX = sG49.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
			tAX.DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode = refundType;
		}

		void GenerateUPDSummaryMOASegment(SegmentGroup49 sG49, decimal monetaryValue)
		{
			MOASegment mOA = sG49.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.Refund;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		#endregion
	}
}
