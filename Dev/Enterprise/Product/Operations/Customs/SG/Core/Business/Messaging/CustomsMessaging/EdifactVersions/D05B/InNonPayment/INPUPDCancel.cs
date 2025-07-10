using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class INPUPDCancel : INPDEC
	{
		public INPUPDCancel(IINPUPD customsDec)
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
			get { return CUSDECEDIMessage.Cancellation; }
		}

		protected override DocumentNameCodeList DocumentNameCode
		{
			get { return DocumentNameCodeList.CustomsDeclarationWithoutItemDetail; }
		}

		protected override MessageFunctionCodeList MessageFunctionCode
		{
			get { return MessageFunctionCodeList.Cancellation; }
		}

		protected override void GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cST = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			cST.CustomsIdentityCodes2.CustomsGoodsIdentifier = SGConstants.UpdateIndicators.CNL;
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

		protected override void GenerateFTX(FTXSegmentMessageSection ftxSection)
		{
			if (CustomsDec.AdditionalMessageInformation != null)
			{
				var fTX = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.ReasonForAmendingAMessage;
				fTX.TextLiteral.FreeText1 = CustomsDec.AdditionalMessageInformation.CancellationCode;
			}
		}

		protected override void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateSegmentGroup1(sg1Section);
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT  *
			PopulateReplacementPermitNoSegment(sg1Section); // AAE
		}

		protected override void PopulatePreviousPermitNumberSegment(SegmentGroup1MessageSection sg1Section)
		{
		}

		protected override void PopulateSupplyIndicatorSegment(SegmentGroup1MessageSection sg1Section)
		{
		}

		protected override void PopulateLicensesAndDocumentsSegments(SegmentGroup1MessageSection sg1Section)
		{
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

		public override void GenerateSegmentGroup10(SegmentGroup10MessageSection sg10Section)
		{
		}

		public override void GenerateSegmentGroup30(SegmentGroup30MessageSection sg30Section)
		{
		}

		protected override void GenerateCNT(CUSDECMessage cusdecEdifactMsg)
		{
			var cNT = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cNT.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}

		protected override void GenerateSegmentGroup49(SegmentGroup49MessageSection sg49Section)
		{
		}
	}
}
