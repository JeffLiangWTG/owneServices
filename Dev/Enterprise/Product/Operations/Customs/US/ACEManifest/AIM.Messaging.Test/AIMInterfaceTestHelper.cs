using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	static class AIMInterfaceTestHelper
	{
		public static Mock<IAIMCargoControlLocation> GetCargoControlLocation(ZString airport, ZString terminalOperator)
		{
			var cargoControlLocationMock = new Mock<IAIMCargoControlLocation>();
			cargoControlLocationMock.Setup(m => m.AirportOfArrival).Returns(airport);
			cargoControlLocationMock.Setup(m => m.CargoTerminalOperator).Returns(terminalOperator);

			return cargoControlLocationMock;
		}

		public static Mock<IAIMAirWaybill> GetAirWaybill(ZString prefix, ZString serialNumber, bool isMaster)
		{
			var airWaybillMock = new Mock<IAIMAirWaybill>();
			airWaybillMock.Setup(m => m.AirWaybillPrefix).Returns(prefix);
			airWaybillMock.Setup(m => m.AWBSerialNumber).Returns(serialNumber);
			airWaybillMock.Setup(m => m.IsMasterAirWaybill).Returns(isMaster);

			return airWaybillMock;
		}

		public static Mock<IAIMWaybill> GetWaybillDetails(ZString airport, ZDecimal numberOfPieces, ZString weightCode, ZDecimal weight, ZString cargoDescription, string permitToProceed, ZDate? dateOfArrival)
		{
			var airWaybilDetailsMock = new Mock<IAIMWaybill>();
			airWaybilDetailsMock.CallBase = true;
			airWaybilDetailsMock.Setup(m => m.AirportOfOrigin).Returns(airport);
			airWaybilDetailsMock.Setup(m => m.PermitToProceedDestinationAirport).Returns(permitToProceed);
			airWaybilDetailsMock.Setup(m => m.NumberOfPieces).Returns(numberOfPieces);
			airWaybilDetailsMock.Setup(m => m.WeightCode).Returns(weightCode);
			airWaybilDetailsMock.Setup(m => m.Weight).Returns(weight);
			airWaybilDetailsMock.Setup(m => m.CargoDescription).Returns(cargoDescription);
			airWaybilDetailsMock.Setup(m => m.DateOfArrivalAtThePermitToProceedDestinationAirport).Returns(dateOfArrival ?? ZDate.Empty);

			return airWaybilDetailsMock;
		}

		public static Mock<IAIMArrival> GetArrival(ZString flightNo, ZDate arrivalDate, string partReference, bool qtyIdentifier, decimal pieceCount, string weightCode, decimal weight)
		{
			var arrivalMock = new Mock<IAIMArrival>();
			arrivalMock.Setup(m => m.FlightNumber).Returns(flightNo);
			arrivalMock.Setup(m => m.ScheduledArrivalDate).Returns(arrivalDate);
			arrivalMock.Setup(m => m.PartArrivalReference).Returns(partReference);
			arrivalMock.Setup(m => m.IsBoardedQuantity).Returns(qtyIdentifier);
			arrivalMock.Setup(m => m.BoardedPieceCount).Returns(pieceCount);
			arrivalMock.Setup(m => m.WeightCode).Returns(weightCode);
			arrivalMock.Setup(m => m.Weight).Returns(weight);

			return arrivalMock;
		}

		public static Mock<IAIMCBPEntryDetail> GetCBPEntryDetail(ZString entryType, string entryNumber)
		{
			var cpbMock = new Mock<IAIMCBPEntryDetail>();
			cpbMock.Setup(m => m.EntryType).Returns(entryType);
			cpbMock.Setup(m => m.EntryNumber).Returns(entryNumber);

			return cpbMock;
		}

		public static Mock<IAIMAgent> GetAgent(ZString participantCode)
		{
			var agentMock = new Mock<IAIMAgent>();
			agentMock.Setup(m => m.AirAMSParticipantCode).Returns(participantCode);

			return agentMock;
		}

		public static Mock<IAIMParty> GetParty(ZString name, ZString city, ZString countryCode, string street, string state, string postalCode, string telNo)
		{
			var shipperMock = new Mock<IAIMParty>();
			shipperMock.Setup(m => m.Name).Returns(name);
			shipperMock.Setup(m => m.StreetAddress).Returns(street);
			shipperMock.Setup(m => m.CityCountyTownship).Returns(city);
			shipperMock.Setup(m => m.StateOrProvince).Returns(state);
			shipperMock.Setup(m => m.CountryCode).Returns(countryCode);
			shipperMock.Setup(m => m.PostalCode).Returns(postalCode);
			shipperMock.Setup(m => m.TelephoneNumber).Returns(telNo);
			return shipperMock;
		}

		public static Mock<IAIMTransfer> GetTransfer(string destinationAirport, string domIntIdentifier, string bondedCarrierIDOrOnwardCarrier, string premisesIdentifierOrControlNo)
		{
			var transferMock = new Mock<IAIMTransfer>();
			transferMock.Setup(m => m.DestinationAirport).Returns(destinationAirport);
			transferMock.Setup(m => m.DomesticInternationalIdentifier).Returns(domIntIdentifier);
			transferMock.Setup(m => m.BondedCarrierIDOrOnwardCarrier).Returns(bondedCarrierIDOrOnwardCarrier);
			transferMock.Setup(m => m.BondedPremisesIdentifierOrInbondControlNumber).Returns(premisesIdentifierOrControlNo);
			return transferMock;
		}

		public static Mock<IAIMCBPShipmentDescription> GetCBPShipmentDescription(ZDecimal declaredValue, ZString isoCurrencyCode, string originOfGoods, string commodityCode)
		{
			var shipmentDescriptionMock = new Mock<IAIMCBPShipmentDescription>();
			shipmentDescriptionMock.Setup(m => m.OriginOfGoods).Returns(originOfGoods);
			shipmentDescriptionMock.Setup(m => m.DeclaredValue).Returns(declaredValue);
			shipmentDescriptionMock.Setup(m => m.ISOCurrencyCode).Returns(isoCurrencyCode);
			shipmentDescriptionMock.Setup(m => m.HarmonizedCommodityCode).Returns(commodityCode);

			return shipmentDescriptionMock;
		}

		public static Mock<IAIMReasonForAmendment> GetReasonForAmendment(ZString amendmentCode, string amendmentExplanation)
		{
			var reasonForAmendmentMock = new Mock<IAIMReasonForAmendment>();
			reasonForAmendmentMock.Setup(m => m.AmendmentCode).Returns(amendmentCode);
			reasonForAmendmentMock.Setup(m => m.AmendmentExplanation).Returns(amendmentExplanation);

			return reasonForAmendmentMock;
		}

		public static Mock<IAIMFDAFreightIndicator> GetFDAFreightIndicator(ZBool isFDAFreight)
		{
			var fdaFreightIndicatorMock = new Mock<IAIMFDAFreightIndicator>();
			fdaFreightIndicatorMock.Setup(m => m.IsFDAFreight).Returns(isFDAFreight);

			return fdaFreightIndicatorMock;
		}

		public static Mock<IAIMDeparture> GetDeparture(ZString flightNo, ZDate dateOfArrival, ZDate liftOffDate, ZString liftOffTime, string actualImportingCarrier, string actualFlightNo)
		{
			var departureMock = new Mock<IAIMDeparture>();
			departureMock.Setup(m => m.FlightNumber).Returns(flightNo);
			departureMock.Setup(m => m.DateOfScheduledArrival).Returns(dateOfArrival);
			departureMock.Setup(m => m.LiftoffDate).Returns(liftOffDate);
			departureMock.Setup(m => m.LiftoffTime).Returns(liftOffTime);
			departureMock.Setup(m => m.ActualImportingCarrier).Returns(actualImportingCarrier);
			departureMock.Setup(m => m.ActualFlightNumber).Returns(actualFlightNo);

			return departureMock;
		}

		public static Mock<IAIMAirlineStatusNotification> GetAirlineStatusNotification(ZString statusCode, ZString actionExplanation)
		{
			var asnMock = new Mock<IAIMAirlineStatusNotification>();
			asnMock.Setup(m => m.StatusCode).Returns(statusCode);
			asnMock.Setup(m => m.ActionExplanation).Returns(actionExplanation);

			return asnMock;
		}

		public static Mock<IFreightStatusQuery> GetFreightStatusQuery(ZString statusRequestCode)
		{
			var asnMock = new Mock<IFreightStatusQuery>();
			asnMock.Setup(m => m.StatusRequestCode).Returns(statusRequestCode);

			return asnMock;
		}
	}
}
