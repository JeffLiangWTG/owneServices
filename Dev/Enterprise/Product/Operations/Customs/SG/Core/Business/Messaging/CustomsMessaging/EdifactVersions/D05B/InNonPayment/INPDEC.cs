using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class INPDEC : CUSDEC
	{
		public INPDEC(IINPDEC customsDec)
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

		protected override void GenerateLOCSegments(LOCSegmentMessageSection locSection)
		{
			base.GenerateLOCSegments(locSection);

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

			if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea)
			{
				GenerateInwardVesselLocationSegment(locSection);
			}

			if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea)
			{
				GenerateOutwardVesselLocationSegment(locSection);
			}

			if (CustomsDec.Is2bStoredBWCY || CustomsDec.IsStorageInFTZ)
			{
				GeneratePlaceOfStorageSegment(locSection);
			}
		}

		protected override void GenerateDTMSegments(DTMSegmentMessageSection dtmSection)
		{
			base.GenerateDTMSegments(dtmSection);
			if (CustomsDec.IsTemporaryConsignment)
			{
				GenerateEndDateOfTemporaryImport(dtmSection);
			}
		}

		void GenerateEndDateOfTemporaryImport(DTMSegmentMessageSection dtmSection)
		{
			GenerateDTMSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.EndDateTime, CustomsDec.EndDateOfTemporaryImport);
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
			PopulateOutwardTransport(sg4Section);
		}

		protected override void GenerateSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			base.GenerateSegmentGroup6(sg6Section);

			PopulateInwardCarrierAgentSegment(sg6Section);
			PopulateOutwardCarrierAgentSegment(sg6Section);
			PopulateImporterSegment(sg6Section);
			PopulateExporterSegment(sg6Section, false);
			PopulateConsigneeSegment(sg6Section);
			PopulateForwarderSegment(sg6Section);
			PopulateClaimantSegments(sg6Section);
			PopulateBGIndicator(sg6Section);
		}

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

		protected override void GenerateSegmentGroup37(SegmentGroup30 sG30, ICusItem item)
		{
			base.GenerateSegmentGroup37(sG30, item);

			if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
			{
				if (!item.OutwardHAWB.IsEmpty)
				{
					GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.HouseBillOfLading, item.OutwardHAWB);
				}
			}
		}

		protected override void GenerateSegmentGroup41(SegmentGroup30 g30, ICusItem item)
		{
			//Note 1
			if (!((CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.APS && item.DGIndicator == DGIndicatorCodeList.Codes.Y) || (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.SHO && CustomsDec.GoodsImportedUnderMESorBWS)))
			{
				if (item.DutyAmount > 0)
				{
					PopulateCustomsDutySegments(g30, item);
				}

				if (item.ExciseAmount > 0)
				{
					PopulateCustomsExciseSegments(g30, item);
				}
			}

			// Note 2
			if (!(CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.APS && item.DGIndicator == DGIndicatorCodeList.Codes.Y))
			{
				PopulateGSTSegments(g30, item);
			}
		}

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateSegmentGroup49(SegmentGroup49MessageSection sg49Section)
		{
			if (!((CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.APS && CustomsDec.IsDG) || (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.SHO && CustomsDec.GoodsImportedUnderMESorBWS)))
			{
				GenerateSegmentGroup49DutyExcise(sg49Section);
			}

			if (!(CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.APS && CustomsDec.IsDG))
			{
				GenerateSegmentGroup49ByType(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.IndividualDutyTaxOrFeeCustomsItem);
				GenerateSegmentGroup49ByType(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.TotalOfAllDutiesTaxesAndFeesCustomsItem);
				GenerateSegmentGroup49ByType(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax);
			}
		}

		void GenerateSegmentGroup49DutyExcise(SegmentGroup49MessageSection sg49Section)
		{
			if (CustomsDec.TotalDutyPayable > 0m || CustomsDec.TotalExcisePayable > 0m)
			{
				if (CustomsDec.TotalDutyPayable > 0m)
				{
					SegmentGroup49 g49 = GenerateTAXQualiferSegment(sg49Section);
					MOASegment mOA = g49.MOA.InstantiateAChildAndAddItToChildrenCollection();
					mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.DutyAmount;
					mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(CustomsDec.TotalDutyPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
				}

				if (CustomsDec.TotalExcisePayable > 0m)
				{
					SegmentGroup49 g49 = GenerateTAXQualiferSegment(sg49Section);
					MOASegment mOA1 = g49.MOA.InstantiateAChildAndAddItToChildrenCollection();
					mOA1.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount;
					mOA1.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(CustomsDec.TotalExcisePayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
				}
			}
		}

		SegmentGroup49 GenerateTAXQualiferSegment(SegmentGroup49MessageSection sg49Section)
		{
			SegmentGroup49 g49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
			TAXSegment tAX = g49.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty;
			return g49;
		}

		protected void GenerateSegmentGroup49ByType(SegmentGroup49MessageSection sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier)
		{
			if (qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.Tax && CustomsDec.TotalGSTPayable > 0)
			{
				SegmentGroup49 g49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
				TAXSegment tAX = g49.TAX.InstantiateAChildAndAddItToChildrenCollection();
				tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
				MOASegment mOA = g49.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.TaxAmount;
				mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(CustomsDec.TotalGSTPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			}

			if (qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.TotalOfAllDutiesTaxesAndFeesCustomsItem && CustomsDec.TotalPayable > 0)
			{
				SegmentGroup49 g49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
				TAXSegment tAX = g49.TAX.InstantiateAChildAndAddItToChildrenCollection();
				tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
				MOASegment mOA = g49.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.AmountDueAmountPayable;
				mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(CustomsDec.TotalPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			}

			if (qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.IndividualDutyTaxOrFeeCustomsItem)
			{
				SegmentGroup49 g49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
				TAXSegment tAX = g49.TAX.InstantiateAChildAndAddItToChildrenCollection();
				tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
				MOASegment mOA = g49.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.FobValue;
				mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(CustomsDec.TotalCustomsValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			}
		}

		#endregion
	}
}
