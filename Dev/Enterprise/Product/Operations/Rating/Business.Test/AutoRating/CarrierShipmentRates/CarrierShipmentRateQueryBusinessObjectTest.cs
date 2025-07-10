using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test.AutoRating.CarrierShipmentRates;

[TestedType(typeof(CarrierShipmentRateQueryBusinessObject))]
public class CarrierShipmentRateQueryBusinessObjectTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		return new CarrierShipmentRateQueryBusinessObject(new CarrierShipmentRateQueryDto(), null, null);
	}

	public void TestOrigin_ReturnsOriginFromConstructor()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto();

		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals("origin", bizObj.Origin);
	}

	public void TestDestination_ReturnsDestinationFromConstructor()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto();

		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals("destination", bizObj.Destination);
	}

	public void TestFreightMode_ReturnsFCLWhenAContainerizedCargoIsAvailable()
	{
		var cargoList = new CarrierShipmentRateCargoDto[]
		{
			new(), new() { ContainerType = "20GP" }
		};

		var rateQueryDto = new CarrierShipmentRateQueryDto { Cargo = cargoList };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(FreightMode.FCL, bizObj.FreightMode);
	}

	public void TestFreightMode_ReturnsFCLWhenNoContainerizedCargoIsAvailable()
	{
		var cargoList = new CarrierShipmentRateCargoDto[]
		{
			new(), new()
		};

		var rateQueryDto = new CarrierShipmentRateQueryDto { Cargo = cargoList };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(FreightMode.FCL, bizObj.FreightMode);
	}

	public void TestCarrier_ReturnsCarrierFromParentDto()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto() { Shipment = new() { Carrier = "testCarrier" } };

		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals("testCarrier", bizObj.Carrier);
	}

	public void TestReadyDate_ReturnsReadyDateFromParentDto()
	{
		var date = new DateTime(2024, 01, 01);
		var rateQueryDto =
			new CarrierShipmentRateQueryDto() { Shipment = new() { ReadyDate = date } };

		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(date, bizObj.ReadyDate);
	}

	public void TestConsignee_ReturnsFirstPartyFromParentPartiesWithRoleCNE()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto();
		SetupParties(rateQueryDto);
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals("consignee1", bizObj.Consignee);
	}

	public void TestConsignee_ReturnsNullIfNoPartyIsAvailableWithCodeCNE()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto { Shipment = new CarrierShipmentRateShipmentDto() };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(null, bizObj.Consignee);
	}

	public void TestConsignor_ReturnsFirstPartyFromParentPartiesWithRoleCNR()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto();
		SetupParties(rateQueryDto);
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals("consignor1", bizObj.Consignor);
	}

	public void TestConsignor_ReturnsNullIfNoPartyIsAvailableWithCodeCNR()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto { Shipment = new CarrierShipmentRateShipmentDto() };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(null, bizObj.Consignor);
	}

	public void TestBookingParty_ReturnsFirstPartyFromParentPartiesWithRoleBKG()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto();
		SetupParties(rateQueryDto);
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals("bookingParty1", bizObj.BookingParty);
	}

	public void TestBookingParty_ReturnsNullIfNoPartyIsAvailableWithCodeBKG()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto { Shipment = new CarrierShipmentRateShipmentDto() };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(null, bizObj.BookingParty);
	}

	public void TestRateType_ReturnsShippingTypes()
	{
		var bizObj = new CarrierShipmentRateQueryBusinessObject(null, "origin", "destination");

		AssertEquals(RateType.Shipping | RateType.ShippingImportDetention | RateType.ShippingExportDetention,
			bizObj.RateType);
	}

	public void TestCargo_ReturnsCargoListFromParentDto()
	{
		var cargoList = new CarrierShipmentRateCargoDto[]
		{
			new(), new()
		};

		var rateQueryDto = new CarrierShipmentRateQueryDto { Cargo = cargoList };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(cargoList, bizObj.Cargo);
	}

	public void TestHasMeasures_ReturnsTrueIfCargoIsAvailable()
	{
		var cargoList = new CarrierShipmentRateCargoDto[]
		{
			new(), new()
		};

		var rateQueryDto = new CarrierShipmentRateQueryDto { Cargo = cargoList };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(true, bizObj.HasMeasures);
	}

	public void TestHasMeasures_ReturnsFalseIfCargoListIsEmpty()
	{
		var cargoList = Array.Empty<CarrierShipmentRateCargoDto>();

		var rateQueryDto = new CarrierShipmentRateQueryDto { Cargo = cargoList };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "origin", "destination");

		AssertEquals(false, bizObj.HasMeasures);
	}

	void SetupParties(CarrierShipmentRateQueryDto rateQueryDto)
	{
		rateQueryDto.Shipment = new()
		{
			RateParties = new CarrierShipmentRateRatePartyDto[]
			{
				new() { Code = "consignor1", Role = DocAddressTypes.Codes.ConsignorDocumentaryAddress }, new() { Code = "consignor2", Role = DocAddressTypes.Codes.ConsignorDocumentaryAddress },
				new() { Code = "consignee1", Role = DocAddressTypes.Codes.ConsigneeDocumentaryAddress }, new() { Code = "consignee2", Role = DocAddressTypes.Codes.ConsigneeDocumentaryAddress },
				new() { Code = "bookingParty1", Role = DocAddressTypes.Codes.BookingPartyDocumentaryAddress }, new() { Code = "bookingParty2", Role = DocAddressTypes.Codes.BookingPartyDocumentaryAddress },
			}
		};
	}
}
