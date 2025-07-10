using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ConsolidationAdviceBuilder
	{
		readonly ForwardingShipment shipment;
		readonly IContext context;

		public ConsolidationAdviceBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		public ConsolidationAdvice Build()
		{
			var consolidationAdvice = new ConsolidationAdvice(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);

			consolidationAdvice.TransportMode = new CodeDescription(context.TransportModes) { Code = shipment.TransportMode };
			consolidationAdvice.ContainerMode = new CodeDescription(shipment.Lookups.JS_PackingMode_List)
			{
				Code = shipment.JS_PackingMode
			};
			consolidationAdvice.ShipmentType = new CodeDescription(shipment.Lookups.JS_ShipmentType_List)
			{
				Code = ShipmentTypes.CoLoadMaster
			};
			consolidationAdvice.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			consolidationAdvice.BookingParty = AddressBuilder.Create(context, shipment.BookingPartyDocumentaryAddress);
			consolidationAdvice.SendingForwarder = AddressBuilder.Create(context, shipment.ConsignorDocumentaryAddress);
			consolidationAdvice.ReceivingForwarder = AddressBuilder.Create(context, shipment?.ConsigneeDocumentaryAddress);

			consolidationAdvice.Origin = Unloco.Create(context, shipment.Origin);
			consolidationAdvice.Destination = Unloco.Create(context, shipment.Destination);
			consolidationAdvice.EstimatedTimeDeparture = shipment.JS_E_DEP;
			consolidationAdvice.EstimatedTimeArrival = shipment.JS_E_ARV;

			consolidationAdvice.PortOfLoading = Unloco.Create(context, FirstSeaLeg?.LoadPort);
			consolidationAdvice.PortOfDischarge = Unloco.Create(context, LastSeaLeg?.DiscPort);
			consolidationAdvice.VesselName = FirstSeaLeg?.JW_Vessel ?? ZString.Empty;
			consolidationAdvice.LloydsIMO = FirstSeaLeg?.Vessel?.RV_LloydsNumber ?? ZString.Empty;
			consolidationAdvice.VoyageNumber = FirstSeaLeg?.JW_VoyageFlight ?? ZString.Empty;

			consolidationAdvice.Transports = Transports.Create(context, TransportsInPortOrder);

			consolidationAdvice.BookingReference = shipment.JS_UniqueConsignRef;
			consolidationAdvice.MasterBillNumber = shipment.JS_HouseBill;
			consolidationAdvice.ContractNumber = shipment.GetCarrierContractNumber();

			PopulateSubShipment(consolidationAdvice);
			PopulateContainer(consolidationAdvice);
			PopulateBookingConfirmationNotes(consolidationAdvice);

			AddValidation(consolidationAdvice);
			consolidationAdvice.ValidateAllIncludingChildren();

			return consolidationAdvice;
		}

		#region Validation

		void AddValidation(ConsolidationAdvice consolidationAdvice)
		{
			consolidationAdvice.Origin.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("2ef0601f-dfeb-4abe-8554-05d3955a0a8c", "Origin is required."));
			consolidationAdvice.EstimatedTimeDepartureInfo.AddMessageErrorIfEmpty(Res.GetString("b110f1cd-c9da-4ef6-817a-56b856c12792", "ETD is required."));
			consolidationAdvice.Destination.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("067DF560-4114-4608-83CB-B8C2C094E7A3", "Destination is required."));
			consolidationAdvice.EstimatedTimeArrivalInfo.AddMessageErrorIfEmpty(Res.GetString("f77da9f8-6bda-4aa3-9c4c-25c543664375", "ETA is required."));
			consolidationAdvice.PortOfLoading.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("71e368a6-1c7d-4520-b105-34bcdf465532", "Port of Load is required at First SEA leg. (Routing tab)"));
			consolidationAdvice.PortOfDischarge.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("3d284dc7-db35-4560-9af8-59eee482976c", "Port of Discharge is required at Last SEA leg. (Routing tab)"));
			consolidationAdvice.VesselNameInfo.AddMessageErrorIfEmpty(Res.GetString("16b5253b-39c4-43d1-bb9a-2eac52acd416", "Vessel is required at First SEA leg. (Routing tab)"));
			consolidationAdvice.VoyageNumberInfo.AddMessageErrorIfEmpty(Res.GetString("52552ae9-5177-4c0d-934d-84897a8b9843", "Voyage is required at First SEA leg. (Routing tab)"));

			consolidationAdvice.BookingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("335af34c-998f-438c-845d-8499714d090a", "Shipment# is required."));

			AddAddressValidation(consolidationAdvice);
			AddTransportDetailsValidation(consolidationAdvice);
			AddSubShipmentsValidation(consolidationAdvice);
		}

		void AddAddressValidation(ConsolidationAdvice consolidationAdvice)
		{
			consolidationAdvice.BookingParty.CompanyNameInfo.AddMessageError(() =>
				(string.IsNullOrWhiteSpace(consolidationAdvice.BookingParty.CompanyName) ||
				 string.IsNullOrWhiteSpace(consolidationAdvice.BookingParty.AddressLine1) ||
				 string.IsNullOrWhiteSpace(consolidationAdvice.BookingParty.Country?.Name)),
				Res.GetString("faa497d0-7661-4680-84d3-d81eb4ad39da", "Booking Party is required. (Addresses > Booking Party Documentary Address)"));
		}

		void AddTransportDetailsValidation(ConsolidationAdvice consolidationAdvice)
		{
			foreach (var transport in consolidationAdvice.Transports.Collection)
			{
				transport.LegOrderInfo.AddMessageErrorIfEmpty(Res.GetString("dbffcdd4-d6ab-4850-9136-c0e099462a61", "Leg Order is required. (Routing tab)"));
				((CodeDescription)transport.Mode).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("cf8fd409-57e7-43a2-ac12-779bfa3ff27d", "Transport Mode is required. (Routing tab)"));
				((CodeDescription)transport.Type).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("5122f4d5-26f5-4f3a-9b9c-58b120dd917e", "Transport Type is required. (Routing tab)"));
				transport.PortOfLoading.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("2d9eb981-497f-4696-a50c-f4f78c71a601", "Load Port is required. (Routing tab)"));
				transport.ETDInfo.AddMessageErrorIfEmpty(Res.GetString("04a88210-62b1-4ce7-903d-c191344bb549", "ETD is required. (Routing tab)"));
				transport.PortOfDischarge.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("e1ce067e-808d-4965-9bbf-f4fc55deba9d", "Discharge Port is required. (Routing tab)"));
				transport.ETAInfo.AddMessageErrorIfEmpty(Res.GetString("1166bf0f-84a8-4521-b4fc-c7b2e8db8a72", "ETA is required. (Routing tab)"));

				if (transport.Mode.Code == Core.Constants.TransportModes.Sea)
				{
					transport.Vessel.NameInfo.AddMessageErrorIfEmpty(Res.GetString("1272394e-ccf3-48d8-ac40-6213037490af", "Vessel is required. (Routing tab)"));
					transport.VoyageFlightNumberInfo.AddMessageErrorIfEmpty(Res.GetString("4a901bb8-3ad2-4993-8135-d60964d0e703", "Voyage/Flight is required. (Routing tab)"));
				}
			}
		}

		void AddSubShipmentsValidation(ConsolidationAdvice consolidationAdvice)
		{
			foreach (var shipment in consolidationAdvice.SubShipments)
			{
				shipment.ShippersRefInfo.AddMessageErrorIfEmpty(Res.GetString("4d83bd2c-4d95-4d73-86c6-37e5243c00b8", "Shipper's Reference is required on related shipment. (Pickup > Shipper's Reference)"));
				shipment.BookingParty.CompanyNameInfo.AddMessageError(() =>
					(string.IsNullOrWhiteSpace(consolidationAdvice.BookingParty.CompanyName) ||
					 string.IsNullOrWhiteSpace(consolidationAdvice.BookingParty.AddressLine1) ||
					 string.IsNullOrWhiteSpace(consolidationAdvice.BookingParty.Country?.Name)),
					Res.GetString("27a1a6f5-4b37-4e9d-abde-0ed37c53a9bf", "Booking Party is required on related shipment. (Addresses > Booking Party Documentary Address)"));
				shipment.BookingParty.CompanyNameInfo.AddMessageError(() => consolidationAdvice.BookingParty.CompanyName != shipment.BookingParty.CompanyName,
					Res.GetString("fae92e81-bf7a-4ddd-af71-bc7650332d89", "Booking Party on related shipment needs to be the same party as the booking party specified at the Co-Load Master Shipment."));
				shipment.MessageRefInfo.AddMessageError(() => string.IsNullOrWhiteSpace(shipment.MessageRef) || !shipment.MessageRef.StartsWith(DocDataConstants.AdditionalReferences.Codes.ShipperReference),
					Res.GetString("71588208-75db-4a4e-a9d0-be6e4da2ab26", "Message Reference is required and must start with 'SHP' on related shipment. (Reference Numbers - Type 'HIR')"));
				shipment.ShipmentNumberInfo.AddMessageErrorIfEmpty(Res.GetString("8fee47f2-6101-4526-bab8-9539f372034d", "Shipment# is required on related shipment."));
				shipment.Origin.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("7731d361-0348-4557-9d59-16b00dd85057", "Origin is required on related shipment."));
				shipment.Destination.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("e8d18570-b059-4f09-aa05-ece42181d0fb", "Destination is required on related shipment."));
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		void PopulateBookingConfirmationNotes(ConsolidationAdvice consolidationAdvice)
		{
			const string bookingConfirmationNotesDescription = "Booking Confirmation Notes";
			var notes = shipment.Notes.FindByDescription(bookingConfirmationNotesDescription);

			var result = new StringBuilder();
			foreach (var note in notes)
			{
				result.AppendLine(note.ST_NoteDataAsText);
			}
			consolidationAdvice.BookingConfirmationNotes = result.ToString();
			consolidationAdvice.BookingConfirmationNotesInfo.AddAsciiCharactersValidation();
		}

		void PopulateSubShipment(ConsolidationAdvice consolidationAdvice)
		{
			if (shipment.IsStandardHouse)
			{
				var subShipments = new List<SubShipment>() { BuildSubShipment(shipment) };
				consolidationAdvice.SubShipments = subShipments;
			}
			else
			{
				consolidationAdvice.SubShipments = shipment.CoLoadShipments.Cast<ForwardingShipment>().Select(x => BuildSubShipment(x)).ToArray();
			}
		}

		#region Implementation

		SubShipment BuildSubShipment(ForwardingShipment shipment)
		{
			var subShipment = new SubShipment(shipment.PK);

			subShipment.ShipmentNumber = shipment.JS_UniqueConsignRef;
			subShipment.ShippersRef = shipment.JS_BookingReference;
			subShipment.MessageRef = shipment.Numbers.Cast<CusEntryNumber>()
				.Where(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.eHubInterchangeReference.HIR)
				.Select(c => c.CE_EntryNum)
				.FirstOrDefault();

			subShipment.BookingParty = AddressBuilder.Create(context, shipment.BookingPartyDocumentaryAddress);

			subShipment.Origin = Unloco.Create(context, shipment.Origin);
			subShipment.Destination = Unloco.Create(context, shipment.Destination);

			return subShipment;
		}

		void PopulateContainer(ConsolidationAdvice consolidationAdvice)
		{
			var containerDoBuilder = new ContainerBuilder();
			var containerDos = new List<Container>();

			var allShipmentNumbers = consolidationAdvice.SubShipments.Select(shipment => shipment.ShipmentNumber);

			var containers = shipment.AllMasterConsols.SelectMany(consol => consol.Containers).OfType<CommonContainer>();

			foreach (var container in containers)
			{
				var packLines = container.PackLines.OfType<PackLine>().Where(packLine => allShipmentNumbers.Contains(packLine.Shipment?.JS_UniqueConsignRef ?? ZString.Empty)).ToArray();

				if (packLines.Length > 0)
				{
					var containerDO = containerDoBuilder.Build(container, context, packLines);
					containerDos.Add(containerDO);
				}
			}
			consolidationAdvice.Containers = containerDos;
			PopulateTotalProperties(consolidationAdvice);
		}

		void PopulateTotalProperties(ConsolidationAdvice consolidationAdvice)
		{
			consolidationAdvice.TotalQuantity = 0;
			consolidationAdvice.TotalWeight = 0;
			consolidationAdvice.TotalVolume = 0;

			foreach (var container in consolidationAdvice.Containers)
			{
				if (container.PackingLines != null)
				{
					var packingLines = container.PackingLines.OfType<PackingLine>();
					consolidationAdvice.TotalQuantity += packingLines.Sum(packingLine => packingLine?.Quantity ?? 0);
					consolidationAdvice.TotalWeight += packingLines.Sum(packingLine => packingLine?.Weight.Value ?? 0M);
					consolidationAdvice.TotalVolume += packingLines.Sum(packingLine => packingLine?.Volume.Value ?? 0M);
				}
			}
		}

		IReadOnlyCollection<Freight.Business.Transport> TransportsInPortOrder
		{
			get
			{
				if (transportsInPortOrder == null)
				{
					var sortedTransports = shipment
						.TransportsIncludingRelated
						.OfType<Freight.Business.Transport>()
						.ToArray();
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
					transportsInPortOrder = sortedTransports;
				}

				return transportsInPortOrder;
			}
		}
		IReadOnlyCollection<Freight.Business.Transport> transportsInPortOrder;

		Freight.Business.Transport FirstSeaLeg => firstSeaLeg ?? (firstSeaLeg = TransportsInPortOrder.FirstOrDefault(t => t.JW_TransportMode == TransportModes.Sea));
		Freight.Business.Transport firstSeaLeg;

		Freight.Business.Transport LastSeaLeg => lastSeaLeg ?? (lastSeaLeg = TransportsInPortOrder.LastOrDefault(t => t.JW_TransportMode == TransportModes.Sea));
		Freight.Business.Transport lastSeaLeg;

		#endregion
	}
}
