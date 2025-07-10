using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	class Inpupd09bCancel : Inpupd09b
	{
		public Inpupd09bCancel(IINPUPD customsDec)
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

		protected override CSTSegment GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cst = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			cst.CustomsIdentityCodes2.CustomsGoodsIdentifier = SGConstants.UpdateIndicators.CNL;
			return cst;
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

		protected override void GenerateHeaderFtxSegment(FTXSegmentMessageSection ftxSection)
		{
			if (CustomsDec.AdditionalMessageInformation != null)
			{
				var fTX = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.ReasonForAmendingAMessage;
				fTX.TextLiteral.FreeText1 = CustomsDec.AdditionalMessageInformation.CancellationCode;
			}
		}

		protected override void GenerateCPCSegments(SegmentGroup1MessageSection sg1Section)
		{
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

		protected override void GenerateHeaderSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			PopulateDeclarantSegments(sg6Section);
		}

		protected override void GenerateInvoiceOrCertificateOfOriginGroups(SegmentGroup11MessageSection sg11Section)
		{
		}

		protected override void GenerateInvoiceLineDetailSegments(SegmentGroup32MessageSection sg32Section)
		{
		}

		protected override void GenerateSummaryCnt(CUSDECMessage cusdecEdifactMsg)
		{
			var cnt = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cnt.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}

		protected override void GenerateSegmentGroup51(SegmentGroup51MessageSection sg51Section)
		{
		}
	}
}
