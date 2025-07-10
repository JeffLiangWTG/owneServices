using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("work in progress")]
	sealed class DangerousGoodsNotificationBuilder
	{
		public DangerousGoodsNotificationBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			Argument.NotNull(parameters, nameof(parameters));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
			isImportMessage = string.Compare(parameters.DataStoreName, ConsolDocumentDataStoreNames.DangerousGoodsNotificationImport, System.StringComparison.OrdinalIgnoreCase) == 0;
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly bool isImportMessage;

		public DangerousGoodsNotification Build()
		{
			var documentName = isImportMessage ? BelgianPortsConstants.DocumentNames.IFTDGNImport : BelgianPortsConstants.DocumentNames.IFTDGNExport;

			var dangerousGoodsNotification = new DangerousGoodsNotification(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			dangerousGoodsNotification.ConsolNumber = consol.JK_UniqueConsignRef;
			dangerousGoodsNotification.CarrierBookingReference = consol.JK_BookingReference;
			dangerousGoodsNotification.BillOfLading = consol.JK_MasterBillNum;

			PopulateDgnInformation(dangerousGoodsNotification);

			PopulateParties(dangerousGoodsNotification);

			PopulateRouting(dangerousGoodsNotification);

			PopulateContainersAndPackingLines(dangerousGoodsNotification);

			PopulateDataObjectWriterFields(dangerousGoodsNotification);

			AddHandlingDateValidation(dangerousGoodsNotification);

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			return dangerousGoodsNotification;
		}

		#region PopulateDgnInformation

		void PopulateDgnInformation(DangerousGoodsNotification dangerousGoodsNotification)
		{
			dangerousGoodsNotification.DgnSecurityNumber = GetDgnSecurityNumberFromEvents();
		}

		public string GetDgnSecurityNumberFromEvents()
		{
			foreach (var log in GetEventLogsInDescendingOrder())
			{
				if (log.SL_SE_NKEvent == Events.MessageWithdrawCancelAcceptedCode)
				{
					return "";
				}
				else
				{
					if (log.SL_SE_NKEvent == Events.MessageAcceptedCode)
					{
						if (log.Parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out var dgnSecurityNumber))
						{
							return dgnSecurityNumber;
						}
					}
				}
			}
			return "";
		}

		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			foreach (var log in consol.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(EventReferenceParameters.Codes.MessageType);
				var logMessageType = isImportMessage ? BelgianPortsConstants.DocumentNames.IFTDGNImport : BelgianPortsConstants.DocumentNames.IFTDGNExport;

				if (string.Compare(messageType, logMessageType, StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}

		#endregion PopulateDgnInformation

		#region PopulateParties

		void PopulateParties(DangerousGoodsNotification dangerousGoodsNotification)
		{
			dangerousGoodsNotification.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			dangerousGoodsNotification.CarrierPortId = consol.ShippingLineAddress.GetPortSystemNumber(context);

			dangerousGoodsNotification.SendingForwarder = consol.SendingForwarderWithContact != null
				? AddressBuilder.Create(context, consol.SendingForwarderWithContact)
				: AddressBuilder.Create(context, GlbCompany.CurrentCompany.OrgProxy?.MainAddress);
			dangerousGoodsNotification.SendingForwarderPortId = consol.SendingForwarderAddress.GetPortSystemNumber(context);

			dangerousGoodsNotification.ReceivingForwarder = consol.ReceivingForwarderWithContact != null
				? AddressBuilder.Create(context, consol.ReceivingForwarderWithContact)
				: AddressBuilder.Create(context, GlbCompany.CurrentCompany.OrgProxy?.MainAddress);
			dangerousGoodsNotification.ReceivingForwarderPortId = consol.ReceivingForwarderAddress.GetPortSystemNumber(context);

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress;
			if (proxyMainAddress == null || proxyMainAddress.Address1.IsEmpty)
			{
				proxyMainAddress = GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			}

			dangerousGoodsNotification.SendingParty = AddressBuilder.CreateForCurrentUser(context);
			dangerousGoodsNotification.SendingPartyPortId = proxyMainAddress.GetPortSystemNumber(context);
			dangerousGoodsNotification.SendingPartyEori = proxyMainAddress.GetEoriNumber(context);
			dangerousGoodsNotification.SendingPartyDuns = proxyMainAddress.GetDunsNumber(context, false);

			dangerousGoodsNotification.ArrivalCTO = AddressBuilder.Create(context, consol.ArrivalCTOAddress);
			dangerousGoodsNotification.ArrivalCTOTerminalId = consol.ArrivalCTOAddress.GetPortSystemNumber(context);

			dangerousGoodsNotification.DepartureCTO = AddressBuilder.Create(context, consol.DepartureCTOAddress);
			dangerousGoodsNotification.DepartureCTOTerminalId = consol.DepartureCTOAddress.GetPortSystemNumber(context);

			AddPartiesValidation(dangerousGoodsNotification);
		}

		void AddPartiesValidation(DangerousGoodsNotification dangerousGoodsNotification)
		{
			void AddMissingSendingPartyEoriDunsValidation(ZPropertyInfo sendingPartyEoriPropertyInfo, ZPropertyInfo sendingPartyDunsPropertyInfo)
			{
				sendingPartyDunsPropertyInfo.AddMessageError(()
					=> sendingPartyEoriPropertyInfo.Value.IsEmpty && sendingPartyDunsPropertyInfo.Value.IsEmpty,
					Res.GetString("BA323FB1-DD83-41D3-86FC-52476BCEDDAE", "DUNS/EORI Number is missing from this organization > Config > Registration Numbers/Codes - type DUN/EOR"));
			}
			void AddMissingPortIdValidation(ZPropertyInfo portSystemNumberPropertyInfo)
			{
				portSystemNumberPropertyInfo.AddMessageError(()
					=> portSystemNumberPropertyInfo.Value.IsEmpty,
					Res.GetString("1B403596-69AE-41BD-9FC3-8E5E10F9E19A", "Port Id is missing from this organization > Config > Registration Numbers/Codes - type PSN"));
			}
			void AddMissingTerminalCodeValidation(ZPropertyInfo portSystemNumberPropertyInfo)
			{
				portSystemNumberPropertyInfo.AddMessageError(()
					=> portSystemNumberPropertyInfo.Value.IsEmpty,
					Res.GetString("C189A210-8E36-42BD-B2A8-BBDE666A610D", "Terminal Code is missing from this organization > Config > Registration Numbers/Codes - type PSN"));
			}
			void AddMissingContactDetailsValidation(Address address)
			{
				var contactErrorMessage = Res.GetString("83BBB12A-727B-4E63-9B75-8C9319BD4FA0", "Contact name is required.");
				var emailErrorMessage = Res.GetString("5077E159-1ACA-487C-8F50-0707281A6B4C", "Email is required.");
				var phoneErrorMessage = Res.GetString("0455C749-AE96-47CF-B830-AFEA6403D7AA", "Phone is required.");
				address.ContactInfo.AddMessageError(() => address.Contact.IsEmpty, contactErrorMessage);
				address.EmailInfo.AddMessageError(() => address.Email.IsEmpty, emailErrorMessage);
				address.PhoneInfo.AddMessageError(() => address.Phone.IsEmpty, phoneErrorMessage);
			}

			AddMissingSendingPartyEoriDunsValidation(dangerousGoodsNotification.SendingPartyEori?.ValueInfo, dangerousGoodsNotification.SendingPartyDuns?.ValueInfo);
			AddMissingPortIdValidation(dangerousGoodsNotification.CarrierPortId.ValueInfo);
			AddMissingPortIdValidation(dangerousGoodsNotification.SendingPartyPortId.ValueInfo);

			if (isImportMessage)
			{
				AddMissingContactDetailsValidation(dangerousGoodsNotification.ReceivingForwarder);
				AddMissingPortIdValidation(dangerousGoodsNotification.ReceivingForwarderPortId.ValueInfo);
				AddMissingTerminalCodeValidation(dangerousGoodsNotification.ArrivalCTOTerminalId.ValueInfo);
			}
			else
			{
				AddMissingContactDetailsValidation(dangerousGoodsNotification.SendingForwarder);
				AddMissingPortIdValidation(dangerousGoodsNotification.SendingForwarderPortId.ValueInfo);
				AddMissingTerminalCodeValidation(dangerousGoodsNotification.DepartureCTOTerminalId.ValueInfo);
			}
		}

		#endregion

		#region PopulateRouting
		void PopulateRouting(DangerousGoodsNotification dangerousGoodsNotification)
		{
			PopulateRelevantTransports(dangerousGoodsNotification);

			dangerousGoodsNotification.HandlingInstruction = isImportMessage ? BelgianPortsConstants.HandlingInstructions.Discharge : BelgianPortsConstants.HandlingInstructions.Loading;
			dangerousGoodsNotification.OperationalPort = isImportMessage
				? dangerousGoodsNotification.Transports?.Main?.PortOfDischarge
				: dangerousGoodsNotification.Transports?.Main?.PortOfLoading;
			if (isImportMessage)
			{
				dangerousGoodsNotification.VesselStayStartDate = dangerousGoodsNotification.Transports.Main == null
					? ZDateTime.Empty
					: dangerousGoodsNotification.Transports.Main.ETA;
			}
			else
			{
				dangerousGoodsNotification.VesselStayEndDate = dangerousGoodsNotification.Transports.Main == null
					? ZDateTime.Empty
					: dangerousGoodsNotification.Transports.Main.ETD;
			}

			AddRoutingValidation(dangerousGoodsNotification);
		}
		CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
					transportModeList.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
					transportModeList.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
					transportModeList.AddPair(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportModeDescriptions.InlandWaterwayTransport);
				}
				return transportModeList;
			}
		}
		CodeDescriptionPairList transportModeList;

		void PopulateRelevantTransports(DangerousGoodsNotification dangerousGoodsNotification)
		{
			consol?.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			Transport preCarriageTransport = null;
			Transport mainTransport = null;
			Transport onForwardingTransport = null;
			foreach (var transport in consol.Transports.Cast<Freight.Business.Transport>())
			{
				if (isImportMessage && mainTransport != null)
				{
					onForwardingTransport = Transport.Create(context, transport);
					onForwardingTransport.LegOrder = 2;
					break;
				}
				if (transport.JW_TransportMode == Constants.TransportModes.Sea)
				{
					if (isImportMessage && (transport.JW_RL_NKDiscPort == "BEANR" || transport.JW_RL_NKDiscPort == "BEZEE"))
					{
						mainTransport = Transport.Create(context, transport);
						mainTransport.LegOrder = 1;
					}
					else if (!isImportMessage && (transport.JW_RL_NKLoadPort == "BEANR" || transport.JW_RL_NKLoadPort == "BEZEE"))
					{
						mainTransport = Transport.Create(context, transport);
						mainTransport.LegOrder = 2;
						break;
					}
				}
				preCarriageTransport = Transport.Create(context, transport);
				preCarriageTransport.LegOrder = 1;
			}

			var preOrOnTransportModeCode = "";
			if (isImportMessage && onForwardingTransport != null)
			{
				preOrOnTransportModeCode = onForwardingTransport.Mode.Code;
				dangerousGoodsNotification.PreOrOnVesselName = onForwardingTransport.Vessel?.Name ?? ZString.Empty;
				dangerousGoodsNotification.PickupDate = onForwardingTransport.ETD;
			}
			else if (!isImportMessage && preCarriageTransport != null)
			{
				preOrOnTransportModeCode = preCarriageTransport.Mode.Code;
				dangerousGoodsNotification.PreOrOnVesselName = preCarriageTransport.Vessel?.Name ?? ZString.Empty;
				dangerousGoodsNotification.DeliveryDate = preCarriageTransport.ETA;
			}
			dangerousGoodsNotification.PreOrOnTransportMode = new CodeDescription(TransportModeList)
			{
				Code = preOrOnTransportModeCode
			};

			var transportCollection = new List<Transport>();
			if (mainTransport != null)
			{
				transportCollection.Add(mainTransport);
			}
			var transports = Transports.Create(transportCollection);
			if (mainTransport != null)
			{
				transports.Main = mainTransport;
			}

			dangerousGoodsNotification.Transports = transports;
		}

		void AddRoutingValidation(DangerousGoodsNotification dangerousGoodsNotification)
		{
			var missingPortDeliveryDetailsTransportModeErrorMessage = Res.GetString("20F79CD8-6BE9-4E61-90C2-0FE4D617AAE1", "Transport Mode of Port Delivery Details is required.");
			var missingDeliveryDateErrorMessage = Res.GetString("5F18AF4E-6E35-4968-AE10-657AFC760CCB", "Expected Arrival date at Port is required.");
			var missingPortCollectionDetailsTransportModeErrorMessage = Res.GetString("3FDFD9EB-1C55-46AC-803D-771021FC8CC9", "Transport Mode of Port Collection Details is required.");
			var missingPickupDateErrorMessage = Res.GetString("B4E582A4-9CC2-41DD-BA5C-F2CF47CA56CF", "Expected Departure date at Port is required.");

			dangerousGoodsNotification.VesselStayReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("E9404F17-FB16-4A89-9819-F429AE543F5A", "Ship's Stay Number is required."));

			((Unloco)dangerousGoodsNotification.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("545C2DD6-9FF7-4D58-8121-06E986DFEC1A", "Port of Call is required."));

			((CodeDescription)dangerousGoodsNotification.PreOrOnTransportMode).CodeInfo.AddMessageErrorIfEmpty(isImportMessage ? missingPortCollectionDetailsTransportModeErrorMessage : missingPortDeliveryDetailsTransportModeErrorMessage);
			dangerousGoodsNotification.PickupDateInfo.AddMessageError(() => isImportMessage && dangerousGoodsNotification.PickupDate.IsEmpty, missingPickupDateErrorMessage);
			dangerousGoodsNotification.DeliveryDateInfo.AddMessageError(() => !isImportMessage && dangerousGoodsNotification.DeliveryDate.IsEmpty, missingDeliveryDateErrorMessage);
		}

		#endregion

		#region PopulateContainersAndPackingLines

		void PopulateContainersAndPackingLines(DangerousGoodsNotification dangerousGoodsNotification)
		{
			var containerBuilder = new DGNContainerBuilder();
			var containerDOs = new List<DGNContainer>();
			var packLineDOs = GetDGNPackingLines();

			foreach (ForwardingContainer containerBO in consol.Containers)
			{
				var containerPackLinePKs = containerBO.PackLines.Cast<PackLine>().Select(p => p.PK).ToArray();
				var containerPackingLineDOs = packLineDOs.Where(x => containerPackLinePKs.Contains((ZGuid)x.Identifier)).ToArray();
				var containerDO = containerBuilder.Build(containerBO, containerPackingLineDOs);

				if (containerDO.HasDangerousGoods)
				{
					containerDOs.Add(containerDO);
				}
			}

			dangerousGoodsNotification.PackingLines = packLineDOs.ToArray();

			dangerousGoodsNotification.Containers = containerDOs
			.OrderBy(container => container.Number)
			.ToArray();

			AddContainersValidation(dangerousGoodsNotification);
			AddMissingDangerousGoodsValidation(dangerousGoodsNotification);
		}

		void AddContainersValidation(DangerousGoodsNotification dangerousGoodsNotification)
		{
			var missingContainerErrorMessage = Res.GetString("85b8e98e-8449-456b-9cff-887afb97d265", "Container details are required for Dangerous Goods Notification(BE).");
			dangerousGoodsNotification.ErrorPlaceHolderInfo.AddMessageError(() => dangerousGoodsNotification.Containers.IsNullOrEmpty(), missingContainerErrorMessage);

			foreach (var container in dangerousGoodsNotification.Containers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("2e1b5af9-cfcc-4323-aff6-70b1b114be77", "Please enter a Container Number."));
			}
		}

		void AddMissingDangerousGoodsValidation(DangerousGoodsNotification dangerousGoodsNotification)
		{
			var missingDangerousGoodsErrorMessage = Res.GetString("2F19F868-F7DB-4962-8921-598AAC904BF2", "No Dangerous Goods found! Dangerous Goods Notification is not required.");

			var packLineDOs = GetDGNPackingLines();
			dangerousGoodsNotification.ErrorPlaceHolderInfo.AddMessageError(() => !packLineDOs.Any(p => p.DangerousGoods != null && p.DangerousGoods.Any()), missingDangerousGoodsErrorMessage);

			var dangerousGoodsNotificationPackLineDOs = dangerousGoodsNotification.PackingLines;
			foreach (var packLineDO in dangerousGoodsNotificationPackLineDOs)
			{
				foreach (var dangerousGood in packLineDO.DangerousGoods)
				{
					dangerousGood.TechnicalNameInfo.AddMessageErrorIfEmpty(Res.GetString("298aa706-1803-4f8b-865f-5955ab310f41", "Technical Name is required."));
					dangerousGood.QuantityInfo.AddMessageErrorIfEmpty(Res.GetString("f4910c7f-3e2b-48de-a88e-5c3d9e437913", "UNDG Packs is required. (Dangerous Goods > Packs)"));
					((CodeDescription)dangerousGood.PackageType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("5c7805a9-9d45-4f02-a119-e71ee218a95f", "UNDG Pack Type is required. (Dangerous Goods > Pack Type)"));
					((Measurement)dangerousGood.Weight).ValueInfo.AddMessageErrorIfEmpty(Res.GetString("3ba68835-a9d1-456e-878b-7bd7bedf2ffe", "UNDG Weight is required. (Dangerous Goods > Weight)"));
				}
			}
		}

		IEnumerable<DGNPackingLine> GetDGNPackingLines()
		{
			var dgnPackLines = new List<DGNPackingLine>();
			var dgnPackLineBuilder = new DGNPackingLineBuilder();
			foreach (PackLine packLineBO in consol.TopLevelShipments.Cast<ForwardingShipment>().SelectMany(s => s.OuterPackLines))
			{
				var dgnPackLine = dgnPackLineBuilder.Build(packLineBO);
				dgnPackLines.Add(dgnPackLine);
			}

			return dgnPackLines;
		}

		#endregion

		#region PopulateDataObjectWriterFields

		void PopulateDataObjectWriterFields(DangerousGoodsNotification dangerousGoodsNotification)
		{
			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Other
			};

			dangerousGoodsNotification.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol.JK_ConsolMode)
					? (ZString)Core.Constants.ContainerModes.FCL
					: consol.JK_ConsolMode
			};

			dangerousGoodsNotification.ShipmentType = new CodeDescription(consol.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol.JK_AgentType
			};
		}

		#endregion

		#region Non-Persistent Validation

		void AddHandlingDateValidation(DangerousGoodsNotification dangerousGoodsNotification)
		{
			var unloadingDateErrorMessage = Res.GetString("f63773b9-4f2b-4f7b-bfe6-7ab9835fe818", "Unloading Date is required.");
			var loadingDateErrorMessage = Res.GetString("f0a07cdc-999b-4118-a309-e019195cba0c", "Loading Date is required.");
			dangerousGoodsNotification.HandlingDateInfo.AddMessageErrorIfEmpty(isImportMessage ? unloadingDateErrorMessage : loadingDateErrorMessage);
		}

		#endregion

	}
}
