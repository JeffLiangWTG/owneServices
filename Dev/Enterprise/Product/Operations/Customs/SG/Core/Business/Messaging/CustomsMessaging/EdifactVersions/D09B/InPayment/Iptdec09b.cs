using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Messages.CUSDEC;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public class Iptdec09b : Cusdec09b
	{
		public Iptdec09b(IIPTDEC sgCusdec)
			: base(sgCusdec)
		{
		}

		protected IIPTDEC CustomsDec
		{
			get { return (IIPTDEC)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.IPTDEC; }
		}

		public override string MessageSubType
		{
			get { return CUSDECEDIMessage.Declaration; }
		}

		protected override string CommonAccessReferenceCode
		{
			get { return "1"; }
		}

		public enum DeclarationType { DUT, GST, DNG, BKT }

		#region Overrides

		#region Header Section Overrides

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
		}

		protected override void GenerateHeaderSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			base.GenerateHeaderSegmentGroup6(sg6Section);

			PopulateInwardCarrierAgentSegment(sg6Section);
			PopulateImporterSegment(sg6Section);

			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.GST || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BKP)
			{
				PopulateClaimantSegments(sg6Section);
			}

			PopulateForwarderSegment(sg6Section);
			PopulateBGIndicator(sg6Section);
		}

		#endregion

		#region Group 37

		protected override bool SupportsInvoiceNumberSegment
		{
			get { return true; }
		}

		protected override bool SupportsRegistrationDateSegment
		{
			get { return true; }
		}

		protected override bool SupportsVehicleRego
		{
			get { return true; }
		}

		protected override bool SupportsSeastoresSegments
		{
			get { return false; }
		}

		protected override bool SupportsSeSegment
		{
			get { return !sgCusdec.IsShortPayment; }
		}

		#endregion

		#endregion
	}
}
