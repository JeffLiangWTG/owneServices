using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class TNPDEC : CUSDEC
	{
		public TNPDEC(ITNPDEC customsDec)
			: base(customsDec)
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

		#region Header Section

		protected override void GenerateLOCSegments(LOCSegmentMessageSection locSection)
		{
			base.GenerateLOCSegments(locSection);

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

			if (CustomsDec.HasOutwardTransport && !CustomsDec.IsSeaStoreDeclaration && !CustomsDec.IsForStorage)
			{
				GenerateCountryOfFinalDestinationSegment(locSection);
			}

			if (CustomsDec.Is2bStoredBWCY || CustomsDec.IsStorageInFTZ)
			{
				GeneratePlaceOfStorageSegment(locSection);
			}
		}

		protected override void GenerateDTMSegments(DTMSegmentMessageSection dtmSection)
		{
			base.GenerateDTMSegments(dtmSection);

			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.REM || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE)
			{
				GenerateStartDateOfCargoRemoval(dtmSection);
			}
		}

		void GenerateStartDateOfCargoRemoval(DTMSegmentMessageSection dtmSection)
		{
			GenerateDTMSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.StartDateTime, CustomsDec.StartDateOfCargoRemoval);
		}

		protected override void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateSegmentGroup1(sg1Section);

			PopulateLicensesAndDocumentsSegments(sg1Section);
			PopulatePreviousPermitNumberSegment(sg1Section);
			PopulateAdditionalRecipientsSegments(sg1Section);
		}

		protected override void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
			PopulateInwardTransport(sg4Section);
			PopulateOutwardTransport(sg4Section);
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

		protected override void GenerateSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			base.GenerateSegmentGroup6(sg6Section);

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

		#region Detail Secion

		public override void GenerateSegmentGroup10(SegmentGroup10MessageSection sg10Section)
		{
		}

		#region SG30 Item Methods Overriden

		protected override void GenerateSegmentGroup33(SegmentGroup30 sG30, ICusItem item)
		{
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.REM || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.IGM)
			{
				GenerateCIF_FOBSegment(sG30, item);
			}
		}

		protected override bool SupportsStrategicGoodsSegments
		{
			get { return true; }
		}

		protected override void GenerateMotorVehicleRegistrationSegment(SegmentGroup35 sG35, string regoNo)
		{
		}

		protected override void GenerateSegmentGroup37(SegmentGroup30 sG30, ICusItem item)
		{
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.TTI || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.TTF)
			{
				//must be in this order...
				if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.MasterBillOfLading, item.InwardMAWB);
				}
				if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.MasterAirWaybill, item.OutwardMAWB);
				}
				if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.HouseWaybill, item.InwardHAWB);
				}
				if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.HouseBillOfLading, item.OutwardHAWB);
				}
			}
			else
			{
				if (CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.HouseWaybill, item.InwardHAWB);
				}
				if (CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CustomsDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.HouseBillOfLading, item.OutwardHAWB);
				}
			}
		}

		protected override void GenerateSegmentGroup41(SegmentGroup30 sG30, ICusItem item)
		{
		}

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateSegmentGroup49(SegmentGroup49MessageSection sg49Section)
		{
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.IGM || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE || CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.REM)
			{
				GenerateSegmentGroup49ByType(sg49Section, DutyOrTaxOrFeeFunctionCodeQualifierList.IndividualDutyTaxOrFeeCustomsItem);
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
