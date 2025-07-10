using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class IPTDEC : CUSDEC
	{
		public IPTDEC(IIPTDEC customsDec)
			: base(customsDec)
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

		public enum DeclarationType { DUT, GST, DNG, BKT }

		#region Overrides

		#region Header Section Overrides

		protected override void GenerateLOCSegments(LOCSegmentMessageSection locSection)
		{
			base.GenerateLOCSegments(locSection);

			if (CustomsDec.IsSea)
			{
				GenerateInwardVesselLocationSegment(locSection);
			}
		}

		protected override void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateSegmentGroup1(sg1Section);
			PopulateLicensesAndDocumentsSegments(sg1Section);
			PopulateSupplyIndicatorSegment(sg1Section);
			PopulatePreviousPermitNumberSegment(sg1Section);
			PopulateAdditionalRecipientsSegments(sg1Section);
		}

		protected override void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
			PopulateInwardTransport(sg4Section);
		}

		protected override void GenerateSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			base.GenerateSegmentGroup6(sg6Section);

			PopulateInwardCarrierAgentSegment(sg6Section);
			PopulateImporterSegment(sg6Section);

			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.GST ||
					CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BKP)
			{
				PopulateClaimantSegments(sg6Section);
			}

			PopulateForwarderSegment(sg6Section);
			PopulateBGIndicator(sg6Section);
		}

		#endregion

		#region Group 35

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

		protected override bool SupportsSESegment
		{
			get { return !sgCusdec.IsShortPayment; }
		}

		#endregion

		#region Summary Section Overrides

		protected override void GenerateSegmentGroup49(SegmentGroup49MessageSection sg49Section)
		{
			GenerateTotalCIF_FOBinSGDSegment(sg49Section);
			GenerateTotalDutyAndExciseSegments(sg49Section);

			GenerateTotalGSTPayableSegment(sg49Section);
			GenerateInvoiceTotalAmountPayableSegment(sg49Section);
		}

		#endregion

		#endregion

		#region Summary Section

		void GenerateTotalDutyAndExciseSegments(SegmentGroup49MessageSection sg49Section)
		{
			if (CustomsDec.TotalDutyPayable > 0)
			{
				GenerateMOAandTAXSegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.DutyAmount, CustomsDec.TotalDutyPayable);
			}

			if (CustomsDec.TotalExcisePayable > 0)
			{
				GenerateMOAandTAXSegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount, CustomsDec.TotalExcisePayable);
			}
		}

		void GenerateTotalGSTPayableSegment(SegmentGroup49MessageSection sg49Section)
		{
			GenerateMOAandTAXSegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, MonetaryAmountTypeCodeQualifierList.TaxAmount, CustomsDec.TotalGSTPayable);
		}

		void GenerateInvoiceTotalAmountPayableSegment(SegmentGroup49MessageSection sg49Section)
		{
			GenerateMOAandTAXSegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.TotalOfAllDutiesTaxesAndFeesCustomsItem, MonetaryAmountTypeCodeQualifierList.AmountDueAmountPayable, CustomsDec.TotalPayable);
		}

		void GenerateTotalCIF_FOBinSGDSegment(SegmentGroup49MessageSection sg49Section)
		{
			GenerateMOAandTAXSegments(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.IndividualDutyTaxOrFeeCustomsItem, MonetaryAmountTypeCodeQualifierList.FobValue, CustomsDec.TotalCustomsValue);
		}

		#region Summary Monetary Segments

		protected void GenerateMOAandTAXSegments(SegmentGroup49MessageSection sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList taxQualifier, MonetaryAmountTypeCodeQualifierList monetaryQualifier, decimal monetaryValue)
		{
			SegmentGroup49 sG49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateSummaryTAXSegment(sG49, taxQualifier);
			GenerateSummaryMOASegment(sG49, monetaryQualifier, monetaryValue);
		}

		void GenerateSummaryTAXSegment(SegmentGroup49 sG49, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier)
		{
			TAXSegment tAX = sG49.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
		}

		void GenerateSummaryMOASegment(SegmentGroup49 sG49, MonetaryAmountTypeCodeQualifierList qualifier, decimal monetaryValue)
		{
			MOASegment mOA = sG49.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		#endregion

		#endregion
	}
}
