using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.MEDPID;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.ZArchitecture.Core;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class CrewOrEquipmentRegistrationMessageBuilder : EDIFACTMessageBuilder<ICrewOrEquipmentRegistration, MEDPIDMessage, EDIMessage>, ICompleteManifestMessageBuilder
	{
		public CrewOrEquipmentRegistrationMessageBuilder(ICrewOrEquipmentRegistration data, MessageSubTypes messageSubType, bool lazyGenerateMessageContent)
			: base(data, messageSubType, new UNOACharacterSet())
		{
			factory = data.Factory;
			this.lazyGenerateMessageContent = lazyGenerateMessageContent;
			messageType = MessageTypes.Codes.CrewOrEquipmentRegistration;
		}

		#region Overrides of EDIFACTMessageBuilder

		public override IMessageBuilderResult PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			foreach (var crewmember in data.CrewMembers.Where(c => c.CrewId.IsEmpty))
			{
				this.crewMember = crewmember;
				AddMessageToResult(messageBuilderResult);
			}
			return messageBuilderResult;
		}

		void AddMessageToResult(MessageBuilderResult messageBuilderResult)
		{
			edifactMessage = new MEDPIDMessage();
			interpretation = new MessageInterpretation(edifactMessage, characterSet);
			var builderResult = new BuilderResult(null, System.Array.Empty<string>(), null) { Message = PopulateMessagesReturningResult() };
			builderResult.Message.EM_MessageType = messageType;
			if (lazyGenerateMessageContent)
			{
				builderResult.Message.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			}
			messageBuilderResult.AddBuilderResult(builderResult);
		}

		protected override void PopulateEdifactMessage()
		{
			if (!lazyGenerateMessageContent)
			{
				#region PopulateUNH

				var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, "MEDPID", "D", "02A", "UN");
				interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

				#endregion

				#region Mandatory Trigger Segment

				var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.GetFromString(string.Empty), string.Empty, string.Empty, MessageFunctionCodeList.Original);
				interpretation.AddNewSegmentInterpretation(bgm, () => messageType.ToCodeDescription<MessageTypes>());

				#endregion

				#region Originator Full Name / Phone

				var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
				var pna = group1.PNA.InstantiateAChildAndAddItToChildrenCollection();

				D08AMessageUtilities.PopulatePNA(
					pna,
					PartyFunctionCodeQualifierList.CurrentSender,
					string.Format(CultureInfo.InvariantCulture, "{0}, {1}", data.OriginatorFullName, data.OriginatorPhone), NameComponentTypeCodeQualifierList.OfficialSecondChristianName);

				var pnaInterpretation = interpretation.AddNewSegmentInterpretation(pna);
				pnaInterpretation.AddElementInterpretation(() => data.OriginatorFullName);
				pnaInterpretation.AddElementInterpretation(() => data.OriginatorPhone);

				#endregion

				#region Conveyance / Equipment / Crew

				if (conveyance != null)
				{
					PopulateConveyance(conveyance);
				}
				else if (equipment != null)
				{
					PopulateEquipment(equipment);
				}
				else if (crewMember != null)
				{
					PopulateCrewMember(crewMember);
				}

				#endregion

				#region PopulateUNT

				var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture), unh.MessageReferenceNumber);
				interpretation.AddUNTInterpretation(unt, unt.MessageReferenceNumber);

				#endregion
			}
		}

		#region PopulateConveyance

		void PopulateConveyance(IConveyance iconveyance)
		{
			//TODO: How do we send DOT number conveyance.DepartmentOfTransportationNumber?
			var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			PopulateActionAndCarrierCodes(group2);

			#region Conveyance Type

			var ihc = group2.IHC.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateIHC(ihc, PersonCharacteristicCodeQualifierList.SkinColour, iconveyance.EquipmentType);
			interpretation.AddNewSegmentInterpretation(ihc, "Conveyance Type", iconveyance.EquipmentType.ToCodeDescription<ConveyanceTypes>());

			#endregion

			#region ACE Id

			if (!iconveyance.ConveyanceACEId.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, iconveyance.ConveyanceACEId);
				interpretation.AddNewSegmentInterpretation(rff, () => iconveyance.ConveyanceACEId);
			}

			#endregion

			#region Conveyance Id

			if (!iconveyance.ConveyanceId.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.TransportMeansJourneyIdentifier, iconveyance.ConveyanceId);
				interpretation.AddNewSegmentInterpretation(rff, () => iconveyance.ConveyanceId);
			}

			#endregion

			#region Vehicle Identification Number (VIN)

			if (!iconveyance.EquipmentId.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.VehicleIdentificationNumberVin, iconveyance.EquipmentId);
				interpretation.AddNewSegmentInterpretation(rff, "Vehicle Identification Number (VIN)", iconveyance.EquipmentId);
			}

			#endregion

			#region Transponder Id

			if (!iconveyance.TransponderId.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.TransactionReferenceNumber, iconveyance.TransponderId);
				interpretation.AddNewSegmentInterpretation(rff, () => iconveyance.TransponderId);
			}

			#endregion

			#region License Plates

			foreach (var licensePlate in iconveyance.LicensePlates)
			{
				PopulateLicencePlate(group2, licensePlate);
			}

			#endregion
		}

		#endregion

		#region Populate Equipment

		void PopulateEquipment(IEquipment iequipment)
		{
			var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			PopulateActionAndCarrierCodes(group2);

			#region Equipment Type

			var ihc = group2.IHC.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateIHC(ihc, PersonCharacteristicCodeQualifierList.SkinColour, iequipment.EquipmentType);
			interpretation.AddNewSegmentInterpretation(ihc, () => iequipment.EquipmentType.ToCodeDescription<EquipmentTypes>());

			#endregion

			#region ACE Id

			if (!iequipment.EquipmentACEId.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, iequipment.EquipmentACEId);
				interpretation.AddNewSegmentInterpretation(rff, () => iequipment.EquipmentACEId);
			}

			#endregion

			#region Equipment Id

			if (!iequipment.EquipmentId.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.EquipmentNumber, iequipment.EquipmentId);
				interpretation.AddNewSegmentInterpretation(rff, () => iequipment.EquipmentId);
			}

			#endregion

			#region License Plates

			foreach (var licensePlate in iequipment.LicensePlates)
			{
				PopulateLicencePlate(group2, licensePlate);
			}

			#endregion
		}

		#endregion

		#region Populate Licence Plate

		void PopulateLicencePlate(SegmentGroup2 group2, ILicensePlate licensePlate)
		{
			#region License Plate Number

			var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.VehicleLicenceNumber, licensePlate.LicensePlateNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => licensePlate.LicensePlateNumber);

			#endregion

			#region Country / State / Province Of Registration

			if (!licensePlate.CountryOfRegistration.IsEmpty)
			{
				var loc = group2.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.MutuallyDefined,
					licensePlate.CountryOfRegistration,
					Converter.IdentificationCodes.Country,
					licensePlate.StateOrProvinceOfRegistration,
					Converter.IdentificationCodes.RegionGeographicLocation);

				var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
				locInterpretation.AddElementInterpretation("Country Of License Plate Registration", licensePlate.CountryOfRegistration);
				locInterpretation.AddElementInterpretationIfNotEmpty("State Or Province Of License Plate Registration", licensePlate.StateOrProvinceOfRegistration);
			}

			#endregion
		}

		#endregion

		#region Populate Crew Member

		void PopulateCrewMember(ICrew crew)
		{
			var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			PopulateActionAndCarrierCodes(group2);

			#region First / Last / Middle Name

			var pna = group2.PNA.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulatePNA(
				pna,
				PartyFunctionCodeQualifierList.CrewMember,
				crew.FirstName,
				NameComponentTypeCodeQualifierList.ChristianName,
				crew.MiddleName,
				NameComponentTypeCodeQualifierList.OfficialSecondChristianName,
				crew.LastName,
				NameComponentTypeCodeQualifierList.Surname);

			var pnaInterpretation = interpretation.AddNewSegmentInterpretation(pna);
			pnaInterpretation.AddElementInterpretation(() => crew.LastName);
			pnaInterpretation.AddElementInterpretation(() => crew.FirstName);
			pnaInterpretation.AddElementInterpretationIfNotEmpty(() => crew.MiddleName);

			#endregion

			#region Date Of Birth

			var dtm = group2.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.PersonBirthDateTime, crew.DateOfBirth, invertedFormat: true);
			interpretation.AddNewSegmentInterpretation(dtm, () => crew.DateOfBirth);

			#endregion

			#region Citizenship

			var nat = group2.NAT.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateNAT(nat, NationalityCodeQualifierList.CurrentNationality, crew.Citizenship);
			interpretation.AddNewSegmentInterpretation(nat, () => crew.Citizenship);

			#endregion

			#region Gender

			var group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
			var pdi = group3.PDI.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulatePDI(pdi, crew.Gender);
			interpretation.AddNewSegmentInterpretation(pdi, () => crew.Gender);

			#endregion

			#region ACE Id

			if (!crew.CrewId.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, crew.CrewId);
				interpretation.AddNewSegmentInterpretation(rff, () => crew.CrewId);
			}

			#endregion

			#region Travel Documents

			foreach (var travelDocument in crew.TravelDocuments)
			{
				PopulateTravelDocument(group2, travelDocument);
			}

			#endregion

			#region Hazmat Endorsement

			if (!crew.HazmatEndorsement.IsEmpty)
			{
				var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
				var qualifier = Converter.GetTravelDocumentQualifierFromType(factory, TravelDocumentTypes.Codes.HazmatEndorsement);
				D08AMessageUtilities.PopulateRFF(rff, qualifier, crew.HazmatEndorsement);
				interpretation.AddNewSegmentInterpretation(rff, () => crew.HazmatEndorsement);
			}

			#endregion
		}

		#endregion

		#region Populate Travel Document

		void PopulateTravelDocument(SegmentGroup2 group2, ITravelDocument document)
		{
			#region Type / Number

			var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
			var qualifier = Converter.GetTravelDocumentQualifierFromType(factory, document.TravelDocumentType);
			var country = document.TravelDocumentType == TravelDocumentTypes.Codes.Passport ? document.CountryOfIssuance : ZString.Empty;
			D08AMessageUtilities.PopulateRFF(rff, qualifier, country + document.TravelDocumentNumber);

			var rffInterpretation = interpretation.AddNewSegmentInterpretation(rff);
			rffInterpretation.AddElementInterpretation(() => document.TravelDocumentType.ToCodeDescription<TravelDocumentTypes>(qualifier));
			if (document.TravelDocumentType == TravelDocumentTypes.Codes.Passport)
			{
				rffInterpretation.AddElementInterpretation(() => document.CountryOfIssuance);
			}
			rffInterpretation.AddElementInterpretation(() => document.TravelDocumentNumber);

			#endregion

			#region Country / State Or Province Of Issuance

			switch (document.TravelDocumentType)
			{
				case TravelDocumentTypes.Codes.CommercialDriversLicense:
				case TravelDocumentTypes.Codes.EnhancedDriversLicense:
				case TravelDocumentTypes.Codes.DrivingLicenseNational:
					if (!document.CountryOfIssuance.IsEmpty)
					{
						var loc = group2.LOC.InstantiateAChildAndAddItToChildrenCollection();
						D08AMessageUtilities.PopulateLOC(
							loc,
							LocationFunctionCodeQualifierList.MutuallyDefined,
							string.Empty,
							string.Empty,
							string.Empty,
							string.Empty,
							document.CountryOfIssuance,
							Converter.IdentificationCodes.Country);

						interpretation.AddNewSegmentInterpretation(loc, "Driver License  Country Of Issuance", document.CountryOfIssuance);
					}

					if (!document.StateOrProvinceOfIssuance.IsEmpty)
					{
						var loc = group2.LOC.InstantiateAChildAndAddItToChildrenCollection();
						D08AMessageUtilities.PopulateLOC(
							loc,
							LocationFunctionCodeQualifierList.MutuallyDefined,
							string.Empty,
							string.Empty,
							string.Empty,
							string.Empty,
							document.StateOrProvinceOfIssuance,
							Converter.IdentificationCodes.StateOfDriverLicense);

						interpretation.AddNewSegmentInterpretation(loc, "Driver License State Or Province Of Issuance", document.StateOrProvinceOfIssuance);
					}
					break;
				case TravelDocumentTypes.Codes.PermanentResidentCard1:
				case TravelDocumentTypes.Codes.PermanentResidentCard2:
					if (!document.CountryOfIssuance.IsEmpty)
					{
						var loc = group2.LOC.InstantiateAChildAndAddItToChildrenCollection();
						D08AMessageUtilities.PopulateLOC(
							loc,
							LocationFunctionCodeQualifierList.MutuallyDefined,
							document.CountryOfIssuance,
							Converter.IdentificationCodes.CountrySubEntity,
							string.Empty,
							string.Empty);

						interpretation.AddNewSegmentInterpretation(loc, "PR Card Country Of Issuance", document.CountryOfIssuance);
					}
					break;
			}

			#endregion
		}

		#endregion

		#region Populate Action Code / Carrier Code

		void PopulateActionAndCarrierCodes(SegmentGroup2 group2)
		{
			#region Message Action Code / Information Type

			var gis = group2.GIS.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateGIS(gis, MessageActionCode, RegistrationInfoTypes.Codes.Crew);

			var gisInterpretation = interpretation.AddNewSegmentInterpretation(gis);
			gisInterpretation.AddElementInterpretation(() => GetMessageSubType().ToCodeDescription<MessageActionCodes>(MessageActionCode));
			gisInterpretation.AddElementInterpretationIfNotEmpty("Information Type", RegistrationInfoTypes.Descriptions.Crew);

			#endregion

			#region Carrier Code

			var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.StandardCarrierAlphaCodeScacNumber, data.CarrierCode);
			interpretation.AddNewSegmentInterpretation(rff, () => data.CarrierCode);

			#endregion
		}

		ProcessingIndicatorDescriptionCodeList MessageActionCode
		{
			get
			{
				ProcessingIndicatorDescriptionCodeList result = null;
				switch (messageSubType)
				{
					case MessageSubTypes.Create:
						result = ProcessingIndicatorDescriptionCodeList.Import;
						break;
					case MessageSubTypes.Withdraw:
						result = ProcessingIndicatorDescriptionCodeList.CancellationExecuted;
						break;
					case MessageSubTypes.Change:
						result = ProcessingIndicatorDescriptionCodeList.ChangedInformation;
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
		readonly ZString messageType;
		readonly ZBool lazyGenerateMessageContent;
		readonly IConveyance conveyance;
		readonly IEquipment equipment;
		ICrew crewMember;
	}
}
