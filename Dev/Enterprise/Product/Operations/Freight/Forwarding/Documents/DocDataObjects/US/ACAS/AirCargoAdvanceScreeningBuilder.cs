using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using EventConstants = CargoWise.EventReference.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using ResString = Enterprise.Freight.Forwarding.Documents.DataObjects.ResString;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	public class AirCargoAdvanceScreeningBuilder
	{
		public AirCargoAdvanceScreeningBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));

			this.parameters = parameters;
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		protected readonly ForwardingShipment shipment;
		protected readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public AirCargoAdvanceScreening Build()
		{
			var acas = CreateACAS();

			var consol = MovementLegComparer.FirstOrDefaultLegForTransportMode(shipment.Consols.OfType<ForwardingConsol>(), Core.Constants.TransportModes.Air);

			var transports = consol != null
				? consol.Transports
				: shipment.Transports;

			var airLegDischargingInUS = transports
				.OfType<Freight.Business.Transport>()
				.Where(t => !t.JW_ETD.IsEmpty)
				.OrderBy(t => t.JW_ETD)
				.FirstOrDefault(transport => transport.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) && transport.IsAir);

			airLegDischargingInUS = airLegDischargingInUS ?? transports
				.OfType<Freight.Business.Transport>()
				.OrderBy(t => t.JW_LegOrder)
				.FirstOrDefault(transport => transport.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) && transport.IsAir);

			PopulateGeneral(acas, consol, airLegDischargingInUS);
			PopulateWaybillDetails(acas, consol);
			PopulateLocations(acas, consol, airLegDischargingInUS);
			PopulateAddressDetails(acas, consol, airLegDischargingInUS);
			PopulateStateDetails(acas);
			PopulateDisplayInformation(acas);

			acas.ValidateAllIncludingChildren();

			return acas;
		}

		#region Implementation

		protected virtual AirCargoAdvanceScreening CreateACAS()
		{
			return new AirCargoAdvanceScreening(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef,
				DocumentNames.AdvancedCargoReport);
		}

		void PopulateGeneral(AirCargoAdvanceScreening acas, ForwardingConsol consol, Freight.Business.Transport airLegDischargingInUS)
		{
			PopulateHarmonizedCodes(acas);

			PopulateGoodsDescription(acas);
			acas.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("969C1E02-C3FA-49D9-A04A-60B14044681E", "Goods Description is required."));
			acas.GoodsDescriptionInfo.AddMessageError(() => acas.GoodsDescription.Length > 490, Res.GetString("7C765818-561F-407E-8833-C30E0FD0EAA8", "Goods Description has a limit of 490 characters."));
			acas.GoodsDescriptionInfo.AddSupportedCharactersValidationForUSCustoms();

			acas.ConsolNumber = consol?.JK_UniqueConsignRef ?? ZString.Empty;
			acas.ConsolNumberInfo.AddMessageError(() => acas.HAWB.IsEmpty && acas.ConsolNumber.IsEmpty, Res.GetString("83F3E1AF-E088-4135-9B1D-265C1D41E7A2", "Either HAWB Number or Consol Number is required."));

			acas.ConsolType = new CodeDescription(consol?.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol?.JK_AgentType ?? ZString.Empty
			};

			acas.FlightNumber = airLegDischargingInUS?.JW_VoyageFlight ?? ZString.Empty;
			acas.ETA = airLegDischargingInUS?.JW_ETA ?? ZDateTime.Empty;

			PopulateNumberOfPacks(acas);
			acas.NumberOfPacksInfo.AddMessageErrorIfEmpty(Res.GetString("4C9F7C71-C4D6-446F-BD75-B65EB9C44DE1", "Packs are required."));

			PopulateWeight(acas);
			acas.Weight.ValueInfo.AddMessageError(() => acas.Weight.Value <= 0m, Res.GetString("428E7AAD-383A-4E33-A88A-572E0FDB7589", "Weight is required."));
		}

		protected virtual void PopulateHarmonizedCodes(AirCargoAdvanceScreening acas)
		{
			acas.HarmonizedCodes = shipment.OuterPackLines.OfType<PackLine>().GroupBy(pk => pk.JL_HarmonisedCode).Where(pl => !pl.Key.IsEmpty).Select(pl => pl.Key).ToList();
		}

		protected virtual void PopulateGoodsDescription(AirCargoAdvanceScreening acas)
		{
			var goodsDescription = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).FirstOrDefault();
			acas.GoodsDescription = goodsDescription != null
				? goodsDescription.ST_NoteDataAsText
				: shipment.JS_GoodsDescription;
		}

		protected virtual void PopulateNumberOfPacks(AirCargoAdvanceScreening acas)
		{
			acas.NumberOfPacks = shipment.JS_OuterPacks;
		}

		protected virtual void PopulateWeight(AirCargoAdvanceScreening acas)
		{
			var newWeightUnit = Core.Constants.Weight.IsImperial(shipment.JS_UnitOfWeight)
				? Core.Constants.Weight.Pounds
				: Core.Constants.Weight.Kilograms;

			acas.Weight = new Measurement()
			{
				Value = Core.Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, newWeightUnit),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = newWeightUnit
				}
			};
		}

		void PopulateWaybillDetails(AirCargoAdvanceScreening acas, ForwardingConsol consol)
		{
			PopulateHAWB(acas);
			acas.HAWBInfo.AddMessageError(() => acas.HAWB.IsEmpty && acas.ConsolNumber.IsEmpty, Res.GetString("8B0D8A21-3E2D-4D7C-8CB1-24571F47929A", "Either HAWB Number or Consol Number is required."));
			acas.HAWBInfo.AddMessageError(() => !acas.ConsolNumber.IsEmpty && acas.HAWB.IsEmpty && acas.ConsolType.Code != Core.Constants.AgentType.Direct, Res.GetString("8dd7f802-b2cd-cc9d-493b-a8b01ae362b9", "HAWB number is required for non-direct consolidations."));

			var mawbNumber = consol?.JK_MasterBillNum ?? ZString.Empty;
			var mawbWarning = !mawbNumber.IsEmpty
				? MasterBillValidator.GetMAWBFormatValidMessage(mawbNumber, consol?.Factory)
				: ZString.Empty;

			acas.MAWB = mawbNumber.FormatAirMAWB();
			acas.MAWBInfo.AddWarning(() => !mawbWarning.IsEmpty, mawbWarning);
			acas.MAWBInfo.AddMessageError(() => acas.MAWB.IsEmpty && acas.ConsolType.Code == Core.Constants.AgentType.Direct, Res.GetString("0c7d7ff5-4ff2-c386-4546-a73b35c29b55", "MAWB number is required for direct consolidations."));
		}

		protected virtual void PopulateHAWB(AirCargoAdvanceScreening acas)
		{
			if (acas.ConsolType.Code != Core.Constants.AgentType.Direct)
			{
				acas.HAWB = shipment.JS_HouseBill;
			}
		}

		void PopulateLocations(AirCargoAdvanceScreening acas, ForwardingConsol consol, Freight.Business.Transport airLegDischargingInUS)
		{
			Unloco departure;
			ZString originIata;
			Unloco arrival;
			ZString destinationIata;

			if (consol == null && airLegDischargingInUS == null)
			{
				departure = Unloco.Create(context, shipment.Origin);
				originIata = shipment.Origin?.RL_IATA ?? ZString.Empty;
				arrival = Unloco.Create(context, shipment.Destination);
				destinationIata = shipment.Destination?.RL_IATA ?? ZString.Empty;
			}
			else
			{
				departure = Unloco.Create(context, airLegDischargingInUS?.LoadPort);
				originIata = airLegDischargingInUS?.LoadPort?.RL_IATA ?? ZString.Empty;
				arrival = Unloco.Create(context, airLegDischargingInUS?.DiscPort);
				destinationIata = airLegDischargingInUS?.DiscPort?.RL_IATA ?? ZString.Empty;
			}

			acas.Departure = departure;
			acas.Arrival = arrival;
			acas.PortOfOriginIata = originIata;
			acas.PortOfFirstArrivalIata = destinationIata;
		}

		void PopulateAddressDetails(AirCargoAdvanceScreening acas, ForwardingConsol consol, Freight.Business.Transport airLegDischargingInUS)
		{
			acas.Carrier = AddressBuilder.Create(context, airLegDischargingInUS?.CarrierAddress)
				.AddSupportedCharactersValidationForUSCustoms();

			acas.CTO = AddressBuilder.Create(context, consol?.ArrivalCTOAddress)
				.AddSupportedCharactersValidationForUSCustoms();
			acas.CTO.CompanyNameInfo.AddMessageError(() => !acas.CTO.IsEmpty() && consol != null && !consol.ArrivalCTOAddress.CustomsCodes.OfType<OrgCusCode>().Any(IsUSAFirmsCode), CTOFIRMSCodeEmptyErrorMessage);

			PopulateShipper(acas);
			PopulateConsignee(acas);

			var orgProxy = GlbBranch.CurrentBranch?.OrgProxy?.CustomsCodes.OfType<OrgCusCode>().Any(IsUSACASCode) ?? false
				? GlbBranch.CurrentBranch?.OrgProxy
				: GlbCompany.CurrentCompany?.OrgProxy;

			acas.BookingParty = AddressBuilder.Create(context, orgProxy?.MainAddress);

			acas.SendersAcasCode = orgProxy?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(IsUSACASCode)?.OK_CustomsRegNo ?? ZString.Empty;
			acas.SendersAcasCodeInfo.AddMessageErrorIfEmpty(Res.GetString("621A56A3-7D95-4861-ABAC-1771DFC565B4", "This code is required for ACAS messaging. {0}", ACASCodeEmptyErrorMessage));

			PopulateAddressNotifyParty(acas, consol);
		}

		protected virtual void PopulateShipper(AirCargoAdvanceScreening acas)
		{
			acas.Shipper = AddressBuilder.Create(context, shipment.ConsignorDocumentaryAddress)
				.AddPartyNameAndAddressValidation(nameof(acas.Shipper))
				.AddSupportedCharactersValidationForUSCustoms();
		}

		protected virtual void PopulateConsignee(AirCargoAdvanceScreening acas)
		{
			acas.Consignee = AddressBuilder.Create(context, shipment.ConsigneeDocumentaryAddress)
				.AddPartyNameAndAddressValidation(nameof(acas.Consignee))
				.AddSupportedCharactersValidationForUSCustoms();
		}

		void PopulateAddressNotifyParty(AirCargoAdvanceScreening acas, ForwardingConsol consol)
		{
			acas.NotifyParty = AddressBuilder.Create(context, shipment.NotifyPartyDocumentaryAddress)
				.AddSupportedCharactersValidationForUSCustoms();

			var notifyPartyType = new CodeDescription(PartyTypes);
			notifyPartyType.Code = GetDefaultNotifyPartyType(consol);
			notifyPartyType.CodeInfo.AddMessageError(() => !acas.NotifyPartyType.Code.IsEmpty && !PartyTypes.ContainsCode(acas.NotifyPartyType.Code), Res.GetString("4D9069CC-37B0-4797-A4AC-A157CF3C1592", "Notify Party Type is not valid."));
			notifyPartyType.CodeInfo.AddMessageError(() => acas.NotifyPartyType.Code.IsEmpty && !acas.NotifyParty.IsEmpty(), Res.GetString("3CBE4F71-FED0-4A48-AF37-0369DB226B27", "Notify Party's type is required when Notify Party is entered."));

			acas.NotifyPartyType = notifyPartyType;
			var notifyPartyACASCode = GetNotifyPartyCode(IsUSACASCode);
			var notifyPartyFirmsCode = GetNotifyPartyCode(IsUSAFirmsCode);

			acas.NotifyPartysAcasCode = string.IsNullOrWhiteSpace(notifyPartyACASCode) ? notifyPartyFirmsCode : notifyPartyACASCode;
		}

		ZString GetDefaultNotifyPartyType(ForwardingConsol consol)
		{
			if (shipment.NotifyParty != null)
			{
				if (consol != null && (shipment.NotifyParty.PK == consol.SendingForwarderPK || shipment.NotifyParty.PK == consol.ReceivingForwarderPK))
				{
					return PartyTypes.Codes.Agent;
				}
				if (shipment.ConsignorPK == shipment.NotifyParty.PK)
				{
					return PartyTypes.Codes.SellingParty;
				}
				if (shipment.ConsigneePK == shipment.NotifyParty.PK)
				{
					return PartyTypes.Codes.BuyingParty;
				}
				return PartyTypes.Codes.AdditionalContact;
			}
			return "";
		}

		ZString GetNotifyPartyCode(Func<OrgCusCode, bool> filter)
		{
			return shipment.NotifyParty?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(filter)?.OK_CustomsRegNo ?? ZString.Empty;
		}

		bool IsUSACASCode(OrgCusCode code)
		{
			return code.OK_CodeType == OrgCusCode.USACodeTypes.ACASOriginatorCode && code.OK_RN_NKCodeCountry.StartsWith(Core.Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase);
		}

		bool IsUSAFirmsCode(OrgCusCode code)
		{
			return code.OK_CodeType == OrgCusCode.USACodeTypes.FIRMSCode && code.OK_RN_NKCodeCountry.StartsWith(Core.Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase);
		}

		PartyTypes PartyTypes => partyTypes ?? (partyTypes = new PartyTypes());
		PartyTypes partyTypes;

		#region State

		protected virtual void PopulateStateDetails(AirCargoAdvanceScreening acas)
		{
			acas.State = GetState(parameters?.LogProvider);
		}

		internal static AcasState GetState(IStmALogProvider logProvider)
		{
			var orderedLogs = GetOrderedLogs(logProvider);
			var state = GetCurrentState(orderedLogs);

			return state;
		}

		static AcasState GetCurrentState(StmALog[] orderedLogs)
		{
			var firstLog = orderedLogs.FirstOrDefault();
			if (firstLog == null)
			{
				return AcasState.None;
			}

			switch (firstLog.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
					return GetStateWhenMessageHasBeenSent(orderedLogs);

				case Events.MessagePendingProcessingCode:
					return AcasState.AssessmentOngoing;

				case Events.InterchangeSentCode:
					return GetStateWhenInterchangeSent(orderedLogs);

				case Events.InterchangeRejectedCode:
				case Events.MessageRejectedCode:
					var logsWithRecentFailureRemoved = RemoveUntilAfterMessageSentEvent(orderedLogs);
					return GetCurrentState(logsWithRecentFailureRemoved);

				case Events.HeldCode:
					return GetStateFromHeldCustomsLog(firstLog);

				case Events.ClearedHoldCode:
					return AcasState.HoldRemoved;

				case Events.ClearanceCompletedCode:
					return AcasState.AssessmentComplete;

				default:
					return AcasState.None;
			}
		}

		static StmALog[] RemoveUntilAfterMessageSentEvent(StmALog[] logs)
		{
			return logs.SkipWhile(log => log.SL_SE_NKEvent != Events.MessageSentCode).Skip(1).ToArray();
		}

		static AcasState GetStateWhenMessageHasBeenSent(StmALog[] orderedLogs)
		{
			var removeUntilAfterMessageSentEvents = RemoveUntilAfterMessageSentEvent(orderedLogs);
			var previouslyReceivedLog = removeUntilAfterMessageSentEvents.FirstOrDefault();
			if (previouslyReceivedLog == null)
			{
				return AcasState.OriginalSent;
			}

			if (previouslyReceivedLog.SL_SE_NKEvent == Events.InterchangeRejectedCode || previouslyReceivedLog.SL_SE_NKEvent == Events.MessageRejectedCode)
			{
				return GetStateWhenMessageHasBeenSent(removeUntilAfterMessageSentEvents);
			}

			return GetCurrentStateFromPreviouslyReceivedLog(previouslyReceivedLog);
		}

		static AcasState GetStateWhenInterchangeSent(StmALog[] orderedLogs)
		{
			var removeUntilAfterMessageSentEvents = RemoveUntilAfterMessageSentEvent(orderedLogs);
			var previouslyReceivedLog = removeUntilAfterMessageSentEvents.FirstOrDefault();
			if (previouslyReceivedLog == null)
			{
				return AcasState.OriginalSent;
			}

			if (previouslyReceivedLog.SL_SE_NKEvent == Events.InterchangeRejectedCode || previouslyReceivedLog.SL_SE_NKEvent == Events.MessageRejectedCode)
			{
				return GetStateWhenInterchangeSent(removeUntilAfterMessageSentEvents);
			}

			return IsAmendmentRequiredForSelecteeDataIssueHold(previouslyReceivedLog)
				? AcasState.AmendmentRequired
				: GetCurrentStateFromPreviouslyReceivedLog(previouslyReceivedLog);
		}

		static AcasState GetCurrentStateFromPreviouslyReceivedLog(StmALog previouslyReceivedLog)
		{
			switch (previouslyReceivedLog.SL_SE_NKEvent)
			{
				case Events.InterchangeSentCode:
					return AcasState.AmendmentSent;

				case Events.HeldCode:
					return GetStateWhenPreviousMessageIsHold(previouslyReceivedLog);

				case Events.MessageSentCode:
				case Events.ClearedHoldCode:
				case Events.ClearanceCompletedCode:
					return AcasState.OriginalSent;

				default:
					return AcasState.None;
			}
		}

		static bool IsAmendmentRequiredForSelecteeDataIssueHold(StmALog latestLogFromCustoms)
		{
			return latestLogFromCustoms.SL_SE_NKEvent == Events.HeldCode
				&& latestLogFromCustoms.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Reason) == EventConstants.ACASActions.Code.SelecteeDataIssueHold;
		}

		static AcasState GetStateWhenPreviousMessageIsHold(StmALog holdLog)
		{
			var reason = holdLog.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Reason);

			switch (reason)
			{
				case EventConstants.ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace:
					return AcasState.AmendmentSent;

				case EventConstants.ACASActions.Code.DoNotLoadHold:
				case EventConstants.ACASActions.Code.SelecteeDataIssueHold:
				case EventConstants.ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold:
					return AcasState.AcknowledgementSent;

				default:
					return AcasState.None;
			}
		}

		static AcasState GetStateFromHeldCustomsLog(StmALog heldLogFromCustoms)
		{
			var reason = heldLogFromCustoms.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Reason);

			switch (reason)
			{
				case EventConstants.ACASActions.Code.DoNotLoadHold:
				case EventConstants.ACASActions.Code.SelecteeDataIssueHold:
				case EventConstants.ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold:
					return AcasState.AcknowledgementRequired;

				case EventConstants.ACASActions.Code.DoNotLoadHoldCurrentlyInPlace:
				case EventConstants.ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace:
					return AcasState.HoldInPlace;

				case EventConstants.ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace:
					return AcasState.AmendmentRequired;

				default:
					return AcasState.None;
			}
		}

		#region DisplayInformation

		void PopulateDisplayInformation(AirCargoAdvanceScreening acas)
		{
			acas.DisplayInformation = GetDisplayInformation(acas.State);
		}

		protected ZString GetDisplayInformation(AcasState state)
		{
			switch (state)
			{
				case AcasState.OriginalSent:
					return Res.GetString("C9EDC91B-EB6D-4BB7-94A2-2D37BBFA53C6", @"The message previously sent has not yet received a response.
Please wait for a response before resending.");

				case AcasState.AmendmentSent:
					return Res.GetString("4D218396-DD5A-4A2D-94E1-99AD0435C7C2", @"An amendment message in response to Selectee Data Issues has been sent.
Please wait for a response before further action.");

				case AcasState.AcknowledgementSent:
					return Res.GetString("D06233DD-2209-4C3C-B53B-2AC67E034378", @"An Acknowledgement message has been sent and has not received a response.
Please wait for a response before further action.");

				case AcasState.AcknowledgementRequired:
					return Res.GetString("8D8D23B1-75B7-4653-91DD-77E1B7D4B7D1", @"The latest response from CBP is ""On Hold"" (6H, 7H or 8H).
Use the 'Send Message' option to send an Acknowledgement message.");

				case AcasState.HoldInPlace:
					return Res.GetString("806281A0-0365-4FB3-9BA0-E797822A48FA", @"This Shipment is currently on ""Hold Currently in Place"" with CBP, per the latest response.
Wait for a 6I, 7I or 8I ""Hold Removed"" response before resending the message.");

				case AcasState.AmendmentRequired:
					return Res.GetString("8EEB7A4B-203C-4007-9FC8-B9DA61F1D3D0", @"The latest status received from US Customs is Selectee Data Issue Hold.
Amend the data and resend the message to resolve the hold.");

				case AcasState.AssessmentOngoing:
					return Res.GetString("0D0CB6C1-F067-405C-B32A-3AF7223613B7", @"A risk assessment is currently being conducted by CBP.
Please wait for an additional response before further action.");

				case AcasState.HoldRemoved:
					return Res.GetString("62B4816D-635F-486A-A231-C4B2AB4F5E1A", @"The ""Hold"" status of this Shipment has been removed by CBP.
This Shipment has been approved to be uplifted/loaded on a flight to United States.");

				case AcasState.AssessmentComplete:
					return Res.GetString("F167AF71-356A-44FF-8C74-F423A898513A", @"This Shipment has been approved to be uplifted/loaded on a flight to United States.");

				case AcasState.None:
				default:
					return ZString.Empty;
			}
		}

		#endregion

		#region Logs

		internal static StmALog[] GetOrderedLogs(IStmALogProvider logProvider)
		{
			return logProvider?
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderByDescending(l => l.SL_PostedTimeUtc)
				.Where(LogIsApplicable)
				.ToArray() ?? Array.Empty<StmALog>();
		}

		static bool LogIsApplicable(StmALog log)
		{
			switch (log.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
				case Events.InterchangeSentCode:
				case Events.InterchangeRejectedCode:
				case Events.MessageRejectedCode:
				case Events.MessagePendingProcessingCode:
				case Events.ClearanceCompletedCode:
				case Events.HeldCode:
				case Events.ClearedHoldCode:
					var location = log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Location) ?? string.Empty;

					if (location.StartsWith(Core.Constants.CountryCodes.UnitedStates))
					{
						var messageType = log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType);
						return string.Compare(messageType,
							DocumentNames.AdvancedCargoReport,
							StringComparison.OrdinalIgnoreCase) == 0;
					}

					return false;

				default:
					return false;
			}
		}

		#endregion

		#endregion

		public static MultilingualString CTOFIRMSCodeEmptyErrorMessage => ResString.GetMultilingualString("F5EC4F36-B7CB-46C6-A9E9-825094E97470", "CTO FIRMS code is required when CTO is entered.");

		public static MultilingualString ACASCodeEmptyErrorMessage => ResString.GetMultilingualString("07bda1ee-5d58-4764-8107-553787203f29", "Raise an eRequest to register your interest. Once provided by WTG, enter the code against the Branch or Company Organization Proxy > Config > Registration Numbers/Codes tab using Type = US ACA.");

		#endregion
	}
}
