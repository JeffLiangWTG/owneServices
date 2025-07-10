using System;
using System.Globalization;
using Enterprise.Edifact;
using Enterprise.Edifact.D98B.Elements;
using Enterprise.Edifact.D98B.Messages.IFCSUM;
using Enterprise.Edifact.D98B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class EIDOEdifactMessageBuilder : IEIDOMessageBuilder
	{
		public string GenerateMessageText(IEIDOMessagingData data)
		{
			return GenerateMessage(data).ToString(CharacterSet);
		}

		#region Generate Segment Group

		static IFCSUMMessage GenerateMessage(IEIDOMessagingData data)
		{
			IFCSUMMessage message = new IFCSUMMessage();

			UNHSegment uNH = message.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			uNH.MessageIdentifier.MessageType = MessageTypeList.ForwardingAndConsolidationSummaryMessage;
			uNH.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.DraftVersionUnEdifactDirectory;
			uNH.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.Release1998B;
			uNH.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnEceTradeWp4;
			uNH.MessageIdentifier.AssociationAssignedCode = "ANZ20";

			BGMSegment bGM = message.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.DeliveryOrder;
			bGM.DocumentMessageIdentification.DocumentMessageNumber = EDIMessage.MessageNumberPlaceHolder;
			bGM.MessageFunctionCoded = GetMessageFuction(data.MessageFunction);
			bGM.ResponseTypeCoded = ResponseTypeCodedList.MessageAcknowledgement;

			GenerateDTMLong(message.DTM, DateTimePeriodQualifierList.DocumentMessageDateTime, data.MessagePrepared);
			GenerateGroup4(message.Group4, data);
			GenerateGroup8(message.Group8, data);
			GenerateGroup19(message.Group19, data);

			UNTSegment uNT = message.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			uNT.NumberOfSegmentsInTheMessage = message.CountIncludingUNT.ToString(CultureInfo.InvariantCulture);

			return message;
		}

		/// <summary>
		/// Segment Group 4 - Name And Address
		/// </summary>
		static void GenerateGroup4(SegmentGroup4MessageSection section, IEIDOMessagingData data)
		{
			SegmentGroup4 group = section.InstantiateAChildAndAddItToChildrenCollection();

			NADSegment password = group.NAD.InstantiateAChildAndAddItToChildrenCollection();
			password.PartyQualifier = PartyQualifierList.AuthorizingOfficial;
			password.PartyIdentificationDetails.PartyIdentification = Trim(data.Password, 35);

			GenerateNAD(group.NAD, PartyQualifierList.MessageRecipient, data.MessageRecipient);
			GenerateNAD(group.NAD, PartyQualifierList.DocumentMessageIssuerSender, data.MessageSender);
		}

		/// <summary>
		/// Segment Group 8 - Details Of Transport
		/// </summary>
		static void GenerateGroup8(SegmentGroup8MessageSection section, IEIDOMessagingData data)
		{
			SegmentGroup8 group = section.InstantiateAChildAndAddItToChildrenCollection();

			TDTSegment tDT = group.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tDT.TransportStageQualifier = TransportStageQualifierList.MainCarriageTransport;
			tDT.ConveyanceReferenceNumber = Trim(data.Voyage, 17);
			tDT.ModeOfTransport.ModeOfTransportCoded = "1";

			string acos = data.CarrierACOS;
			if (!string.IsNullOrEmpty(acos))
			{
				tDT.Carrier.CarrierIdentification = Trim(acos, 17);
				tDT.Carrier.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.AuAcosAustralianChamberOfShipping;
			}

			tDT.Carrier.CarrierName = Trim(data.CarrierName, 35);
			tDT.TransportIdentification.IdOfMeansOfTransportIdentification = Trim(data.VesselLloyds, 9);
			tDT.TransportIdentification.IdOfTheMeansOfTransport = Trim(data.VesselName, 35);

			GenerateLOC(group.LOC, PlaceLocationQualifierList.PlacePortOfDischarge, data.DischargePort);

			GenerateDTMShort(group.DTM, DateTimePeriodQualifierList.ArrivalDateTimeEstimated, data.EstimatedArrivalDate);
			GenerateGroup12(group.Group12, data);
		}

		/// <summary>
		/// Segment Group 12 - Name And Address
		/// </summary>
		static void GenerateGroup12(SegmentGroup12MessageSection section, IEIDOMessagingData data)
		{
			SegmentGroup12 group = section.InstantiateAChildAndAddItToChildrenCollection();

			GenerateNAD(group.NAD, PartyQualifierList.Carrier, data.Issuer);
			GenerateNAD(group.NAD, PartyQualifierList.ShipFrom, data.CargoCollection);
		}

		/// <summary>
		/// Segment Group 19 - Consignment Information
		/// </summary>
		static void GenerateGroup19(SegmentGroup19MessageSection section, IEIDOMessagingData data)
		{
			SegmentGroup19 group = section.InstantiateAChildAndAddItToChildrenCollection();

			GenerateCNI(group.CNI, "1", data.ReferenceNumber);
			GenerateGroup26(group.Group26, data);

			foreach (IEIDOEquiptmentData equiptment in data.Equipment)
			{
				GenerateGroup60(group.Group60, equiptment);
			}
		}

		/// <summary>
		/// Segment Group 26 - References
		/// </summary>
		static void GenerateGroup26(SegmentGroup26MessageSection section, IEIDOMessagingData data)
		{
			SegmentGroup26 group = section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateRFF(group.RFF, ReferenceQualifierList.DeliveryOrderNumber, data.PIN);
			GenerateRFF(group.RFF, ReferenceQualifierList.BillOfLadingNumber, data.BillOfLading);
		}

		/// <summary>
		/// Segment Group 60 - Equiptment Details
		/// </summary>
		static void GenerateGroup60(SegmentGroup60MessageSection section, IEIDOEquiptmentData equiptment)
		{
			SegmentGroup60 group = section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateEQD(group.EQD, equiptment.ContainerNumber, equiptment.ContainerISOCode, equiptment.IsEmpty);
			GenerateMEA(group.MEA, PropertyMeasuredCodedList.GrossWeight, "KGM", equiptment.GrossKilograms);

			foreach (string seal in equiptment.SealNumbers)
			{
				GenerateSEL(group.SEL, seal);
			}

			GenerateHAN(group.HAN, equiptment.IMDGClassCode, equiptment.IMDGClass, equiptment.HandlingInstructions);
			GenerateFTX(group.FTX, TextSubjectQualifierList.GoodsDescription, equiptment.GoodsDescription);
			GenerateGroup62(group.Group62, equiptment);
		}

		/// <summary>
		/// Segment Group 62 - Name And Address
		/// </summary>
		static void GenerateGroup62(SegmentGroup62MessageSection section, IEIDOEquiptmentData equiptment)
		{
			IEIDOOrganisation emptyReturn = equiptment.EmptyReturn;

			if (emptyReturn != null)
			{
				SegmentGroup62 group = section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateNAD(group.NAD, PartyQualifierList.EmptyEquipmentReturnParty, emptyReturn);
				GenerateDTMShort(group.DTM, DateTimePeriodQualifierList.EquipmentPositioningDateTimeUltimate, equiptment.EmptyReturnBy);
			}
		}

		#endregion

		#region Generate Segment

		/// <summary>
		/// CNI - Consignment Information
		/// </summary>
		static void GenerateCNI(CNISegmentMessageSection section, string consignmentNumber, string referenceNumber)
		{
			CNISegment cNI = section.InstantiateAChildAndAddItToChildrenCollection();
			cNI.ConsolidationItemNumber = consignmentNumber;
			cNI.DocumentMessageDetails.DocumentMessageNumber = referenceNumber;
		}

		/// <summary>
		/// DTM - Date Time (Ccyymmddhhmm)
		/// </summary>
		static void GenerateDTMLong(DTMSegmentMessageSection section, DateTimePeriodQualifierList periodQualifier, DateTime value)
		{
			DTMSegment dTM = section.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodQualifier = periodQualifier;
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmddhhmm;
			dTM.DateTimePeriod.DateTimePeriod = value.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// DTM - Date Time (Ccyymmdd)
		/// </summary>
		static void GenerateDTMShort(DTMSegmentMessageSection section, DateTimePeriodQualifierList periodQualifier, DateTime? value)
		{
			if (value.HasValue)
			{
				GenerateDTMShort(section, periodQualifier, value.Value);
			}
		}

		/// <summary>
		/// DTM - Date Time (Ccyymmdd)
		/// </summary>
		static void GenerateDTMShort(DTMSegmentMessageSection section, DateTimePeriodQualifierList periodQualifier, DateTime value)
		{
			DTMSegment dTM = section.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodQualifier = periodQualifier;
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;
			dTM.DateTimePeriod.DateTimePeriod = value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// EQD - Equiptment Details
		/// </summary>
		static void GenerateEQD(EQDSegmentMessageSection section, string containerNumber, string isoCode, bool isEmpty)
		{
			EQDSegment eQD = section.InstantiateAChildAndAddItToChildrenCollection();
			eQD.EquipmentQualifier = EquipmentQualifierList.Container;
			eQD.EquipmentIdentification.EquipmentIdentificationNumber = Trim(containerNumber, 17);
			eQD.EquipmentSizeAndType.EquipmentSizeAndTypeIdentification = EquipmentSizeAndTypeIdentificationList.GetFromString(Trim(isoCode, 10));
			eQD.FullEmptyIndicatorCoded = isEmpty ? FullEmptyIndicatorCodedList.Empty : FullEmptyIndicatorCodedList.Full;
		}

		/// <summary>
		/// FTX - Free Text
		/// </summary>
		static void GenerateFTX(FTXSegmentMessageSection section, TextSubjectQualifierList subjectQualifier, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				TextSplitter splitter = new TextSplitter(70);
				splitter.Text = value;

				FTXSegment fTX = section.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = subjectQualifier;
				fTX.TextLiteral.FreeText1 = splitter[0];
				fTX.TextLiteral.FreeText2 = splitter[1];
				fTX.TextLiteral.FreeText3 = splitter[2];
				fTX.TextLiteral.FreeText4 = splitter[3];
				fTX.TextLiteral.FreeText5 = splitter[4];
			}
		}

		/// <summary>
		/// HAN - Handling Instructions
		/// </summary>
		static void GenerateHAN(HANSegmentMessageSection section, string imdgCode, string imdgClass, string handlingInstructions)
		{
			if (!string.IsNullOrEmpty(imdgClass) || !string.IsNullOrEmpty(imdgCode))
			{
				HANSegment hAN = section.InstantiateAChildAndAddItToChildrenCollection();
				hAN.HandlingInstructions.HandlingInstructionsCoded = "HAZ";
				hAN.HandlingInstructions.HandlingInstructions = Trim(handlingInstructions, 70);
				hAN.HazardousMaterial.HazardousMaterialClassCodeIdentification = Trim(imdgCode, 4);
				hAN.HazardousMaterial.HazardousMaterialClass = Trim(imdgClass, 35);
			}
			else if (!string.IsNullOrEmpty(handlingInstructions))
			{
				HANSegment hAN = section.InstantiateAChildAndAddItToChildrenCollection();
				hAN.HandlingInstructions.HandlingInstructionsCoded = "GEN";
				hAN.HandlingInstructions.HandlingInstructions = handlingInstructions;
			}
		}

		/// <summary>
		/// LOC - Place/Location Identification
		/// </summary>
		static void GenerateLOC(LOCSegmentMessageSection section, PlaceLocationQualifierList qualifier, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				LOCSegment lOC = section.InstantiateAChildAndAddItToChildrenCollection();
				lOC.PlaceLocationQualifier = qualifier;
				lOC.LocationIdentification.PlaceLocationIdentification = Trim(value, 25);
			}
		}

		/// <summary>
		/// MEA - Measurements
		/// </summary>
		static void GenerateMEA(MEASegmentMessageSection section, PropertyMeasuredCodedList property, string unit, decimal value)
		{
			if (value > 0)
			{
				MEASegment mEA = section.InstantiateAChildAndAddItToChildrenCollection();
				mEA.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.Measurement;
				mEA.MeasurementDetails.PropertyMeasuredCoded = property;
				mEA.ValueRange.MeasureUnitQualifier = unit;
				mEA.ValueRange.MeasurementValue = value.ToString("0.###", CultureInfo.InvariantCulture);
			}
		}

		/// <summary>
		/// NAD - Name and Address
		/// </summary>
		static void GenerateNAD(NADSegmentMessageSection section, PartyQualifierList qualifier, IEIDOOrganisation organisation)
		{
			if (organisation != null)
			{
				string acosCode = organisation.AcosCode;
				string nameAndAddress = organisation.NameAndAddress;

				if (!string.IsNullOrEmpty(acosCode) || !string.IsNullOrEmpty(nameAndAddress))
				{
					TextSplitter splitter = new TextSplitter(35);
					splitter.Text = nameAndAddress;

					NADSegment nAD = section.InstantiateAChildAndAddItToChildrenCollection();
					nAD.PartyQualifier = qualifier;

					if (!string.IsNullOrEmpty(acosCode))
					{
						nAD.PartyIdentificationDetails.PartyIdentification = Trim(acosCode, 35);
						nAD.PartyIdentificationDetails.CodeListQualifier = CodeListQualifierList.PartyIdentification;
						nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.AuAcosAustralianChamberOfShipping;
					}

					nAD.NameAndAddress.NameAndAddressLine1 = splitter[0];
					nAD.NameAndAddress.NameAndAddressLine2 = splitter[1];
					nAD.NameAndAddress.NameAndAddressLine3 = splitter[2];
					nAD.NameAndAddress.NameAndAddressLine4 = splitter[3];
					nAD.NameAndAddress.NameAndAddressLine5 = splitter[4];
				}
			}
		}

		/// <summary>
		/// RFF - Reference
		/// </summary>
		static void GenerateRFF(RFFSegmentMessageSection section, ReferenceQualifierList referenceQualifier, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				RFFSegment rFF = section.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceQualifier = referenceQualifier;
				rFF.Reference.ReferenceNumber = Trim(value, 35);
			}
		}

		/// <summary>
		/// SEL - Seal Number
		/// </summary>
		static void GenerateSEL(SELSegmentMessageSection section, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				SELSegment sEL = section.InstantiateAChildAndAddItToChildrenCollection();
				sEL.SealNumber = Trim(value, 10);
			}
		}

		#endregion

		#region Implementation

		static string Trim(string inString, int length)
		{
			if (inString == null)
			{
				return null;
			}
			else if (inString.Length <= length)
			{
				return inString;
			}
			else
			{
				return inString.Substring(0, length);
			}
		}

		static MessageFunctionCodedList GetMessageFuction(EIDOMessageFunction function)
		{
			switch (function)
			{
				case EIDOMessageFunction.Original:
					return MessageFunctionCodedList.Original;
				case EIDOMessageFunction.Cancelation:
					return MessageFunctionCodedList.Cancellation;
				default:
					return null;
			}
		}

		static UNCharacterSet CharacterSet
		{
			get { return new UNOACharacterSet(); }
		}

		#endregion
	}
}


