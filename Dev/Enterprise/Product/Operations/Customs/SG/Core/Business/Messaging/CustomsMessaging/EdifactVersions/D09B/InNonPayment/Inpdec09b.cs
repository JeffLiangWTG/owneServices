using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public class Inpdec09b : Cusdec09b
	{
		public Inpdec09b(IINPDEC customsDec)
			: base(customsDec)
		{
		}

		protected IINPDEC CustomsDec
		{
			get { return (IINPDEC)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.INPDEC; }
		}

		public override string MessageSubType
		{
			get { return CUSDECEDIMessage.Declaration; }
		}

		protected override string CommonAccessReferenceCode
		{
			get { return "2"; }
		}

		#region Header Section

		protected override void GenerateHeaderLocSegments(LOCSegmentMessageSection locSection)
		{
			base.GenerateHeaderLocSegments(locSection);

			if (CustomsDec.HasOutwardTransport && !CustomsDec.IsSeaStoreDeclaration
				&& (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.REX || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.SFZ))
			{
				GenerateCountryOfFinalDestinationSegment(locSection);
			}

			if (CustomsDec.HasOutwardTransport && CustomsDec.IsSeaStoreDeclaration)
			{
				GenerateNextPortOfCallSegment(locSection);
				if (CustomsDec.HasLiquorOrTobacco)
				{
					GenerateFinalPortOfCallSegment(locSection);
				}
			}

			if (CustomsDec.Is2bStoredBWCY || CustomsDec.IsStorageInFTZ)
			{
				GeneratePlaceOfStorageSegment(locSection);
			}
		}

		protected override void GenerateHeaderDtmSegments(DTMSegmentMessageSection dtmSection)
		{
			base.GenerateHeaderDtmSegments(dtmSection);
			if (CustomsDec.IsTemporaryConsignment)
			{
				GenerateStartDateOfTemporaryImport(dtmSection);
				GenerateEndDateOfTemporaryImport(dtmSection);
			}
		}

		void GenerateStartDateOfTemporaryImport(DTMSegmentMessageSection dtmSection)
		{
			GenerateDtmSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.StartDateTime, CustomsDec.StartDateOfTemporaryImport);
		}

		void GenerateEndDateOfTemporaryImport(DTMSegmentMessageSection dtmSection)
		{
			GenerateDtmSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.EndDateTime, CustomsDec.EndDateOfTemporaryImport);
		}

		protected override void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateHeaderSegmentGroup1(sg1Section);
			PopulateLicensesAndDocumentsSegments(sg1Section);
			PopulateSupplyIndicatorSegment(sg1Section);
			PopulatePreviousPermitNumberSegment(sg1Section);
			PopulateAdditionalRecipientsSegments(sg1Section);
		}

		protected override void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
			GenerateGroup4SegmentForInwardTransport(sg4Section);
			GenerateGroup4SegmentsForOutwardTransport(sg4Section);
		}

		protected override void GenerateHeaderSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			base.GenerateHeaderSegmentGroup6(sg6Section);

			PopulateInwardCarrierAgentSegment(sg6Section);
			PopulateOutwardCarrierAgentSegment(sg6Section);
			PopulateImporterSegment(sg6Section);
			PopulateExporterSegment(sg6Section, false);
			PopulateConsigneeSegment(sg6Section);
			PopulateForwarderSegment(sg6Section);
			PopulateClaimantSegments(sg6Section);
			PopulateBGIndicator(sg6Section);
		}

		#endregion

		#region Detail Section

		#region Invoice Lines

		#region Group 35

		protected override bool SupportsInvoiceNumberSegment
		{
			get { return true; }
		}

		protected override bool SupportsRegistrationDateSegment
		{
			get { return true; }
		}

		#endregion

		#endregion

		#endregion
	}
}
