using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public class Tnpdec09b : Cusdec09b
	{
		public Tnpdec09b(ITNPDEC sgCusdec)
			: base(sgCusdec)
		{
		}

		protected ITNPDEC CustomsDec
		{
			get { return (ITNPDEC)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.TNPDEC; }
		}

		public override string MessageSubType
		{
			get { return CUSDECEDIMessage.Declaration; }
		}

		protected override string CommonAccessReferenceCode
		{
			get { return "7"; }
		}

		#region Header Section

		protected override void GenerateHeaderLocSegments(LOCSegmentMessageSection locSection)
		{
			base.GenerateHeaderLocSegments(locSection);

			if (CustomsDec.HasOutwardTransport && CustomsDec.IsSeaStoreDeclaration)
			{
				GenerateNextPortOfCallSegment(locSection);
				if (CustomsDec.HasLiquorOrTobacco)
				{
					GenerateFinalPortOfCallSegment(locSection);
				}
			}

			if (CustomsDec.HasOutwardTransport && !CustomsDec.IsSeaStoreDeclaration)
			{
				GenerateCountryOfFinalDestinationSegment(locSection);
			}

			if (CustomsDec.Is2bStoredBWCY || CustomsDec.IsStorageInFTZ)
			{
				GeneratePlaceOfStorageSegment(locSection);
			}
		}

		protected override void GenerateHeaderDtmSegments(DTMSegmentMessageSection dtmSection)
		{
			base.GenerateHeaderDtmSegments(dtmSection);

			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.REM || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE)
			{
				GenerateStartDateOfCargoRemoval(dtmSection);
			}
		}

		void GenerateStartDateOfCargoRemoval(DTMSegmentMessageSection dtmSection)
		{
			GenerateDtmSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.StartDateTime, CustomsDec.StartDateOfCargoRemoval);
		}

		protected override void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateHeaderSegmentGroup1(sg1Section);

			PopulateLicensesAndDocumentsSegments(sg1Section);
			PopulatePreviousPermitNumberSegment(sg1Section);
			PopulateAdditionalRecipientsSegments(sg1Section);
		}

		protected override void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
			GenerateGroup4SegmentForInwardTransport(sg4Section);
			GenerateGroup4SegmentsForOutwardTransport(sg4Section);
		}

		protected override void GenerateSegmentGroup5(SegmentGroup5MessageSection sg5Section)
		{
			if (!(CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.TTI || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.TTF))
			{
				if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					PopulateInwardMasterBill(sg5Section);
				}

				if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					PopulateOutwardMasterBill(sg5Section);
				}
			}

			PopulateSupportingDocs(sg5Section);
		}

		protected override void GenerateHeaderSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			base.GenerateHeaderSegmentGroup6(sg6Section);

			PopulateInwardCarrierAgentSegment(sg6Section);
			PopulateOutwardCarrierAgentSegment(sg6Section);
			PopulateImporterSegment(sg6Section);
			PopulateHandlingAgentSegment(sg6Section);
			PopulateEndUserSegment(sg6Section);
			PopulateConsigneeSegment(sg6Section);
			PopulateForwarderSegment(sg6Section);
			PopulateBGIndicator(sg6Section);
		}

		#endregion

		#region Detail Section

		protected override void GenerateInvoiceOrCertificateOfOriginGroups(SegmentGroup11MessageSection sg11Section)
		{
		}

		#region SG32 Item Methods Overriden

		protected override void GenerateUnitPriceSegment(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
		}

		protected override void GenerateSegmentGroups35And36(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.REM || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.IGM)
			{
				GenerateCifFobSegment(sg35Section, item);
			}
		}

		protected override bool SupportsStrategicGoodsSegments
		{
			get { return true; }
		}

		protected override void GenerateMotorVehicleRegistrationSegment(SegmentGroup37 sg37, string regoNo)
		{
		}

		protected override void GenerateSegmentGroup39(SegmentGroup39MessageSection sg39Section, ICusItem item)
		{
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.TTI || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.TTF)
			{
				//must be in this order...
				if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.MasterBillOfLading, item.InwardMAWB);
				}
				if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.MasterAirWaybill, item.OutwardMAWB);
				}
				if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.HouseWaybill, item.InwardHAWB);
				}
				if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.HouseBillOfLading, item.OutwardHAWB);
				}
			}
			else
			{
				if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.HouseWaybill, item.InwardHAWB);
				}
				if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.HouseBillOfLading, item.OutwardHAWB);
				}
			}
		}

		protected override void GenerateSegmentGroup43(SegmentGroup43MessageSection sg43Section, ICusItem item)
		{
		}

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateSegmentGroup51(SegmentGroup51MessageSection sg51Section)
		{
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.IGM || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.REM)
			{
				GenerateGroup51TaxAndMoa(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.IndividualDutyTaxOrFeeCustomsItem, MonetaryAmountTypeCodeQualifierList.FobValue, sgCusdec.TotalCustomsValue);
			}
		}

		protected void GenerateSegmentGroup49ByType(SegmentGroup49MessageSection sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier)
		{
			var g49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
			TAXSegment tAX = g49.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
			MOASegment mOA = g49.MOA.InstantiateAChildAndAddItToChildrenCollection();
			if (qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.IndividualDutyTaxOrFeeCustomsItem)
			{
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.FobValue;
				decimal monetaryValue = CustomsDec.TotalCustomsValue;
				mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			}
		}

		#endregion
	}
}
