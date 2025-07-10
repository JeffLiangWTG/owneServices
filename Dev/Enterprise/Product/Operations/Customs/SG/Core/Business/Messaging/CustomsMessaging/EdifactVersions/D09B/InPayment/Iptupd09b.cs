using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public class Iptupd09b : Iptdec09b
	{
		public Iptupd09b(IIPTUPD customsDec)
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

		protected override CSTSegment GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cst = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			if (DocumentNameCode == DocumentNameCodeList.CustomsDeclarationWithCommercialAndItemDetail)
			{
				cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = CustomsDec.CargoPackingType;
			}

			cst.CustomsIdentityCodes2.CustomsGoodsIdentifier = UpdateIndicator;

			return cst;
		}

		protected virtual ZString UpdateIndicator
		{
			get { return Enterprise.Customs.SG.V4.Business.SGConstants.UpdateIndicators.AME; }
		}

		protected override void GenerateHeaderFtxSegment(FTXSegmentMessageSection ftxSection)
		{
			//note 1
			base.GenerateHeaderFtxSegment(ftxSection);
			if (CustomsDec.AdditionalMessageInformation != null)
			{
				//note 3c
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
					var remark = new TextSplitElegantly(70, 4);
					remark.Text = CustomsDec.AdditionalMessageInformation.ReasonForAmending;
					reasonFTX.TextLiteral.FreeText1 = remark[0];
					reasonFTX.TextLiteral.FreeText2 = remark[1];
					reasonFTX.TextLiteral.FreeText3 = remark[2];
					reasonFTX.TextLiteral.FreeText4 = remark[3];
				}
			}
		}

		#region SegmentGroup1

		protected override void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateHeaderSegmentGroup1(sg1Section);
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT
		}

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateSummaryCnt(CUSDECMessage cusdecEdifactMsg)
		{
			base.GenerateSummaryCnt(cusdecEdifactMsg);

			var cNT = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cNT.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}

		protected override void GenerateSegmentGroup51(SegmentGroup51MessageSection sg51Section)
		{
			base.GenerateSegmentGroup51(sg51Section);

			if (!CustomsDec.AdditionalMessageInformation.RefundCode.IsEmpty)
			{
				GenerateSummaryRefundSegments(sg51Section);
			}
		}

		void GenerateSummaryRefundSegments(SegmentGroup51MessageSection sg51Section)
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

		void GenerateUPDSummaryTAXandMOASegments(SegmentGroup51MessageSection sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList taxQualifier, DutyOrTaxOrFeeTypeNameCodeList refundTypeQualifier, decimal monetaryValue)
		{
			var sG51 = sg51Section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateUPDSummaryTAXSegment(sG51, taxQualifier, refundTypeQualifier);
			GenerateUPDSummaryMOASegment(sG51, monetaryValue);
		}

		void GenerateUPDSummaryTAXSegment(SegmentGroup51 sG51, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier, DutyOrTaxOrFeeTypeNameCodeList refundType)
		{
			TAXSegment tAX = sG51.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
			tAX.DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode = refundType;
		}

		void GenerateUPDSummaryMOASegment(SegmentGroup51 sG51, decimal monetaryValue)
		{
			MOASegment mOA = sG51.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.Refund;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, Enterprise.Customs.SG.V4.Business.SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		#endregion
	}
}
