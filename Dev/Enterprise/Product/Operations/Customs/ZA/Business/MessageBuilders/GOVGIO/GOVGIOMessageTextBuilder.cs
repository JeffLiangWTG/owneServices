using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.GOVCBR;
using Enterprise.Edifact.D16A.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	class GOVGIOMessageTextBuilder
	{
		public GOVGIOMessageTextBuilder(GOVCBRMessage edifactMessage, AsycudaManifestHeader source, MessageSubTypes subType)
		{
			this.edifactMessage = edifactMessage;
			this.source = source;
			this.subType = subType;
		}

		internal void Create()
		{
			CreateUNH();
			CreateBGM();
			CreateSG1_LOC11_PlacePortOfDischarge();
			CreateSG1_LOC34_PlaceOrPortOfLoading();
			CreateSG7_NAD_TB_Submitter();
			CreateSG7_NAD_CA_Carrier();
			CreateSG7_NAD_TR_TerminalOperator();
			CreateSG7_NAD_DC_OutturnProvider();
			CreateSG9_DOC_998_DocumentToBeAmended();
			CreateSG9_DOC_706_PrincipalCarrierConveyanceNumber();
			CreateSG34();
			CreateUNS_Details();
			CreateHYN();
			CreateSG159_ConsignmentDetails();
			CreateUNS_Summary();
			CreateUNT_MessageTrailer();
		}

		void CreateUNH()
		{
			var unhSegment = edifactMessage.UNH[0];
			unhSegment.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unhSegment.MessageIdentifier.MessageType = "GOVCBR"; // Government cross border regulatory message
			unhSegment.MessageIdentifier.MessageVersionNumber = "D"; // Draft version/UN/EDIFACT Directory
			unhSegment.MessageIdentifier.MessageReleaseNumber = "16A"; // Release 2016 - A
			unhSegment.MessageIdentifier.ControllingAgency = "UN"; // UN/CEFACT
			unhSegment.MessageIdentifier.AssociationAssignedCode = "RCG001"; // RCG Version 1
			unhSegment.CommonAccessReference = "GOVGIO"; // Gate In / Gate Out
		}

		void CreateBGM()
		{
			var bgmSegment = edifactMessage.BGM[0];
			bgmSegment.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GatePass;
			bgmSegment.DocumentMessageName.DocumentName = source.GateInOutMessageType;
			bgmSegment.DocumentMessageIdentification.DocumentIdentifier = EDIMessage.SystemCommonAccessReferencePkPlaceholder;
			bgmSegment.MessageFunctionCode = Edifact.D16A.Elements.MessageFunctionCodeList.GetFromString(subType.GetMessageFunction_D16A());
		}

		void CreateSG1_LOC11_PlacePortOfDischarge()
		{
			AddNewLOC(edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection().LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlaceOfDischarge, UnlocoToIata(source.MasterBill.ABL_RL_NKPortOfDischarge));
		}

		void CreateSG1_LOC34_PlaceOrPortOfLoading()
		{
			var gateInOutMessageType = source.GateInOutMessageType;
			// LOC+34 is also not need for STO, but STO is not supported yet.
			if (gateInOutMessageType != GateInOutMessageTypeCodeList.Codes.DepotGateIn
				&& gateInOutMessageType != GateInOutMessageTypeCodeList.Codes.TerminalGateOut)
			{
				AddNewLOC(edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection().LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.BaseportOfLoading, UnlocoToIata(source.MasterBill.ABL_RL_NKPortOfLoading));
			}
		}

		static void AddNewLOC(LOCSegment loc, LocationFunctionCodeQualifierList locationFunctionCodeQualifier, ZString locationIdentifier)
		{
			loc.LocationFunctionCodeQualifier = locationFunctionCodeQualifier;
			loc.LocationIdentification.LocationIdentifier = locationIdentifier;
		}

		void CreateSG7_NAD_TB_Submitter()
		{
			var sg7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();

			var nad = sg7.NAD[0];
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Submitter;
			nad.PartyIdentificationDetails.PartyIdentifier = ((IInterchangeSenderIdProvider)source).SenderID;

			var ifd = sg7.IFD[0];
			ifd.InformationDetailsCodeQualifier = InformationDetailsCodeQualifierList.BusinessInformation;
			ifd.InformationType.InformationTypeCode = GetManifestType();
		}

		void CreateSG7_NAD_CA_Carrier()
		{
			var sg7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();

			var nad = sg7.NAD[0];
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Carrier;
			nad.PartyIdentificationDetails.PartyIdentifier = source.CarrierCode;
		}

		void CreateSG7_NAD_TR_TerminalOperator()
		{
			var gateInOutMessageType = source.GateInOutMessageType;
			if (gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.BreakBulkGateIn
				|| gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.AirTerminalGateIn
				|| gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateIn
				|| gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateOut)
			{
				var sg7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();

				var nad = sg7.NAD[0];
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.TerminalOperator;
				nad.PartyIdentificationDetails.PartyIdentifier = source.TerminalBerth;
			}
		}

		void CreateSG7_NAD_DC_OutturnProvider()
		{
			var sg7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();

			var nad = sg7.NAD[0];
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Deconsolidator;
			nad.PartyIdentificationDetails.PartyIdentifier = source.DeconsolidateAddress?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DepotControlledPremisesID, Core.Constants.CountryCodes.SouthAfrica);
		}

		void CreateSG9_DOC_998_DocumentToBeAmended()
		{
			if (subType.IsAmendmentOrCancellation())
			{
				var doc = edifactMessage.Group9.InstantiateAChildAndAddItToChildrenCollection().DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.PreviousCustomsDocumentMessage;
				doc.DocumentMessageDetails.DocumentIdentifier = source.GetLastAcceptedGOVGIOEDIMessage()?.ParentMessageNumber ?? ZString.Empty;
			}
		}

		void CreateSG9_DOC_706_PrincipalCarrierConveyanceNumber()
		{
			var doc = edifactMessage.Group9.InstantiateAChildAndAddItToChildrenCollection().DOC.InstantiateAChildAndAddItToChildrenCollection();
			doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.BillOfLadingOriginal;
			doc.DocumentMessageDetails.DocumentIdentifier = source.AMA_Voyage;
		}

		void CreateSG34()
		{
			var sg34 = edifactMessage.Group34.InstantiateAChildAndAddItToChildrenCollection();

			var tdt = sg34.TDT[0];
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
			tdt.MeansOfTransportJourneyIdentifier = source.AMA_Voyage;
			tdt.ModeOfTransport.TransportModeNameCode = GetModeOfTransportCode();
			if (!source.IsAir)
			{
				var carrierName = source.Vessel?.RV_CarrierCode ?? ZString.Empty;
				if (!carrierName.IsEmpty)
				{
					tdt.Carrier.CarrierName = carrierName;
				}
				tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier = source.Vessel?.RV_RadioCallSign;
				tdt.TransportIdentification.CodeListIdentificationCode = "103";
			}

			// todo: refactor in the future because DTM+132 is also mandatory for STO
			var gateInOutMessageType = source.GateInOutMessageType;
			if (gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.DepotGateOut || gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateOut)
			{
				AddDTM_Ccyymmddhhmm(sg34.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, source.MasterBill.ABL_E_ARV);
			}

			AddDTM_Ccyymmdd(sg34.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeEstimated, source.MasterBill.ABL_E_DEP);

			var qty = sg34.QTY[0];
			qty.QuantityDetails.QuantityTypeCodeQualifier = QuantityTypeCodeQualifierList.EquipmentQuantity;
			qty.QuantityDetails.Quantity = GetEquipmentQuantity().ToString(CultureInfo.InvariantCulture);

			var poc = sg34.POC[0];
			poc.PurposeOfConveyanceCall.ConveyanceCallPurposeDescriptionCode = GetPurposeOfConveyanceCall();
		}

		ConveyanceCallPurposeDescriptionCodeList GetPurposeOfConveyanceCall()
		{
			var shipmentType = source.AMA_Nature;
			switch (shipmentType)
			{
				case NatureList.Codes.Import23:
					return ConveyanceCallPurposeDescriptionCodeList.UnloadingCargo;
				case NatureList.Codes.Export22:
				case NatureList.Codes.Transit24:
				case NatureList.Codes.Transhipment28:
					return ConveyanceCallPurposeDescriptionCodeList.LoadingCargo;
				default:
					return null;
			}
		}

		void CreateUNS_Details()
		{
			var uns = edifactMessage.UNS1[0];
			uns.SectionIdentification = "D";
		}

		void CreateHYN()
		{
			var uns = edifactMessage.HYN[0];
			uns.HierarchyObjectCodeQualifier = HierarchyObjectCodeQualifierList.NoHierarchy;
		}

		void CreateSG159_ConsignmentDetails()
		{
			var bills = source.Bills.Cast<AsycudaBill>().ToList();
			for (var billNumber = 1; billNumber <= bills.Count; billNumber++)
			{
				var sg159 = edifactMessage.Group159.InstantiateAChildAndAddItToChildrenCollection();
				var bill = bills[billNumber - 1];

				var cni = sg159.CNI.InstantiateAChildAndAddItToChildrenCollection();
				cni.ConsolidationItemNumber = billNumber.ToString(CultureInfo.InvariantCulture);

				var externalReference = source.AMA_JobReference;
				if (!externalReference.IsEmpty)
				{
					var acdRFF = sg159.RFF.InstantiateAChildAndAddItToChildrenCollection();
					acdRFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.AdditionalReferenceNumber;
					acdRFF.Reference.ReferenceIdentifier = externalReference;
				}

				var gateInOutMessageType = source.GateInOutMessageType;
				var bookingNumber = source.BookingNumber;
				if (!bookingNumber.IsEmpty
					&& (gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateIn || gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateOut))
				{
					var srnRFF = sg159.RFF.InstantiateAChildAndAddItToChildrenCollection();
					srnRFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.ShipmentReferenceNumber;
					srnRFF.Reference.ReferenceIdentifier = bookingNumber;
				}

				CreateSG163_DOC_704_MasterTransportDocument(sg159);
				CreateSG163_DOC_703_TransportDocument(sg159, bill);

				if (source.IsAir)
				{
					CreateEmptySG177(sg159.Group177.InstantiateAChildAndAddItToChildrenCollection());
				}
				else
				{
					var containers = bill.Packs.Cast<AsycudaPack>().Where(p => p.Container != null).Select(p => p.Container).Distinct().ToList();
					var containerIndex = 0;
					foreach (var container in containers)
					{
						containerIndex++;
						CreateSG177(sg159.Group177.InstantiateAChildAndAddItToChildrenCollection(), containerIndex, container);
					}
				}

				CreateSG186_CargoTypeIndicator(sg159);
				CreateSG267_LineItem(sg159, bill);
			}
		}

		void CreateSG163_DOC_704_MasterTransportDocument(SegmentGroup159 sg159)
		{
			var sg163 = sg159.Group163.InstantiateAChildAndAddItToChildrenCollection();

			var doc = sg163.DOC[0];
			doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.MasterBillOfLading;
			doc.DocumentMessageDetails.DocumentIdentifier = source.MasterBill.ABL_BillNumber;

			var carrierCode = source.CarrierCode;
			switch (source.GateInOutMessageType)
			{
				case GateInOutMessageTypeCodeList.Codes.DepotGateIn:
				case GateInOutMessageTypeCodeList.Codes.SeaDepotConsignmentGateIn:
				case GateInOutMessageTypeCodeList.Codes.DepotGateOut:
				case GateInOutMessageTypeCodeList.Codes.TerminalGateIn:
				case GateInOutMessageTypeCodeList.Codes.TerminalGateOut:
				case GateInOutMessageTypeCodeList.Codes.AirDepotGateIn when !carrierCode.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.AirTerminalGateIn when !carrierCode.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.BreakBulkGateIn when !carrierCode.IsEmpty:
					CreateSG164_MasterCargoCarrierCode(sg163, carrierCode);
					break;
			}
		}

		static void CreateSG164_MasterCargoCarrierCode(SegmentGroup163 sg163, ZString carrierCode)
		{
			var sg164 = sg163.Group164.InstantiateAChildAndAddItToChildrenCollection();

			var nad = sg164.NAD[0];
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.DocumentMessageIssuerSender;
			nad.PartyIdentificationDetails.PartyIdentifier = carrierCode;
		}

		void CreateSG163_DOC_703_TransportDocument(SegmentGroup159 sg159, AsycudaBill bill)
		{
			var sg163 = sg159.Group163.InstantiateAChildAndAddItToChildrenCollection();

			var doc = sg163.DOC[0];
			doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.HouseWaybill;
			doc.DocumentMessageDetails.DocumentIdentifier = bill.ABL_BillNumber;

			var billIssuer = bill.ABL_BillIssuer;
			switch (source.GateInOutMessageType)
			{
				case GateInOutMessageTypeCodeList.Codes.DepotGateIn when !billIssuer.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.TerminalGateIn when !billIssuer.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.SeaDepotConsignmentGateIn:
				case GateInOutMessageTypeCodeList.Codes.DepotGateOut when !billIssuer.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.TerminalGateOut when !billIssuer.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.AirDepotGateIn when !billIssuer.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.BreakBulkGateIn when !billIssuer.IsEmpty:
				case GateInOutMessageTypeCodeList.Codes.AirTerminalGateIn when !billIssuer.IsEmpty:
					CreateSG164_CargoCarrierCode(sg163, billIssuer);
					break;
			}
		}

		static void CreateSG164_CargoCarrierCode(SegmentGroup163 sg163, ZString cargoCarrierCode)
		{
			var sg164 = sg163.Group164.InstantiateAChildAndAddItToChildrenCollection();

			var nad = sg164.NAD[0];
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.DocumentMessageIssuerSender;
			nad.PartyIdentificationDetails.PartyIdentifier = cargoCarrierCode;
		}

		void CreateSG177(SegmentGroup177 sg177, ZInt containerIndex, AsycudaContainer container)
		{
			CreateSG177_EquipmentDetails(sg177, containerIndex, eqd =>
			{
				eqd.EquipmentTypeCodeQualifier = GetEquipmentTypeCodeQualifier();
				eqd.EquipmentIdentification.EquipmentIdentifier = container.ACN_ContainerNumber;
				eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescriptionCode = EquipmentSizeAndTypeDescriptionCodeList.GetFromString(container.ContainerType?.RC_ISOType);
				eqd.EquipmentStatusCode = GetEquipmentStatusCode();
				eqd.FullOrEmptyIndicatorCode = GetFullOrEmptyIndicatorCode(container);
			});

			CreateSG178_SealNumber(sg177.Group178.InstantiateAChildAndAddItToChildrenCollection(), sel =>
			{
				sel.TransportUnitSealIdentifier = container.ACN_Seal1.IsEmpty ? "NO SEAL NO" : (string)container.ACN_Seal1;
				sel.SealIssuer.SealingPartyNameCode = GetSealingPartyNameCode(container);
			});
		}

		static void CreateEmptySG177(SegmentGroup177 sg177)
		{
			CreateSG177_EquipmentDetails(sg177, 1, eqd =>
			{
				eqd.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.Container;
			});

			CreateSG178_SealNumber(sg177.Group178.InstantiateAChildAndAddItToChildrenCollection());
		}

		static void CreateSG177_EquipmentDetails(SegmentGroup177 sg177, ZInt containerIndex, Action<EQDSegment> eqdSetter = null)
		{
			var eqd = sg177.EQD[0];
			eqdSetter?.Invoke(eqd);

			var seq1 = sg177.SEQ[0];
			seq1.SequenceInformation.SequencePositionIdentifier = containerIndex.ToString();
		}

		static void CreateSG178_SealNumber(SegmentGroup178 sg178, Action<SELSegment> selSetter = null)
		{
			var sel = sg178.SEL[0];
			selSetter?.Invoke(sel);

			var seq2 = sg178.SEQ[0];
			seq2.SequenceInformation.SequencePositionIdentifier = "1";
		}

		void CreateSG186_CargoTypeIndicator(SegmentGroup159 sg159)
		{
			var sg186 = sg159.Group186.InstantiateAChildAndAddItToChildrenCollection();

			var tdt = sg186.TDT[0];
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;

			AddDTM_Ccyymmddhhmm(sg186.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.MovedFromLocationDate, GetGateInOutDate());

			var gds = sg186.GDS[0];
			gds.NatureOfCargo.CargoTypeClassificationCode = GetCargoTypeClassificationCode();
			gds.NatureOfCargo.CodeListIdentificationCode = "ZZZ"; // ZZZ - South African Revenue Service
		}

		ZDateTime GetGateInOutDate()
		{
			if (!source.UseGateInOutDatePerContainer)
			{
				return source.GateInOutDate;
			}
			// todo: rework this part when UI for container selection is added
			var containersWithDate = source.Containers.Cast<AsycudaContainer>().Where(c => !c.GateInOutDate.IsEmpty).ToList();
			return containersWithDate.Count == 0 ? ZDateTime.Empty : containersWithDate.Min(c => c.GateInOutDate);
		}

		void CreateSG267_LineItem(SegmentGroup159 sg159, AsycudaBill bill)
		{
			var sg267 = sg159.Group267.InstantiateAChildAndAddItToChildrenCollection();

			var lin = sg267.LIN[0];
			// bill has only one line (itself)
			lin.LineItemIdentifier = "1";

			var gateInOutMessageType = source.GateInOutMessageType;
			if (gateInOutMessageType != GateInOutMessageTypeCodeList.Codes.TerminalGateOut
				&& gateInOutMessageType != GateInOutMessageTypeCodeList.Codes.TerminalGateIn)
			{
				var mrn = bill.MRN;
				if (!mrn.IsEmpty)
				{
					CreateSG271_MRN(sg267, mrn);
				}
			}
		}

		static void CreateSG271_MRN(SegmentGroup267 sg267, ZString mrn)
		{
			var sg271 = sg267.Group271.InstantiateAChildAndAddItToChildrenCollection();

			var doc = sg271.DOC.InstantiateAChildAndAddItToChildrenCollection();
			doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CustomsDeclarationWithCommercialAndItemDetail;
			doc.DocumentMessageDetails.DocumentIdentifier = mrn;
		}

		void CreateUNS_Summary()
		{
			var uns = edifactMessage.UNS2[0];
			uns.SectionIdentification = "S";
		}

		void CreateUNT_MessageTrailer()
		{
			var unt = edifactMessage.UNT[0];
			unt.NumberOfSegmentsInTheMessage = edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture);
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		SealingPartyNameCodeList GetSealingPartyNameCode(AsycudaContainer container)
		{
			ZString partyType = container.ACN_SealingPartyType;

			if (partyType.IsEmpty)
			{
				return null;
			}

			switch (partyType)
			{
				case "AGT":
					return SealingPartyNameCodeList.Unknown;
				case "CAR":
					return SealingPartyNameCodeList.Carrier;
				case "CUS":
					return SealingPartyNameCodeList.Customs;
				case "EXP":
					return SealingPartyNameCodeList.Shipper;
				case "TOR":
					return SealingPartyNameCodeList.TerminalOperator;
				default:
					return null;
			}
		}

		void AddDTM_Ccyymmdd(DTMSegmentMessageSection dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList dateFunction, ZDateTime date)
		{
			var dtm = dtmSection[dtmSection.Count];
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateFunction;
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
		}

		static void AddDTM_Ccyymmddhhmm(DTMSegmentMessageSection dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList dateFunction, ZDateTime date)
		{
			var dtm = dtmSection[dtmSection.Count];
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateFunction;
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = date.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm;
		}

		EquipmentTypeCodeQualifierList GetEquipmentTypeCodeQualifier()
		{
			var gateInOutMessageType = source.GateInOutMessageType;
			if (gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.DepotGateIn
				|| gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.DepotGateOut
				|| gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateIn
				|| gateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateOut)
			{
				return EquipmentTypeCodeQualifierList.Container;
			}
			return EquipmentTypeCodeQualifierList.UldUnitLoadDevice;
		}

		int GetEquipmentQuantity()
		{
			if (source.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised)
			{
				return source.Containers.Count;
			}
			return source.Bills.Cast<AsycudaBill>().SelectMany(b => b.Packs).Cast<AsycudaPack>().Sum(p => p.APA_PackQty);
		}

		FullOrEmptyIndicatorCodeList GetFullOrEmptyIndicatorCode(AsycudaContainer container)
		{
			var emptyFullIndicator = container.ACN_EmptyFullIndicator;
			switch (emptyFullIndicator)
			{
				case EmptyFullList.Codes.EmptyContainer:
					return FullOrEmptyIndicatorCodeList.Empty;
				case EmptyFullList.Codes.FullContainerLoad:
					return FullOrEmptyIndicatorCodeList.Full;
				case EmptyFullList.Codes.LessThanFullContainerLoad:
					return FullOrEmptyIndicatorCodeList.FullMixedConsignment;
				default:
					return null;
			}
		}

		string GetModeOfTransportCode()
		{
			var transportMode = source.AMA_TransportMode;
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Sea:
					return "1";
				case Core.Constants.TransportModes.Rail:
					return "2";
				case Core.Constants.TransportModes.Road:
					return "3";
				case Core.Constants.TransportModes.Air:
					return "4";
				default:
					return null;
			}
		}

		string GetManifestType()
		{
			var shipmentType = source.AMA_Nature;
			switch (shipmentType)
			{
				case NatureList.Codes.Export22:
					return "22";
				case NatureList.Codes.Import23:
					return "23";
				case NatureList.Codes.Transhipment28:
					return "24";
				case NatureList.Codes.Transit24:
					return "28";
				default:
					return ZString.Empty;
			}
		}

		EquipmentStatusCodeList GetEquipmentStatusCode()
		{
			var shipmentType = source.AMA_Nature;
			switch (shipmentType)
			{
				case NatureList.Codes.Export22:
					return EquipmentStatusCodeList.Export;
				case NatureList.Codes.Import23:
					return EquipmentStatusCodeList.Import;
				case NatureList.Codes.Transhipment28:
					return EquipmentStatusCodeList.Transhipment;
				// NatureList.Codes.Transit24 - is invalid for this property
				default:
					return null;
			}
		}

		CargoTypeClassificationCodeList GetCargoTypeClassificationCode()
		{
			if (source.IsAir)
			{
				return GOVGIOCargoTypeClassificationCodeList.BreakBulk;
			}

			var containerMode = source.AMA_ContainerMode;
			switch (containerMode)
			{
				case Core.Constants.ContainerModes.Bulk:
					return GOVGIOCargoTypeClassificationCodeList.DryBulk;
				case Core.Constants.ContainerModes.Liquid:
					return GOVGIOCargoTypeClassificationCodeList.LiquidBulk;
				case Core.Constants.ContainerModes.BreakBulk:
					return GOVGIOCargoTypeClassificationCodeList.BreakBulk;
				case Core.Constants.ContainerModes.Containerised:
					return GOVGIOCargoTypeClassificationCodeList.Container;
				case Core.Constants.ContainerModes.Other:
					return GOVGIOCargoTypeClassificationCodeList.MixedCargo;
				default:
					return null;
			}
		}

		string UnlocoToIata(ZString unloco)
		{
			return MessageBuilderHelper.UnlocoToIata(Factory, source.IsAir, unloco);
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		readonly GOVCBRMessage edifactMessage;
		readonly AsycudaManifestHeader source;
		readonly MessageSubTypes subType;
	}
}
