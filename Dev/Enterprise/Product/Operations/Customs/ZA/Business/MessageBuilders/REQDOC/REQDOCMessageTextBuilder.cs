using System.Globalization;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.REQDOC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public static class REQDOCMessageTextBuilder
	{
		public static void PopulateREQDOCMessage(REQDOCMessage message, IREQDOCMessageDataProvider source)
		{
			PopulateUNHSegment(message.UNH[0]);
			PopulateBGMSegment(message.BGM[0], source);
			PopulateDOCSegment(message.DOC[0], source);
			PopulateDTMSegments(message.DTM, source);
			PopulateSG2Groups(message.Group2, source);
			PopulateSG4Groups(message.Group4);
			PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT);
		}

		#region Fields Population

		static void PopulateUNHSegment(UNHSegment unhSegment)
		{
			unhSegment.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unhSegment.MessageIdentifier.MessageType = Edifact.D96B.Elements.MessageTypeList.RequestForDocumentMessage;
			unhSegment.MessageIdentifier.MessageVersionNumber = "D";
			unhSegment.MessageIdentifier.MessageReleaseNumber = "99B";
			unhSegment.MessageIdentifier.ControllingAgency = Edifact.D96B.Elements.ControllingAgencyList.UnEceTradeWp4;
			unhSegment.MessageIdentifier.AssociationAssignedCode = "ZZZ01";
		}

		static void PopulateBGMSegment(BGMSegment bgmSegment, IREQDOCMessageDataProvider source)
		{
			bgmSegment.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString(source.MessageType);
			bgmSegment.DocumentMessageIdentification.DocumentMessageNumber = source.MessageType == DocumentNameCodeList.StatementOfAccountMessage ? source.SenderReference : source.LocalReferenceNumber;
			bgmSegment.MessageFunctionCode = MessageFunctionCodeList.GetFromString(source.MessageFunction);
		}

		static void PopulateDOCSegment(DOCSegment docSegment, IREQDOCMessageDataProvider source)
		{
			docSegment.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString(source.MessageTypeForDoc);
			docSegment.DocumentMessageDetails.DocumentMessageNumber = source.MessageType == DocumentNameCodeList.StatementOfAccountMessage ? source.FinancialAccountNumber : source.FinalMRN;
			var documentMessageSource = source.DocumentMessageSource;
			if (!documentMessageSource.IsEmpty)
			{
				docSegment.DocumentMessageDetails.DocumentMessageSource = documentMessageSource;
			}
			var manifestDocumentType = source.ManifestDocumentType;
			if (!manifestDocumentType.IsEmpty)
			{
				docSegment.DocumentMessageDetails.Version = manifestDocumentType;
			}
		}

		static void PopulateDTMSegments(DTMSegmentMessageSection dtmSection, IREQDOCMessageDataProvider source)
		{
			AddNewDTMSegment(dtmSection, source.RequestDate, DateTimePeriodFunctionCodeQualifierList.RequestDate, DateTimePeriodFormatCodeList.Ccyymmdd);
			AddNewDTMSegment(dtmSection, source.StartDate, DateTimePeriodFunctionCodeQualifierList.ReportStartDate, DateTimePeriodFormatCodeList.Ccyymmdd);
			AddNewDTMSegment(dtmSection, source.EndDate, DateTimePeriodFunctionCodeQualifierList.ReportEndDate, DateTimePeriodFormatCodeList.Ccyymmdd);
		}

		static void AddNewDTMSegment(DTMSegmentMessageSection dtmSection, ZDateTime dateTime, DateTimePeriodFunctionCodeQualifierList dateTimeType, DateTimePeriodFormatCodeList dateTimeFormat)
		{
			if (dateTime.IsValid)
			{
				var dtmSegment = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
				dtmSegment.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = dateTimeType;
				dtmSegment.DateTimePeriod.DateTimePeriodValue = dateTime.ToCCYYMMDD();
				dtmSegment.DateTimePeriod.DateTimePeriodFormatCode = dateTimeFormat;
			}
		}

		static void PopulateSG2Groups(SegmentGroup2MessageSection sg2Section, IREQDOCMessageDataProvider source)
		{
			AddNewSG2GroupWithCodeOnly(sg2Section, PartyFunctionCodeQualifierList.DocumentMessageIssuerSender, source.MessageSender, 35); //NAD+MS
			AddNewSG2GroupWithCodeOnly(sg2Section, PartyFunctionCodeQualifierList.Customs, source.ReleaseAuthority, 35); //NAD+CM
		}

		static void AddNewSG2GroupWithCodeOnly(SegmentGroup2MessageSection sg2Section, PartyFunctionCodeQualifierList addressType, ZString addressCode, int addressCodeSize)
		{
			if (addressType != null && !addressCode.IsEmpty)
			{
				var sg2 = sg2Section.InstantiateAChildAndAddItToChildrenCollection();
				AddNewNADSegmentWithCodeOnly(sg2.NAD, addressType, addressCode, addressCodeSize);
			}
		}

		static NADSegment AddNewNADSegmentWithCodeOnly(NADSegmentMessageSection nadSection, PartyFunctionCodeQualifierList partyQualifier, ZString addressCode, int addressCodeSize)
		{
			var result = nadSection.InstantiateAChildAndAddItToChildrenCollection();
			result.PartyFunctionCodeQualifier = partyQualifier;
			result.PartyIdentificationDetails.PartyIdentifier = addressCode.Left(addressCodeSize);
			return result;
		}

		static void PopulateSG4Groups(SegmentGroup4MessageSection sg4Section)
		{
			AddNewSG4Group(sg4Section);
		}

		static void AddNewSG4Group(SegmentGroup4MessageSection group4)
		{
			var sg4 = group4.InstantiateAChildAndAddItToChildrenCollection();
			var linSegment = sg4.LIN[0];
			linSegment.LineItemNumber = "1";
		}

		static void PopulateUNTSegment(UNTSegment untSegment, int segmentCount)
		{
			untSegment.NumberOfSegmentsInTheMessage = segmentCount.ToString(CultureInfo.InvariantCulture);
			untSegment.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		#endregion
	}
}
