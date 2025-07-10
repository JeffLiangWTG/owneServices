using CargoWise.Types;
using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class IPTUPD : IPTDEC
	{
		public IPTUPD(IIPTUPD customsDec)
			: base(customsDec)
		{
		}

		protected new IIPTUPD CustomsDec
		{
			get { return (IIPTUPD)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.IPTUPD; }
		}

		public override string MessageSubType
		{
			get { return CUSDECEDIMessage.Amendment; }
		}

		#region Header Section

		protected override MessageFunctionCodeList MessageFunctionCode
		{
			get { return MessageFunctionCodeList.Request; }
		}

		protected override void GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cST = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			if (DocumentNameCode == DocumentNameCodeList.CustomsDeclarationWithCommercialAndItemDetail)
			{
				cST.CustomsIdentityCodes1.CustomsGoodsIdentifier = CustomsDec.CargoPackingType;
			}

			cST.CustomsIdentityCodes2.CustomsGoodsIdentifier = UpdateIndicator;
		}

		protected virtual ZString UpdateIndicator
		{
			get { return SGConstants.UpdateIndicators.AME; }
		}

		protected override void GenerateFTX(FTXSegmentMessageSection ftxSection)
		{
			//note 1
			base.GenerateFTX(ftxSection);
			if (CustomsDec.AdditionalMessageInformation != null)
			{
				//note 4c
				if (DocumentNameCode == DocumentNameCodeList.CustomsDeclarationWithCommercialAndItemDetail)
				{
					//note 4a
					if (CustomsDec.AdditionalMessageInformation != null && CustomsDec.AdditionalMessageInformation.ExtendingTemporaryImportPeriod)
					{
						var codeFTX = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
						codeFTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.PeriodOfTime;
						codeFTX.TextLiteral.FreeText1 = CodeForExtensionOfPermitValidityCodeList.Codes.Y;
					}

					//note 4b
					var reasonFTX = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					reasonFTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalAttributeInformation;
					TextSplitElegantly remark = new TextSplitElegantly(70, 4);
					remark.Text = CustomsDec.AdditionalMessageInformation.ReasonForAmending;
					reasonFTX.TextLiteral.FreeText1 = remark[0];
					reasonFTX.TextLiteral.FreeText2 = remark[1];
					reasonFTX.TextLiteral.FreeText3 = remark[2];
					reasonFTX.TextLiteral.FreeText4 = remark[3];
				}
			}
		}

		#region SegmentGroup1

		protected override void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			GenerateMessageSenderMailboxSegment(sg1Section); //MS
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT  *
			PopulateLicensesAndDocumentsSegments(sg1Section); // DM
			PopulateSupplyIndicatorSegment(sg1Section); // TN
			PopulatePreviousPermitNumberSegment(sg1Section); // ACE
			PopulateAdditionalRecipientsSegments(sg1Section); // MR  *
		}

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateCNT(CUSDECMessage cusdecEdifactMsg)
		{
			base.GenerateCNT(cusdecEdifactMsg);

			var cNT = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cNT.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}

		protected override void GenerateSegmentGroup49(SegmentGroup49MessageSection sg49Section)
		{
			base.GenerateSegmentGroup49(sg49Section);

			if (!CustomsDec.AdditionalMessageInformation.RefundCode.IsEmpty)
			{
				GenerateSummaryRefundSegments(sg49Section);
			}
		}

		void GenerateSummaryRefundSegments(SegmentGroup49MessageSection sg49Section)
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
