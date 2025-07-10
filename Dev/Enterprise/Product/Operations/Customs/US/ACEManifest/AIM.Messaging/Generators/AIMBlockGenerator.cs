using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;

namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public abstract class AIMBlockGenerator
	{
		public AWBMessageBlock[] Generate()
		{
			messageBlocks = new List<AWBMessageBlock>();
			PopulateMessageBlocks();
			return messageBlocks.ToArray();
		}

		#region Implementation

		#region Populating Methods

		protected void AddBlock(AWBMessageBlock block)
		{
			messageBlocks.Add(block);
		}

		protected virtual void PopulateMessageBlocks()
		{
		}

		protected void PopulateStandardMessageIdentifier(IAIMMessageHeader header)
		{
			AddBlock(new AIMStandardMessageIdentifier
			{
				ComponentIdentifier = header.MessageType
			});
		}

		protected void PopulateCargoControlLocation(IAIMCargoControlLocation cargoControlLine)
		{
			AddBlock(new AIMCargoControlLocation
			{
				AirportOfArrival = cargoControlLine.AirportOfArrival,
				CargoTerminalOperator = cargoControlLine.CargoTerminalOperator
			});
		}

		protected void PopulateAirWayBill(IAIMAirWaybill airWaybill)
		{
			AddBlock(new AIMAirWaybill
			{
				AirWaybillPrefix = airWaybill.AirWaybillPrefix,
				AWBSerialNumber = airWaybill.AWBSerialNumber,
				HAWBNumber = airWaybill.IsMasterAirWaybill ? (ZString)AIMInboundMessage.MasterAirWaybillIndicator : airWaybill.HAWBNumber,
				PackageTrackingIdentifier = airWaybill.PackageTrackingIdentifier
			});
		}

		protected void PopulateWayBillDetails(IAIMWaybill waybill)
		{
			AddBlock(new AIMWaybill
			{
				AirportOfOrigin = waybill.AirportOfOrigin,
				CargoDescription = waybill.CargoDescription,
				DateOfArrivalAtThePermitToProceedDestinationAirport = waybill.DateOfArrivalAtThePermitToProceedDestinationAirport,
				NumberOfPieces = waybill.NumberOfPieces,
				PermitToProceedDestinationAirport = waybill.PermitToProceedDestinationAirport,
				Weight = waybill.Weight,
				WeightCode = waybill.WeightCode
			});
		}

		protected void PopulateArrivalDetails(IAIMArrival arrival)
		{
			AddBlock(new AIMArrival
			{
				BoardedPieceCount = arrival.BoardedPieceCount,
				BoardedQuantityIdentifier = arrival.IsBoardedQuantity ? (ZString)"B" : ZString.Empty,
				FlightNumber = arrival.FlightNumber,
				PartArrivalReference = arrival.PartArrivalReference,
				ScheduledArrivalDate = arrival.ScheduledArrivalDate,
				Weight = arrival.Weight,
				WeightCode = arrival.WeightCode
			});
		}

		protected void PopulateAgent(IAIMAgent agent)
		{
			if (agent != null && !agent.AirAMSParticipantCode.IsEmpty)
			{
				AddBlock(new AIMAgent
				{
					AirAMSParticipantCode = agent.AirAMSParticipantCode
				});
			}
		}

		protected void PopulateShipper(IAIMParty shipper)
		{
			if (shipper != null)
			{
				AddBlock(new AIMShipper
				{
					CityCountyTownship = shipper.CityCountyTownship,
					CountryCode = shipper.CountryCode,
					Name = shipper.Name,
					PostalCode = shipper.PostalCode,
					StateOrProvince = shipper.StateOrProvince,
					StreetAddress = shipper.StreetAddress,
					TelephoneNumber = shipper.TelephoneNumber
				});
			}
		}

		protected void PopulateConsignee(IAIMParty consignee)
		{
			if (consignee != null)
			{
				AddBlock(new AIMConsignee
				{
					CityCountyTownship = consignee.CityCountyTownship,
					CountryCode = consignee.CountryCode,
					Name = consignee.Name,
					PostalCode = consignee.PostalCode,
					StateOrProvince = consignee.StateOrProvince,
					StreetAddress = consignee.StreetAddress,
					TelephoneNumber = consignee.TelephoneNumber,
				});
			}
		}

		protected void PopulateTransferDetails(IAIMTransfer transfer)
		{
			if (transfer != null)
			{
				AddBlock(new AIMTransfer
				{
					BondedCarrierIDOrOnwardCarrier = transfer.BondedCarrierIDOrOnwardCarrier,
					BondedPremisesIdentifierOrInbondControlNumber = transfer.BondedPremisesIdentifierOrInbondControlNumber,
					DestinationAirport = transfer.DestinationAirport,
					DomesticInternationalIdentifier = transfer.DomesticInternationalIdentifier
				});
			}
		}

		protected void PopulateCBPShipmentDescription(IAIMCBPShipmentDescription cpbShipmentDescription)
		{
			if (cpbShipmentDescription != null)
			{
				AddBlock(new AIMCBPShipmentDescription
				{
					DeclaredValue = cpbShipmentDescription.DeclaredValue,
					HarmonizedCommodityCode = cpbShipmentDescription.HarmonizedCommodityCode,
					ISOCurrencyCode = cpbShipmentDescription.ISOCurrencyCode,
					OriginOfGoods = cpbShipmentDescription.OriginOfGoods
				});
			}
		}

		protected void PopulateFDAFreightIndicator(IAIMFDAFreightIndicator fdaFreightIndicator)
		{
			if (fdaFreightIndicator?.IsFDAFreight ?? ZBool.False)
			{
				AddBlock(new AIMFDAFreightIndicator());
			}
		}

		protected void PopulateCBPEntryDetails(IAIMCBPEntryDetail cbpEntryDetails)
		{
			if (cbpEntryDetails != null)
			{
				AddBlock(new AIMCBPEntryDetail
				{
					EntryType = cbpEntryDetails.EntryType,
					EntryNumber = cbpEntryDetails.EntryNumber
				});
			}
		}

		protected void PopulateCBPEntryDetailsForCancellation()
		{
			AddBlock(new AIMCBPEntryDetailForCancellation());
		}

		protected void PopulateReasonForAmendment(IAIMReasonForAmendment reasonForAmendment)
		{
			AddBlock(new AIMReasonForAmendment
			{
				AmendmentCode = reasonForAmendment.AmendmentCode,
				AmendmentExplanation = reasonForAmendment.AmendmentExplanation
			});
		}

		protected void PopulateAirlineStatusNotification(IAIMAirlineStatusNotification airlineStatusNotification)
		{
			AddBlock(new AIMAirlineStatusNotification
			{
				StatusCode = airlineStatusNotification.StatusCode,
				ActionExplanation = airlineStatusNotification.ActionExplanation
			});
		}

		protected void PopulateDepartureDetails(IAIMDeparture departureDetails)
		{
			AddBlock(new AIMDeparture
			{
				FlightNumber = departureDetails.FlightNumber,
				DateOfScheduledArrival = departureDetails.DateOfScheduledArrival,
				LiftoffDate = departureDetails.LiftoffDate,
				LiftoffTime = departureDetails.LiftoffTime,
				ActualImportingCarrier = departureDetails.ActualImportingCarrier,
				ActualFlightNumber = departureDetails.ActualFlightNumber
			});
		}

		protected void PopulateFreightStatusQuery(IFreightStatusQuery freightStatusQuery)
		{
			AddBlock(new AIMFreightStatusQuery
			{
				StatusRequestCode = freightStatusQuery.StatusRequestCode
			});
		}

		#endregion

		protected List<AWBMessageBlock> messageBlocks;

		#endregion
	}
}
