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
	sealed class ImportManifestBuilder
	{
		public ImportManifestBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			Argument.NotNull(consol, nameof(consol));

			this.consol = consol;
			this.parameters = parameters;
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public ImportManifest Build()
		{
			var importManifest = new ImportManifest(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			importManifest.ConsolNumber = consol.JK_UniqueConsignRef;
			importManifest.RequiresTemperatureControl = consol.JK_RequiresTemperatureControl;
			importManifest.TemperatureMinimum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMinimum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};

			importManifest.TemperatureMaximum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMaximum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};

			PopulateTransportDetailsAndAddresses(importManifest);
			PopulateGoodsDetails(importManifest);
			PopulateContainers(importManifest);

			AddValidations(importManifest);
			importManifest.ValidateAllIncludingChildren();

			return importManifest;
		}

		void PopulateTransportDetailsAndAddresses(ImportManifest importManifest)
		{
			PopulateTransportDetails(importManifest);
			PopulateAddresses(importManifest);
		}

		void PopulateAddresses(ImportManifest importManifest)
		{
			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			importManifest.SendingParty = AddressBuilder.CreateForCurrentUser(context);
			importManifest.SendingPartySON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, importManifest.OperationalPort);
			importManifest.SendingPartyCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, importManifest.OperationalPort);
			importManifest.FormattedSendingPartyProviderID = FRPortHelpers.CreateFormattedProviderID(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, importManifest.SendingPartySON.ValueInfo, importManifest.SendingPartyCI5.ValueInfo);

			importManifest.Transporter = AddressBuilder.Create(context, consol.ArrivalUnpackCFSTransportAddress);
			importManifest.TransporterSON = consol.ArrivalUnpackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			importManifest.TransporterCI5 = consol.ArrivalUnpackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			importManifest.FormattedTransporterProviderID = FRPortHelpers.CreateFormattedProviderID(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, importManifest.TransporterSON.ValueInfo, importManifest.TransporterCI5.ValueInfo);

			importManifest.SendingForwarder = AddressBuilder.Create(context, consol.SendingForwarderWithContact);
			importManifest.SendingForwarderSON = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			importManifest.SendingForwarderCI5 = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

			importManifest.ReceivingForwarder = AddressBuilder.Create(context, consol.ReceivingForwarderWithContact);
			importManifest.ReceivingForwarderSON = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, importManifest.OperationalPort);
			importManifest.ReceivingForwarderCI5 = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, importManifest.OperationalPort);
			importManifest.FormattedReceivingForwarderProviderID = FRPortHelpers.CreateFormattedProviderID(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, importManifest.ReceivingForwarderSON.ValueInfo, importManifest.ReceivingForwarderCI5.ValueInfo);

			importManifest.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			importManifest.CarrierSON = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			importManifest.CarrierCI5 = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			importManifest.FormattedCarrierProviderID = FRPortHelpers.CreateFormattedProviderID(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, importManifest.CarrierSON.ValueInfo, importManifest.CarrierCI5.ValueInfo);
		}

		void PopulateTransportDetails(ImportManifest importManifest)
		{
			var firstSeaLeg = SeaTransportsInLegOrder.FirstOrDefault();
			var lastSeaLeg = SeaTransportsInLegOrder.LastOrDefault();
			var arrivalFRSeaLeg = SeaTransportsInLegOrder.LastOrDefault(t => Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKDiscPort));

			importManifest.PortOfOrigin = Unloco.Create(context, firstSeaLeg?.LoadPort);
			importManifest.PortOfTranshipment = Unloco.Create(context, SeaTransportsInLegOrder.GetFranceTransshipmentPort(false));
			importManifest.PortOfDestination = Unloco.Create(context, arrivalFRSeaLeg?.DiscPort);

			var operationalPort = consol.UnpackDepotAddress?.RelatedPortCode;
			importManifest.PCS = operationalPort?.GetPCS() ?? ZString.Empty;
			importManifest.OperationalPort = Unloco.Create(context, operationalPort);

			importManifest.ETA = arrivalFRSeaLeg?.JW_ETA ?? ZDateTime.Empty;
			importManifest.VesselName = arrivalFRSeaLeg?.JW_Vessel ?? ZString.Empty;
			importManifest.VoyageNumber = arrivalFRSeaLeg?.JW_VoyageFlight ?? ZString.Empty;
			importManifest.ATPReference = arrivalFRSeaLeg?.Sailing?.Destination.JB_ArrivalReference ?? ZString.Empty;
			importManifest.BillOfLadingNumber = consol.JK_MasterBillNum;
			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.Other
			};

			importManifest.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List) { Code = consol.JK_ConsolMode };

			importManifest.ShipmentType = new CodeDescription(consol.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol.JK_AgentType
			};

			importManifest.PortArea = consol.UnpackDepotAddress.GetPortArea();
			importManifest.PortLocation = consol.UnpackDepotAddress.GetPortLocation();

			importManifest.PCSInfo.AddPCSValidation();
			((Unloco)importManifest.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("9a62a5f3-01b6-4fc8-8d6f-77d874734589", "Operational Port is required, UNLOCO missing from Consol > Arrival > CFS Address."));
		}

		void PopulateGoodsDetails(ImportManifest importManifest)
		{
			var goodsDetailBuilder = new GoodsDetailBuilder();
			var goodsDetails = new List<GoodsDetail>();

			foreach (ForwardingShipment shipment in consol.Shipments.OfType<ForwardingShipment>().Where(s => !s.IsMasterShipmentRepresentingAllChildShipments))
			{
				var goodsDetail = goodsDetailBuilder.Build(shipment, pcs: importManifest.PCS);

				goodsDetail.ThirdParty = AddressBuilder.Create(context, shipment.DeliveryAgent?.MainAddress);
				goodsDetail.ThirdPartySON = shipment.DeliveryAgent?.MainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, importManifest.OperationalPort);
				goodsDetail.ThirdPartyCI5 = shipment.DeliveryAgent?.MainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, importManifest.OperationalPort);

				bool thirdPartyHasSONOrCI5 = (goodsDetail.ThirdPartySON != null && !goodsDetail.ThirdPartySON.Value.IsEmpty) || (goodsDetail.ThirdPartyCI5 != null && !goodsDetail.ThirdPartyCI5.Value.IsEmpty);
				if (!thirdPartyHasSONOrCI5)
				{
					goodsDetail.ThirdParty = AddressBuilder.Create(context, consol.ReceivingForwarderWithContact);
					goodsDetail.ThirdPartySON = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, importManifest.OperationalPort);
					goodsDetail.ThirdPartyCI5 = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, importManifest.OperationalPort);
				}

				goodsDetails.Add(goodsDetail);
			}

			importManifest.GoodsDetails = goodsDetails.OrderBy(x => x.ShipmentNumber).ToList();
		}

		void PopulateContainers(ImportManifest importManifest)
		{
			var containerBuilder = new BookingContainerBuilder();
			var containerDOs = new List<BookingContainer>();
			var packLineDOs = importManifest.GoodsDetails.SelectMany(x => x.PackingLines);
			var containerBizObjs = GetContainers();

			foreach (ForwardingContainer containerBO in containerBizObjs)
			{
				var containerPackLinePKs = containerBO.PackLines.Cast<PackLine>().Select(p => p.PK).ToArray();
				var containerPackingLineDOs = packLineDOs.Where(x => containerPackLinePKs.Contains((ZGuid)x.Identifier)).ToArray();
				var containerDO = containerBuilder.Build(containerBO, containerPackingLineDOs);
				containerDO.TransportMode = new CodeDescription(() => null)
				{
					Code = "RTE" // non-translatable
				};
				containerDO.LPDReference = GetLPDReferenceFromEvents(containerDO);
				containerDOs.Add(containerDO);
				containerDO.IsLPDStatusFinalized = true;
			}

			importManifest.Containers = containerDOs.OrderBy(container => container.Number).ToArray();
		}

		void AddValidations(ImportManifest importManifest)
		{
			AddAddressValidations(importManifest);
			AddContainerValidations(importManifest);
			AddTransportDetailsValidation(importManifest);
			AddGoodsDetailsValidation(importManifest);
			AddTransportDetailsValidation(importManifest);
		}

		void AddAddressValidations(ImportManifest importManifest)
		{
			var messageValidation = Res.GetString("c9981c74-475f-4988-aa4c-83cb025fb0aa", "name and address is required.");
			var carrierPartyName = Res.GetString("e132baf1-e4a2-4ce0-ade2-8669dabfc9a5", "Carrier");

			importManifest.SendingParty.AddPartyNameAndAddressValidation(Res.GetString("AA056261-F620-4F3E-99B6-201774E12CF9", "Sending Party"), messageValidation: messageValidation);
			importManifest.ReceivingForwarder.AddPartyNameAndAddressValidation(Res.GetString("89B2A493-CAA9-42A2-9D4F-5EC8FB20DB64", "Receiving Agent"), messageValidation: messageValidation);
			importManifest.Carrier.AddPartyNameAndAddressValidation(carrierPartyName, messageValidation: messageValidation);

			var sendingPartyName = Res.GetString("E04D39B0-22B0-4CE2-BE36-43EC58BEA395", "Sending Party");
			var sendingPartyType = Res.GetString("09db260b-ad15-4a56-ab65-7ee2a8af5c59", "Forwarder");
			var sendingPartyConfigPath = Res.GetString("d5dec2b5-4cb9-4e12-8aa1-8504f27fae16", "Org. Proxy");

			var receivingForwarderPartyName = Res.GetString("bc3a41f1-87c4-46f0-b796-808beb8811e3", "Receiving Agent");
			var receivingForwarderConfigPath = Res.GetString("7b4eb372-5bf8-4c36-adfd-c04700105b98", "Consol > Receiving Agent");

			var carrierConfigPath = Res.GetString("ff1bbc42-1b76-410b-8086-549a69a011cd", "Consol > Carrier");

			var thirdPartyName = Res.GetString("B7696767-37EA-4004-8F85-DE9B12F4CE44", "Third Party");
			var extraThirdPartyConfigPath = Res.GetString("ACFD9C78-C66B-4129-8E3E-BA320B442FFB", "Shipment > Delivery > Delivery Agent");
			var thirdPartyConfigPath = Res.GetString("46D1262B-5106-4158-8EB5-F017DE9C6525", "Consol > Receiving Agent");

			importManifest.FormattedSendingPartyProviderIDInfo.AddFormattedProviderIDValidations(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, sendingPartyName, sendingPartyType, sendingPartyConfigPath);
			importManifest.FormattedReceivingForwarderProviderIDInfo.AddFormattedProviderIDValidations(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, receivingForwarderPartyName, sendingPartyType, receivingForwarderConfigPath);
			importManifest.FormattedCarrierProviderIDInfo.AddFormattedProviderIDValidations(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, carrierPartyName, string.Empty, carrierConfigPath, true);

			foreach (var goodsDetail in importManifest.GoodsDetails)
			{
				goodsDetail.ThirdPartyProviderID.ValueInfo.AddFormattedProviderIDValidations(importManifest.PCS, OrgCusCode.FranceCodeTypes.SON, thirdPartyName, sendingPartyType, thirdPartyConfigPath, extraConfigPath: extraThirdPartyConfigPath);

				if (consol.DischargePort != null && consol.DischargePort.Country.Code == Core.Constants.CountryCodes.Reunion)
				{
					goodsDetail.Shipper.CompanyNameInfo.AddMessageErrorIfEmpty(Res.GetString("40BBFAF7-17B7-4480-AF1C-C4FBACB2A793", "Shipper company name is required as per SIMAR requirement."));
					goodsDetail.Shipper.AddressLine1Info.AddMessageError(() => goodsDetail.Shipper.AddressLine1.IsEmpty && goodsDetail.Shipper.AddressLine2.IsEmpty, Res.GetString("4D834FFF-8F12-40D2-9076-BD25119510AB", "Shipper address is required as per SIMAR requirement."));
					goodsDetail.Shipper.CityInfo.AddMessageErrorIfEmpty(Res.GetString("568A2AFB-3AE9-4190-B582-A98FE70B495C", "Shipper city is required as per SIMAR requirement."));
					goodsDetail.Shipper.Country.NameInfo.AddMessageErrorIfEmpty(Res.GetString("007A4F12-CF18-4581-8685-A364D264333B", "Shipper country is required as per SIMAR requirement."));
					goodsDetail.Shipper.AddValidationDependenciesToValidateAll(goodsDetail.Shipper.CompanyNameInfo, goodsDetail.Shipper.AddressLine1Info, goodsDetail.Shipper.AddressLine2Info, goodsDetail.Shipper.CityInfo, goodsDetail.Shipper.Country.NameInfo);

					goodsDetail.Consignee.CompanyNameInfo.AddMessageErrorIfEmpty(Res.GetString("0113CADF-DA2C-492B-8549-68DE6F256E25", "Consignee company name is required as per SIMAR requirement."));
					goodsDetail.Consignee.AddressLine1Info.AddMessageError(() => goodsDetail.Consignee.AddressLine1.IsEmpty && goodsDetail.Consignee.AddressLine2.IsEmpty, Res.GetString("C63A7BBB-71C9-46E1-95DE-3CE001346CB2", "Consignee address is required as per SIMAR requirement."));
					goodsDetail.Consignee.CityInfo.AddMessageErrorIfEmpty(Res.GetString("C2EDACB6-309F-44B1-BECA-699C11A2CA64", "Consignee city is required as per SIMAR requirement."));
					goodsDetail.Consignee.Country.NameInfo.AddMessageErrorIfEmpty(Res.GetString("E81AF6A7-E484-4C0D-A2A4-5D957BFC1A60", "Consignee country is required as per SIMAR requirement."));
					goodsDetail.Consignee.AddValidationDependenciesToValidateAll(goodsDetail.Consignee.CompanyNameInfo, goodsDetail.Consignee.AddressLine1Info, goodsDetail.Consignee.AddressLine2Info, goodsDetail.Consignee.CityInfo, goodsDetail.Consignee.Country.NameInfo);
				}
			}
		}

		void AddContainerValidations(ImportManifest importManifest)
		{
			foreach (var container in importManifest.Containers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("24BC460C-2DCD-4A38-9777-E95890C43129", "Container number is required."));
				container.Type.ISOCodeInfo.AddMessageError(() => container.Type.ISOCode.IsEmpty, Res.GetString("5B391DC9-26DB-4907-A26C-43600B74D538", "This container does not have a valid ISO Code. Enter a valid ISO code here or add it to the Container Reference File via Consol > Container > Container Type."));
				var lpdError = Res.GetString("8FA91C57-8B0A-4BBC-9085-DDCB259493CC", "LPD Status is required.");
				container.IsLPDStatusProvisionalInfo.AddMessageError(() => container.LPDStatus.IsEmpty, lpdError);
				container.IsLPDStatusFinalizedInfo.AddMessageError(() => container.LPDStatus.IsEmpty, lpdError);
				container.IsLPDStatusFinalizedNoPacksWeightInfo.AddMessageError(() => container.LPDStatus.IsEmpty, lpdError);
				container.IsLPDStatusFinalizedNoPacksOnlyInfo.AddMessageError(() => container.LPDStatus.IsEmpty, lpdError);
			}
		}

		void AddTransportDetailsValidation(ImportManifest importManifest)
		{
			importManifest.BillOfLadingNumberInfo.AddMessageErrorIfEmpty(Res.GetString("AE35E6FD-F6B3-4446-BCC1-62E2F9809537", "Bill of Lading (BOL) Number is required."));
			importManifest.PortAreaInfo.AddMessageErrorIfEmpty(Res.GetString("ABE50926-C3BA-40CD-9998-27424B820575", "Port Area is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')."));
			importManifest.PortLocationInfo.AddMessageErrorIfEmpty(Res.GetString("B5BD90CC-1068-411C-A0DA-66A54AB38C9C", "Port Location is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')."));

			importManifest.ATPReferenceInfo.AddAsciiCharactersValidation();
			importManifest.OTCReferenceInfo.AddAsciiCharactersValidation();
			importManifest.VoyageNumberInfo.AddAsciiCharactersValidation();

			var allReferencesAreEmptyMessageError = Res.GetString("5D931CC3-DDCB-45E9-9D7C-FE0D9E823DA8", "Voyage Number, OTC Reference or ATP reference is required before sending this message.");
			bool allReferencesAreEmpty() => importManifest.ATPReference.IsEmpty && importManifest.OTCReference.IsEmpty && importManifest.VoyageNumber.IsEmpty;

			importManifest.OTCReferenceInfo.AddMessageError(allReferencesAreEmpty, allReferencesAreEmptyMessageError);
			importManifest.VoyageNumberInfo.AddMessageError(allReferencesAreEmpty, allReferencesAreEmptyMessageError);
			importManifest.ATPReferenceInfo.AddMessageError(allReferencesAreEmpty, allReferencesAreEmptyMessageError);

			importManifest.AddValidationDependencies(importManifest.VoyageNumberInfo, importManifest.ATPReferenceInfo, importManifest.OTCReferenceInfo);
			importManifest.AddValidationDependencies(importManifest.ATPReferenceInfo, importManifest.VoyageNumberInfo, importManifest.OTCReferenceInfo);
			importManifest.AddValidationDependencies(importManifest.OTCReferenceInfo, importManifest.VoyageNumberInfo, importManifest.ATPReferenceInfo);
		}

		void AddGoodsDetailsValidation(ImportManifest importManifest)
		{
			foreach (var goodsDetail in importManifest.GoodsDetails)
			{
				goodsDetail.PortOfOrigin.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("94CF6CEC-EA69-4A23-AB01-0FE014EC632D", "Origin Port is required."));
				goodsDetail.ShipmentNumberInfo.AddMessageErrorIfEmpty(Res.GetString("7318F3CE-E23F-4384-8D38-F39502854DA9", "Shipment Number is required."));
				goodsDetail.HouseBillNumberInfo.AddMessageErrorIfEmpty(Res.GetString("B2DDD655-0580-46AA-8F41-FAF922944F8E", "House Bill Number is required."));
				goodsDetail.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("BD08BB34-6188-464C-9B5F-99F301DBD8D5", "Goods Description is required."));
				goodsDetail.MarksAndNumbersInfo.AddMessageErrorIfEmpty(Res.GetString("32B07D1B-0052-435A-81B3-0B7E3E3084D8", "Marks & Numbers are required."));
			}
		}

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

		string GetLPDReferenceFromEvents(BookingContainer container)
		{
			foreach (var log in GetEventLogsInDescendingOrder())
			{
				if (log.SL_SE_NKEvent == Events.MessageAcceptedCode)
				{
					log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumber);
					if (equipmentReferenceNumber == container.Number)
					{
						if (log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber))
						{
							return referenceNumber;
						}
					}
				}
			}
			return string.Empty;
		}

		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			foreach (var log in consol.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);
				var documentName = FrenchPortsConstants.DocumentNames.ProvisionalUnpackingListLPD;

				if (string.Compare(messageType, documentName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}
		#endregion
	}
}
