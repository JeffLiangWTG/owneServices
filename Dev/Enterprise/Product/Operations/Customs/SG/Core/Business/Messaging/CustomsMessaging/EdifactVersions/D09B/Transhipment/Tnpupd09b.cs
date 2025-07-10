using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	class Tnpupd09b : Tnpdec09b
	{
		public Tnpupd09b(ITNPUPD customsDec)
			: base(customsDec)
		{
		}

		protected new ITNPUPD CustomsDec
		{
			get { return (ITNPUPD)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.TNPUPD; }
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
				if (!CustomsDec.AdditionalMessageInformation.ReasonForAmending.IsEmpty)
				{
					var ftx1 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					ftx1.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalAttributeInformation;
					ftx1.TextLiteral.FreeText1 = CustomsDec.AdditionalMessageInformation.ReasonForAmending;
				}

				if (CustomsDec.AdditionalMessageInformation != null && CustomsDec.AdditionalMessageInformation.ExtendingTemporaryImportPeriod)
				{
					var ftx2 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					ftx2.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.PeriodOfTime;
					ftx2.TextLiteral.FreeText1 = CodeForExtensionOfPermitValidityCodeList.Codes.Y;
				}
			}
		}

		protected override void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateHeaderSegmentGroup1(sg1Section);  // MS, DM, ACE, MR (Note 1, 2, 4 & 7)
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT (Note 5)
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.GTR)
			{
				PopulateSupplyIndicatorSegment(sg1Section); // TN (Note 3)
			}
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
