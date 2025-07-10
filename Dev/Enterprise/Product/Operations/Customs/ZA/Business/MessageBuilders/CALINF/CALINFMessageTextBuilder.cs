using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CALINF;
using Enterprise.Edifact.D16A.Segments;
using Enterprise.Messaging.Business;
using CALINFMessage = Enterprise.Edifact.D16A.Messages.CALINF.CALINFMessage;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	internal class CALINFMessageTextBuilder
	{
		public CALINFMessageTextBuilder(CALINFMessage edifactMessage, ICALINFMessageDataProvider messageData, MessageSubTypes subType)
		{
			this.edifactMessage = edifactMessage;
			source = messageData;
			this.subType = subType;
		}

		internal void Create()
		{
			PopulateUNH(edifactMessage.UNH[0]);
			PopulateBGM(edifactMessage.BGM[0], source, subType);
			PopulateDTM(edifactMessage.DTM);

			if (subType.IsAmendmentOrCancellation())
			{
				AddNewRFF(edifactMessage.Group2[0].RFF, ReferenceCodeQualifierList.ReferenceNumberToPreviousMessage, source.DocumentToBeAmended);
			}

			PopulateNAD(edifactMessage.Group3, source);

			PopulateTDT(edifactMessage.Group5[0].TDT[0]);
			PopulatedRFF(edifactMessage.Group5[0].RFF);

			group6Index = -1;
			PopulateGroup6(source.Transport.DepartureDetails, LocationFunctionCodeQualifierList.PlaceOfDeparture, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeActual);
			PopulateGroup6(source.Transport.DischargeDetails, LocationFunctionCodeQualifierList.PlaceOfDischarge, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeActual);
			PopulateGroup6(source.Transport.CallDetails, LocationFunctionCodeQualifierList.PortOfCall, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated);

			PopulateUNT(edifactMessage.UNT[0]);
		}

		#region Fields Population

		static void PopulateUNH(UNHSegment unh)
		{
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.MessageIdentifier.MessageType = "CALINF";
			unh.MessageIdentifier.MessageVersionNumber = "D";
			unh.MessageIdentifier.MessageReleaseNumber = "16A";
			unh.MessageIdentifier.ControllingAgency = "UN";
			unh.MessageIdentifier.AssociationAssignedCode = "RCG001";
		}

		static void PopulateBGM(BGMSegment bgm, ICALINFMessageDataProvider source, MessageSubTypes subType)
		{
			bgm.DocumentMessageName.DocumentNameCode = Enterprise.Edifact.D16A.Elements.DocumentNameCodeList.ImpendingArrival;
			bgm.DocumentMessageName.DocumentName = source.CALINFMessageType;
			bgm.DocumentMessageIdentification.DocumentIdentifier = EDIMessage.SystemCommonAccessReferencePkPlaceholder; // "A unique reference number assigned to the electronic document." - treat this as a common access reference - SYS-CAR
			bgm.MessageFunctionCode = MessageFunctionCodeList.GetFromString(subType.GetMessageFunction_D16A());
		}

		void PopulateDTM(DTMSegmentMessageSection dtm)
		{
			AddNewDTM(dtm, source.DocumentIssueDateTime, DateOrTimeOrPeriodFunctionCodeQualifierList.DocumentIssueDateTime, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);
		}

		static void AddNewDTM(DTMSegmentMessageSection dtmSection, ZDateTime dateTime, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimeType, DateOrTimeOrPeriodFormatCodeList dateTimeFormat, string format = "yyyyMMddHHmm")
		{
			if (dateTime.IsValid)
			{
				var dtm = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateTimeType;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = dateTime.ToString(format, CultureInfo.InvariantCulture);
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = dateTimeFormat;
			}
		}

		static void PopulateNAD(SegmentGroup3MessageSection sg3Section, ICALINFMessageDataProvider source)
		{
			AddNewNAD(sg3Section, PartyFunctionCodeQualifierList.DocumentMessageIssuerSender, source.MessageSender);
		}

		static void AddNewNAD(SegmentGroup3MessageSection sg3Section, PartyFunctionCodeQualifierList partyFunctionCodeQualifier, string partyIdentifier)
		{
			var sg3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();
			var nad = sg3.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = partyFunctionCodeQualifier;
			nad.PartyIdentificationDetails.PartyIdentifier = partyIdentifier;
			nad.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		void PopulateTDT(TDTSegment tdt)
		{
			var transport = source.Transport;

			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
			tdt.MeansOfTransportJourneyIdentifier = transport.ConveyanceNumber;
			tdt.ModeOfTransport.TransportModeNameCode = transport.TransportMode;

			tdt.Carrier.CarrierIdentifier = transport.CarrierCode;
			tdt.Carrier.CarrierName = transport.CarrierName;
			tdt.Carrier.CodeListIdentificationCode = "172";
			tdt.Carrier.CodeListResponsibleAgencyCode = transport.CarrierCodeListResponsibleAgencyCode;

			tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier = transport.MeansOfTransportId;
			tdt.TransportIdentification.TransportMeansIdentificationName = transport.MeansOfTransportName;
			tdt.TransportIdentification.TransportMeansNationalityCode = transport.MeansOfTransportNationality;
			tdt.TransportIdentification.CodeListIdentificationCode = transport.MeansOfTransportCodeListIdentificationCode;
		}

		void PopulatedRFF(RFFSegmentMessageSection rffSection)
		{
			AddNewRFF(rffSection, ReferenceCodeQualifierList.PrincipalReferenceNumber, source.Transport.PrincipalCarrierConveyanceNumber);
		}

		static void AddNewRFF(RFFSegmentMessageSection rffSection, ReferenceCodeQualifierList referenceCodeQualifier, string referenceIdentifier, string versionIdentifier = null)
		{
			var rff = rffSection.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = referenceCodeQualifier;
			rff.Reference.ReferenceIdentifier = referenceIdentifier;
			if (versionIdentifier != null)
			{
				rff.Reference.VersionIdentifier = versionIdentifier;
			}
		}

		void PopulateGroup6(List<ICALINFCallInformation> callInfoList, LocationFunctionCodeQualifierList locationQualifier, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimeQualifier)
		{
			foreach (var callInfo in callInfoList)
			{
				var location = callInfo?.CallLocation ?? ZString.Empty;
				var dateTime = callInfo?.CallDateTime ?? ZDateTime.Empty;

				if (!location.IsEmpty && !dateTime.IsEmpty)
				{
					group6Index++;
					PopulateSG6_LOC(edifactMessage.Group5[0].Group6[group6Index].LOC, locationQualifier, callInfo);
					PopulateSG6_DTM(edifactMessage.Group5[0].Group6[group6Index].DTM, dateTimeQualifier, dateTime);
				}
			}
		}

		void PopulateSG6_LOC(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList locationQualifier, ICALINFCallInformation callInfo)
		{
			AddNewLOC(locSection, locationQualifier, callInfo);
		}

		void AddNewLOC(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList locationQualifier, ICALINFCallInformation callInfo)
		{
			var loc = locSection.InstantiateAChildAndAddItToChildrenCollection();
			loc.LocationFunctionCodeQualifier = locationQualifier;
			loc.LocationIdentification.LocationIdentifier = callInfo.CallLocation;
			loc.LocationIdentification.CodeListIdentificationCode = callInfo.LocationCodeListIdentificationCode;
			loc.LocationIdentification.CodeListResponsibleAgencyCode = callInfo.LocationCodeListResponsibleAgencyCode;
		}

		void PopulateSG6_DTM(DTMSegmentMessageSection dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimeQualifier, ZDateTime dateTime)
		{
			AddNewDTM(dtmSection, dateTime, dateTimeQualifier, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);
		}

		void PopulateUNT(UNTSegment unt)
		{
			unt.NumberOfSegmentsInTheMessage = edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture);
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		#endregion

		int group6Index;
		readonly MessageSubTypes subType;
		readonly CALINFMessage edifactMessage;
		readonly ICALINFMessageDataProvider source;
	}
}
