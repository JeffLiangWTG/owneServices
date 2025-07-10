using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;
using Constants = CargoWise.EventReference.Constants;
using FRPortHelpers = Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Helpers;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using TransportBizo = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class FinalManifestBuilder
	{
		public FinalManifestBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
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

		public FinalManifest Build()
		{
			var wrapper = new FinalManifest(nameof(ForwardingConsol), consol.JK_UniqueConsignRef);

			wrapper.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List) { Code = consol.JK_ConsolMode };
			wrapper.ShipmentType = new CodeDescription(consol.JK_AgentType_List) { Code = consol.JK_AgentType };

			var operationalPort = consol.PackDepotAddress?.RelatedPortCode;
			wrapper.PCS = operationalPort?.GetPCS() ?? ZString.Empty;
			wrapper.OperationalPort = Unloco.Create(context, operationalPort);

			wrapper.PortOfOrigin = Unloco.Create(context, consol.LoadPort);
			wrapper.PortOfDestination = Unloco.Create(context, consol.DischargePort);

			PopulateAddresses(wrapper);
			PopulateTransportDetails(wrapper);
			PopulateDeliveryDetails(wrapper);
			PopulateAdditionalReferences(wrapper);
			PopulateGoodsAndEquipmentDetails(wrapper);

			AddAddressValidation(wrapper);
			AddDeliveryDetailsValidation(wrapper);
			AddAdditionalReferencesValidation(wrapper);
			AddOperationalPortAndPCSValidation(wrapper);
			AddErrorPlaceHolderValidation(wrapper);
			wrapper.Containers.ForEach(c => AddContainerValidation(c));
			wrapper.GoodsDetails.ForEach(g =>
			{
				AddGoodsDetailValidation(g);
				g.PackingLines.ForEach(p => AddPackingLineValidation(p));
			});
			wrapper.ValidateAllIncludingChildren();

			return wrapper;
		}

		void PopulateAddresses(FinalManifest wrapper)
		{
			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			wrapper.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			wrapper.CurrentUserCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, wrapper.OperationalPort);
			wrapper.CurrentUserSOA = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			wrapper.CurrentUserSON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, wrapper.OperationalPort);
			wrapper.CurrentUserSOW = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);

			wrapper.SendingParty = AddressBuilder.Create(context, consol.PackDepotAddress);
			wrapper.SendingPartyCI5 = consol.PackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			wrapper.SendingPartySOA = consol.PackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			wrapper.SendingPartySON = consol.PackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			wrapper.SendingPartySOW = consol.PackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			wrapper.FormattedSendingPartyProviderID = FRPortHelpers.CreateFormattedProviderID(wrapper.PCS, OrgCusCode.FranceCodeTypes.SOW, wrapper.SendingPartySOW.ValueInfo, wrapper.SendingPartyCI5.ValueInfo);

			wrapper.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			wrapper.CarrierCI5 = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			wrapper.CarrierSON = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			wrapper.FormattedCarrierProviderID = FRPortHelpers.CreateFormattedProviderID(wrapper.PCS, OrgCusCode.FranceCodeTypes.SON, wrapper.CarrierSON.ValueInfo, wrapper.CarrierCI5.ValueInfo);

			wrapper.ReceivingForwarder = AddressBuilder.Create(context, consol.ReceivingForwarderWithContact);
			wrapper.ReceivingForwarderSON = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			wrapper.ReceivingForwarderCI5 = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

			wrapper.PortArea = consol.PackDepotAddress.GetPortArea();
			wrapper.PortLocation = consol.PackDepotAddress.GetPortLocation();
		}

		void PopulateTransportDetails(FinalManifest wrapper)
		{
			wrapper.Vessel = FirstFRLoadLeg?.JW_Vessel ?? ZString.Empty;
			wrapper.ATPReference = FirstFRLoadLeg?.Sailing?.Origin?.JA_DepartReference ?? ZString.Empty;
		}

		void PopulateDeliveryDetails(FinalManifest wrapper)
		{
			wrapper.Transporter = AddressBuilder.Create(context, consol.DeparturePackCFSTransportAddress);
			wrapper.TransporterSON = consol.DeparturePackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			wrapper.TransporterCI5 = consol.DeparturePackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			wrapper.FormattedTransporterProviderID = FRPortHelpers.CreateFormattedProviderID(wrapper.PCS, OrgCusCode.FranceCodeTypes.SON, wrapper.TransporterSON.ValueInfo, wrapper.TransporterCI5.ValueInfo);

			wrapper.TransportMode = BookingContainer.TransportModeRTE;

			wrapper.DeliveryArea = consol.DepartureCTOAddress.GetPortArea();
			wrapper.DeliveryLocation = consol.DepartureCTOAddress.GetPortLocation();

			wrapper.ExpectedArrivalAtPort = consol.Containers.Cast<ForwardingContainer>().FirstOrDefault()?.JC_DepartureSlotDateTime ?? ZDateTime.Empty;
		}

		void PopulateAdditionalReferences(FinalManifest wrapper)
		{
			wrapper.ConsolNumber = consol.JK_UniqueConsignRef;
			wrapper.CarrierBookingRef = consol.JK_BookingReference;
		}

		List<GoodsDetail> GetGoodsDetails(ForwardingContainer container)
		{
			var goodsDetails = new List<GoodsDetail>();
			var packLineBuilder = new BookingPackingLineBuilder();

			var packlinesGroups = container.PackLines.Cast<ForwardingPackLine>().GroupBy(p => GetGroupKey(p));

			foreach (var packlinesGroup in packlinesGroups)
			{
				if (packlinesGroup.Any())
				{
					var goodsDetail = new GoodsDetail(packlinesGroup.Key);
					var firstPackingLine = packlinesGroup.First();
					var ecvNumber = firstPackingLine.JL_ExportRefNumber;
					var shipment = firstPackingLine.Shipment;

					goodsDetail.ShipmentNumber = shipment.JS_UniqueConsignRef;
					goodsDetail.ContainerNumber = container.JC_ContainerNum;
					goodsDetail.CommodityReference = ecvNumber;
					goodsDetail.ConsignmentNumber = GetCusEntryNumberByType(firstPackingLine.AdditionalReferenceNumbers.Cast<CusEntryNumber>(), ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, firstPackingLine);

					if (!ecvNumber.IsEmpty)
					{
						goodsDetail.CustomsStatus = (firstPackingLine.PortReferences.Cast<CusEntryNumber>()
							.FirstOrDefault(x => x.CE_EntryNum == ecvNumber && x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN && x.CE_RN_NKCountryCode == (shipment.ExportReceivingDepot?.OA_RN_NKCountryCode ?? "FR"))
							?.CE_EntryStatus ?? ZString.Empty)
							== TransitWarehouseReferenceStatus.Codes.Cleared ? FinalManifest.CustomsStatusCleared : string.Empty;

						if (goodsDetail.CustomsStatus.IsEmpty)
						{
							var scmEvent = GetLatestEventLog(shipment, Events.ClearanceCompletedCode, ecvNumber);
							var shlEvent = GetLatestEventLog(shipment, Events.HeldCode, ecvNumber);
							if (scmEvent != null || shlEvent != null)
							{
								goodsDetail.CustomsStatus = ((scmEvent?.SL_EventTime) ?? ZDateTime.MinSmallDateTimeValue) >= ((shlEvent?.SL_EventTime) ?? ZDateTime.MinSmallDateTimeValue) ? FinalManifest.CustomsStatusCleared : FinalManifest.CustomsStatusHeld;
							}
						}
					}

					goodsDetail.PackingLines = packlinesGroup.Select(p => packLineBuilder.Build(p, groupKey: GetGroupKey(p))).ToArray();

					var shipmentWeight = goodsDetail.PackingLines.Sum(p => Core.Constants.Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, Core.Constants.Weight.Kilograms));
					goodsDetail.ShipmentWeight = new Measurement
					{
						Value = shipmentWeight,
						Unit = new CodeDescription(context.WeightUnits)
						{
							Code = Core.Constants.Weight.Kilograms
						}
					};

					goodsDetail.DeclaredPackCount = goodsDetail.PackingLines.Sum(p => p.Quantity);
					goodsDetail.DeclaredPackWeight = new Measurement()
					{
						Value = shipmentWeight,
						Unit = new CodeDescription(context.WeightUnits)
						{
							Code = Core.Constants.Weight.Kilograms
						}
					};

					goodsDetail.DeclarationReferenceNumber = ecvNumber;
					goodsDetail.AppliesToAllPacks = false;

					goodsDetails.Add(goodsDetail);
				}
			}

			return goodsDetails;
		}

		ZString GetGroupKey(ForwardingPackLine packLine)
		{
			var ercNumber = GetCusEntryNumberByType(packLine.AdditionalReferenceNumbers.Cast<CusEntryNumber>(), ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, packLine);

			return packLine.JL_Calc_ContainerNum + packLine.JL_ExportRefNumber + (!ercNumber.IsEmpty ? ercNumber : packLine.Shipment.JS_UniqueConsignRef);
		}

		ZString GetCusEntryNumberByType(IEnumerable<CusEntryNumber> number, string type, ForwardingPackLine packLine)
		{
			return number.FirstOrDefault(x => x.CE_EntryType == type && x.CE_RN_NKCountryCode == (packLine.Shipment.ExportReceivingDepot?.OA_RN_NKCountryCode ?? Core.Constants.CountryCodes.France))
						  ?.CE_EntryNum ?? ZString.Empty;
		}

		void PopulateGoodsAndEquipmentDetails(FinalManifest wrapper)
		{
			var containerBuilder = new BookingContainerBuilder();
			var containerDOs = new List<BookingContainer>();
			var goodsDetails = new List<GoodsDetail>();

			foreach (var containerBO in Containers.Cast<ForwardingContainer>().Where(t => t.PackLines.Any()))
			{
				var containerGoodsDetails = GetGoodsDetails(containerBO);
				goodsDetails.AddRange(containerGoodsDetails);

				var packLineDOs = containerGoodsDetails.SelectMany(x => x.PackingLines);
				var containerPackLinePKs = containerBO.PackLines.Cast<PackLine>().Select(p => p.PK).ToArray();
				var containerPackingLineDOs = packLineDOs.Where(x => containerPackLinePKs.Contains((ZGuid)x.Identifier)).ToArray();
				var containerDO = containerBuilder.Build(containerBO, containerPackingLineDOs);
				containerDO.PackingReference = consol.JK_UniqueConsignRef;

				containerDO.LDEIsFinal = true;
				var ldeMessage = GetEventLogsInDescendingOrder(consol, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE)
					.FirstOrDefault(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode && log.Parameters.TryGetValue(EventReferenceParameters.Codes.EquipmentReferenceNumber, out var containerNumber) && containerNumber == containerBO.JC_ContainerNum);
				if (ldeMessage != null)
				{
					if (ldeMessage.Parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out var ldeReference))
					{
						containerDO.LDEReference = ldeReference;
					}

					if (ldeMessage.Parameters.TryGetValue(EventReferenceParameters.Codes.Status, out var ldeStatus))
					{
						containerDO.LDEStatus = ldeStatus;
						containerDO.LDEIsFinal = ldeStatus == BookingContainer.LDEIsFinalCode;
						containerDO.LDEIsProvisional = !containerDO.LDEIsFinal;
					}
				}

				containerDO.PortDuesPortCode = Unloco.Create(context, consol.LoadPort);
				containerDO.PortDuesCurrency = new CodeDescription(consol.RefCurrency_List)
				{
					Code = consol.LoadPort?.Country.RN_RX_NKLocalCurrency ?? ZString.Empty
				};

				containerDO.TransportMode = new CodeDescription(() => null)
				{
					Code = BookingContainer.TransportModeRTE
				};

				containerDOs.Add(containerDO);
			}

			wrapper.GoodsDetails = goodsDetails;

			wrapper.Containers = containerDOs.OrderBy(container => container.Number).ToArray();

			wrapper.TemperatureMinimum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMinimum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};

			wrapper.TemperatureMaximum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMaximum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};
		}

		#region Validation

		void AddAddressValidation(FinalManifest wrapper)
		{
			var messageValidation = Res.GetString("c9981c74-475f-4988-aa4c-83cb025fb0aa", "name and address is required.");

			wrapper.Carrier.AddPartyNameAndAddressValidation(Res.GetString("A366BDC5-B54C-405E-A948-98E2D2158E1E", "Carrier"), messageValidation: messageValidation);
			wrapper.SendingParty.AddPartyNameAndAddressValidation(Res.GetString("F83FE9EB-3D3B-4FC2-AFAF-B7915B275D9A", "Sending Party"), messageValidation: messageValidation);

			var carrierPartyName = Res.GetString("df5e986d-bdfb-4223-9ffa-1c7752d96d73", "Carrier");
			var carrierConfigPath = Res.GetString("d3e6574e-e3de-4672-99f9-61e2cab20333", "Consol > Carrier");

			var sendingPartyName = Res.GetString("E04D39B0-22B0-4CE2-BE36-43EC58BEA395", "Sending Party");
			var sendingPartyConfigPath = Res.GetString("9d67de88-734e-4ae2-8811-2ae508c362f9port", "Consol > Departure > CFS");

			wrapper.FormattedCarrierProviderIDInfo.AddFormattedProviderIDValidations(wrapper.PCS, OrgCusCode.FranceCodeTypes.SON, carrierPartyName, configPath: carrierConfigPath, isOrganization: true);
			wrapper.FormattedSendingPartyProviderIDInfo.AddFormattedProviderIDValidations(wrapper.PCS, OrgCusCode.FranceCodeTypes.SOW, sendingPartyName, configPath: sendingPartyConfigPath, isOrganization: true);

			wrapper.PortLocationInfo.AddMessageErrorIfEmpty(Res.GetString("F612F953-5B5A-43D1-AF58-A64D951CABCD", "Port Location is missing from Organization Consol > Departure > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')."));
			wrapper.PortAreaInfo.AddMessageErrorIfEmpty(Res.GetString("71CA3619-8BEB-4C98-B2EB-5B81CBC7F9D6", "Port Area is missing from Organization Consol > Departure > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')."));
		}

		void AddDeliveryDetailsValidation(FinalManifest wrapper)
		{
			wrapper.DeliveryLocationInfo.AddMessageErrorIfEmpty(Res.GetString("48A4691F-8778-46A1-8CF6-72284289B65C", "Delivery Location is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')."));
			wrapper.DeliveryAreaInfo.AddMessageErrorIfEmpty(Res.GetString("A1BF853B-E800-4AED-B8A1-08A065091DD8", "Delivery Area is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')."));

			wrapper.TransportModeInfo.AddMessageErrorIfEmpty(Res.GetString("F4A1C584-B952-45AB-8B7F-F144C11D324C", "Transport Mode is required."));
		}

		void AddAdditionalReferencesValidation(FinalManifest wrapper)
		{
			wrapper.ConsolNumberInfo.AddMessageErrorIfEmpty(Res.GetString("DA50A31E-7765-459C-9C96-FF927EE29C85", "Consol Number is required."));
			wrapper.CarrierBookingRefInfo.AddMessageErrorIfEmpty(Res.GetString("4F628E59-DD4F-436C-86C2-fA0E25E05DC2F", "Carrier Booking Ref is required."));
		}

		void AddContainerValidation(BookingContainer container)
		{
			container.PackingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("7C5C7BD8-53B0-431E-AE63-D53F3BF50DB0", "Packing Reference is required."));

			container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("B35332F7-DA39-4B76-A48A-851208BDEF6A", "Container Number is required."));
			container.Type.ISOCodeInfo.AddMessageErrorIfEmpty(Res.GetString("B06E9158-CC5A-4DC1-9F2C-617D91951468", "This container does not have a valid ISO Code. Enter a valid ISO code here or add it to the Container Reference File via Consol > Container > Container Type."));
			container.GoodsWeight.ValueInfo.AddMessageError(() => container.GoodsWeight.Value <= 0, Res.GetString("9e26605e-0cac-491b-bd75-563a6f978a9f", "Container goods weight is required."));
			container.TareWeight.ValueInfo.AddMessageError(() => container.TareWeight.Value <= 0, Res.GetString("58821c8a-9509-40c6-a606-d9d5be372ea3", "Container tare weight is required."));
			((CodeDescription)container.PortDuesCurrency).CodeInfo.AddInvalidCodeValidation();
		}

		void AddGoodsDetailValidation(GoodsDetail goodsDetail)
		{
			goodsDetail.CommodityReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("68030FF0-F226-4E1E-B0B0-1290EA49D6C6", "ECV Reference is required. Export Ref Number missing from Shipment > Packing > Pack Line."));
			goodsDetail.CustomsStatusInfo.AddMessageError(() => goodsDetail.CustomsStatus != FinalManifest.CustomsStatusCleared, Res.GetString("AFF919FA-DAF4-45C4-AAC4-39127F143BFF", "Customs clearance is not received."));

			goodsDetail.DeclaredPackCountInfo.AddMessageError(() => goodsDetail.DeclaredPackCount <= 0, Res.GetString("2A1D6B49-BF9D-4E14-9666-B8A9834134A5", "Declared pack count is required."));
			((Measurement)goodsDetail.DeclaredPackWeight).ValueInfo.AddMessageError(() => goodsDetail.DeclaredPackWeight.Value <= 0, Res.GetString("de27a9b7-d70c-42be-a01c-d15e49b6d02d", "Declared goods weight is required."));
			goodsDetail.DeclarationReferenceNumberInfo.AddMessageErrorIfEmpty(Res.GetString("A83AB40D-0FDE-476A-9B06-69DDF9B10525", "Reference Number is required."));
		}

		void AddPackingLineValidation(BookingPackingLine packingLine)
		{
			((CodeDescription)packingLine.PackageType).CodeInfo.AddInvalidCodeValidation();
			packingLine.QuantityInfo.AddMessageError(() => packingLine.Quantity <= 0, Res.GetString("EF0C569D-5BAB-4A78-82D4-529AF130073E", "Number of Packs is required."));
			packingLine.Volume.ValueInfo.AddMessageError(() => packingLine.Volume.Value <= 0, Res.GetString("6a5240bb-28d6-456c-9c06-300b38e49a67", "Package volume is required."));
			packingLine.MarksAndNumbersInfo.AddMessageErrorIfEmpty(Res.GetString("0C9DEA76-ACA4-4A5A-84E2-D286264FD955", "Marks & Numbers are required."));
			packingLine.PackingLineIDInfo.AddMessageErrorIfEmpty(Res.GetString("FEBFEF46-00B4-487D-8A3B-A3BF528103C0", "Packing Line ID is required."));
		}

		void AddOperationalPortAndPCSValidation(FinalManifest wrapper)
		{
			wrapper.PCSInfo.AddPCSValidation();
			((Unloco)wrapper.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("11e3bd68-6e7c-4d03-a822-b43413b8f268", "Operational Port is required, UNLOCO missing from Consol > Departure > CFS Address."));
		}

		void AddErrorPlaceHolderValidation(FinalManifest wrapper)
		{
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => consol.TopLevelShipments.Cast<ForwardingShipment>().Any(s => s.OuterPackLines.Cast<PackLine>().Any(p => Containers.Any(c => c.JC_ContainerNum == p.JL_Calc_ContainerNum) && p.JL_LastKnownTransitWarehouseStatus.IsEmpty))
			, Res.GetString("38B121F3-02E5-4F5E-93B2-EF8C9AF9BD9B", "There are pack lines with 'Last Known Transit Warehouse Status' blank. All pack lines must have 'Last Known Transit Warehouse Status' to send this message."));
		}

		#endregion

		#region Implementation

		#region Transports

		IReadOnlyCollection<TransportBizo> SeaTransports => seaTransports ?? (seaTransports = GetSeaTransports());
		IReadOnlyCollection<TransportBizo> seaTransports;

		IReadOnlyCollection<TransportBizo> GetSeaTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));
			return consol
				.Transports
				.OfType<TransportBizo>()
				.Where(t => t.TransportMode == Core.Constants.TransportModes.Sea)
				.ToArray();
		}

		TransportBizo FirstFRLoadLeg => SeaTransports.FirstOrDefault(t => Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKLoadPort));

		#endregion

		#region Containers

		IReadOnlyCollection<ForwardingContainer> Containers => containers ?? (containers = GetContainers());
		IReadOnlyCollection<ForwardingContainer> containers;

		IReadOnlyCollection<ForwardingContainer> GetContainers()
		{
			if (parameters?.Data is IReadOnlyCollection<ForwardingContainer> containers)
			{
				return containers;
			}

			return Array.Empty<ForwardingContainer>();
		}

		#endregion

		#region Event Logs

		static StmALog GetLatestEventLog(IStmALogProvider logProvider, string eventCode, string customsReferenceNumber)
			=> GetEventLogsInDescendingOrder(logProvider).FirstOrDefault(
						log => log.SL_SE_NKEvent == eventCode
						&& !log.SL_IsCancelled
						&& log.Parameters.TryGetValue(EventReferenceParameters.Codes.CustomsReferenceNumber, out var crfNumber)
						&& crfNumber == customsReferenceNumber);

		static IEnumerable<StmALog> GetEventLogsInDescendingOrder(IStmALogProvider logProvider, string documentName = "")
		{
			foreach (var log in logProvider?.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				if (!string.IsNullOrEmpty(documentName))
				{
					var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);

					if (string.Compare(messageType, documentName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						yield return log;
					}
				}
				else
				{
					yield return log;
				}
			}
		}

		#endregion

		#endregion
	}
}
