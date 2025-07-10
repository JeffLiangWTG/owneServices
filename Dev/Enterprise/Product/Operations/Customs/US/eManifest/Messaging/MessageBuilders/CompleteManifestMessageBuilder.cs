using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.CUSCAR;
using Enterprise.Edifact.D08A.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.Messaging.Business.MessageBuilders;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;
using IAddress = Enterprise.Customs.Business.MessageBuilders.eManifest.IAddress;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging
{
	class CompleteManifestMessageBuilder : EDIFACTMessageBuilder<ICompleteManifest, CUSCARMessage, EDIMessage>, ICompleteManifestMessageBuilder
	{
		internal CompleteManifestMessageBuilder(ICompleteManifest data, ZString messageType, MessageSubTypes messageSubType, bool lazyGenerateMessageContent = false)
			: base(data, messageSubType, new UNOACharacterSet())
		{
			isCompleteManifest = messageType == MessageTypes.Codes.eManifest;
			this.lazyGenerateMessageContent = lazyGenerateMessageContent;
			this.messageType = messageType;
			partyTypes = new PartyTypes();
			Shipments = data.Shipments.Where(s => !s.ShipmentActionCode.IsEmpty).ToList();
		}

		IEnumerable<IShipment> Shipments
		{
			get;
			set;
		}
		readonly PartyTypes partyTypes;

		#region Overrides of EDIFACTMessageBuilder

		public override IMessageBuilderResult PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			var allShipments = Shipments.ToList();
			if (isCompleteManifest && (messageSubType == MessageSubTypes.Create || messageSubType == MessageSubTypes.Change))
			{
				var maximumShipmentsToSent = USeManifestDataRegistry.Instance.MaximumShipmentsToSendInOneMessage.Value;
				if (maximumShipmentsToSent <= 0 || maximumShipmentsToSent >= allShipments.Count)
				{
					PopulateMessageToBuilderResult(messageBuilderResult);
				}
				else
				{
					var skipIndex = 0;
					Shipments = allShipments.Take(maximumShipmentsToSent).ToList();
					while (Shipments.Any())
					{
						if (messageSubType == MessageSubTypes.Create && skipIndex != 0)
						{
							messageSubTypeShouldBeChange = true;
						}
						PopulateMessageToBuilderResult(messageBuilderResult);

						edifactMessage = Activator.CreateInstance<CUSCARMessage>();
						interpretation = new MessageInterpretation(edifactMessage, characterSet);
						skipIndex += maximumShipmentsToSent;
						Shipments = allShipments.Skip(skipIndex).Take(maximumShipmentsToSent).ToList();
					}
				}
			}
			else
			{
				messageBuilderResult = (MessageBuilderResult)base.PopulateMessages();
			}

			return messageBuilderResult;
		}
		bool messageSubTypeShouldBeChange;

		void PopulateMessageToBuilderResult(MessageBuilderResult messageBuilderResult)
		{
			var builderResult = new BuilderResult(null, Array.Empty<string>(), null);
			builderResult.Message = PopulateMessagesReturningResult();

			messageBuilderResult.AddBuilderResult(builderResult);
		}

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
				D08AMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, "CUSCAR", "D", "03B", "UN");
				interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

				#endregion

				#region Trip Reference / Action Code

				var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
				var messageType = isCompleteManifest ? DocumentNameCodeList.CustomsManifest : DocumentNameCodeList.GeneralCargoSummaryManifestReport;
				D08AMessageUtilities.PopulateBGM(bgm, messageType, "STANDARD", data.TripReference, MessageActionCode);

				var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
				bgmInterpretation.AddElementInterpretation(() => this.messageType.ToCodeDescription<MessageTypes>(messageType));
				bgmInterpretation.AddElementInterpretation(() => data.TripReference);
				bgmInterpretation.AddElementInterpretation(() => GetMessageSubType().ToCodeDescription<MessageActionCodes>(MessageActionCode));

				#endregion

				#region Transmission Reference Number

				if (!data.TransmissionReferenceNumber.IsEmpty)
				{
					var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
					var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference, data.TransmissionReferenceNumber);
					interpretation.AddNewSegmentInterpretation(rff, () => data.TransmissionReferenceNumber);
				}

				#endregion

				if (data.TripReference != "SYSTEM")
				{
					#region Estimated Date Of Arrival

					var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, data.EstimatedDateOfArrival);
					interpretation.AddNewSegmentInterpretation(dtm, () => data.EstimatedDateOfArrival);

					#endregion
				}

				PopulateForNonWithdraw();

				#region Shipments

				var types = new[] { MessageSubTypes.Withdraw, MessageSubTypes.ReplaceHeader };
				if (!isCompleteManifest || !types.Contains(messageSubType))
				{
					var sequentialNumber = 1;
					foreach (var shipment in Shipments)
					{
						PopulateShipment(shipment, sequentialNumber++);
					}
				}

				#endregion

				#region PopulateUNT

				var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture), unh.MessageReferenceNumber);
				interpretation.AddUNTInterpretation(unt, unt.MessageReferenceNumber);

				#endregion
			}
		}

		void PopulateForNonWithdraw()
		{
			if (isCompleteManifest && messageSubType != MessageSubTypes.Withdraw)
			{
				#region First Expected Port Of Arrival

				var loc = edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.PlaceOfArrival,
					data.FirstExpectedPortOfArrival,
					Converter.IdentificationCodes.ScheduleD);
				interpretation.AddNewSegmentInterpretation(loc, () => data.FirstExpectedPortOfArrival);

				#endregion

				#region Amendment Reason

				if (data.IsFinalized)
				{
					var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
					RFFSegment rff = null;
					var amendmentReasonCode = data.AmendmentReasonCode;
					if (messageSubType == MessageSubTypes.Change || messageSubType == MessageSubTypes.ReplaceHeader)
					{
						rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
						if (amendmentReasonCode.IsEmpty)
						{
							amendmentReasonCode = AmendmentReasonCodes.Codes.C03;
						}
					}
					else if (messageSubTypeShouldBeChange)
					{
						rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
						if (amendmentReasonCode.IsEmpty)
						{
							amendmentReasonCode = AmendmentReasonCodes.Codes.C24;
						}
					}
					if (rff != null)
					{
						D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GetFromString("RFA"), amendmentReasonCode);
						interpretation.AddNewSegmentInterpretation(rff, () => amendmentReasonCode);
					}
				}

				#endregion

				#region Carrier Code

				var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var nad = group2.NAD.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Carrier, data.CarrierCode, Converter.IdentificationCodes.SCAC);
				interpretation.AddNewSegmentInterpretation(nad, () => data.CarrierCode);

				#endregion

				#region Pre Registered Crew

				if (data.CrewMembers.All(crew => !crew.CrewId.IsEmpty))
				{
					foreach (var crew in data.CrewMembers)
					{
						PopulatePreRegisteredCrew(crew);
					}
				}

				#endregion

				#region Conveyance / Equipment

				PopulateConveyance();

				foreach (var equipment in data.Equipment)
				{
					// Group 18 in D08A acts as Group 7 in the CUSCAR specification, this might to be the reason CW1 is using D08A, not D03B
					PopulateEquipment(
						equipment,
						() => edifactMessage.Group5.InstantiateAChildAndAddItToChildrenCollection(),
						sg => sg.EQD.InstantiateAChildAndAddItToChildrenCollection(),
						sg => sg.SEL.InstantiateAChildAndAddItToChildrenCollection(),
						sg => sg.Group18.InstantiateAChildAndAddItToChildrenCollection(),
						sg => sg.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						sg => sg.LOC.InstantiateAChildAndAddItToChildrenCollection(),
						interpretation);
				}

				#endregion
			}
		}

		#region Populate Pre Registered Crew

		void PopulatePreRegisteredCrew(ICrew crew)
		{
			var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			PopulateCrew(data.Factory, group2.NAD.InstantiateAChildAndAddItToChildrenCollection(), interpretation, crew, populateName: false);
		}

		internal static void PopulateCrew(BusinessObjectFactory factory, NADSegment nad, MessageInterpretation interpretation, ICrew crew, bool populateName)
		{
			var qualifier = Converter.GetCrewPartyQualifier(factory, crew.CrewType);
			var type = Converter.GetIdentificationCodeFromCrewACEIdType(factory, crew.IdType);
			D08AMessageUtilities.PopulateNAD(nad, qualifier, crew.CrewId, type);

			if (populateName)
			{
				var formatCode = PartyNameFormatCodeList.NameComponentsInSequenceAsDefinedInDescriptionBelow;
				D08AMessageUtilities.PopulateNAD(nad, formatCode, crew.LastName, crew.FirstName, crew.MiddleName);
			}

			var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);
			nadInterpretation.AddElementInterpretation(() => crew.CrewType.ToCodeDescription<CrewTypes>(qualifier));
			nadInterpretation.AddElementInterpretation(() => crew.CrewId);
			nadInterpretation.AddElementInterpretation(() => crew.IdType.ToCodeDescription<CrewACEIdTypes>(type));

			if (populateName)
			{
				nadInterpretation.AddElementInterpretation(() => crew.LastName);
				nadInterpretation.AddElementInterpretation(() => crew.FirstName);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => crew.MiddleName);
			}

			PopulateAddress(nad, nadInterpretation, crew.USAddress, populateCountryCode: false);
		}

		#region Populate Address

		static void PopulateAddress(NADSegment nad, ISegmentInterpretation nadInterpretation, IAddress address, bool populateCountryCode = true)
		{
			if (address != null && !address.IsEmpty)
			{
				const int addressMaxLength = 35;
				var splitter = new TextSplitter(addressMaxLength) { Text = address.Address };
				nad.Street.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = splitter[0].Trim();
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = splitter[1].Trim();
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier3 = splitter[2].Trim();

				nadInterpretation.AddElementInterpretation("Address", address.Address.Left(addressMaxLength * 3));

				const int cityMaxLength = 35;
				nad.CityName = address.City.Left(cityMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty("City", address.City.Left(cityMaxLength));

				const int stateMaxLength = 3;
				nad.CountrySubdivisionDetails.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.CountrySubdivisionDetails.CodeListIdentificationCode = Converter.IdentificationCodes.CountrySubEntity;
				nad.CountrySubdivisionDetails.CountrySubdivisionIdentifier = address.StateOrProvince.Left(stateMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty("State Or Province", address.StateOrProvince.Left(stateMaxLength));

				const int postcodeMaxLength = 10;
				nad.PostalIdentificationCode = address.Postcode.Left(postcodeMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty("Postcode", address.Postcode.Left(postcodeMaxLength));

				if (populateCountryCode)
				{
					const int countryMaxLength = 2;
					nad.CountryIdentifier = address.Country.Left(countryMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty("Country", address.Country.Left(countryMaxLength));
				}
			}
		}

		#endregion

		#endregion

		#region Populate Conveyance

		void PopulateConveyance()
		{
			#region Conveyance Identifiers

			Func<RFFSegment> getRffSegment = () =>
			{
				var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
				return group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			};

			Func<TDTSegment> getTdtSegment = () =>
			{
				var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
				return group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			};

			Func<LOCSegment> getLocSegment = () =>
			{
				var group4 = edifactMessage.Group4.Cast<SegmentGroup4>().Last();
				return group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			};

			PopulateConveyanceIdentifiers(getRffSegment, getTdtSegment, getLocSegment, interpretation, data);

			#endregion

			#region Insurance Information

			var insurance = data.Conveyance.Insurance;
			if (insurance != null
				&& (!insurance.InsuranceName.IsEmpty
					|| !insurance.InsurancePolicyNumber.IsEmpty
					|| !insurance.InsuranceAmount.IsEmpty
					|| !insurance.InsuranceYearPolicyIssue.IsEmpty))
			{
				var ftx = edifactMessage.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateFTX(
					ftx,
					TextSubjectCodeQualifierList.InsuranceInformation,
					"NAME" + insurance.InsuranceName,
					"PLCY" + insurance.InsurancePolicyNumber,
					"AMNT" + insurance.InsuranceAmount.Round(0).ToZInt(),
					!insurance.InsuranceYearPolicyIssue.IsEmpty ? "YEAR" + insurance.InsuranceYearPolicyIssue : null);

				var ftxInterpretation = interpretation.AddNewSegmentInterpretation(ftx);
				ftxInterpretation.AddElementInterpretation(() => insurance.InsuranceName);
				ftxInterpretation.AddElementInterpretation(() => insurance.InsurancePolicyNumber);
				ftxInterpretation.AddElementInterpretation(() => insurance.InsuranceAmount);

				if (!insurance.InsuranceYearPolicyIssue.IsEmpty)
				{
					ftxInterpretation.AddElementInterpretation(() => insurance.InsuranceYearPolicyIssue);
				}
			}

			#endregion
		}

		#region Populate Conveyance Identifiers

		internal static void PopulateConveyanceIdentifiers(Func<RFFSegment> getRffSegment, Func<TDTSegment> getTdtSegment,
			Func<LOCSegment> getLocSegment, MessageInterpretation interpretation, ICompleteManifest data)
		{
			var conveyance = data.Conveyance;

			#region Seal Numbers

			foreach (var sealNumber in conveyance.SealNumbers)
			{
				var rff = getRffSegment();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.TransportEquipmentSealIdentifier, sealNumber);
				interpretation.AddNewSegmentInterpretation(rff, SealNumberDescription, sealNumber);
			}

			#endregion

			#region IIT Entity Indicators

			foreach (var indicator in conveyance.IITEntityIndicators)
			{
				var rff = getRffSegment();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GetFromString("IIT"), indicator);
				interpretation.AddNewSegmentInterpretation(rff, IITEntityIndicatorDescription, indicator);
			}

			#endregion

			#region Conveyance Identifiers / License Plates

			PopulateConveyanceIdentifier(getTdtSegment, interpretation, data, CompleteManifestDataConverter.IdentificationCodes.ACEId, () => conveyance.ConveyanceACEId.Left(10));
			PopulateConveyanceIdentifier(getTdtSegment, interpretation, data, CompleteManifestDataConverter.IdentificationCodes.ConveyanceId, () => conveyance.ConveyanceId.Left(23));
			PopulateConveyanceIdentifier(getTdtSegment, interpretation, data, CompleteManifestDataConverter.IdentificationCodes.TransponderId, () => conveyance.TransponderId);
			PopulateConveyanceIdentifier(getTdtSegment, interpretation, data, CompleteManifestDataConverter.IdentificationCodes.VIN, conveyance.EquipmentId, "Vehicle Identification Number (VIN)");
			PopulateConveyanceIdentifier(getTdtSegment, interpretation, data, CompleteManifestDataConverter.IdentificationCodes.DOTNumber, () => data.DepartmentOfTransportationNumber);

			foreach (var licensePlate in conveyance.LicensePlates)
			{
				PopulateConveyanceLicensePlate(getTdtSegment, getLocSegment, interpretation, data, licensePlate);
			}

			#endregion
		}

		#endregion

		#region Populate Conveyance Identifier

		static void PopulateConveyanceIdentifier(Func<TDTSegment> getTdtSegment, MessageInterpretation interpretation, ICompleteManifest data,
			string identificationCode, Expression<Func<ZString>> identifier, string countryOfRegistration = null)
		{
			PopulateConveyanceIdentifier(getTdtSegment, interpretation, data, identificationCode, identifier.Compile()(), PropertyNameProvider.GetFriendlyPropertyName(identifier), countryOfRegistration);
		}

		internal static void PopulateConveyanceIdentifier(Func<TDTSegment> getTdtSegment, MessageInterpretation interpretation, ICompleteManifest data,
			string identificationCode, ZString identifier, string identifierDescription, string countryOfRegistration = null)
		{
			if (!identifier.IsEmpty)
			{
				var tdt = getTdtSegment();
				var conveyance = data.Conveyance;
				var methodOfTransportation = Converter.GetMethodOfTransportationQualifier(data.Factory, data.MethodOfTransportation);
				D08AMessageUtilities.PopulateTDT(
					tdt,
					TransportStageCodeQualifierList.AtBorder,
					methodOfTransportation,
					conveyance.EquipmentType,
					data.TransitDirectionCode,
					identificationCode,
					identifier,
					countryOfRegistration);

				var tdtInterpretation = interpretation.AddNewSegmentInterpretation(tdt);
				tdtInterpretation.AddElementInterpretation("Method Of Transportation", data.MethodOfTransportation.ToCodeDescription<TransportModes>(methodOfTransportation));
				tdtInterpretation.AddElementInterpretation("Conveyance Type", conveyance.EquipmentType.ToCodeDescription<ConveyanceTypes>());
				tdtInterpretation.AddElementInterpretation("Transit Direction Code", data.TransitDirectionCode.ToCodeDescription<TransitDirectionCodes>());
				tdtInterpretation.AddElementInterpretation(identifierDescription, identifier);
				tdtInterpretation.AddElementInterpretationIfNotEmpty("Country Of Registration", (ZString)countryOfRegistration);
			}
		}

		#endregion

		#region Populate Conveyance License Plate

		static void PopulateConveyanceLicensePlate(Func<TDTSegment> getTdtSegment, Func<LOCSegment> getLocSegment,
			MessageInterpretation interpretation, ICompleteManifest data, ILicensePlate licensePlate)
		{
			if (!licensePlate.LicensePlateNumber.IsEmpty)
			{
				PopulateConveyanceIdentifier(
					getTdtSegment,
					interpretation,
					data,
					Converter.IdentificationCodes.LicensePlate,
					() => licensePlate.LicensePlateNumber,
					licensePlate.CountryOfRegistration);

				var loc = getLocSegment();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.PlaceOfRegistration,
					licensePlate.StateOrProvinceOfRegistration,
					Converter.IdentificationCodes.CountrySubEntity);

				interpretation.AddNewSegmentInterpretation(loc, () => licensePlate.StateOrProvinceOfRegistration);
			}
		}

		#endregion

		const string SealNumberDescription = "Seal Number";
		const string IITEntityIndicatorDescription = "IIT Entity Indicator";

		#endregion

		#region Populate Equipment

		internal static void PopulateEquipment<TSegment, TSubSegment>(IEquipment equipment, Func<TSegment> getSegmentGroup, Func<TSegment, EQDSegment> getEqdSegment, Func<TSegment, SELSegment> getSelSegment,
			Func<TSegment, TSubSegment> getSegmentSubGroup, Func<TSubSegment, RFFSegment> getRffSegment, Func<TSubSegment, LOCSegment> getLocSegment, MessageInterpretation interpretation)
			where TSegment : SegmentGroup
			where TSubSegment : SegmentGroup
		{
			var segmentGroup = getSegmentGroup();

			#region Equipment ACE Id / Mark/Initial + Number

			if (!equipment.EquipmentACEId.IsEmpty)
			{
				PopulateEquipmentIdentifier(equipment, segmentGroup, getEqdSegment, interpretation, Converter.IdentificationCodes.ACEId, "Equipment ACE Id", equipment.EquipmentACEId.Left(10));
			}
			else if (!equipment.EquipmentId.IsEmpty)
			{
				PopulateEquipmentIdentifier(equipment, segmentGroup, getEqdSegment, interpretation, Converter.IdentificationCodes.EquipmentNumber, "Equipment Id", equipment.EquipmentId);
			}
			else
			{
				PopulateEquipmentIdentifier(equipment, segmentGroup, getEqdSegment, interpretation, string.Empty, string.Empty, string.Empty);
			}

			#endregion

			#region Seal Numbers

			foreach (var sealNumber in equipment.SealNumbers)
			{
				var sel = getSelSegment(segmentGroup);
				D08AMessageUtilities.PopulateSEL(sel, sealNumber);
				interpretation.AddNewSegmentInterpretation(sel, SealNumberDescription, sealNumber);
			}

			#endregion

			#region IIT Entity Indicators

			foreach (var indicator in equipment.IITEntityIndicators)
			{
				var segmentSubGroup = getSegmentSubGroup(segmentGroup);
				var rff = getRffSegment(segmentSubGroup);
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GetFromString("IIT"), indicator);
				interpretation.AddNewSegmentInterpretation(rff, IITEntityIndicatorDescription, indicator);
			}

			#endregion

			#region License Plates

			foreach (var licensePlate in equipment.LicensePlates)
			{
				PopulateEquipmentLicensePlate(segmentGroup, getSegmentSubGroup, getRffSegment, getLocSegment, interpretation, licensePlate);
			}

			#endregion
		}

		#region Populate Equipment Identifier

		static void PopulateEquipmentIdentifier<TSegment>(IEquipment equipment, TSegment segmentGroup, Func<TSegment, EQDSegment> getEqdSegment,
			MessageInterpretation interpretation, ZString identificationCode, string identifierDescription, ZString identifier) where TSegment : SegmentGroup
		{
			var eqd = getEqdSegment(segmentGroup);
			var qualifier = EquipmentTypeCodeQualifierList.GetFromString(equipment.EquipmentType);
			D08AMessageUtilities.PopulateEQD(eqd, qualifier, identifier, identificationCode);

			var eqdInterpretation = interpretation.AddNewSegmentInterpretation(eqd);
			eqdInterpretation.AddElementInterpretation(() => equipment.EquipmentType.ToCodeDescription<EquipmentTypes>());
			eqdInterpretation.AddElementInterpretationIfNotEmpty(identifierDescription, identifier);
		}

		#endregion

		#region Populate Equipment License Plate

		static void PopulateEquipmentLicensePlate<TSegment, TSubSegment>(TSegment segmentGroup, Func<TSegment, TSubSegment> getSegmentSubGroup,
			Func<TSubSegment, RFFSegment> getRffSegment, Func<TSubSegment, LOCSegment> getLocSegment, MessageInterpretation interpretation, ILicensePlate licensePlate)
			where TSegment : SegmentGroup
			where TSubSegment : SegmentGroup
		{
			#region License Plate Number

			var segmentSubGroup = getSegmentSubGroup(segmentGroup);
			var rff = getRffSegment(segmentSubGroup);
			D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.VehicleLicenceNumber, licensePlate.LicensePlateNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => licensePlate.LicensePlateNumber);

			#endregion

			#region State Or Province Of Registration

			var loc = getLocSegment(segmentSubGroup);

			D08AMessageUtilities.PopulateLOC(
				loc,
				LocationFunctionCodeQualifierList.PlaceOfRegistration,
				licensePlate.StateOrProvinceOfRegistration,
				Converter.IdentificationCodes.CountrySubEntity);

			interpretation.AddNewSegmentInterpretation(loc, () => licensePlate.StateOrProvinceOfRegistration);

			#endregion

			#region Country Of Registration

			loc = getLocSegment(segmentSubGroup);

			D08AMessageUtilities.PopulateLOC(
				loc,
				LocationFunctionCodeQualifierList.PlaceOfRegistration,
				licensePlate.CountryOfRegistration,
				Converter.IdentificationCodes.Country);

			interpretation.AddNewSegmentInterpretation(loc, () => licensePlate.CountryOfRegistration);

			#endregion

		}

		#endregion

		#endregion

		#region Populate Shipment

		void PopulateShipment(IShipment shipment, int shipmentSequentialNumber)
		{
			var group7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();
			var inBond = shipment.InBond;
			var isInBondMovement = shipment.ShipmentType == ShipmentTypes.Codes.Inbond && inBond != null;
			var isGoodsAstray = shipment.ShipmentType == ShipmentTypes.Codes.GoodsAstray;
			var isSplitShipment = shipment.ShipmentType == ShipmentTypes.Codes.SplitShipment;

			#region Sequential Number / Action Code

			var shipmentActionCode = GetShipmentActionCode(shipment.ShipmentActionCode, isSplitShipment);
			var cni = group7.CNI.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateCNI(cni, shipmentSequentialNumber.ToString(CultureInfo.InvariantCulture), shipmentActionCode);
			var cniInterpretation = interpretation.AddNewSegmentInterpretation(cni);
			cniInterpretation.AddElementInterpretation("Shipment Sequential Number", shipmentSequentialNumber);
			cniInterpretation.AddElementInterpretation("Shipment Action Code", shipment.ShipmentActionCode.ToCodeDescription<MessageActionCodes>(shipmentActionCode));

			#endregion

			#region Shipment Control Number

			var group8 = group7.Group8.InstantiateAChildAndAddItToChildrenCollection();
			var rff = group8.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.WaybillNumber, shipment.ShipmentControlNumber);
			interpretation.AddNewSegmentInterpretation(rff, "Shipment Control Number", shipment.ShipmentControlNumber);

			var isNotCancellation = shipment.ShipmentActionCode != MessageActionCodes.Codes.Cancellation;
			#endregion

			PopulateForNotCancellation(shipment, isInBondMovement, isSplitShipment, isGoodsAstray, isNotCancellation, inBond, group7, group8);

			#region Commodities

			var commoditySequenceNumber = 1;
			var commodities = shipment.Commodities;
			if (isNotCancellation && !isSplitShipment && commodities.Any())
			{
				foreach (var commodity in commodities)
				{
					PopulateCommodity(group8, shipment.ShipmentType, commodity, commoditySequenceNumber++);
				}
			}
			else
			{
				var group14 = group8.Group14.InstantiateAChildAndAddItToChildrenCollection();
				var gid = group14.GID.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateGID(gid, commoditySequenceNumber);
				interpretation.AddMandatoryTriggerSegmentInterpretation(gid);

				var ftx = group14.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.GoodsItemDescription, "DUMMY");
				interpretation.AddMandatoryTriggerSegmentInterpretation(ftx);
			}

			#endregion
		}

		void PopulateForNotCancellation(IShipment shipment, ZBool isInBondMovement, ZBool isSplitShipment, ZBool isGoodsAstray, ZBool isNotCancellation, IInBond inBond, SegmentGroup7 group7, SegmentGroup8 group8)
		{
			if (isNotCancellation)
			{
				#region Boarded Quantity

				if (isCompleteManifest && !shipment.BoardedQuantity.IsEmpty)
				{
					var cnt = group8.CNT.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateCNT(cnt, ControlTotalTypeCodeQualifierList.DaysUnderCustomsTransitControl, shipment.BoardedQuantity);
					interpretation.AddNewSegmentInterpretation(cnt, () => shipment.BoardedQuantity);
				}

				#endregion

				#region  Shipment Identifier / Entry Type

				PopulateShipmentIdentifier(group7, DocumentNameCodeList.ShippingNote, CompleteManifestDataConverter.EntryTypeCodes.ShipmentIdentifier, ZString.Empty, "Shipment Identifier", shipment.ShipmentIdentifier, string.Empty, ZString.Empty);

				var entryTypeCode = Converter.GetEntryTypeCode(data.Factory, shipment.ShipmentType, ZString.Empty, shipment.FDAFreightIndicator);
				var entryTypeDescription = shipment.ShipmentType.ToCodeDescription<ShipmentTypes>(entryTypeCode);
				PopulateShipmentIdentifier(group7, DocumentNameCodeList.GoodsDeclarationForImportation, entryTypeCode, entryTypeDescription, string.Empty, ZString.Empty, string.Empty, ZString.Empty);

				#endregion

				PopulateInbond7512NumberAndMexicanPedimentoNumber(isInBondMovement, inBond, entryTypeCode, entryTypeDescription, group7, shipment);
				PopulatePortOrPointOfLoading(isSplitShipment, group8, interpretation, shipment);
				PopulatePlaceOfReceipt(group8, shipment);
				PopulateTransferDestinationFIRMSCode(group8, shipment, interpretation);
				PopulateForeignPortOfDestination(isInBondMovement, inBond, group8, interpretation);
				PopulateInbondDestinationScheduleD(isInBondMovement, inBond, interpretation, group8);
				PopulateForJobsOutOfUSFor45DaysOrLessIndicators(isGoodsAstray, shipment, group8);
				var group9 = group8.Group9.InstantiateAChildAndAddItToChildrenCollection();
				PopulateMandatoryTriggerSegment(group9, isInBondMovement, inBond, isGoodsAstray, shipment);
				PopulateEstimatedDateOfUSExit(isInBondMovement, inBond, group9);
				PopulateExportDate(isGoodsAstray, shipment, group9);

				#region Amendment Reason Code

				if (shipment.ShipmentActionCode == MessageActionCodes.Codes.Change || isSplitShipment)
				{
					var group10 = group9.Group10.InstantiateAChildAndAddItToChildrenCollection();
					var rff = group10.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GetFromString("RFA"), shipment.ShipmentAmendmentReasonCode);
					interpretation.AddNewSegmentInterpretation(rff, () => shipment.ShipmentAmendmentReasonCode);
				}

				#endregion

				PopulateServiceType(shipment, group9);

				#region Bonded Carrier Id

				if (isInBondMovement && !inBond.BondedCarrier.IsEmpty)
				{
					var group11 = group8.Group11.InstantiateAChildAndAddItToChildrenCollection();
					var nad = group11.NAD.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.GoodsCustodian, inBond.BondedCarrier, Converter.IdentificationCodes.SSNOrEIN);
					interpretation.AddNewSegmentInterpretation(nad, () => inBond.BondedCarrier);
				}

				#endregion

				#region Onward Carrier Code

				if (isInBondMovement && !inBond.OnwardCarrier.IsEmpty)
				{
					var group11 = group8.Group11.InstantiateAChildAndAddItToChildrenCollection();
					var nad = group11.NAD.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.GetFromString("OCG"), inBond.OnwardCarrier, Converter.IdentificationCodes.SCAC);
					interpretation.AddNewSegmentInterpretation(nad, () => inBond.OnwardCarrier);
				}

				#endregion

				#region Transfer Carrier

				if (isInBondMovement && !inBond.TransferCarrier.IsEmpty)
				{
					var group11 = group8.Group11.InstantiateAChildAndAddItToChildrenCollection();
					var nad = group11.NAD.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.ConnectingCarrier, inBond.TransferCarrier, Converter.IdentificationCodes.SSNOrEIN);
					interpretation.AddNewSegmentInterpretation(nad, () => inBond.TransferCarrier);
				}

				#endregion

				#region Parties (Shipper/Consignee/Secondary Notify Party)

				foreach (var party in shipment.Parties)
				{
					PopulateParty(group8, party);
				}

				#endregion
			}
		}

		#region Service Type
		void PopulateServiceType(IShipment shipment, SegmentGroup9 group9)
		{
			if (!shipment.ServiceType.IsEmpty)
			{
				var group10 = group9.Group10.InstantiateAChildAndAddItToChildrenCollection();
				if (shipment.ShipmentActionCode != MessageActionCodes.Codes.Change)
				{
					var rff = group10.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.ServiceCategoryReference);
					interpretation.AddMandatoryTriggerSegmentInterpretation(rff);
				}

				var tsr = group10.TSR.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateTSR(tsr, ContractAndCarriageConditionCodeList.GetFromString(shipment.ServiceType));
				interpretation.AddNewSegmentInterpretation(tsr, () => shipment.ServiceType.ToCodeDescription<ServiceTypes>());
			}
		}
		#endregion

		#region Export Date
		void PopulateExportDate(ZBool isGoodsAstray, IShipment shipment, SegmentGroup9 group9)
		{
			if (isGoodsAstray && !shipment.ExportDate.IsEmpty)
			{
				var dtm = group9.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateDTM(
					dtm,
					DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeEstimated,
					shipment.ExportDate);

				interpretation.AddNewSegmentInterpretation(dtm, () => shipment.ExportDate);
			}
		}
		#endregion

		#region Estimated Date Of US Exit
		void PopulateEstimatedDateOfUSExit(ZBool isInBondMovement, IInBond inBond, SegmentGroup9 group9)
		{
			if (isInBondMovement && !inBond.EstimatedDateOfUSExit.IsEmpty)
			{
				var dtm = group9.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateDTM(
					dtm,
					DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeEstimated,
					inBond.EstimatedDateOfUSExit);

				interpretation.AddNewSegmentInterpretation(dtm, () => inBond.EstimatedDateOfUSExit);
			}
		}
		#endregion

		#region Mandatory Trigger Segment
		void PopulateMandatoryTriggerSegment(SegmentGroup9 group9, ZBool isInBondMovement, IInBond inBond, ZBool isGoodsAstray, IShipment shipment)
		{
			if ((isInBondMovement && !inBond.EstimatedDateOfUSExit.IsEmpty)
				|| (isGoodsAstray && !shipment.ExportDate.IsEmpty)
				|| !shipment.ShipmentAmendmentReasonCode.IsEmpty
				|| !shipment.ServiceType.IsEmpty)
			{
				var tdt = group9.TDT.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.AtBorder);
				interpretation.AddMandatoryTriggerSegmentInterpretation(tdt);
			}
		}
		#endregion

		#region FDA Freight / Shipment Was Out Of US For 45 Days Or Less Indicators

		void PopulateForJobsOutOfUSFor45DaysOrLessIndicators(ZBool isGoodsAstray, IShipment shipment, SegmentGroup8 group8)
		{
			var qualifier = ProcessingInformationCodeQualifierList.GetFromString("7");
			if (isGoodsAstray && shipment.ShipmentWasOutOfUSFor45DaysOrLessIndicator)
			{
				var gei = group8.GEI.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateGEI(gei, qualifier, FDAFreightIndicators.Codes.ShipmentWasOutOfUSFor45DaysOrLess);

				var value = TableInterpretation.GetBooleanAsString(true);
				interpretation.AddNewSegmentInterpretation(gei, () => shipment.ShipmentWasOutOfUSFor45DaysOrLessIndicator, value);
			}
			else
			{
				var gei = group8.GEI.InstantiateAChildAndAddItToChildrenCollection();
				var indicator = shipment.FDAFreightIndicator
									? FDAFreightIndicators.Codes.FoodProductsIncludedInShipment
									: FDAFreightIndicators.Codes.NoFoodProductsIncludedInShipment;
				D08AMessageUtilities.PopulateGEI(gei, qualifier, indicator);

				var value = TableInterpretation.GetBooleanAsString(shipment.FDAFreightIndicator);
				interpretation.AddNewSegmentInterpretation(gei, () => shipment.FDAFreightIndicator, value);
			}
		}
		#endregion

		#region Inbond Destination (Schedule D)
		void PopulateInbondDestinationScheduleD(ZBool isInBondMovement, IInBond inBond, MessageInterpretation interpretation, SegmentGroup8 group8)
		{
			if (isInBondMovement && !inBond.InbondDestination.IsEmpty)
			{
				var loc = group8.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.CustomsOfficeOfDestinationTransit,
					inBond.InbondDestination,
					Converter.IdentificationCodes.ScheduleD);

				interpretation.AddNewSegmentInterpretation(loc, () => inBond.InbondDestination);
			}
		}
		#endregion

		#region Foreign Port Of Destination

		void PopulateForeignPortOfDestination(ZBool isInBondMovement, IInBond inBond, SegmentGroup8 group8, MessageInterpretation interpretation)
		{
			if (isInBondMovement && !inBond.ForeignPortOfDestination.IsEmpty)
			{
				var loc = group8.LOC.InstantiateAChildAndAddItToChildrenCollection();
				var codeType = Converter.GetIdentificationCodeFromLocationCodeType(data.Factory, inBond.ForeignPortOfDestinationCodeType);
				D08AMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfDestination, inBond.ForeignPortOfDestination, codeType);

				var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
				locInterpretation.AddElementInterpretation(() => inBond.ForeignPortOfDestination);
				locInterpretation.AddElementInterpretation(() => inBond.ForeignPortOfDestinationCodeType.ToCodeDescription<PortCodeTypes>(codeType));
			}
		}
		#endregion

		#region Transfer Destination FIRMS Code

		void PopulateTransferDestinationFIRMSCode(SegmentGroup8 group8, IShipment shipment, MessageInterpretation interpretation)
		{
			if (!shipment.TransferDestinationFIRMSCode.IsEmpty)
			{
				var loc = group8.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.PlaceOfTransfer,
					shipment.TransferDestinationFIRMSCode,
					Converter.IdentificationCodes.FIRMS);

				interpretation.AddNewSegmentInterpretation(loc, () => shipment.TransferDestinationFIRMSCode);
			}
		}

		#endregion

		#region Place Of Receipt
		void PopulatePlaceOfReceipt(SegmentGroup8 group8, IShipment shipment)
		{
			if (!shipment.PlaceOfReceipt.IsEmpty)
			{
				var loc = group8.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.GoodsReceiptPlace,
					shipment.PlaceOfReceipt,
					Converter.IdentificationCodes.FreeFormText);

				interpretation.AddNewSegmentInterpretation(loc, () => shipment.PlaceOfReceipt);
			}
		}

		#endregion

		#region Port Or Point Of Loading
		void PopulatePortOrPointOfLoading(ZBool isSplitShipment, SegmentGroup8 group8, MessageInterpretation interpretation, IShipment shipment)
		{
			if (!isSplitShipment)
			{
				var loc = group8.LOC.InstantiateAChildAndAddItToChildrenCollection();
				var codeType = Converter.GetIdentificationCodeFromLocationCodeType(data.Factory, shipment.PortOrPointOfLoadingCodeType);
				D08AMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfLoading, shipment.PortOrPointOfLoading, codeType);

				var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
				locInterpretation.AddElementInterpretation("Port Or Point Of Loading", shipment.PortOrPointOfLoading);
				locInterpretation.AddElementInterpretation("Port Or Point Of Loading Code Type", shipment.PortOrPointOfLoadingCodeType.ToCodeDescription<PortCodeTypes>(codeType));
			}
		}

		#endregion

		#region Inbond 7512 Number / Mexican Pedimento Number
		void PopulateInbond7512NumberAndMexicanPedimentoNumber(ZBool isInBondMovement, IInBond inBond, ZString entryTypeCode, ZString entryTypeDescription, SegmentGroup7 group7, IShipment shipment)
		{
			if (isInBondMovement)
			{
				entryTypeCode = Converter.GetEntryTypeCode(data.Factory, shipment.ShipmentType, inBond.InbondType, shipment.FDAFreightIndicator);
				entryTypeDescription = inBond.InbondType.ToCodeDescription<InbondTypes>(entryTypeCode);

				PopulateShipmentIdentifier(
					group7,
					DocumentNameCodeList.GoodsDeclarationForCustomsTransit,
					entryTypeCode,
					entryTypeDescription,
					"Inbond 7512 Number",
					inBond.Inbond7512Number,
					string.Empty,
					ZString.Empty);

				PopulateShipmentIdentifier(
					group7,
					DocumentNameCodeList.GoodsDeclarationForImportation,
					Converter.EntryTypeCodes.MexicanPedimento,
					ZString.Empty,
					"Mexican Pedimento Number",
					inBond.MexicanPedimentoNumber.ToString(),
					string.Empty,
					ZString.Empty);
			}
		}

		#endregion

		#region Populate Shipment Idintifier

		void PopulateShipmentIdentifier(SegmentGroup7 group7, DocumentNameCodeList docCode, ZString entryTypeCode, ZString entryTypeDescription,
			string identifier1Description, ZString identifier1, string identifier2Description, ZString identifier2)
		{
			if (!identifier1.IsEmpty || !identifier2.IsEmpty || (!entryTypeCode.IsEmpty && !entryTypeDescription.IsEmpty))
			{
				var doc = group7.DOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateDOC(doc, docCode, entryTypeCode, identifier1, identifier2);
				var docInterpretation = interpretation.AddNewSegmentInterpretation(doc);
				if (!entryTypeDescription.IsEmpty)
				{
					docInterpretation.AddElementInterpretation(() => entryTypeCode, entryTypeDescription);
				}
				docInterpretation.AddElementInterpretationIfNotEmpty(identifier1Description, identifier1);
				if (!identifier2.IsEmpty)
				{
					docInterpretation.AddElementInterpretation(identifier2Description, identifier2);
				}
			}
		}

		#endregion

		#region Populate Party

		void PopulateParty(SegmentGroup8 group8, IParty party)
		{
			var group11 = group8.Group11.InstantiateAChildAndAddItToChildrenCollection();
			var nad = group11.NAD.InstantiateAChildAndAddItToChildrenCollection();

			var qualifier = Converter.GetPartyQualifier(data.Factory, party.PartyType);
			D08AMessageUtilities.PopulateNAD(nad, qualifier, ZString.Empty, ZString.Empty, party.ABIRoutingCode, party.PartyName);

			var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);
			nadInterpretation.AddElementInterpretation("Party Type", partyTypes.GetCodeDescription(party.PartyType, qualifier));
			nadInterpretation.AddElementInterpretationIfNotEmpty("ABI Routing Code", party.ABIRoutingCode);
			nadInterpretation.AddElementInterpretationIfNotEmpty("Party Name", party.PartyName);

			PopulateAddress(nad, nadInterpretation, party);

			#region Phone / Email

			if (!party.Phone.IsEmpty || !party.Email.IsEmpty)
			{
				var group12 = group11.Group12.InstantiateAChildAndAddItToChildrenCollection();
				var cta = group12.CTA.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateCTA(cta, ContactFunctionCodeList.InformationContact);
				interpretation.AddMandatoryTriggerSegmentInterpretation(cta);

				if (!party.Phone.IsEmpty)
				{
					var com = group12.COM.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateCOM(com, CommunicationMeansTypeCodeList.Telephone, party.Phone);
					interpretation.AddNewSegmentInterpretation(com, "Phone", party.Phone);
				}

				if (party.Phone.IsEmpty && !party.Email.IsEmpty)
				{
					var com = group12.COM.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateCOM(com, CommunicationMeansTypeCodeList.ElectronicMail, party.Email);
					interpretation.AddNewSegmentInterpretation(com, "Email", party.Email);
				}
			}

			#endregion
		}

		#endregion

		#region Populate Comodity

		void PopulateCommodity(SegmentGroup8 group8, ZString shipmentType, ICommodity commodity, int commoditySequenceNumber)
		{
			var group14 = group8.Group14.InstantiateAChildAndAddItToChildrenCollection();

			#region Commodity Sequence Number

			var gid = group14.GID.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateGID(gid, commoditySequenceNumber);
			interpretation.AddNewSegmentInterpretation(gid, "Commodity Sequence Number", commoditySequenceNumber);

			#endregion

			#region Number/Type Of Packages

			var pac = group14.PAC.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulatePAC(pac, commodity.NumberOfPackages, commodity.TypeOfPackages);

			var pacInterpretation = interpretation.AddNewSegmentInterpretation(pac);
			pacInterpretation.AddElementInterpretation("Number Of Packages", commodity.NumberOfPackages);
			pacInterpretation.AddElementInterpretation("Type Of Packages", commodity.TypeOfPackages);

			#endregion

			#region Description Of Cargo

			var goodsDescriptions = commodity.DescriptionOfCargo.Split(System.Environment.NewLine.ToCharArray());
			foreach (var goodsDescription in goodsDescriptions.Where(d => !d.IsEmpty))
			{
				var friendlyName = PropertyNameProvider.GetFriendlyPropertyName(() => commodity.DescriptionOfCargo);
				var splitter = new TextSplitter(45) { Text = goodsDescription };
				for (var i = 0; i < splitter.Count; i++)
				{
					var partOfDescription = splitter[i].Trim();
					var ftx = group14.FTX.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.GoodsItemDescription, partOfDescription);
					interpretation.AddNewSegmentInterpretation(ftx, friendlyName, partOfDescription);
					if (i == 0 && splitter.Count > 1)
					{
						friendlyName = GetContinued(friendlyName);
					}
				}
			}

			#endregion

			#region Vehicle Identification Numbers

			const int countOfVinsPerSegment = 5;
			var vinsDescription = PropertyNameProvider.GetFriendlyPropertyName(() => commodity.VehicleIdentificationNumbers);
			IEnumerable<ZString> vins = commodity.VehicleIdentificationNumbers.ToArray();
			while (vins.Any())
			{
				var someVins = vins.Take(countOfVinsPerSegment).ToArray();
				var ftx = group14.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.ProductInformation, someVins);
				interpretation.AddNewSegmentInterpretation(ftx, vinsDescription, someVins.ToStringDelimited(", "));
				vins = vins.Skip(countOfVinsPerSegment).ToArray();
			}

			#endregion

			#region Hazardous Goods Details

			foreach (var details in commodity.HazardousGoodsDetails)
			{
				PopulateHazardousGoodsDetails(group14, details);
			}

			#endregion

			#region Cargo Gross Weight / Weight Unit Of Measure

			var mea = group14.MEA.InstantiateAChildAndAddItToChildrenCollection();
			var weight = WeightUnits.ConvertWeightToKilogramsIfRequired(commodity.CargoGrossWeight, commodity.WeightUnitOfMeasure);
			var units = WeightUnits.GetWeightUOM(commodity.WeightUnitOfMeasure);
			D08AMessageUtilities.PopulateMEA(mea, MeasurementPurposeCodeQualifierList.ItemWeight, weight, units);

			var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
			meaInterpretation.AddElementInterpretation("Weight Unit Of Measure", units.ToCodeDescription<WeightUnits>());
			meaInterpretation.AddElementInterpretation("Cargo Gross Weight", weight);

			#endregion

			#region Customs Value

			if (!commodity.CustomsValue.IsEmpty)
			{
				var moa = group14.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.GoodsItemForCustomsDeclaredValueAmount, new ZDecimal(commodity.CustomsValue), 0);
				interpretation.AddNewSegmentInterpretation(moa, "Customs Value", commodity.CustomsValue);
			}

			#endregion

			#region Equipment Identifier

			PopulateEquipmentIdentifier(commodity, group14);

			#endregion

			#region Shipping Marks

			PopulateShippingMarks(commodity, group14);

			#endregion

			#region C4 Codes / Harmonized Numbers

			if (shipmentType == ShipmentTypes.Codes.BRASS)
			{
				PopulateCommodityCodes(group14, "C4 Codes", commodity.C4Codes, Converter.IdentificationCodes.C4Code);
			}
			
			PopulateCommodityCodes(group14, "Harmonized Numbers", commodity.HarmonizedNumbers, Converter.IdentificationCodes.HarmonizedTariffCode);

			#endregion

			#region Country Of Origin
			PopulateCountryOfOrigin(commodity, group14);
			#endregion
		}

		#region Populate Country Of Origin
		void PopulateCountryOfOrigin(ICommodity commodity, SegmentGroup14 group14)
		{
			if (!commodity.CountryOfOrigin.IsEmpty)
			{
				var loc = group14.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.CountryOfOrigin,
					commodity.CountryOfOrigin,
					Converter.IdentificationCodes.Country);

				interpretation.AddNewSegmentInterpretation(loc, "Country Of Origin", commodity.CountryOfOrigin);
			}
		}
		#endregion

		#region Populate Shipping Marks
		void PopulateShippingMarks(ICommodity commodity, SegmentGroup14 group14)
		{
			const int countOfMarksPerSegment = 10;

			var first = true;
			var shippingMarksDescription = PropertyNameProvider.GetFriendlyPropertyName(() => commodity.ShippingMarks);
			IEnumerable<ZString> shippingMarksSplitted = (from mark in commodity.ShippingMarks.Split(System.Environment.NewLine.ToCharArray())
														  from part in mark.Split(35)
														  where !part.IsEmpty
														  select part).ToArray();

			while (shippingMarksSplitted.Any())
			{
				var parts = shippingMarksSplitted.Take(countOfMarksPerSegment).ToArray();
				var pci = group14.PCI.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulatePCI(pci, parts.Select(s => s.Trim()));
				interpretation.AddNewSegmentInterpretation(pci, shippingMarksDescription, parts.ToStringDelimited(string.Empty));
				shippingMarksSplitted = shippingMarksSplitted.Skip(countOfMarksPerSegment).ToArray();

				if (first)
				{
					shippingMarksDescription = GetContinued(shippingMarksDescription);
					first = false;
				}
			}
		}
		#endregion

		#region Populate Equipment Identifier
		void PopulateEquipmentIdentifier(ICommodity commodity, SegmentGroup14 group14)
		{
			var equipment = commodity.Equipment;
			if (equipment != null)
			{
				var sgp = group14.SGP.InstantiateAChildAndAddItToChildrenCollection();
				if (!equipment.EquipmentACEId.IsEmpty)
				{
					D08AMessageUtilities.PopulateSGP(sgp, equipment.EquipmentACEId, Converter.IdentificationCodes.ACEId);
					interpretation.AddNewSegmentInterpretation(sgp, "Equipment ACE Id", equipment.EquipmentACEId);
				}
				else if (!equipment.EquipmentId.IsEmpty)
				{
					D08AMessageUtilities.PopulateSGP(sgp, equipment.EquipmentId, Converter.IdentificationCodes.EquipmentNumber);
					interpretation.AddNewSegmentInterpretation(sgp, "Equipment Id", equipment.EquipmentId);
				}
				else if (equipment.LicensePlates.Any())
				{
					var equipmentLicensePlateNumber = equipment.LicensePlates.First().LicensePlateNumber;
					D08AMessageUtilities.PopulateSGP(sgp, equipmentLicensePlateNumber, Converter.IdentificationCodes.LicensePlate);
					interpretation.AddNewSegmentInterpretation(sgp, "Equipment License Plate Number", equipmentLicensePlateNumber);
				}
			}
		}
		#endregion

		#region Populate Hazardous Goods Details

		void PopulateHazardousGoodsDetails(SegmentGroup14 group14, IHazardousGoods details)
		{
			var ftx = group14.FTX.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateFTX(
				ftx,
				TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation,
				"UNDG" + details.HazardousGoodsCode,
				"NAME" + details.HazardousGoodsContactName,
				"TELE" + details.HazardousGoodsContactPhone);

			var ftxInterpretation = interpretation.AddNewSegmentInterpretation(ftx);
			ftxInterpretation.AddElementInterpretation("Hazardous Goods Code", details.HazardousGoodsCode);
			ftxInterpretation.AddElementInterpretation("Hazardous Goods Contact Name", details.HazardousGoodsContactName);
			ftxInterpretation.AddElementInterpretation("Hazardous Goods Contact Phone", details.HazardousGoodsContactPhone);

			var dgs = group14.DGS.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateDGS(dgs, details.HazardousGoodsCode);
			interpretation.AddNewSegmentInterpretation(dgs, "Hazardous Goods Code", details.HazardousGoodsCode);
		}

		#endregion

		#region Populate Commodity Codes

		void PopulateCommodityCodes(SegmentGroup14 group14, string friendlyName, IEnumerable<ZString> commodityCodes, string identificationCode)
		{
			const int countOfCodesPerSegment = 5;
			IEnumerable<ZString> codes = commodityCodes.ToArray();
			while (codes.Any())
			{
				var someCodes = codes.Take(countOfCodesPerSegment).ToArray();
				var cst = group14.CST.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateCST(cst, someCodes, identificationCode);
				var value = someCodes.ToStringDelimited(", ");
				interpretation.AddNewSegmentInterpretation(cst, friendlyName, value);
				codes = codes.Skip(countOfCodesPerSegment).ToArray();
			}
		}

		#endregion

		static string GetContinued(string friendlyName)
		{
			return string.Format(Culture.Invariant, "{0} (Continued)", friendlyName);
		}

		#endregion

		#endregion

		#region Action Codes

		MessageFunctionCodeList MessageActionCode
		{
			get
			{
				MessageFunctionCodeList result = null;
				switch (messageSubType)
				{
					case MessageSubTypes.Create:
						if (messageSubTypeShouldBeChange)
						{
							result = MessageFunctionCodeList.Replace;
						}
						else
						{
							result = isCompleteManifest ? MessageFunctionCodeList.FinalTransmission : MessageFunctionCodeList.Addition;
						}
						break;
					case MessageSubTypes.Withdraw:
						result = MessageFunctionCodeList.Deletion;
						break;
					case MessageSubTypes.Change:
						result = isCompleteManifest ? MessageFunctionCodeList.Replace : MessageFunctionCodeList.Change;
						break;
					case MessageSubTypes.ReplaceHeader:
						result = MessageFunctionCodeList.Replace;
						break;
				}
				return result;
			}
		}

		static DocumentStatusCodeList GetShipmentActionCode(ZString shipmentActionCode, bool isSplitShipment)
		{
			DocumentStatusCodeList result = null;
			if (isSplitShipment)
			{
				result = DocumentStatusCodeList.Status2;
			}
			else
			{
				switch (shipmentActionCode)
				{
					case MessageActionCodes.Codes.Original:
						result = DocumentStatusCodeList.Status1;
						break;
					case MessageActionCodes.Codes.Cancellation:
						result = DocumentStatusCodeList.Status0;
						break;
					case MessageActionCodes.Codes.Change:
						result = DocumentStatusCodeList.Status2;
						break;
				}
			}
			return result;
		}

		protected override ZString GetMessageSubType()
		{
			var result = ZString.Empty;
			if (messageSubType == MessageSubTypes.ReplaceHeader || messageSubTypeShouldBeChange)
			{
				result = (ZString)MessageActionCodes.Codes.Change;
			}
			else
			{
				result = base.GetMessageSubType();
			}

			return result;
		}

		void ICompleteManifestMessageBuilder.GenerateMessageContent(Enterprise.Messaging.Business.EDIMessage message)
		{
			this.GenerateMessageContent(PopulateEdifactMessage, () => edifactMessage.ToString(characterSet), () => interpretation.ToHtml(), message);
		}

		readonly bool isCompleteManifest;
		readonly ZBool lazyGenerateMessageContent;
		readonly ZString messageType;

		#endregion

		#endregion
	}
}
