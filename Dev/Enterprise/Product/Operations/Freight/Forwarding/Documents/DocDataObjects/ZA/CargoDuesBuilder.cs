using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA.CargoDuesHelper;
using CoreConstants = Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal class CargoDuesBuilder
	{
		public CargoDuesBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = consol;
			this.parameters = parameters;
			logProvider = parameters?.LogProvider;
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IDocDataObjectParameters parameters;
		readonly IStmALogProvider logProvider;
		readonly IContext context;

		public CargoDues Build()
		{
			var cargoDues = new CargoDues(nameof(ForwardingConsol), consol.JK_UniqueConsignRef, DataContext.ShippingInstruction)
			{
				IsQuotationDocument = parameters.DocumentTitle.EndsWith((NoResString)"(Quotation)", StringComparison.Ordinal), // programmatic constant
			};

			var isCoastwiseDocument = new string[] { DocumentNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesDischargeCoastwise, DocumentNames.CargoDuesLoadCoastwiseQuotation, DocumentNames.CargoDuesDischargeCoastwiseQuotation }.Contains(parameters?.DocumentTitle);
			cargoDues.IsExportDocument = new string[] { DocumentNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesLoadCoastwiseQuotation, DocumentNames.CargoDuesExport, DocumentNames.CargoDuesExportQuotation }.Contains(parameters?.DocumentTitle);
			cargoDues.Direction = isCoastwiseDocument ? cargoDues.IsExportDocument ? (NoResString)"Load Coastwise" : (NoResString)"Discharge Coastwise" : cargoDues.IsExportDocument ? (NoResString)"Export" : (NoResString)"Import"; // programmatic constant

			var mostInterestingLeg = FindMostInterestingLeg(cargoDues);

			PopulateTransportLegInfos(cargoDues, mostInterestingLeg);

			PopulateDocumentProperties(cargoDues);
			PopulateAddresses(cargoDues);
			PopulatePortInformation(cargoDues, mostInterestingLeg);
			PopulateTNPAInformation(cargoDues);
			PopulateCountryInformation(cargoDues);
			PopulateAdditionalInformation(cargoDues, mostInterestingLeg);
			PopulateContainers(cargoDues);
			PopulateGoodsInfos(cargoDues);
			PopulateCargoDuesInformation(cargoDues);

			AddValidations(cargoDues);
			cargoDues.ValidateAllIncludingChildren();

			return cargoDues;
		}

		#region Populations

		Transport FindMostInterestingLeg(CargoDues cargoDues)
		{
			if (cargoDues.IsExportDocument)
			{
				var firstLegToLoadAtSouthAfrica = consol.Transports.OfType<Freight.Business.Transport>().FirstOrDefault(transport =>
					transport.JW_TransportMode.EqualsIgnoringCase(CoreConstants.TransportModes.Sea)
					&& transport.JW_RL_NKLoadPort.StartsWith(CoreConstants.CountryCodes.SouthAfrica, StringComparison.OrdinalIgnoreCase));
				if (firstLegToLoadAtSouthAfrica != null)
				{
					return Transport.Create(context, firstLegToLoadAtSouthAfrica);
				}
			}
			else
			{
				var lastLegToDischargeAtSouthAfrica = consol.Transports.OfType<Freight.Business.Transport>().LastOrDefault(transport =>
					transport.JW_TransportMode.EqualsIgnoringCase(CoreConstants.TransportModes.Sea)
					&& transport.JW_RL_NKDiscPort.StartsWith(CoreConstants.CountryCodes.SouthAfrica, StringComparison.OrdinalIgnoreCase));
				if (lastLegToDischargeAtSouthAfrica != null)
				{
					return Transport.Create(context, lastLegToDischargeAtSouthAfrica);
				}
			}
			return null;
		}

		void PopulateTransportLegInfos(CargoDues cargoDues, Transport mostInterestingLeg)
		{
			if (mostInterestingLeg == null)
			{
				return;
			}

			cargoDues.TNPAArrivalNumber = cargoDues.IsExportDocument ? mostInterestingLeg.DepartureReference : mostInterestingLeg.ArrivalReference;
			cargoDues.IMONumber = mostInterestingLeg.Vessel.LloydsIMO;
			cargoDues.RadioCallSign = mostInterestingLeg.Vessel.RadioCallSign;
			cargoDues.Eta = mostInterestingLeg.ETA;
			cargoDues.Etd = mostInterestingLeg.ETD;

			if (cargoDues.IsExportDocument && mostInterestingLeg.PortOfDischarge != null)
			{
				var onForwardingTransportLegBO = consol.Transports.OfType<Freight.Business.Transport>()
					.FirstOrDefault(transport =>
						transport.TransportMode.EqualsIgnoringCase(CoreConstants.TransportModes.Sea)
						&& transport.JW_RL_NKLoadPort.EqualsIgnoringCase(mostInterestingLeg.PortOfDischarge.Code));

				if (onForwardingTransportLegBO != null)
				{
					var onForwardingTransport = Transport.Create(context, onForwardingTransportLegBO);
					mostInterestingLeg.LegOrder = 1;
					onForwardingTransport.LegOrder = 2;

					cargoDues.Transports = Transports.Create(new[] { mostInterestingLeg, onForwardingTransport });
					cargoDues.Transports.Main = mostInterestingLeg;
					cargoDues.Transports.OnForwarding = onForwardingTransport;
				}
				else
				{
					cargoDues.Transports = Transports.Create(new[] { mostInterestingLeg });
					cargoDues.Transports.Main = mostInterestingLeg;
					mostInterestingLeg.LegOrder = 1;
				}
			}
			else if (mostInterestingLeg.PortOfLoading != null)
			{
				var preTransportLegBO = consol.Transports.OfType<Freight.Business.Transport>()
					.FirstOrDefault(transport =>
						transport.TransportMode.EqualsIgnoringCase(CoreConstants.TransportModes.Sea)
						&& transport.JW_RL_NKDiscPort.EqualsIgnoringCase(mostInterestingLeg.PortOfLoading.Code));

				if (preTransportLegBO != null)
				{
					var preCarriageTransport = Transport.Create(context, preTransportLegBO);
					preCarriageTransport.LegOrder = 1;
					mostInterestingLeg.LegOrder = 2;

					cargoDues.Transports = Transports.Create(new[] { preCarriageTransport, mostInterestingLeg });
					cargoDues.Transports.PreCarriage = preCarriageTransport;
					cargoDues.Transports.Main = mostInterestingLeg;
				}
				else
				{
					cargoDues.Transports = Transports.Create(new[] { mostInterestingLeg });
					cargoDues.Transports.Main = mostInterestingLeg;
					mostInterestingLeg.LegOrder = 1;
				}
			}
		}

		void PopulateDocumentProperties(CargoDues cargoDues)
		{
			cargoDues.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List)
			{
				Code = consol.JK_ConsolMode
			};

			cargoDues.IsBulk = (new ZString[] { CoreConstants.ContainerModes.Bulk, CoreConstants.ContainerModes.Liquid }).Contains(consol.JK_ConsolMode);
			cargoDues.IsBreakBulk = (new ZString[] { CoreConstants.ContainerModes.BreakBulk, CoreConstants.ContainerModes.RollOnRollOff }).Contains(consol.JK_ConsolMode);
			cargoDues.IsCoastwise = consol.JK_TransportMode.EqualsIgnoringCase(CoreConstants.TransportModes.Sea) && IsInSouthAfrica(consol.JK_RL_NKLoadPort) && IsInSouthAfrica(consol.JK_RL_NKDischargePort);
			cargoDues.IsDeepSea = consol.JK_TransportMode.EqualsIgnoringCase(CoreConstants.TransportModes.Sea) && (!IsInSouthAfrica(consol.JK_RL_NKLoadPort) || !IsInSouthAfrica(consol.JK_RL_NKDischargePort));
			cargoDues.IsTranship = false;
			cargoDues.CancellingOrder = false;
			cargoDues.IsContainerised = consol.TransportMode.EqualsIgnoringCase(CoreConstants.TransportModes.Sea)
				&& (new ZString[]
					{
						CoreConstants.ContainerModes.FCL,
						CoreConstants.ContainerModes.LCL,
						CoreConstants.ContainerModes.Groupage,
						CoreConstants.ContainerModes.BuyersConsol,
						CoreConstants.ContainerModes.ShippersConsol
					}).Contains(consol.JK_ConsolMode);
		}

		bool IsInSouthAfrica(ZString port) => port.SubstringSafe(0, 2).EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void PopulateTNPAInformation(CargoDues cargoDues)
		{
			var lastMessageWithRFN = logProvider
				?.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode
					&& CheckLogValue(log, EventReferenceParameters.Codes.Department, "TNPA")
					&& CheckLogValue(log, EventReferenceParameters.Codes.MessageType, parameters.DocumentTitle)
					&& CheckLogValue(log, EventReferenceParameters.Codes.ReferenceNumber))
				.GroupBy(log => log.SL_EventTime)
				?.OrderBy(log => log.Key)
				.LastOrDefault()
				?.OrderBy(log => log.SL_PostedTimeUtc)
				.LastOrDefault();
			if (lastMessageWithRFN != null)
			{
				if (lastMessageWithRFN.Parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out var tnpaOrderNumber))
				{
					cargoDues.TNPAOrderNumber = tnpaOrderNumber;
				}
			}
			cargoDues.TNPAQuotationNumber = "";

			var tnpaAccountNumber = PortMessagingRegistry.Instance.TNPAAccountNumber.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Env.CurrentBranchPK, Guid.Empty)
					.OfType<TNPAAccountNumber>()
					.FirstOrDefault(x => x.Port == cargoDues.ServicePort.Code);
			if (tnpaAccountNumber != null)
			{
				cargoDues.TNPAAccountNumber = cargoDues.IsCoastwise ? tnpaAccountNumber.CoastwiseNumber : cargoDues.IsExportDocument ? tnpaAccountNumber.ExportNumber : tnpaAccountNumber.ImportNumber;
			}
			else
			{
				cargoDues.TNPAAccountNumber = ZString.Empty;
			}

			var carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			var tnpaRegistrationCode = carrier.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("TNP"))?.Value ?? ZString.Empty;

			var consolCarrierCode = carrier.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("CCC"))?.Value ?? ZString.Empty;
			cargoDues.CarrierCode = !string.IsNullOrEmpty(tnpaRegistrationCode) ? tnpaRegistrationCode : consolCarrierCode;
		}

		bool CheckLogValue(StmALog log, ZString parameterName, string expectedValue = null)
		{
			return !(log.Parameters.TryGetValue(parameterName, out string referenceNumber)
				&& !string.IsNullOrEmpty(expectedValue)) || expectedValue.Equals(referenceNumber, StringComparison.OrdinalIgnoreCase);
		}

		void PopulateAddresses(CargoDues cargoDues)
		{
			cargoDues.Agent = cargoDues.IsExportDocument ? AddressBuilder.Create(context, consol.SendingForwarderAddress) : AddressBuilder.Create(context, consol.ReceivingForwarderAddress);
			cargoDues.ArrivalCTO = AddressBuilder.Create(context, consol.ArrivalCTOAddress);
			cargoDues.DepartureCTO = AddressBuilder.Create(context, consol.DepartureCTOAddress);
			cargoDues.ShippingLine = AddressBuilder.Create(context, consol.ShippingLineAddress);

			var isDirect = consol.JK_AgentType.EqualsIgnoringCase(CoreConstants.AgentType.Direct);
			cargoDues.Shipper = (isDirect || consol.ShipmentCount == 1) ? AddressBuilder.Create(context, consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault()?.GetConsignorDocAddress) : AddressBuilder.Create(context, consol.SendingForwarderAddress);
			cargoDues.Consignee = (isDirect || consol.ShipmentCount == 1) ? AddressBuilder.Create(context, consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault()?.GetConsigneeDocAddress) : AddressBuilder.Create(context, consol.ReceivingForwarderAddress);
			cargoDues.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
		}

		void PopulateCountryInformation(CargoDues cargoDues)
		{
			cargoDues.PlaceOfReceipt = Unloco
				.Create(context, consol.LoadPort)
				.WithCustomNameProvider(UnlocoExtensions.GetDetailedPortName);

			cargoDues.PlaceOfDelivery = Unloco
				.Create(context, consol.DischargePort)
				.WithCustomNameProvider(UnlocoExtensions.GetDetailedPortName);
		}

		void PopulatePortInformation(CargoDues cargoDues, Transport mostInterestingLeg)
		{
			cargoDues.PortOfLoading = Unloco.Create(context, consol.Transports.FirstTransportWithTransportMode(CoreConstants.TransportModes.Sea)?.LoadPort);
			cargoDues.PortOfDischarge = Unloco.Create(context, consol.Transports.LastTransportWithTransportMode(CoreConstants.TransportModes.Sea)?.DiscPort);
			cargoDues.ServicePort = Unloco.Create(context, cargoDues.IsExportDocument ? mostInterestingLeg?.PortOfLoading : mostInterestingLeg?.PortOfDischarge);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void PopulateAdditionalInformation(CargoDues cargoDues, Transport mostInterestingLeg)
		{
			cargoDues.ClientRef = consol.JK_UniqueConsignRef;
			cargoDues.WayBillNumber = consol.JK_MasterBillNum;

			var carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			var consolCarrierCode = carrier.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("CCC"))?.Value ?? ZString.Empty;
			var legCarrierCode = mostInterestingLeg?.Carrier.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("CCC"))?.Value ?? ZString.Empty;
			cargoDues.ContainerOperator = cargoDues.IsContainerised
				? consolCarrierCode
				: (!string.IsNullOrEmpty(legCarrierCode) ? legCarrierCode : consolCarrierCode);

			var departureFromTNP = mostInterestingLeg?.DepartureFrom.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("TNP"))?.Value ?? ZString.Empty;
			var departureCTOTNP = cargoDues.DepartureCTO.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.Equals(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("TNP"))?.Value ?? ZString.Empty;
			var arrivalFromTNP = mostInterestingLeg?.ArrivalAt.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("TNP"))?.Value ?? ZString.Empty;
			var arrivalCTOTNP = cargoDues.ArrivalCTO.RegistrationNumbers.FirstOrDefault(number => number.CountryOfIssue.Code.EqualsIgnoringCase(CoreConstants.CountryCodes.SouthAfrica) && number.Type.Code.EqualsIgnoringCase("TNP"))?.Value ?? ZString.Empty;
			cargoDues.Terminal = cargoDues.IsExportDocument
				? !string.IsNullOrEmpty(departureFromTNP) ? departureFromTNP : departureCTOTNP
				: !string.IsNullOrEmpty(arrivalFromTNP) ? arrivalFromTNP : arrivalCTOTNP;
		}

		void PopulateContainers(CargoDues cargoDues)
		{
			if (!cargoDues.IsContainerised)
			{
				return;
			}

			var containerBuilder = new ContainerBuilder();
			var containerDOs = new List<Container>();

			foreach (ForwardingContainer containerBO in consol.Containers)
			{
				var containerDO = containerBuilder.Build(containerBO, context, containerBO.PackLines.Cast<ForwardingPackLine>().ToArray());
				containerDO.IsEmpty = containerBO.JC_IsEmptyContainer;

				containerDOs.Add(containerDO);
			}

			cargoDues.Containers = containerDOs
			.OrderBy(container => container.Number)
			.ToArray();

			cargoDues.TotalNumberOfPacks = cargoDues.Containers.Sum(x => x.PackCount);
		}

		void PopulateGoodsInfos(CargoDues cargoDues)
		{
			if (cargoDues.IsContainerised)
			{
				return;
			}

			cargoDues.ShipmentPackingInfos = consol.TopLevelShipments.OfType<ForwardingShipment>().Select(s => BuildShipment(s)).ToList();
			cargoDues.TotalNumberOfPacks = cargoDues.ShipmentPackingInfos.SelectMany(s => s.AllGoodsInfoIncludeCoLoadCollection).Sum(s => s.NumberOfPacks);
		}

		ShipmentPackingInfo BuildShipment(ForwardingShipment shipmentBO)
		{
			var shipment = new ShipmentPackingInfo(shipmentBO.PK);
			shipment.ShipmentType = new CodeDescription(shipmentBO.Lookups.JS_ShipmentType_List)
			{
				Code = shipmentBO.JS_ShipmentType
			};

			shipment.TotalWeight = new Measurement()
			{
				Value = CoreConstants.Weight.Convert(shipmentBO.JS_ActualWeight, shipmentBO.JS_UnitOfWeight, CoreConstants.Weight.Kilograms),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = CoreConstants.Weight.Kilograms
				}
			};

			shipment.OuterPacks = shipmentBO.JS_OuterPacks;

			shipment.PackType = new CodeDescription(shipmentBO.Lookups.PackTypes)
			{
				Code = shipmentBO.JS_F3_NKPackType
			};

			shipment.Consignee = AddressBuilder.Create(context, shipmentBO.ConsigneeDocumentaryAddress);
			shipment.Consignor = AddressBuilder.Create(context, shipmentBO.ConsignorDocumentaryAddress);

			shipment.GoodsInfoCollection = shipmentBO.OuterPackLines.OfType<ForwardingPackLine>().Select(packingLine => BuildGoodsInfo(packingLine, shipmentBO)).ToList();
			shipment.ShipmentPackingInfos = shipmentBO.CoLoadShipments.Cast<ForwardingShipment>().Select(x => BuildShipment(x)).ToArray();

			return shipment;
		}

		GoodsInfo BuildGoodsInfo(ForwardingPackLine packingLine, ForwardingShipment shipmentBO)
		{
			return new GoodsInfo(packingLine.PK)
			{
				MarksAndNos = !string.IsNullOrEmpty(packingLine.JL_MarksAndNumbers) ? packingLine.JL_MarksAndNumbers : shipmentBO.Notes.FindByDescription((NoResString)"Marks & Numbers").FirstOrDefault()?.ST_NoteText ?? ZString.Empty, // programmatic constant
				NumberOfPacks = packingLine.JL_PackageCount,
				PackType = packingLine.JL_F3_NKPackType,
				GoodsDescription = string.Format(CultureInfo.InvariantCulture, "{0} - {1}",
								GetPackageDescription(consol.JK_ConsolMode),
								new[] { packingLine.JL_DetailedDescription, packingLine.JL_Description, shipmentBO.DetailedGoodsDescriptionNoteText, shipmentBO.JS_GoodsDescription }.FirstOrDefault(text => !string.IsNullOrEmpty(text))), // programmatic constant
				GrossMass = new Measurement
				{
					Value = CoreConstants.Weight.Convert(packingLine.JL_ActualWeight, packingLine.JL_ActualWeightUQ, CoreConstants.Weight.Kilograms),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = CoreConstants.Weight.Kilograms
					}
				}
			};
		}

		ZString GetPackageDescription(ZString containerModeCode)
		{
			switch (containerModeCode)
			{
				case CoreConstants.ContainerModes.Bulk:
					return (NoResString)"Dry Bulk"; // non-translatable constants
				case CoreConstants.ContainerModes.Liquid:
					return (NoResString)"Liquid Bulk"; // non-translatable constants
				case CoreConstants.ContainerModes.BreakBulk:
					return (NoResString)"Break Bulk"; // non-translatable constants
				case CoreConstants.ContainerModes.RollOnRollOff:
					return "RO-RO"; // non-translatable constants
				default:
					return (NoResString)"Other"; // non-translatable constants
			}
		}

		void PopulateCargoDuesInformation(CargoDues cargoDues)
		{
			cargoDues.CargoDuesSectionTitle = (NoResString)"Cargo Dues"; // non-translatable form title
			cargoDues.DuesCollectionElement1 = new DuesCollectionRow();
			cargoDues.DuesCollectionElement2 = new DuesCollectionRow();
			cargoDues.DuesCollectionElement3 = new DuesCollectionRow();
			cargoDues.DuesCollectionElement4 = new DuesCollectionRow();
			cargoDues.DuesCollectionElement5 = new DuesCollectionRow();
			cargoDues.DuesCollectionElement6 = new DuesCollectionRow();
			cargoDues.DuesCollectionElement7 = new DuesCollectionRow();
			cargoDues.DuesCollectionElement8 = new DuesCollectionRow();

			var relatedEDIMessage = GetMessageAcceptedUniversalEventLogsInDescendingOrder(consol.Logs, parameters?.DocumentTitle)?.FirstOrDefault()?.RelatedEDIMessage;
			if (relatedEDIMessage != null)
			{
				var universalEvent = relatedEDIMessage.Message.GetEM_MessageTextReader().Parse<UniversalEvent>();
				var contextCollection = universalEvent?.ContextCollection;

				if (contextCollection != null)
				{
					SetDuesCollectionElement(1, cargoDues.DuesCollectionElement1, contextCollection);
					SetDuesCollectionElement(2, cargoDues.DuesCollectionElement2, contextCollection);
					SetDuesCollectionElement(3, cargoDues.DuesCollectionElement3, contextCollection);
					SetDuesCollectionElement(4, cargoDues.DuesCollectionElement4, contextCollection);
					SetDuesCollectionElement(5, cargoDues.DuesCollectionElement5, contextCollection);
					SetDuesCollectionElement(6, cargoDues.DuesCollectionElement6, contextCollection);
					SetDuesCollectionElement(7, cargoDues.DuesCollectionElement7, contextCollection);
					SetDuesCollectionElement(8, cargoDues.DuesCollectionElement8, contextCollection);

					var subTotal = contextCollection.FirstOrDefault(c => c.Type == ContextCollectionTypes.TotalCargoDuesAmount);
					cargoDues.SubTotal = RoundToTwoDecimals(ParseStringToDecimal(subTotal != null ? subTotal.Value.ToString() : string.Empty));
					cargoDues.VAT = RoundToTwoDecimals((decimal)cargoDues.SubTotal * GetTaxRateForCountryOfCurrentCompany(context.Factory));
					cargoDues.TotalR = RoundToTwoDecimals(cargoDues.SubTotal + cargoDues.VAT);
				}
			}
		}

		#endregion

		#region Validations

		void AddValidations(CargoDues cargoDues)
		{
			AddCompanyNameValidation(cargoDues);
			AddTNPADataValidation(cargoDues);
			AddCountryVesselAndPortValidation(cargoDues);
			AddAdditionalInformationValidation(cargoDues);
			AddGoodsAndContainerDetailsValidation(cargoDues);
			AddCargoDuesValidation(cargoDues);
		}

		void AddCompanyNameValidation(CargoDues cargoDues)
		{
			cargoDues.Shipper.CompanyNameInfo.AddMessageError(() =>
				!cargoDues.IsContainerised
				&& string.IsNullOrEmpty(cargoDues.Shipper.CompanyName)
				&& cargoDues.PortOfLoading.Code.StartsWith("ZA", StringComparison.OrdinalIgnoreCase),
				Res.GetString("0A48EF04-5A4D-47D3-8347-2F4FAB031501", "Shipper name is mandatory for non-containerized consol when place of receipt is in ZA."));

			cargoDues.Consignee.CompanyNameInfo.AddMessageError(() =>
				!cargoDues.IsContainerised
				&& string.IsNullOrEmpty(cargoDues.Consignee.CompanyName)
				&& cargoDues.PortOfDischarge.Code.StartsWith("ZA", StringComparison.OrdinalIgnoreCase),
				Res.GetString("AE8CFAF7-02A3-4321-BD9D-207C20EC0CF1", "Consignee name is mandatory for non-containerized consol when place of delivery is in ZA."));
		}

		void AddTNPADataValidation(CargoDues cargoDues)
		{
			cargoDues.TNPAOrderNumberInfo.AddMessageError(() => string.IsNullOrEmpty(cargoDues.TNPAOrderNumber) && cargoDues.CancellingOrder, Res.GetString("2AE622E1-75C0-4615-B1F7-2AB613B5BD04", "TNPA Order Number is mandatory for cancellation message."));
			cargoDues.TNPAAccountNumberInfo.AddMessageErrorIfEmpty(Res.GetString("DE90A91F-83B0-47DF-945C-8B74DDD4C89D", "TNPA Account Number is required."));
			cargoDues.TNPAArrivalNumberInfo.AddMessageErrorIfEmpty(Res.GetString("253CDC2E-D386-447B-AE35-C1D743013C5C", "TNPA Arrival Number is required."));
			cargoDues.IMONumberInfo.AddMessageErrorIfEmpty(Res.GetString("73BEDABE-2258-43FD-95E6-0F3B68090688", "IMO Number is required."));
			cargoDues.CarrierCodeInfo.AddMessageErrorIfEmpty(Res.GetString("7688A52A-DAE7-441C-927A-48C0EA6A3718", "Carrier Code is required."));
			cargoDues.EtaInfo.AddMessageErrorIfEmpty(Res.GetString("F1EE5E93-7522-4AD1-9661-64B48A9B1849", "ETA/ETD is required."));
			cargoDues.EtdInfo.AddMessageErrorIfEmpty(Res.GetString("FBB8F672-D0C7-46EC-8006-81728CA0B228", "ETA/ETD is required."));
		}

		void AddCountryVesselAndPortValidation(CargoDues cargoDues)
		{
			cargoDues.PlaceOfReceipt.Country.NameInfo.AddMessageErrorIfEmpty(Res.GetString("7B7332BE-8343-44E0-8F82-A5F1EC635630", "Country/Region of Origin is required."));
			cargoDues.PlaceOfDelivery.Country.NameInfo.AddMessageErrorIfEmpty(Res.GetString("E2E00391-3D0D-4A38-8F9A-F52F40B72934", "Country/Region of Destination is required."));
			cargoDues.Transports?.Main?.VoyageFlightNumberInfo.AddMessageErrorIfEmpty(Res.GetString("240ACDC1-402E-4FC1-BE1D-E1C526C1BD58", "Voyage number is required."));
			cargoDues.Transports?.Main?.Vessel?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("0790F1D4-25DB-4C8C-8977-BB4FBDD40338", "Vessel Name is required."));
			cargoDues.PortOfLoading.NameInfo.AddMessageErrorIfEmpty(Res.GetString("BDF75559-F8A2-4F89-8C97-C78943BE6FC8", "Port of Loading is required."));
			cargoDues.PortOfDischarge.NameInfo.AddMessageErrorIfEmpty(Res.GetString("1D8C0633-C3FA-4153-B5EE-716AC247B411", "Port of Discharge is required."));
			cargoDues.ServicePort.NameInfo.AddMessageErrorIfEmpty(Res.GetString("EBE87043-F935-4806-AAE4-B1CC0AEB6811", "Service Port is required."));
		}

		void AddAdditionalInformationValidation(CargoDues cargoDues)
		{
			cargoDues.ClientRefInfo.AddMessageErrorIfEmpty(Res.GetString("263419EB-3535-4C1E-A9A6-26145E7601F7", "Client Ref. number is required."));
			cargoDues.WayBillNumberInfo.AddMessageError(() => !cargoDues.IsQuotationDocument && string.IsNullOrEmpty(cargoDues.WayBillNumber), Res.GetString("34181019-2BE0-482D-A34A-CD30A93D39CC", "Bill of Lading/Mates Receipt is required."));
			cargoDues.ContainerOperatorInfo.AddMessageErrorIfEmpty(Res.GetString("B21548BF-B321-4F34-B600-8729BC8FA3BB", "Container Operator/Vessels Agent is required."));
			cargoDues.TerminalInfo.AddMessageError(() => !cargoDues.IsQuotationDocument && string.IsNullOrEmpty(cargoDues.Terminal), Res.GetString("0E6236DA-843B-4654-A151-E8952629C7C2", "TNPA CTO Code is mandatory for Cargo Dues. Please configure CTO Code from Organization /> Config /> Registration Numbers/Codes as TNP code for country/region ZA."));
		}

		void AddGoodsAndContainerDetailsValidation(CargoDues cargoDues)
		{
			if (cargoDues.IsContainerised)
			{
				foreach (var container in cargoDues.Containers)
				{
					container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("9C185170-6E0F-42F9-B5BB-15D8B6421D23", "Container Number is required."));
					container.Type?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("B994E3D6-2F31-4A1F-AE34-FB6F981D2472", "Container Type is required."));
				}
				cargoDues.CargoDuesWarningPlaceHolderInfo.AddMessageError(() => cargoDues.Containers.Count == 0, Res.GetString("B01E7E22-CF1C-445A-8690-025A220F8360", "Container Number is required."));
			}
			else
			{
				cargoDues.CargoDuesWarningPlaceHolderInfo.AddMessageError(() => cargoDues.ShipmentPackingInfos.Count == 0, Res.GetString("8F142EDF-D2F2-4B97-A7B7-033EE2C70CA4", "There are no Shipments registered on this consol."));
			}
		}

		void AddCargoDuesValidation(CargoDues cargoDues)
		{
			cargoDues.CargoDuesSectionTitleInfo.AddWarning(() => true, Res.GetString("34837907-3634-4E02-8FCA-6AE5DDDE23F8", "This section is for information only and is not included in Cargo Dues EDI message."));
		}

		#endregion
	}
}
