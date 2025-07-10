using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public class Iptupd09bRefund : Iptupd09b
	{
		public Iptupd09bRefund(IIPTUPD customsDec)
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

		protected override void GenerateHeaderLocSegments(LOCSegmentMessageSection locSection)
		{
		}

		protected override void GenerateHeaderDtmSegments(DTMSegmentMessageSection dtmSection)
		{
		}

		protected override void GenerateHeaderMeaSegments(MEASegmentMessageSection meaSection)
		{
		}

		protected override void GenerateEQDAndSELSegments(EQDSegmentMessageSection eqdSection, SELSegmentMessageSection selSection)
		{
		}

		protected override void GenerateHeaderFtxSegment(FTXSegmentMessageSection ftxSegment)
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

		protected override void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			GenerateMessageSenderMailboxSegment(sg1Section);
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT  *
			PopulateReplacementPermitNoSegment(sg1Section); // AAE
			PopulateAdditionalRecipientsSegments(sg1Section); // MR  *
		}

		protected override void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
		}

		protected override void GenerateSegmentGroup5(SegmentGroup5MessageSection sg5Section)
		{
			PopulateSupportingDocs(sg5Section);
		}

		protected override void GenerateHeaderSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			PopulateDeclarantSegments(sg6Section);
		}

		#endregion

		#region Detail Section

		protected override void GenerateInvoiceOrCertificateOfOriginGroups(SegmentGroup11MessageSection sg11Section)
		{
		}

		protected override void GenerateInvoiceLineDetailSegments(SegmentGroup32MessageSection sg32Section)
		{
			if (CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.FRF || CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.PRG)
			{
				// no line detail (group 32) required
			}
			else
			{
				base.GenerateInvoiceLineDetailSegments(sg32Section);
			}
		}

		protected override void GenerateGroup32Ftx(SegmentGroup32 sG32, ICusItem item)
		{
		}

		protected override void GenerateGroup32Loc(SegmentGroup32 sG32, ICusItem item)
		{
		}

		protected override void GenerateGroup32Mea(SegmentGroup32 sG32, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroups33And34(SegmentGroup33MessageSection sg33Section, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroups35And36(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroups37And38(SegmentGroup37MessageSection sg37Section, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroup39(SegmentGroup39MessageSection sg39Section, ICusItem item)
		{
		}

		#endregion

		#region Summary Section

		protected override void GenerateSummaryCnt(CUSDECMessage cusdecEdifactMsg)
		{
			var cnt = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cnt.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}

		protected override void GenerateSegmentGroup51(SegmentGroup51MessageSection sg51Section)
		{
			GenerateSummaryRefundSegments(sg51Section);
		}

		void GenerateSummaryRefundSegments(SegmentGroup51MessageSection sg51Section)
		{
			if (CustomsDec.AdditionalMessageInformation != null)
			{
				if (CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.FRF || CustomsDec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.PRG)
				{
					if (CustomsDec.AdditionalMessageInformation.DutyRefundAmount > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.CustomsDuty, CustomsDec.AdditionalMessageInformation.DutyRefundAmount);
					}

					if (CustomsDec.AdditionalMessageInformation.ExciseRefundAmount > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.ExciseDuty, CustomsDec.AdditionalMessageInformation.ExciseRefundAmount);
					}

					if (CustomsDec.AdditionalMessageInformation.GSTRefundAmount > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, DutyOrTaxOrFeeTypeNameCodeList.GoodsAndServicesTax, CustomsDec.AdditionalMessageInformation.GSTRefundAmount);
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
						GenerateUPDSummaryTAXandMOASegments(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.CustomsDuty, duty);
					}

					if (excise > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.ExciseDuty, excise);
					}

					if (gST > 0)
					{
						GenerateUPDSummaryTAXandMOASegments(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, DutyOrTaxOrFeeTypeNameCodeList.GoodsAndServicesTax, gST);
					}
				}
			}
		}

		void GenerateUPDSummaryTAXandMOASegments(SegmentGroup51MessageSection sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList taxQualifier, DutyOrTaxOrFeeTypeNameCodeList refundTypeQualifier, decimal monetaryValue)
		{
			var sg51 = sg51Section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateUPDSummaryTAXSegment(sg51, taxQualifier, refundTypeQualifier);
			GenerateUPDSummaryMOASegment(sg51, monetaryValue);
		}

		void GenerateUPDSummaryTAXSegment(SegmentGroup51 sg51, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier, DutyOrTaxOrFeeTypeNameCodeList refundType)
		{
			TAXSegment tAX = sg51.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
			tAX.DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode = refundType;
		}

		void GenerateUPDSummaryMOASegment(SegmentGroup51 sg51, decimal monetaryValue)
		{
			MOASegment mOA = sg51.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.Refund;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		#endregion
	}
}
