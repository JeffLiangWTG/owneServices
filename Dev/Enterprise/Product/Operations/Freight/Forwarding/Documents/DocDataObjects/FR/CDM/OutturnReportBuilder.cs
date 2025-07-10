using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Customs.Common;
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
	sealed class OutturnReportBuilder
	{
		public OutturnReportBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			Argument.NotNull(consol, nameof(consol));

			this.consol = consol;
			this.parameters = parameters;
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public OutturnReport Build()
		{
			var outturnReport = new OutturnReport(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			outturnReport.ConsolNumber = consol.JK_UniqueConsignRef;
			outturnReport.BillOfLading = consol.JK_MasterBillNum;

			PopulateTransportDetails(outturnReport);
			PopulateAddresses(outturnReport);
			PopulateGoodsDetails(outturnReport);
			PopulateContainers(outturnReport);

			AddValidations(outturnReport);
			outturnReport.ValidateAllIncludingChildren();

			return outturnReport;
		}

		void PopulateAddresses(OutturnReport outturnReport)
		{
			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			outturnReport.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			outturnReport.CurrentUserSON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			outturnReport.CurrentUserCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			outturnReport.CurrentUserSOW = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			outturnReport.CurrentUserSOA = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);

			outturnReport.SendingParty = AddressBuilder.Create(context, consol.UnpackDepotAddress);
			outturnReport.SendingPartySON = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			outturnReport.SendingPartyCI5 = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			outturnReport.SendingPartySOW = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			outturnReport.SendingPartySOA = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			outturnReport.FormattedSendingPartyProviderID = FRPortHelpers.CreateFormattedProviderID(outturnReport.PCS, OrgCusCode.FranceCodeTypes.SOW, outturnReport.SendingPartySOW.ValueInfo, outturnReport.SendingPartyCI5.ValueInfo);

			outturnReport.CFS = AddressBuilder.Create(context, consol.UnpackDepotAddress);
			outturnReport.CFSSON = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			outturnReport.CFSCI5 = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			outturnReport.CFSSOW = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			outturnReport.CFSSOA = consol.UnpackDepotAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			outturnReport.FormattedCFSProviderID = FRPortHelpers.CreateFormattedProviderID(outturnReport.PCS, OrgCusCode.FranceCodeTypes.SOW, outturnReport.CFSSOW.ValueInfo, outturnReport.CFSCI5.ValueInfo);

			outturnReport.SendingForwarder = AddressBuilder.Create(context, consol.SendingForwarderWithContact);
			outturnReport.SendingForwarderSON = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			outturnReport.SendingForwarderCI5 = ((OrgAddress)consol.SendingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

			outturnReport.ReceivingForwarder = AddressBuilder.Create(context, consol.ReceivingForwarderWithContact);
			outturnReport.ReceivingForwarderSON = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			outturnReport.ReceivingForwarderCI5 = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
		}

		void PopulateTransportDetails(OutturnReport outturnReport)
		{
			var firstSeaLeg = SeaTransportsInLegOrder.FirstOrDefault();
			var lastSeaLeg = SeaTransportsInLegOrder.LastOrDefault();
			var arrivalFRSeaLeg = SeaTransportsInLegOrder.LastOrDefault(t => Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKDiscPort));

			outturnReport.PortOfOrigin = Unloco.Create(context, firstSeaLeg?.LoadPort);
			outturnReport.PortOfDestination = Unloco.Create(context, arrivalFRSeaLeg?.DiscPort);

			var operationalPort = consol.UnpackDepotAddress?.RelatedPortCode;
			outturnReport.PCS = operationalPort?.GetPCS() ?? ZString.Empty;
			outturnReport.OperationalPort = Unloco.Create(context, operationalPort);

			outturnReport.VesselName = arrivalFRSeaLeg?.JW_Vessel ?? ZString.Empty;
			outturnReport.VoyageFlightNo = arrivalFRSeaLeg?.JW_VoyageFlight ?? ZString.Empty;
			outturnReport.ATPReference = arrivalFRSeaLeg?.Sailing?.Destination.JB_ArrivalReference ?? ZString.Empty;

			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Other
			};

			outturnReport.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol.JK_ConsolMode)
					? (ZString)Core.Constants.ContainerModes.FCL
					: consol.JK_ConsolMode
			};

			outturnReport.ShipmentType = new CodeDescription(consol.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol.JK_AgentType
			};
		}

		void PopulateGoodsDetails(OutturnReport outturnReport)
		{
			var goodsDetailBuilder = new GoodsDetailBuilder();
			var goodsDetails = new List<GoodsDetail>();

			foreach (ForwardingShipment shipment in consol.Shipments.OfType<ForwardingShipment>().Where(s => !s.IsMasterShipmentRepresentingAllChildShipments))
			{
				var goodDetail = goodsDetailBuilder.Build(shipment);
				goodDetail.ICVReference = shipment.Numbers
					.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ImportConventional, Core.Constants.CountryCodes.France)
					?.CE_EntryNum ?? ZString.Empty;

				goodsDetails.Add(goodDetail);
			}

			outturnReport.GoodsDetails = goodsDetails.OrderBy(x => x.ShipmentNumber).ToList();
		}

		void PopulateContainers(OutturnReport outturnReport)
		{
			var containerBuilder = new BookingContainerBuilder();
			var containerDOs = new List<BookingContainer>();
			var packLineDOs = outturnReport.GoodsDetails.SelectMany(x => x.PackingLines);
			var containerBizObjs = GetContainers();

			foreach (ForwardingContainer containerBO in containerBizObjs)
			{
				var containerPackLinePKs = containerBO.PackLines.Cast<PackLine>().Select(p => p.PK).ToArray();
				var containerPackingLineDOs = packLineDOs.Where(x => containerPackLinePKs.Contains((ZGuid)x.Identifier)).ToArray();
				var containerDO = containerBuilder.Build(containerBO, containerPackingLineDOs, true);

				containerDO.UnpackArea = consol.UnpackDepotAddress.GetPortArea();
				containerDO.UnpackLocation = consol.UnpackDepotAddress.GetPortLocation();
				containerDO.LPDReference = GetLPDReferenceFromEvents(containerDO);

				containerDOs.Add(containerDO);
			}

			outturnReport.Containers = containerDOs.OrderBy(container => container.Number).ToArray();
		}

		#region Validations

		void AddValidations(OutturnReport outturnReport)
		{
			outturnReport.BillOfLadingInfo.AddMessageErrorIfEmpty(Res.GetString("5546a86a-7f12-416e-b990-c3e4f192f27b", "Bill of Lading (BOL) Number is required."));
			outturnReport.ATPReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("0fee41c9-f25e-4ddd-b016-75f475a73815", "ATP Reference is required, Arrival Reference missing from Sailing Schedule of Consol > Routing > Last SEA leg."));

			AddContainerValidations(outturnReport);
			AddAddressValidations(outturnReport);
			AddTransportDetailsValidation(outturnReport);
			AddGoodsDetailsValidations(outturnReport);
		}

		void AddContainerValidations(OutturnReport outturnReport)
		{
			foreach (var container in outturnReport.Containers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("24BC460C-2DCD-4A38-9777-E95890C43129", "Container number is required."));

				foreach (var packingSummary in container.PackingSummaries)
				{
					packingSummary.ICVReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("8fbed493-057f-4db9-872b-16a6beff98b0", "ICV Reference is required. Import Ref Number missing from Shipment > Packing > Pack Line."));

					packingSummary.TotalPackagesInfo.AddMessageError(() => packingSummary.TotalPackages == 0, Res.GetString("ef3efb52-e321-47f3-8c52-cf39c773c86a", "Number of Packs is required."));
					packingSummary.TotalWeight.ValueInfo.AddMessageError(() => packingSummary.TotalWeight.Value == 0, Res.GetString("33582f71-d624-4dd7-9bf2-335f1cef85de", "Weight is required."));

					packingSummary.TotalOutturnedPackagesInfo.AddMessageError(() => packingSummary.TotalOutturnedPackages == 0, Res.GetString("eba6b7ac-e40d-46c1-8e65-ccb86571804a", "Number of Outturn packs is required."));
					packingSummary.TotalOutturnedWeight.ValueInfo.AddMessageError(() => packingSummary.TotalOutturnedWeight.Value == 0, Res.GetString("395c87c5-4c2b-489f-901c-93a8e5416595", "Outturn weight is required."));

					packingSummary.TotalPackagesInfo.AddMessageError(() => packingSummary.IsAnyLastKnownTWStatusEmpty, Res.GetString("a00532e6-61f7-43ed-95db-33a4fd0cda61", "There are pack lines with 'Last Known Transit Warehouse Status' blank. All pack lines must have 'Last Known Transit Warehouse Status' to send this message."));
				}

				container.UnpackLocationInfo.AddMessageErrorIfEmpty(Res.GetString("67a5075f-167a-4102-bc24-10f38610a63c", "Unpacking Location is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')."));
				container.UnpackLocationInfo.AddAsciiCharactersValidation();
				container.UnpackAreaInfo.AddMessageErrorIfEmpty(Res.GetString("4201d19c-0e83-45e6-b329-0b2114ea4f21", "Unpacking Area is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')."));
				container.UnpackAreaInfo.AddAsciiCharactersValidation();

				container.LPDReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("f952bb62-9e52-491c-9bd9-90e87f7aa56f", "The Outturn Report (CDM) message cannot be sent without the LPD Reference.  Please send Provisional Unpacking List (LPD) message first and wait for Acknowledgment and LPD Notification before sending Outturn Report (CDM) message."));
			}
		}

		void AddAddressValidations(OutturnReport outturnReport)
		{
			var sendingPartyName = Res.GetString("D840409F-B512-40D0-86F0-5B15C83D26FF", "Sending Party");
			var cfsPartyName = Res.GetString("5DA2C581-C033-4F11-8E86-C438B17F692D", "CFS");

			var configPath = Res.GetString("0ab7ba64-ab16-4391-8508-ceb394a0f5a2", "Consol > Carrier");

			var messageValidation = Res.GetString("c9981c74-475f-4988-aa4c-83cb025fb0aa", "name and address is required.");

			outturnReport.SendingParty.AddPartyNameAndAddressValidation(sendingPartyName, messageValidation: messageValidation);
			outturnReport.CFS.AddPartyNameAndAddressValidation(cfsPartyName, messageValidation: messageValidation);

			outturnReport.FormattedSendingPartyProviderIDInfo.AddFormattedProviderIDValidations(outturnReport.PCS, OrgCusCode.FranceCodeTypes.SOW, sendingPartyName, configPath: configPath, isOrganization: true);
			outturnReport.FormattedCFSProviderIDInfo.AddFormattedProviderIDValidations(outturnReport.PCS, OrgCusCode.FranceCodeTypes.SOW, cfsPartyName, configPath: configPath, isOrganization: true);
		}

		void AddGoodsDetailsValidations(OutturnReport outturnReport)
		{
			foreach (var goodsDetail in outturnReport.GoodsDetails)
			{
				goodsDetail.HouseBillNumberInfo.AddMessageErrorIfEmpty(Res.GetString("3DBF416F-5035-4D11-844D-78DE4A2DDFDC", "House Bill Number is required."));
				goodsDetail.MarksAndNumbersInfo.AddMessageErrorIfEmpty(Res.GetString("80d74470-1a7a-4475-910c-957a18407d1d", "Marks & Numbers are required."));
			}
		}

		void AddTransportDetailsValidation(OutturnReport outturnReport)
		{
			outturnReport.PCSInfo.AddPCSValidation();
			((Unloco)outturnReport.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("9e6c626c-4977-43bc-8f5b-b9369759fd03", "Operational Port is required, UNLOCO missing from Consol > Arrival > CFS Address."));
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
