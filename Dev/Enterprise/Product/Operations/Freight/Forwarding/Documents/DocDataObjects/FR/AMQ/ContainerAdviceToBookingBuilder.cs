using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using FRPortHelpers = Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Helpers;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class ContainerAdviceToBookingBuilder
	{
		public ContainerAdviceToBookingBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			Argument.NotNull(consol, nameof(consol));
			Argument.NotNull(parameters, nameof(parameters));

			this.consol = consol;
			this.parameters = parameters;
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public ContainerAdviceToBooking Build()
		{
			var containerAdvice = new ContainerAdviceToBooking(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			containerAdvice.ConsolNumber = consol.JK_UniqueConsignRef;
			containerAdvice.CarrierBookingReference = consol.JK_BookingReference;
			containerAdvice.BillOfLading = consol.JK_MasterBillNum;

			containerAdvice.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List) { Code = consol.JK_ConsolMode };

			containerAdvice.ShipmentType = new CodeDescription(consol.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol.JK_AgentType
			};

			containerAdvice.RequiresTemperatureControl = consol.JK_RequiresTemperatureControl;
			containerAdvice.TemperatureMinimum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMinimum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};

			containerAdvice.TemperatureMaximum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMaximum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};

			PopulateTransportDetailsAndAddresses(containerAdvice);
			PopulateContainers(containerAdvice);

			AddValidations(containerAdvice);
			containerAdvice.ValidateAllIncludingChildren();

			return containerAdvice;
		}

		void PopulateTransportDetailsAndAddresses(ContainerAdviceToBooking containerAdvice)
		{
			PopulateTransportDetails(containerAdvice);
			PopulateAddresses(containerAdvice);
		}

		void PopulateAddresses(ContainerAdviceToBooking containerAdvice)
		{
			containerAdvice.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			containerAdvice.CarrierSON = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			containerAdvice.CarrierCI5 = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			containerAdvice.FormattedCarrierProviderID = FRPortHelpers.CreateFormattedProviderID(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SON, containerAdvice.CarrierSON.ValueInfo, containerAdvice.CarrierCI5.ValueInfo);

			containerAdvice.CarrierBookingAgent = AddressBuilder.Create(context, consol.CarrierBookingAgentDocumentaryAddress);
			containerAdvice.CarrierBookingAgentSON = consol.CarrierBookingAgentDocumentaryAddress?.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			containerAdvice.CarrierBookingAgentCI5 = consol.CarrierBookingAgentDocumentaryAddress?.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

			containerAdvice.CTO = AddressBuilder.Create(context, consol.DepartureCTOAddress);
			containerAdvice.CTOSON = consol.DepartureCTOAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			containerAdvice.CTOCI5 = consol.DepartureCTOAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			containerAdvice.FormattedCTOProviderID = FRPortHelpers.CreateFormattedProviderID(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SON, containerAdvice.CTOSON.ValueInfo, containerAdvice.CTOCI5.ValueInfo);

			var transporterAddress = consol.DeparturePackCFSTransportAddress ?? consol.TopLevelShipments.Cast<CommonShipment>().FirstOrDefault()?.DocsAndCartage.PickupCartageCoAddr;
			containerAdvice.Transporter = AddressBuilder.Create(context, transporterAddress);
			containerAdvice.TransporterSON = transporterAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			containerAdvice.TransporterCI5 = transporterAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			containerAdvice.FormattedTransporterProviderID = FRPortHelpers.CreateFormattedProviderID(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SON, containerAdvice.TransporterSON.ValueInfo, containerAdvice.TransporterCI5.ValueInfo);

			containerAdvice.SendingParty = AddressBuilder.Create(context, consol.SendingForwarderWithContact);
			containerAdvice.SendingPartySON = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			containerAdvice.SendingPartyCI5 = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, false, containerAdvice.OperationalPort);
			containerAdvice.SendingPartySOA = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA, false, containerAdvice.OperationalPort);
			containerAdvice.SendingPartySOW = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			containerAdvice.FormattedSendingPartyProviderID = FRPortHelpers.CreateFormattedProviderID(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SOA, containerAdvice.SendingPartySOA.ValueInfo, containerAdvice.SendingPartyCI5.ValueInfo);

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			containerAdvice.Forwarder = AddressBuilder.CreateForCurrentUser(context);
			containerAdvice.ForwarderCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, containerAdvice.OperationalPort);
			containerAdvice.ForwarderSOA = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			containerAdvice.ForwarderSON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, containerAdvice.OperationalPort);
			containerAdvice.ForwarderSOW = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			containerAdvice.FormattedForwarderProviderID = FRPortHelpers.CreateFormattedProviderID(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SON, containerAdvice.ForwarderSON.ValueInfo, containerAdvice.ForwarderCI5.ValueInfo);

			containerAdvice.SendingForwarder = AddressBuilder.Create(context, consol.SendingForwarderWithContact);
			containerAdvice.SendingForwarderSON = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			containerAdvice.SendingForwarderCI5 = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

			containerAdvice.ReceivingForwarder = AddressBuilder.Create(context, consol.ReceivingForwarderWithContact);
			containerAdvice.ReceivingForwarderSON = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			containerAdvice.ReceivingForwarderCI5 = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
		}

		void PopulateTransportDetails(ContainerAdviceToBooking containerAdvice)
		{
			var firstFrenchSeaLeg = SeaTransportsInLegOrder.FirstOrDefault(t => Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKLoadPort));

			containerAdvice.PortOfOrigin = Unloco.Create(context, firstFrenchSeaLeg?.LoadPort);
			var transShipment = Unloco.Create(context, SeaTransportsInLegOrder.GetFranceTransshipmentPort(true));
			containerAdvice.PortOfTranshipment = transShipment.Code.IsEmpty ? Unloco.Create(context, consol.DischargePort) : transShipment;
			containerAdvice.PortOfDestination = Unloco.Create(context, SeaTransportsInLegOrder.LastOrDefault()?.DiscPort);

			var operationalPort = consol.DepartureCTOAddress?.RelatedPortCode ?? SeaTransportsInLegOrder.FirstOrDefault()?.LoadPort;
			containerAdvice.PCS = operationalPort?.GetPCS() ?? ZString.Empty;
			containerAdvice.OperationalPort = Unloco.Create(context, operationalPort);

			containerAdvice.PortArea = consol.DepartureCTOAddress.GetPortArea();
			containerAdvice.PortLocation = consol.DepartureCTOAddress.GetPortLocation();

			containerAdvice.ATPReference = firstFrenchSeaLeg?.Sailing?.JX_JA_DepartureReference ?? ZString.Empty;
			containerAdvice.VoyageNumber = firstFrenchSeaLeg?.JW_VoyageFlight ?? ZString.Empty;
		}

		void PopulateContainers(ContainerAdviceToBooking containerAdvice)
		{
			var containerBizObjs = GetContainers();
			var containers = new List<BookingContainer>();

			var containerBuilder = new BookingContainerBuilder();
			var packingLineBuilder = new BookingPackingLineBuilder();

			foreach (var containerBizObj in containerBizObjs)
			{
				var packingLines = new List<BookingPackingLine>();

				foreach (var packlineBizObj in containerBizObj.PackLines.OfType<ForwardingPackLine>())
				{
					var packingLine = packingLineBuilder.Build(packlineBizObj);
					packingLines.Add(packingLine);
				}

				var container = containerBuilder.Build(containerBizObj, packingLines.ToArray());
				container.TransportMode = new CodeDescription(() => null)
				{
					Code = "RTE" // non-translatable
				};

				container.ECTReference = containerBizObj.JC_ExportDepotCustomsReference;
				container.AMQReference = GetAMQReferenceFromEvents(container);

				containers.Add(container);
			}

			containerAdvice.Containers = containers;
		}

		string GetAMQReferenceFromEvents(BookingContainer container)
		{
			foreach (var log in GetEventLogsInDescendingOrder())
			{
				if (log.SL_SE_NKEvent == Events.MessageAcceptedCode)
				{
					log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumber);
					if (equipmentReferenceNumber == container.Number)
					{
						log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber);
						return referenceNumber;
					}
				}
			}
			return string.Empty;
		}

		#region Validations

		void AddValidations(ContainerAdviceToBooking containerAdvice)
		{
			AddAddressValidations(containerAdvice);
			AddTransportValidations(containerAdvice);
			AddContainerValidations(containerAdvice);
			AddAdditionalValidations(containerAdvice);
		}

		void AddAddressValidations(ContainerAdviceToBooking containerAdvice)
		{
			var sendingPartyName = Res.GetString("E04D39B0-22B0-4CE2-BE36-43EC58BEA395", "Sending Party");
			var sendingPartyCodeType = Res.GetString("1db527f2-3062-4554-8bd9-29b505c721b5", "Agent");
			var sendingPartyConfigPath = Res.GetString("b772e64c-7329-4891-a387-dde4776aa1b7", "Consol > Sending Agent");

			var forwarderPartyName = Res.GetString("40516b7c-7ea8-43d2-9712-570fd29b9241", "Forwarder");
			var forwarderConfigPath = Res.GetString("d588267f-bf20-42f0-ba05-d9ebc2fc293b", "Org. Proxy");

			var carrierPartyName = Res.GetString("df5e986d-bdfb-4223-9ffa-1c7752d96d73", "Carrier");
			var carrierConfigPath = Res.GetString("0ab7ba64-ab16-4391-8508-ceb394a0f5a2", "Consol > Carrier");

			var messageValidation = Res.GetString("c9981c74-475f-4988-aa4c-83cb025fb0aa", "name and address is required.");
			containerAdvice.SendingParty.AddPartyNameAndAddressValidation(sendingPartyName, messageValidation);
			containerAdvice.Forwarder.AddPartyNameAndAddressValidation(forwarderPartyName, messageValidation);
			containerAdvice.Carrier.AddPartyNameAndAddressValidation(carrierPartyName, messageValidation);

			containerAdvice.FormattedSendingPartyProviderIDInfo.AddFormattedProviderIDValidations(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SOA, sendingPartyName, sendingPartyCodeType, sendingPartyConfigPath);
			containerAdvice.FormattedForwarderProviderIDInfo.AddFormattedProviderIDValidations(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SON, forwarderPartyName, forwarderPartyName, forwarderConfigPath);
			containerAdvice.FormattedCarrierProviderIDInfo.AddFormattedProviderIDValidations(containerAdvice.PCS, OrgCusCode.FranceCodeTypes.SON, carrierPartyName, configPath: carrierConfigPath, isOrganization: true);
		}

		void AddTransportValidations(ContainerAdviceToBooking containerAdvice)
		{
			containerAdvice.PortAreaInfo.AddMessageErrorIfEmpty(Res.GetString("0522E592-1573-4C32-ACB5-09F06631FB88", "Port Area is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')"));
			containerAdvice.PortLocationInfo.AddMessageErrorIfEmpty(Res.GetString("0EB8A46E-1C00-4F4B-86F0-0B262D14E2A8", "Port Location is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')"));
		}

		void AddContainerValidations(ContainerAdviceToBooking containerAdvice)
		{
			foreach (var container in containerAdvice.Containers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("bc118cf5-d0a4-4b10-8ac6-79a161caa3e0", "Container number is required."));
				container.TransportMode.CodeInfo.AddMessageError(() => container.TransportMode.Code.IsEmpty, Res.GetString("171A7A68-32BA-424E-85CD-870DAF3C3DC4", "Transport Mode is required."));
				container.TransportMode.CodeInfo.AddMessageError(() => !container.TransportMode.Code.IsEmpty && container.TransportMode.Code.Length > 3, Res.GetString("e2e04593-e80e-4c85-98fb-b5103c85f1fe", "Transport Mode must not exceed three (3) characters."));
				container.Type.ISOCodeInfo.AddMessageError(() => container.Type.ISOCode.IsEmpty, Res.GetString("5FA716C5-F58A-429E-B820-02BB449214D8", "This container does not have a valid ISO Code. Enter a valid ISO code here or add it to the Container Reference File via Consol > Container > Container Type."));
				container.ECTReferenceInfo.AddAsciiCharactersValidation();

				foreach (var packingLine in container.PackingLines)
				{
					packingLine.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("6F5808B8-A75B-4510-BEE7-EECC7CE54D5D", "Goods Description is required."));
					packingLine.GoodsDescriptionInfo.AddAsciiCharactersValidation();
				}

				if (container.FumigationService is AdditionalService fumigationService)
				{
					fumigationService.ServiceNoteInfo.AddAsciiCharactersValidation();
				}

				container.HandlingNotesInfo.AddAsciiCharactersValidation();
			}
		}

		void AddAdditionalValidations(ContainerAdviceToBooking containerAdvice)
		{
			containerAdvice.BookingConfirmationCBKInfo.AddAsciiCharactersValidation();
			containerAdvice.OTCInfo.AddAsciiCharactersValidation();
			containerAdvice.ATPReferenceInfo.AddAsciiCharactersValidation();
			containerAdvice.OTCReferenceInfo.AddAsciiCharactersValidation();

			bool areBothCBKEmpty() => containerAdvice.CarrierBookingReference.IsEmpty && containerAdvice.BookingConfirmationCBK.IsEmpty;
			containerAdvice.CarrierBookingReferenceInfo.AddMessageError(areBothCBKEmpty, Res.GetString("F11E8EB2-6175-4FC8-A2D7-FC317EAC8D2C", "Carrier Booking Ref or CBK reference is required before sending this message."));
			containerAdvice.BookingConfirmationCBKInfo.AddMessageError(areBothCBKEmpty, Res.GetString("2faaa6a2-aa13-481c-9303-aa80d8fb7375", "Carrier Booking Ref or CBK reference is required before sending this message."));

			bool areOTCATPVoyageEmpty() => containerAdvice.OTCReference.IsEmpty && containerAdvice.ATPReference.IsEmpty && containerAdvice.VoyageNumber.IsEmpty;
			containerAdvice.OTCReferenceInfo.AddMessageError(areOTCATPVoyageEmpty, Res.GetString("0C246F18-1BF3-44B8-BBE9-3A81BED330E6", "OTC Reference or ATP reference or Voyage Number is required before sending this message."));
			containerAdvice.ATPReferenceInfo.AddMessageError(areOTCATPVoyageEmpty, Res.GetString("C65AAAF3-363A-47ED-AD17-413936559B6F", "OTC Reference or ATP reference or Voyage Number is required before sending this message."));
			containerAdvice.VoyageNumberInfo.AddMessageError(areOTCATPVoyageEmpty, Res.GetString("33E48270-5F21-42E2-840D-AE3940185E0D", "OTC Reference or ATP reference or Voyage Number is required before sending this message."));

			containerAdvice.PCSInfo.AddPCSValidation();
			((Unloco)containerAdvice.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("fd7838e1-a0a8-4ce5-9c43-119cc5eba3bf", "Operational Port is required, UNLOCO is missing from Consol > Departure > CTO Address or from the Load Port of the first SEA leg."));
		}

		#endregion

		#region Implement

		IReadOnlyCollection<ForwardingContainer> GetContainers()
		{
			if (parameters?.Data is IReadOnlyCollection<ForwardingContainer> containers)
			{
				return containers;
			}

			return Array.Empty<ForwardingContainer>();
		}

		IReadOnlyCollection<Freight.Business.Transport> SeaTransportsInLegOrder => seaTransportsInLegOrder ?? (seaTransportsInLegOrder = GetSeaTransports());
		IReadOnlyCollection<Freight.Business.Transport> seaTransportsInLegOrder;

		IReadOnlyCollection<Freight.Business.Transport> GetSeaTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea)
				.ToArray();
		}

		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			foreach (var log in consol.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);
				var documentName = FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ;

				if (string.Compare(messageType, documentName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}

		#endregion
	}
}
