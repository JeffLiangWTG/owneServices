using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.PAXLST;
using Enterprise.ZArchitecture.Core;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class CrewAndPassengersMessageBuilder : EDIFACTMessageBuilder<ICompleteManifest, PAXLSTMessage, EDIMessage>, ICompleteManifestMessageBuilder
	{
		public CrewAndPassengersMessageBuilder(ICompleteManifest data, ZString messageType, MessageSubTypes messageSubType, bool lazyGenerateMessageContent = false)
			: base(data, messageSubType, new UNOACharacterSet())
		{
			factory = data.Factory;
			this.lazyGenerateMessageContent = lazyGenerateMessageContent;
			this.messageType = messageType;
		}

		#region Overrides of EDIFACTMessageBuilder

		protected override EDIMessage PopulateMessagesReturningResult()
		{
			var result = base.PopulateMessagesReturningResult();
			result.EM_MessageType = messageType;
			if (lazyGenerateMessageContent)
			{
				result.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			}
			return result;
		}

		protected override void PopulateEdifactMessage()
		{
			if (!lazyGenerateMessageContent)
			{
				#region PopulateUNH

				var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, "PAXLST", "D", "03B", "UN");
				interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

				#endregion

				#region Trip Reference / Action Code

				var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.PartyInformation, "STANDARD", data.TripReference, MessageActionCode);

				var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
				bgmInterpretation.AddElementInterpretation("Message Type", messageType.ToCodeDescription<MessageTypes>(DocumentNameCodeList.PartyInformation));
				bgmInterpretation.AddElementInterpretation(() => data.TripReference);
				bgmInterpretation.AddElementInterpretation(() => GetMessageSubType().ToCodeDescription<MessageActionCodes>(MessageActionCode));

				#endregion

				#region Transmission Reference Number

				if (!data.TransmissionReferenceNumber.IsEmpty)
				{
					var rff = edifactMessage.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference, data.TransmissionReferenceNumber);
					interpretation.AddNewSegmentInterpretation(rff, () => data.TransmissionReferenceNumber);
				}

				#endregion

				#region Amendment Reason

				if (messageSubType == MessageSubTypes.Change)
				{
					var rff = edifactMessage.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GetFromString("RFA"), data.AmendmentReasonCode);
					interpretation.AddNewSegmentInterpretation(rff, () => data.AmendmentReasonCode);
				}

				#endregion

				#region Method Of Transportation / Carrier Code

				var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var tdt = group2.TDT.InstantiateAChildAndAddItToChildrenCollection();
				var methodOfTransportation = Converter.GetMethodOfTransportationQualifier(data.Factory, data.MethodOfTransportation);
				D08AMessageUtilities.PopulateTDT(
					tdt,
					TransportStageCodeQualifierList.AtBorder,
					methodOfTransportation,
					data.CarrierCode,
					Converter.IdentificationCodes.SCAC);

				var tdtInterpretation = interpretation.AddNewSegmentInterpretation(tdt);
				tdtInterpretation.AddElementInterpretation(() => data.MethodOfTransportation.ToCodeDescription<TransportModes>(methodOfTransportation));
				tdtInterpretation.AddElementInterpretation(() => data.CarrierCode);

				#endregion

				#region Estimated Date Of Arrival

				var dtm = group2.DTM.InstantiateAChildAndAddItToChildrenCollection();
				var estimatedDateOfArrival = data.EstimatedDateOfArrival.Date;
				D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, estimatedDateOfArrival);
				interpretation.AddNewSegmentInterpretation(dtm, () => estimatedDateOfArrival);

				#endregion

				#region Crew / Passengers

				foreach (var crew in data.CrewMembers)
				{
					PopulateCrewMemberOrPassenger(crew);
				}

				#endregion

				#region PopulateUNT

				var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture), unh.MessageReferenceNumber);
				interpretation.AddUNTInterpretation(unt, unt.MessageReferenceNumber);

				#endregion
			}
		}

		#region Populate Crew Member / Passenger

		void PopulateCrewMemberOrPassenger(ICrew crew)
		{
			#region ID / Name / Address

			var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			var nad = group4.NAD.InstantiateAChildAndAddItToChildrenCollection();
			CompleteManifestMessageBuilder.PopulateCrew(data.Factory, nad, interpretation, crew, populateName: true);

			#endregion

			#region Gender

			var att = group4.ATT.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateATT(att, AttributeFunctionCodeQualifierList.Person, crew.Gender);
			interpretation.AddNewSegmentInterpretation(att, () => crew.Gender);

			#endregion

			#region Date Of Birth

			var dtm = group4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.PersonBirthDateTime, crew.DateOfBirth);
			interpretation.AddNewSegmentInterpretation(dtm, () => crew.DateOfBirth);

			#endregion

			#region Hazmat Endorsement

			if (!crew.HazmatEndorsement.IsEmpty)
			{
				var emp = group4.EMP.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateEMP(emp, EmploymentDetailsCodeQualifierList.Profession, crew.HazmatEndorsement);
				interpretation.AddNewSegmentInterpretation(emp, () => crew.HazmatEndorsement);
			}

			#endregion

			#region Citizenship

			var nat = group4.NAT.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateNAT(
				nat,
				NationalityCodeQualifierList.CurrentNationality,
				crew.Citizenship,
				CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization);

			interpretation.AddNewSegmentInterpretation(nat, () => crew.Citizenship);

			#endregion

			#region Travel Documents

			foreach (var travelDocument in crew.TravelDocuments)
			{
				PopulateTravelDocument(group4, travelDocument);
			}

			#endregion
		}

		#endregion

		#region Populate Travel Document

		void PopulateTravelDocument(SegmentGroup4 group4, ITravelDocument document)
		{
			#region Type / Number

			var group5 = group4.Group5.InstantiateAChildAndAddItToChildrenCollection();
			var doc = group5.DOC.InstantiateAChildAndAddItToChildrenCollection();
			var code = Converter.GetTravelDocumentCodeFromType(factory, document.TravelDocumentType);
			D08AMessageUtilities.PopulateDOC(doc, code, null, document.TravelDocumentNumber);

			var docInterpretation = interpretation.AddNewSegmentInterpretation(doc);
			docInterpretation.AddElementInterpretation(() => document.TravelDocumentType.ToCodeDescription<TravelDocumentTypes>(code));
			docInterpretation.AddElementInterpretation(() => document.TravelDocumentNumber);

			#endregion

			#region State Or Province Of Issuance

			if (!document.StateOrProvinceOfIssuance.IsEmpty)
			{
				var loc = group5.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.PlaceOfDocumentIssue,
					document.StateOrProvinceOfIssuance,
					Converter.IdentificationCodes.CountrySubEntity);

				interpretation.AddNewSegmentInterpretation(loc, () => document.StateOrProvinceOfIssuance);
			}

			#endregion

			#region Country Of Issuance

			if (!document.CountryOfIssuance.IsEmpty)
			{
				var loc = group5.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.PlaceOfDocumentIssue,
					document.CountryOfIssuance,
					Converter.IdentificationCodes.Country);

				interpretation.AddNewSegmentInterpretation(loc, () => document.CountryOfIssuance);
			}

			#endregion
		}

		#endregion

		#region Action Code

		MessageFunctionCodeList MessageActionCode
		{
			get
			{
				MessageFunctionCodeList result = null;
				switch (messageSubType)
				{
					case MessageSubTypes.Create:
						result = MessageFunctionCodeList.Addition;
						break;
					case MessageSubTypes.Withdraw:
						result = MessageFunctionCodeList.Deletion;
						break;
					case MessageSubTypes.Change:
						result = MessageFunctionCodeList.Change;
						break;
				}
				return result;
			}
		}

		#endregion

		#endregion

		void ICompleteManifestMessageBuilder.GenerateMessageContent(Enterprise.Messaging.Business.EDIMessage message)
		{
			this.GenerateMessageContent(PopulateEdifactMessage, () => edifactMessage.ToString(characterSet), () => interpretation.ToHtml(), message);
		}

		readonly BusinessObjectFactory factory;
		readonly ZBool lazyGenerateMessageContent;
		readonly ZString messageType;
	}
}
