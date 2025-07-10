using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Edifact.Utilities;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	class Inpupd09b : Inpdec09b
	{
		public Inpupd09b(IINPDEC customsDec)
			: base(customsDec)
		{
		}

		protected new IINPUPD CustomsDec
		{
			get { return (IINPUPD)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.INPUPD; }
		}

		public override string MessageSubType
		{
			get { return CUSDECEDIMessage.Amendment; }
		}

		protected override MessageFunctionCodeList MessageFunctionCode
		{
			get { return MessageFunctionCodeList.Request; }
		}

		protected override CSTSegment GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cst = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = CustomsDec.CargoPackingType;
			cst.CustomsIdentityCodes2.CustomsGoodsIdentifier = SGConstants.UpdateIndicators.AME;
			return cst;
		}

		protected override void GenerateHeaderFtxSegment(FTXSegmentMessageSection ftxSection)
		{
			base.GenerateHeaderFtxSegment(ftxSection);

			if (CustomsDec.AdditionalMessageInformation != null)
			{
				if (CustomsDec.AdditionalMessageInformation.ExtendingTemporaryImportPeriod)
				{
					var ftx2 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					ftx2.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.PeriodOfTime;
					ftx2.TextLiteral.FreeText1 = CodeForExtensionOfPermitValidityCodeList.Codes.Y;

					if (!CustomsDec.AdditionalMessageInformation.ReasonForExtendingTemporaryImportPeriod.IsEmpty)
					{
						var ftx3 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
						ftx3.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.TypeOfTransactionReason;
						var remark2 = new TextSplitElegantly(70, 4);
						remark2.Text = CustomsDec.AdditionalMessageInformation.ReasonForExtendingTemporaryImportPeriod;
						ftx3.TextLiteral.FreeText1 = remark2[0];
						ftx3.TextLiteral.FreeText2 = remark2[1];
						ftx3.TextLiteral.FreeText3 = remark2[2];
						ftx3.TextLiteral.FreeText4 = remark2[3];
					}
				}

				if (!CustomsDec.AdditionalMessageInformation.ReasonForAmending.IsEmpty)
				{
					var ftx2 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					ftx2.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalAttributeInformation;
					var remark1 = new TextSplitElegantly(70, 4);
					remark1.Text = CustomsDec.AdditionalMessageInformation.ReasonForAmending;
					ftx2.TextLiteral.FreeText1 = remark1[0];
					ftx2.TextLiteral.FreeText2 = remark1[1];
					ftx2.TextLiteral.FreeText3 = remark1[2];
					ftx2.TextLiteral.FreeText4 = remark1[3];
				}
			}
		}

		protected override void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateHeaderSegmentGroup1(sg1Section);
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT  *
			PopulateReplacementPermitNoSegment(sg1Section); // AAE
		}

		protected override void GenerateSummaryCnt(CUSDECMessage cusdecEdifactMsg)
		{
			base.GenerateSummaryCnt(cusdecEdifactMsg);
			var cnt = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cnt.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}
	}
}
