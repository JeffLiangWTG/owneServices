using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	// NB this uses D16A - sixteen-A
	public abstract class CUSCARMessageTextBuilder
	{
		protected CUSCARMessageTextBuilder(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode)
		{
			this.edifactMessage = edifactMessage;
			source = dataSource;
			this.subType = subType;
			this.billIssuerCode = billIssuerCode;
		}

		internal void Create()
		{
			CreateUNH();
			CreateBGM();
			CreateDTMs();
			CreateReferences();
			CreateHeaderParties();
			CreateGroup4s();
			CreateGEIs();
			CreateGroup5Containers();
			CreateGroup6s();
			CreateCNT();
			CreateGroup7HeaderAndChildBills();
			CreateUNT();
		}

		protected string GetGisModeByContainerMode(ZString containerMode)
		{
			var query = new ZQuery(RefCusMapSchema.ZZM_CW1orCommercialValue, containerMode);
			return Factory.LoadTop1<RefCusMap>(query)?.ZZM_CustomsValue;
		}

		#region UNH

		void CreateUNH()
		{
			var unh = edifactMessage.UNH[0];
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.MessageIdentifier.MessageType = "CUSCAR";
			unh.MessageIdentifier.MessageVersionNumber = "D";
			unh.MessageIdentifier.MessageReleaseNumber = "16A";
			unh.MessageIdentifier.ControllingAgency = "UN";
			unh.MessageIdentifier.AssociationAssignedCode = "RCG001";
		}
		#endregion

		#region BGM

		void CreateBGM()
		{
			var bgm = edifactMessage.BGM[0];
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CustomsManifest; //85
			bgm.DocumentMessageName.DocumentName = source.ManifestDocumentType.ToString(); //COH etc
			bgm.DocumentMessageIdentification.DocumentIdentifier = EDIMessage.SystemCommonAccessReferencePkPlaceholder; // "A unique reference number assigned to the electronic document." - treat this as a common access reference - SYS-CAR
			bgm.MessageFunctionCode = Edifact.D16A.Elements.MessageFunctionCodeList.GetFromString(subType.GetMessageFunction_D16A());
		}

		#endregion

		#region DTMs

		protected virtual void CreateDTMs()
		{
			AddHeaderDTM(ZDateTime.Now, DateOrTimeOrPeriodFunctionCodeQualifierList.DocumentIssueDateTime);

			if (RequiredHeaderDTMDepartureDate)
			{
				AddHeaderDTMDepartureDate();
			}
		}

		protected virtual ZBool RequiredHeaderDTMDepartureDate => ZBool.True;

		protected void AddHeaderDTMDepartureDate()
		{
			AddHeaderDTM(source.DepartureDate, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeActual);
		}

		protected virtual void AddHeaderDTM(ZDateTime date, DateOrTimeOrPeriodFunctionCodeQualifierList qualifier)
		{
			if (!date.IsEmpty)
			{
				var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = qualifier;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd; // 102
			}
		}

		#endregion

		#region SG1-RFFs

		void CreateReferences()
		{
			AddHeaderRFF_ACW_MRN_IfNeeded();
			AddHeaderRFF(source.CW1Reference, ReferenceCodeQualifierList.LoadPlanningNumber);
			AddHeaderRFF_AFB_CARN();
			AddHeaderRFF_ACL();
		}

		protected virtual void AddHeaderRFF_ACW_MRN_IfNeeded()
		{
			if (IsAmendmentOrCancellation)
			{
				AddHeaderRFF(source.MRNForAmendOrDelete, ReferenceCodeQualifierList.ReferenceNumberToPreviousMessage);
			}
		}

		protected virtual void AddHeaderRFF_AFB_CARN()
		{
			var carnForAmendOrDelete = source.CARNForAmendOrDelete;

			if (!carnForAmendOrDelete.IsEmpty && IsAmendmentOrCancellation)
			{
				AddHeaderRFF(carnForAmendOrDelete, ReferenceCodeQualifierList.CargoManifestNumber);
			}
		}

		protected virtual void AddHeaderRFF(ZString number, string type)
		{
			if (!number.IsEmpty)
			{
				var grp1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
				var rff = grp1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(type);
				rff.Reference.ReferenceIdentifier = number.Left(35);
			}
		}

		protected virtual void AddHeaderRFF_ACL()
		{
			var voyage = source.ConveyanceNumberOrTransportName;
			if (!voyage.IsEmpty)
			{
				AddHeaderRFF(voyage, ReferenceCodeQualifierList.PrincipalReferenceNumber);
			}
		}

		#endregion

		#region SG2-NADs

		protected virtual void CreateHeaderParties()
		{
			AddHeaderPartyCarrierRL();
			AddHeaderPartyGroupingFZ();
			AddHeaderPartyMessageSenderMS();

			// These do need to do anything yet:
			AddHeaderPartyAgentAH();
			AddHeaderPartyDriverDR();
			AddHeaderPartyCrewMemberFM();
			AddHeaderPartyPassengerFL();
			AddHeaderPartyCarrierDEG();
		}

		protected void AddHeaderParty(PartyType type)
		{
			var party = source.GetParties(billIssuerCode).FirstOrDefault(p => p.PartyType == type);
			var identificationCode = party?.IdentificationCode ?? ZString.Empty;
			if (!identificationCode.IsEmpty)
			{
				var sg2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var nad = sg2.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(new ZString(type.ToString()).Right(2));
				nad.PartyIdentificationDetails.PartyIdentifier = identificationCode.Left(35);
			}
		}

		protected virtual void AddHeaderPartyCarrierRL()
		{
			AddHeaderParty(PartyType.ReportingCarrier_RL);
		}

		protected virtual void AddHeaderPartyGroupingFZ()
		{
			AddHeaderParty(PartyType.GroupingCentre_FZ);
		}

		protected virtual void AddHeaderPartyMessageSenderMS()
		{
			AddHeaderParty(PartyType.MessageSender_MS);
		}

		protected virtual void AddHeaderPartyAgentAH()
		{
		}

		protected virtual void AddHeaderPartyDriverDR()
		{
		}

		protected virtual void AddHeaderPartyCrewMemberFM()
		{
		}

		protected virtual void AddHeaderPartyPassengerFL()
		{
		}

		protected virtual void AddHeaderPartyCarrierDEG()
		{
		}

		#endregion

		#region SG4

		void CreateGroup4s()
		{
			CreateGroup4_1();
		}

		void CreateGroup4_1()
		{
			var sg4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			CreateGroup4TDT20_TransportInformation(sg4);
			if (RequiresGroup4Loc60_PlaceOfDischarge)
			{
				CreateGroup4Loc60(sg4);
			}
			CreateGroup4Loc35(sg4);
			CreateGroup4Loc36(sg4);
			CreateGroup4Loc17(sg4);
			CreateGroup4Loc42(sg4);
			CreateGroup4Loc9(sg4);
			CreateGroup4DTMArrivalDate(sg4);
			CreateGroup4DTM369_LoadingDate(sg4);

			if (RequiresSG4_TranshimentDetails)
			{
				var transport = source.Transports.FirstOrDefault();
				if (transport != null)
				{
					CreateGroup4TDT4_TransportInformation(sg4, transport);
					CreateGroup4DTM133(sg4, transport.ETD);
				}
			}
		}

		protected virtual ZBool RequiresGroup4Loc60_PlaceOfDischarge => true;

		void CreateGroup4TDT20_TransportInformation(SegmentGroup4 grp4)
		{
			var tdt = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport; //20
			AddGroup4TdtConveyanceReferenceNumber(tdt);
			tdt.ModeOfTransport.TransportModeNameCode = new TransportModeTranslator().TranslateToWCOCode(source.TransportMode);
			var carrierCode = source.CarrierCode;
			tdt.Carrier.CarrierIdentifier = carrierCode.Left(17);
			tdt.Carrier.CodeListIdentificationCode = "172";
			tdt.Carrier.CodeListResponsibleAgencyCode = GetGroup4TdtResponsibleParty();
			var vesselId = source.VesselID;
			AddGroup4TDT20_TransportIdentification(tdt, vesselId);
		}

		protected virtual void AddGroup4TDT20_TransportIdentification(TDTSegment tdt, ZString vesselId)
		{
			if (!vesselId.IsEmpty)
			{
				tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier = vesselId.Left(9);
				tdt.TransportIdentification.CodeListIdentificationCode = "103"; // 103
			}
			tdt.TransportIdentification.TransportMeansNationalityCode = source.TransportNationality.Left(3);
		}

		protected virtual void AddGroup4TdtConveyanceReferenceNumber(TDTSegment tdt)
		{
			tdt.MeansOfTransportJourneyIdentifier = source.ConveyanceNumberOrTransportName.Left(17);
		}

		protected virtual CodeListResponsibleAgencyCodeList GetGroup4TdtResponsibleParty()
		{
			return CodeListResponsibleAgencyCodeList.BicBureauInternationalDesContaineurs; // 20 - needed for COM, COH and BBB.  
		}

		protected virtual void CreateGroup4Loc60(SegmentGroup4 sg4)
		{
			var loc60 = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc60.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceOfArrival; //60
			loc60.LocationIdentification.LocationIdentifier = UnlocoToIata(source.PortOfDischarge);
		}

		protected string UnlocoToIata(ZString unloco)
		{
			return MessageBuilderHelper.UnlocoToIata(Factory, source.TransportMode, unloco);
		}

		protected virtual void CreateGroup4Loc35(SegmentGroup4 sg4)
		{
		}

		protected virtual void CreateGroup4Loc36(SegmentGroup4 sg4)
		{
		}

		protected virtual void CreateGroup4Loc17(SegmentGroup4 sg4)
		{
		}

		protected virtual void CreateGroup4Loc42(SegmentGroup4 sg4)
		{
		}

		protected virtual void CreateGroup4Loc9(SegmentGroup4 sg4)
		{
		}

		protected virtual void CreateGroup4DTMArrivalDate(SegmentGroup4 sg4)
		{
			// COH and COM want date; air modes want dateTime
			var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated; //132
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.ArrivalDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd; // 102
		}

		protected virtual void CreateGroup4DTM369_LoadingDate(SegmentGroup4 sg4)
		{
		}

		protected virtual ZBool RequiresSG4_TranshimentDetails => ZBool.False;

		static void CreateGroup4TDT4_TransportInformation(SegmentGroup4 sg4, ICusTransport transport)
		{
			var tdt = sg4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MeansOfTransportAtTransit;
			tdt.MeansOfTransportJourneyIdentifier = transport.VoyageFlightNumber;
			tdt.Carrier.CarrierIdentifier = transport.CarrierCode;
			tdt.Carrier.CodeListIdentificationCode = "172";
			tdt.Carrier.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.BicBureauInternationalDesContaineurs;
			tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier = transport.CallSign;
			tdt.TransportIdentification.CodeListIdentificationCode = (transport.TranportMode == Core.Constants.TransportModes.Air ? "146" : "103");
		}

		static void CreateGroup4DTM133(SegmentGroup4 sg4, ZDateTime etd)
		{
			var dtm133 = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dtm133.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeEstimated; //133
			dtm133.DateTimePeriod.DateOrTimeOrPeriodText = etd.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
			dtm133.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm;
		}

		#endregion

		#region GEIs

		void CreateGEIs()
		{
			if (RequiresGET_ManifestType)
			{
				CreateGEI_ManifestType();
			}
			CreateGEI_CargoType();
			CreateGEI_CallPurpose();
			CreateGEI_TranshipmentIndicator();
		}

		protected void CreateGEI_ManifestType()
		{
			var manifestType = source.ManifestTypeOrBolNature;
			var gei = edifactMessage.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation;
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(manifestType);
			gei.ProcessingIndicator.CodeListIdentificationCode = "71";
			gei.ProcessingIndicator.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		protected virtual ZBool RequiresGET_ManifestType => ZBool.False;

		protected virtual void CreateGEI_CargoType()
		{
		}

		protected virtual void CreateGEI_CallPurpose()
		{
			var callPurpose = source.CustomsCodeForImportExportNature;
			var gei = edifactMessage.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation; // 5
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(callPurpose);
			gei.ProcessingIndicator.CodeListIdentificationCode = "176"; // Flow of the goods
			gei.ProcessingIndicator.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		protected virtual void CreateGEI_TranshipmentIndicator()
		{
		}

		#endregion

		#region SG5

		protected virtual void CreateGroup5Containers()
		{
			if (RequiresContainers)
			{
				foreach (var container in GetDistinctContainersByContainerNumber(Containers))
				{
					var sg5 = edifactMessage.Group5.InstantiateAChildAndAddItToChildrenCollection();
					CreateGroup5ContainerEQD(container, sg5);
					CreateMEAs(container, sg5);
					CreateGroup5SEL(container, sg5);
				}
			}
		}

		protected virtual IEnumerable<ICusCarContainer> Containers => source.GetContainersByBillIssuer(billIssuerCode);

		protected virtual bool RequiresContainers => Containers.Any();

		protected static IEnumerable<ICusCarContainer> GetDistinctContainersByContainerNumber(IEnumerable<ICusCarContainer> contsFromWhichToSelect)
		{
			return contsFromWhichToSelect.GroupBy(p => p.ContainerNumber).Select(g => g.First());
		}

		protected virtual void CreateGroup5ContainerEQD(ICusCarContainer container, SegmentGroup5 sg5)
		{
			var eqd = sg5.EQD.InstantiateAChildAndAddItToChildrenCollection();
			eqd.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.Container; //CN
			eqd.EquipmentIdentification.EquipmentIdentifier = container.ContainerNumber.Left(17);
			eqd.EquipmentSizeAndType.CodeListIdentificationCode = "102"; // 102
			if (source.TransportMode == Core.Constants.TransportModes.Air)
			{
				eqd.EquipmentSizeAndType.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation; // IATA=3
			}
			else if (source.TransportMode == Core.Constants.TransportModes.Sea)
			{
				eqd.EquipmentSizeAndType.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization; // ISO = 5
			}
			eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescriptionCode = EquipmentSizeAndTypeDescriptionCodeList.GetFromString(container.ContainerTypeISO);

			eqd.EquipmentStatusCode = GetEquipmentStatusCode(container);
			eqd.FullOrEmptyIndicatorCode = GetFullOrEmptyIndicatorCode(container);
		}

		static FullOrEmptyIndicatorCodeList GetFullOrEmptyIndicatorCode(ICusCarContainer container)
		{
			return FullOrEmptyIndicatorCodeList.GetFromString(((int)container.GetEmptyFullServiceType(Core.Constants.CountryCodes.SouthAfrica)).ToString(CultureInfo.InvariantCulture));
		}

		static EquipmentStatusCodeList GetEquipmentStatusCode(ICusCarContainer container) => EquipmentStatusCodeList.GetFromString(GetLandedPurpose(container));

		static ZString GetLandedPurpose(ICusCarContainer container)
		{
			var zaContainer = (IZACusCarContainer)container;
			return zaContainer.GetLandedPurpose();
		}

		void CreateMEAs(ICusCarContainer container, SegmentGroup5 sg5)
		{
			if (RequiresMEA_GrossMass)
			{
				AddMEA_GrossMass(container, sg5);
			}

			if (RequiresMEA_VerifiedGrossMass)
			{
				AddMEA_VerifiedGrossMass(container, sg5);
			}
		}

		void AddMEA_GrossMass(ICusCarContainer currentContainer, SegmentGroup5 sg5)
		{
			AddMEA(sg5.MEA.InstantiateAChildAndAddItToChildrenCollection()
				, MeasurementPurposeCodeQualifierList.Measurement // AAE
				, MeasuredAttributeCodeList.TransportMeansGrossWeight // AAM
				, "KGM"
				, currentContainer.GrossMassInKilos);
		}

		protected virtual ZBool RequiresMEA_GrossMass => ZBool.False;

		void AddMEA_VerifiedGrossMass(ICusCarContainer currentContainer, SegmentGroup5 sg5)
		{
			AddMEA(sg5.MEA.InstantiateAChildAndAddItToChildrenCollection()
				, MeasurementPurposeCodeQualifierList.Measurement // AAE
				, MeasuredAttributeCodeList.TransportEquipmentVerifiedGrossMassWeight // VGM
				, "KGM"
				, currentContainer.VerifiedGrossMassInKilos);
		}

		protected virtual ZBool RequiresMEA_VerifiedGrossMass => ZBool.False;

		protected void AddMEA(MEASegment mea, MeasurementPurposeCodeQualifierList measurementPurposeCodeQualifier, MeasuredAttributeCodeList measuredAttributeCode, ZString measurementUnitCode, ZDecimal measure)
		{
			mea.MeasurementPurposeCodeQualifier = measurementPurposeCodeQualifier;
			mea.MeasurementDetails.MeasuredAttributeCode = measuredAttributeCode;
			mea.ValueRange.MeasurementUnitCode = measurementUnitCode;
			mea.ValueRange.Measure = measure.ToStringTrimZeros();
		}

		protected virtual void CreateGroup5SEL(ICusCarContainer container, SegmentGroup5 sg5)
		{
			var seals = container.GetSeals(Core.Constants.CountryCodes.SouthAfrica).Where(x => !x.SealNumber.IsEmpty);
			foreach (var seal in seals)
			{
				var sel = sg5.SEL.InstantiateAChildAndAddItToChildrenCollection();
				sel.TransportUnitSealIdentifier = seal.SealNumber;
				sel.SealIssuer.SealingPartyNameCode = SealingPartyNameCodeList.GetFromString(seal.SealingParty);
				sel.SealTypeCode = SealTypeCodeList.GetFromString(seal.SealType);
			}

			if (!seals.Any())
			{
				var sel = sg5.SEL.InstantiateAChildAndAddItToChildrenCollection();
				sel.TransportUnitSealIdentifier = COSTCO.Contants.SealNumber.NoSealNo; // Should no 'Seal Number' be available, "NO SEAL NO" must be specified within this field.
			}
		}

		#endregion

		#region SG6
		protected virtual void CreateGroup6s() { }
		#endregion

		#region CNT

		void CreateCNT()
		{
			if (RequiresContainers)
			{
				PopulateCNTNumberOfContainers(edifactMessage.CNT.InstantiateAChildAndAddItToChildrenCollection());
			}
		}

		void PopulateCNTNumberOfContainers(CNTSegment cnt)
		{
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.TotalNumberOfEquipment; // 16
			var conts = Containers.ToArray();
			cnt.Control.ControlTotalQuantity = conts.Any() ? GetDistinctContainersByContainerNumber(conts).Count().ToString(CultureInfo.InvariantCulture) : "0";
		}

		#endregion

		#region CreateGroup7HeaderAndChildBills

		protected virtual void CreateGroup7HeaderAndChildBills()
		{
			var sg7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();
			// Header details:
			CreateCNI(sg7);

			if (RequiresContainers)
			{
				PopulateCNTInSg7(sg7);
			}
			// Child bills:
			var billLineNumber = 0;
			foreach (var bill in source.GetLinesByBillIssuer(billIssuerCode))
			{
				billLineNumber++;
				CreateGroup7ChildBill(bill, billLineNumber, sg7);
			}
		}

		protected virtual void PopulateCNTInSg7(SegmentGroup7 sg7)
		{
			PopulateCNTNumberOfContainers(sg7.CNT.InstantiateAChildAndAddItToChildrenCollection());
		}

		protected virtual void CreateCNI(SegmentGroup7 sg7)
		{
			var cni = sg7.CNI.InstantiateAChildAndAddItToChildrenCollection();
			cni.ConsolidationItemNumber = "1";
			cni.DocumentMessageDetails.DocumentIdentifier = GetManifestNumber();
			var parentOrMasterBillType = GetBillsMasterBillTypeCode();
			if (!parentOrMasterBillType.IsEmpty)
			{
				cni.DocumentMessageDetails.DocumentStatusCode = DocumentStatusCodeList.GetFromString(parentOrMasterBillType);
			}
			cni.DocumentMessageDetails.DocumentSourceDescription = GetBillsMasterBillDocumentNumber();
			cni.DocumentMessageDetails.VersionIdentifier = source.ManifestDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
		}

		protected virtual ZString GetManifestNumber()
		{
			return source.ManifestNumber.Left(35);
		}

		protected virtual ZString GetBillsMasterBillTypeCode()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetBillsMasterBillDocumentNumber()
		{
			return ZString.Empty;
		}

		protected void CreateGroup7ChildBill(ICusCarLine line, int billLineNumber, SegmentGroup7 sg7)
		{
			var sg8 = sg7.Group8.InstantiateAChildAndAddItToChildrenCollection();
			AddSG8_RFF(line, billLineNumber, sg8);
			AddLineGroup8LOCs(sg8, line);
			AddLineGroup8GEIs(sg8, line);
			AddLineGroup9References(sg8, line);
			AddLineGroup7Parties(sg8, line);

			var packLineNumber = 0;
			foreach (var pack in line.Packages)
			{
				packLineNumber++;
				AddLineGroup14Package(sg8.Group14.InstantiateAChildAndAddItToChildrenCollection(), pack, packLineNumber);
			}
		}

		protected virtual void AddSG8_RFF(ICusCarLine line, int billLineNumber, SegmentGroup8 sg8)
		{
			var rff = sg8.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.BillOfLadingNumber; // BM
			rff.Reference.ReferenceIdentifier = GetTransportDocumentNumber(line).Left(35);
			AddBillLineReferenceLineNumber(billLineNumber, rff);
		}

		protected virtual ZString GetTransportDocumentNumber(ICusCarLine line) => line.BillNumber;

		protected virtual void AddBillLineReferenceLineNumber(int billLineNumber, RFFSegment rff)
		{
			// RFM needs this:
			//		rff.Reference.DocumentLineIdentifier = billLineNumber.ToString();
		}

		protected void AddLineGroup8LOCs(SegmentGroup8 sg8, ICusCarLine bill)
		{
			if (RequiresLOC8_PlaceOfDestination)
			{
				AddLineGroup8LOC(sg8, UnlocoToIata(bill.FinalDestination), LocationFunctionCodeQualifierList.PlaceOfDestination);
			}

			if (RequiresLOC9_PlaceOfLoading)
			{
				AddLineGroup8LOC(sg8, UnlocoToIata(bill.Origin), LocationFunctionCodeQualifierList.PlaceOfLoading);
			}

			var placeOfDispatch = UnlocoToIata(bill.PlaceOfDispatch);
			if (RequiresLOC80_PlaceOfDespatch(placeOfDispatch))
			{
				AddLineGroup8LOC(sg8, placeOfDispatch, LocationFunctionCodeQualifierList.PlaceOfDespatch);
			}

			var depotOfUnpack = bill.DepotOfUnpack;
			if (RequiresLOC104_DepotOfUnpack(depotOfUnpack))
			{
				AddLineGroup8LOC(sg8, depotOfUnpack.Left(17), LocationFunctionCodeQualifierList.PlaceOfDeconsolidation);
			}

			var terminalOfDischarge = bill.TerminalOfDischarge;
			if (RequiresLOC65_TerminalOfDischarge(terminalOfDischarge))
			{
				AddLineGroup8LOC(sg8, terminalOfDischarge.Left(17), LocationFunctionCodeQualifierList.FinalPortOrPlaceOfDischarge);
			}
		}

		protected virtual ZBool RequiresLOC8_PlaceOfDestination => ZBool.True;

		protected virtual ZBool RequiresLOC9_PlaceOfLoading => ZBool.True;

		protected virtual ZBool RequiresLOC80_PlaceOfDespatch(ZString placeOfDespatch) => !placeOfDespatch.IsEmpty;

		protected virtual ZBool RequiresLOC104_DepotOfUnpack(ZString depotOfUnpack) => ZBool.False;

		protected virtual ZBool RequiresLOC65_TerminalOfDischarge(ZString terminalOfDischarge) => ZBool.False;

		protected ZBool IsDepotOfUnpackAndTerminalOfDischargeMandatory => IsImport || IsTranshipment || IsTransit;

		protected void AddLineGroup8LOC(SegmentGroup8 sg8, ZString port, LocationFunctionCodeQualifierList qual)
		{
			var loc = sg8.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc.LocationFunctionCodeQualifier = qual;
			loc.LocationIdentification.LocationIdentifier = port.Left(35);
		}

		protected virtual void AddLineGroup8GEIs(SegmentGroup8 sg8, ICusCarLine bill)
		{
		}

		protected virtual void AddLineGroup9References(SegmentGroup8 sg8, ICusCarLine line)
		{
			var ucr = line.UCRNumber;
			var shipmentType = line.ShipmentType;
			var otherReferences = line.CustomsNumbers.Where(x => RequiresReference(shipmentType, x.Type)).ToList();
			var requiresRFF_UCR = RequiresRFF_UCR(ucr);
			if (otherReferences.Count > 0 || requiresRFF_UCR)
			{
				var sg9 = sg8.Group9.InstantiateAChildAndAddItToChildrenCollection();
				var tdt = sg9.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport; //20

				otherReferences.ForEach(x => AddCustomsNumber(sg9, ReferenceCodeQualifierList.GetFromString(x.Type), x.Number));

				if (requiresRFF_UCR)
				{
					AddCustomsNumber(sg9, ReferenceCodeQualifierList.UniqueConsignmentReferenceNumber, ucr);
				}
			}
		}

		protected virtual bool RequiresReference(ZString shipmentType, ZString type) => false;

		protected virtual ZBool RequiresRFF_UCR(ZString ucr) => !ucr.IsEmpty;

		void AddCustomsNumber(SegmentGroup9 sg9, ReferenceCodeQualifierList qualifier, ZString number)
		{
			var sg10 = sg9.Group10.InstantiateAChildAndAddItToChildrenCollection();
			var rff = sg10.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = qualifier;
			rff.Reference.ReferenceIdentifier = number;
		}

		protected virtual void AddLineGroup7Parties(SegmentGroup8 sg8, ICusCarLine bill)
		{
			foreach (var party in bill.Parties)
			{
				AddLineGroup7Party(sg8, party);
			}
		}

		protected virtual void AddLineGroup7Party(SegmentGroup8 sg8, ICusCarParty party)
		{
			if (!party.PartyName.IsEmpty)
			{
				var partyTypeCode = new ZString(party.PartyType.ToString()).Right(2);
				var partyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(partyTypeCode == "N1" ? "NI" : partyTypeCode.ToString()); //CN, CZ, etc
				if (RequiresParty(partyFunctionCodeQualifier))
				{
					var sg11 = sg8.Group11.InstantiateAChildAndAddItToChildrenCollection();
					var nad = sg11.NAD.InstantiateAChildAndAddItToChildrenCollection();
					nad.PartyFunctionCodeQualifier = partyFunctionCodeQualifier;
					nad.NameAndAddress.NameAndAddressDescription1 = party.PartyName.Left(35);
					nad.NameAndAddress.NameAndAddressDescription2 = party.Address1.Left(35);
					nad.NameAndAddress.NameAndAddressDescription3 = party.City.Left(35);
					nad.NameAndAddress.NameAndAddressDescription4 = party.State.Left(35);
					nad.NameAndAddress.NameAndAddressDescription5 = party.Postcode.Left(35);
					if (RequiresNAD_Street(partyFunctionCodeQualifier))
					{
						var postalAddress = party.PostalAddress;
						if (postalAddress != null)
						{
							nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = postalAddress.Address1.Left(35);
							nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = postalAddress.Address2.Left(35);
							nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier3 = postalAddress.City.Left(35);
							nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier4 = new ZString(Invariant($"{postalAddress.Postcode} {postalAddress.State} {postalAddress.Country?.Code ?? ZString.Empty}")).Left(35);
						}
					}
				}
			}
		}

		protected virtual ZBool RequiresParty(PartyFunctionCodeQualifierList partyFunctionCodeQualifierList) => true;

		protected virtual ZBool RequiresNAD_Street(PartyFunctionCodeQualifierList partyFunctionCodeQualifier)
		{
			return partyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Consignee || partyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Consignor;
		}

		protected virtual void AddLineGroup14Package(SegmentGroup14 sg14, ICusCarPackage pack, int packLineNumber)
		{
			AddGID_GoodsItemDetails(sg14.GID.InstantiateAChildAndAddItToChildrenCollection(), pack, packLineNumber);

			AddFTX_GoodsDescription(sg14, pack);

			if (RequiresMEA_GrossVolume)
			{
				AddMEA_GrossVolume(sg14, pack);
			}

			AddMEA_GrossMass(sg14, pack);

			if (RequiresContainers && !pack.ContainerNumber.IsEmpty)
			{
				AddSGP_SplitGoodsPlacement(sg14, pack);
			}
			AddDGS_DangerousGoods(pack, sg14);
			AddPCI_MarksAndNumbers(pack, sg14);
			AddCST_CustomsStatusOfGoods(pack, sg14);
		}

		protected void AddGID_GoodsItemDetails(GIDSegment gid, ICusCarPackage pack, int packLineNumber)
		{
			gid.GoodsItemNumber = packLineNumber.ToString(CultureInfo.InvariantCulture);
			gid.NumberAndTypeOfPackages1.PackageQuantity = pack.NumberOfPacks.ToString();
			gid.NumberAndTypeOfPackages1.PackageTypeDescriptionCode = GetPackageTypeDescriptionCode(pack);
		}

		protected virtual ZString GetPackageTypeDescriptionCode(ICusCarPackage pack) => pack.PackUQ;

		ZBool RequiresMEA_GrossVolume => source.CustomsCodeForContainerMode == CargoType_LiquidBulk;

		const string CargoType_LiquidBulk = "LB";

		static void AddMEA_GrossVolume(SegmentGroup14 sg14, ICusCarPackage pack)
		{
			var meaVol = sg14.MEA.InstantiateAChildAndAddItToChildrenCollection();
			meaVol.MeasurementPurposeCodeQualifier = MeasurementPurposeCodeQualifierList.Measurement; //AAE
			meaVol.MeasurementDetails.MeasuredAttributeCode = MeasuredAttributeCodeList.GrossVolume; //AAW
			meaVol.ValueRange.MeasurementUnitCode = pack.GrossVolumeUnitCode;
			meaVol.ValueRange.Measure = pack.GrossVolume.ToStringTrimZeros();
		}

		protected virtual void AddMEA_GrossMass(SegmentGroup14 sg14, ICusCarPackage pack)
		{
			var meaMass = sg14.MEA.InstantiateAChildAndAddItToChildrenCollection();
			meaMass.MeasurementPurposeCodeQualifier = MeasurementPurposeCodeQualifierList.Measurement; //AAE
			meaMass.MeasurementDetails.MeasuredAttributeCode = MeasuredAttributeCodeList.GoodsItemGrossWeight; //AAB
			meaMass.ValueRange.MeasurementUnitCode = "KGM";
			meaMass.ValueRange.Measure = pack.GrossMassInKilos.ToStringTrimZeros();
		}

		protected virtual void AddFTX_GoodsDescription(SegmentGroup14 sg14, ICusCarPackage pack)
		{
			var ftx = sg14.FTX.InstantiateAChildAndAddItToChildrenCollection();
			ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GoodsItemDescription; //AAA
			SetLineGroup10CargoStatusIndicatorForPartShipment(pack, ftx);
			ftx.TextLiteral.FreeText1 = pack.Description.SubstringSafe(0, 70);
			ftx.TextLiteral.FreeText2 = pack.Description.SubstringSafe(70, 70);
			ftx.TextLiteral.FreeText3 = pack.Description.SubstringSafe(140, 70);
			ftx.TextLiteral.FreeText4 = pack.Description.SubstringSafe(210, 70);
			ftx.TextLiteral.FreeText5 = pack.Description.SubstringSafe(280, 70);
		}

		protected virtual void AddSGP_SplitGoodsPlacement(SegmentGroup14 sg14, ICusCarPackage pack)
		{
			var sgp = sg14.SGP.InstantiateAChildAndAddItToChildrenCollection();
			sgp.EquipmentIdentification.EquipmentIdentifier = pack.ContainerNumber.Left(17);
			SetLineGroup10SgpNumberOfPackages(pack, sgp);
		}

		protected virtual void SetLineGroup10SgpNumberOfPackages(ICusCarPackage pack, SGPSegment sgp)
		{
		}

		protected virtual void SetLineGroup10CargoStatusIndicatorForPartShipment(ICusCarPackage pack, FTXSegment ftx)
		{
		}

		protected virtual void AddCST_CustomsStatusOfGoods(ICusCarPackage pack, SegmentGroup14 sg14)
		{
			if (!pack.CommodityCode.IsEmpty)
			{
				var cst = sg14.CST.InstantiateAChildAndAddItToChildrenCollection();
				cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = pack.CommodityCode.Left(18);
			}
		}

		protected virtual void AddDGS_DangerousGoods(ICusCarPackage pack, SegmentGroup14 sg14)
		{
			if (!pack.UNDGClass.IsEmpty || !pack.UNDGNumber.IsEmpty)
			{
				// Dangerous goods
				var dgs = sg14.DGS.InstantiateAChildAndAddItToChildrenCollection();
				dgs.DangerousGoodsRegulationsCode = DangerousGoodsRegulationsCodeList.ImoImdgCode; // IMD
				dgs.HazardCode.HazardIdentificationCode = pack.UNDGClass;
				dgs.UndgInformation.UnitedNationsDangerousGoods = pack.UNDGNumber;
			}
		}

		protected virtual void AddPCI_MarksAndNumbers(ICusCarPackage pack, SegmentGroup14 sg14)
		{
			var pci = sg14.PCI.InstantiateAChildAndAddItToChildrenCollection();
			pci.MarkingInstructionsCode = MarkingInstructionsCodeList.ShipperAssigned; //24
			AddMarksAndNumbersToPCI(pci, pack);
		}

		protected virtual void AddMarksAndNumbersToPCI(PCISegment pci, ICusCarPackage pack)
		{
			pci.MarksLabels.ShippingMarksDescription1 = pack.MarksAndNumbers.SubstringSafe(0, 35);
			pci.MarksLabels.ShippingMarksDescription2 = pack.MarksAndNumbers.SubstringSafe(35, 35);
			pci.MarksLabels.ShippingMarksDescription3 = pack.MarksAndNumbers.SubstringSafe(70, 35);
			pci.MarksLabels.ShippingMarksDescription4 = pack.MarksAndNumbers.SubstringSafe(105, 35);
			pci.MarksLabels.ShippingMarksDescription5 = pack.MarksAndNumbers.SubstringSafe(140, 35);
			pci.MarksLabels.ShippingMarksDescription6 = pack.MarksAndNumbers.SubstringSafe(175, 35);
			pci.MarksLabels.ShippingMarksDescription7 = pack.MarksAndNumbers.SubstringSafe(210, 35);
			pci.MarksLabels.ShippingMarksDescription8 = pack.MarksAndNumbers.SubstringSafe(245, 35);
			pci.MarksLabels.ShippingMarksDescription9 = pack.MarksAndNumbers.SubstringSafe(280, 35);
			pci.MarksLabels.ShippingMarksDescription10 = pack.MarksAndNumbers.SubstringSafe(315, 35);
		}

		#endregion

		protected virtual void CreateUNT()
		{
			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture);
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		readonly MessageSubTypes subType;
		protected CUSCARMessage edifactMessage;
		protected readonly ICusCarHeader source;
		protected readonly string billIssuerCode;

		BusinessObjectFactory factory;
		protected BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		protected bool IsAmendmentOrCancellation => subType.IsAmendmentOrCancellation();

		protected ZBool IsImport => source.ImportExportNature == ShipmentTypeList.Codes.Import23;

		protected ZBool IsExport => source.ImportExportNature == ShipmentTypeList.Codes.Export22;

		protected ZBool IsTranshipment => source.ImportExportNature == ShipmentTypeList.Codes.Transhipment28;

		protected ZBool IsTransit => source.ImportExportNature == ShipmentTypeList.Codes.Transit24;
	}
}

// For unit tests, refer to => C:\Dev\Enterprise\Product\Operations\Customs\ASYCUDA\ZAManifest\ZAManifest.Business\Messaging\ZaAsycudaToCuscarTest.cs
