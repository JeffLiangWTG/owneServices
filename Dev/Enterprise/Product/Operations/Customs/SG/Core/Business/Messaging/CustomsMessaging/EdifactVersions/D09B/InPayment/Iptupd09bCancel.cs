using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public class Iptupd09bCancel : Iptupd09b
	{
		public Iptupd09bCancel(IIPTUPD customsDec)
			: base(customsDec)
		{
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

		#region Header Section

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

		protected override void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateHeaderSegmentGroup1(sg1Section);
			PopulateReplacementPermitNoSegment(sg1Section); // AAE
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

		#endregion

		#region Detail Section

		protected override void GenerateInvoiceOrCertificateOfOriginGroups(SegmentGroup11MessageSection sg11Section)
		{
		}

		protected override void GenerateInvoiceLineDetailSegments(SegmentGroup32MessageSection sg32Section)
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
		}

		#endregion
	}
}
