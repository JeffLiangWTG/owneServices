using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.OceanCarrier.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Test.AutoRating.CarrierShipmentRates;

public class RateQueryDtoConverterTest : RatingTestCase
{
	public void TestConverter_ReturnsErrorIfShipmentIsNotFound()
	{
		var shipmentId = Guid.NewGuid();
		var converter = new RateQueryDtoConverter();

		var result = converter.Convert(GetParameterObject(shipmentId, []), Factory);

		AssertEquals(null, result.rateQueryDto);
		AssertEquals($"Carrier Shipment with ID: '{shipmentId}' was not found.", result.errors.ElementAt(0));
	}

	public void TestConverter_CreatesShipmentDtoFromId()
	{
		var date = new DateTimeOffset(new DateTime(2025,03,05));
		var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
		shipment.CSH_ReceiptTerm = "LI";
		shipment.CSH_ReceiptType = "CY";
		shipment.CSH_DeliveryTerm = "LO";
		shipment.CSH_DeliveryType = "CY";
		shipment.CSH_ReadyDate = date;
		Factory.Save();

		var converter = new RateQueryDtoConverter();
		var result = converter.Convert(GetParameterObject(shipment.PK.ToGuid(), []), Factory);

		AssertEquals("LI", result.rateQueryDto.Shipment.ReceiptTerm);
		AssertEquals("CY", result.rateQueryDto.Shipment.ReceiptType);
		AssertEquals("LO", result.rateQueryDto.Shipment.DeliveryTerm);
		AssertEquals("CY", result.rateQueryDto.Shipment.DeliveryType);
		AssertEquals(date.Date, result.rateQueryDto.Shipment.ReadyDate);
	}

	public void TestConverter_CreatesRatePartyDtosFromShipment()
	{
		var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
		shipment.CSH_ReadyDate = DateTimeOffset.Now;
		var party1OrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		party1OrgHeader.OH_Code = "TESTORG";
		var party1OrgAddress = party1OrgHeader.Addresses.AddNew();
		party1OrgAddress.Address1 = "TESTADDRESS";
		var party1JobDocAddress = shipment.DocAddresses.AddNew();
		party1JobDocAddress.E2_OA_Address = party1OrgAddress.PK;
		party1JobDocAddress.E2_AddressType = "CNE";

		var party2JobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
		Factory.Save();

		var converter = new RateQueryDtoConverter();
		var result = converter.Convert(GetParameterObject(shipment.PK.ToGuid(), []), Factory);

		AssertEquals(1, result.rateQueryDto.Shipment.RateParties.Count());
		AssertEquals("TESTORG", result.rateQueryDto.Shipment.RateParties.ElementAt(0).Code);
		AssertEquals("CNE", result.rateQueryDto.Shipment.RateParties.ElementAt(0).Role);
	}

	public void TestConverter_CreatesCargoDtosFromShipment()
	{
		var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
		shipment.CSH_ReadyDate = DateTimeOffset.Now;
		var cargo1 = shipment.Cargoes.AddNew();
		var cargo2 = shipment.Cargoes.AddNew();
		SetupCargoes(cargo1, cargo2);

		Factory.Save();

		var converter = new RateQueryDtoConverter();
		var result = converter.Convert(GetParameterObject(shipment.PK.ToGuid(), []), Factory);

		AssertEquals(2, result.rateQueryDto.Cargo.Count());
		AssertCargoes(result.rateQueryDto.Cargo.ToArray());
	}

	public void TestConverter_ReturnsErrorIfRouteSegmentIsNotFound()
	{
		var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
		shipment.CSH_ReadyDate = DateTimeOffset.Now;
		Factory.Save();

		var segmentId = Guid.NewGuid();
		var converter = new RateQueryDtoConverter();
		var result = converter.Convert(GetParameterObject(shipment.PK.ToGuid(), [segmentId]), Factory);

		AssertEquals($"Route segment with ID: '{segmentId}' was not found.", result.errors.ElementAt(0));
	}

	public void TestConverter_CreatesRouteLegDtosFromShipment()
	{
		var transportProvider = TransportProvider1.PK;
		var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
		shipment.CSH_ReadyDate = DateTimeOffset.Now;

		var routeSegment1 = Factory.NewWithValidTestData<RouteSegment>();
		var routeSegment2 = Factory.NewWithValidTestData<RouteSegment>();
		SetupRouteSegment(routeSegment1, 0, transportProvider);
		SetupRouteSegment(routeSegment2, 1, transportProvider);

		Factory.Save();

		var converter = new RateQueryDtoConverter();
		var result = converter.Convert(GetParameterObject(shipment.PK.ToGuid(), [routeSegment1.PK.ToGuid(), routeSegment2.PK.ToGuid()]), Factory);

		AssertEquals(2, result.rateQueryDto.RouteLegs.Count());
		AssertRoutes(result.rateQueryDto.RouteLegs.ToArray());
	}

	CalculateRatesQueryParameters GetParameterObject(Guid shipmentId, Guid[] segmentIds, bool getRatesForCosts = true, bool getRatesForSales = true)
	{
		return new CalculateRatesQueryParameters
		{
			ShipmentHeaderId = shipmentId,
			RouteLegIds = segmentIds,
			GetRatesForCosts = getRatesForCosts,
			GetRatesForSales = getRatesForSales
		};
	}

	void SetupRouteSegment(RouteSegment routeSegment, int i, ZGuid transportProvider)
	{
		var testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		testOrgHeader.OH_Code = $"TESTORG{i}";
		var fromOrgAddress = testOrgHeader.Addresses.AddNew();
		fromOrgAddress.Address1 = $"FROMADDRESS{i}";
		fromOrgAddress.OA_RL_NKRelatedPortCode = $"AUSY{i}";
		var toOrgAddress = testOrgHeader.Addresses.AddNew();
		toOrgAddress.Address1 = $"TOADDRESS{i}";
		toOrgAddress.OA_RL_NKRelatedPortCode = $"USLA{i}";
		routeSegment.RSG_OA_FromAddress = fromOrgAddress.PK;
		routeSegment.RSG_OA_ToAddress = toOrgAddress.PK;
		routeSegment.RSG_Distance = i + 1;
		routeSegment.RSG_DistanceUnit = "KM";
		var service = Factory.NewWithValidTestData<CarrierService>();
		service.CSV_Code = $"TSTSV{i}";
		routeSegment.RSG_CSV_Service = service.PK;
		routeSegment.RSG_OH_TransportOperator = transportProvider;
	}

	void AssertRoutes(CarrierShipmentRateRouteLegDto[] routelegs)
	{
		for (int i = 0; i < routelegs.Length; i++)
		{
			AssertNotNull(routelegs[i]);
			AssertEquals($"AUSY{i}", routelegs[i].FromAddress);
			AssertEquals($"USLA{i}", routelegs[i].ToAddress);
			AssertEquals(i + 1m, routelegs[i].Distance);
			AssertEquals("KM", routelegs[i].UnitOfDistance);
			AssertEquals($"TSTSV{i}", routelegs[i].Service);
			AssertEquals("TRASPROV1", routelegs[i].Supplier);
		}
	}

	void SetupCargoes(params CarrierShipmentCargo[] cargoes)
	{
		var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
		for (int i = 0; i < cargoes.Length; i++)
		{
			cargoes[i].CSC_IsChargeable = true;
			cargoes[i].CSC_PieceCount = i + 1;
			cargoes[i].CSC_RC_ChargeableEquipmentType = refContainer.PK;
			cargoes[i].CSC_EquipmentNo = $"EQU{i}";
			cargoes[i].CSC_Model = $"MOD{i}";
			cargoes[i].CSC_OH_Manufacturer = Consignor.PK;
			cargoes[i].CSC_RH_NKCommodityCode = "GLUE";
			cargoes[i].CSC_F3_NKPackType = "BAG";
			cargoes[i].CSC_ChargeableGrossWeight = i + 1;
			cargoes[i].CSC_UnitOfWeight = "KG";
			cargoes[i].CSC_ChargeableRevenueTons = i + 1;
			cargoes[i].CSC_ChargeableLength = i + 1;
			cargoes[i].CSC_ChargeableWidth = i + 1;
			cargoes[i].CSC_ChargeableHeight = i + 1;
			cargoes[i].CSC_ChargeableUnitOfDimension = "CM";
			cargoes[i].CSC_ChargeableArea = i + 1;
			cargoes[i].CSC_ChargeableUnitOfArea = "M2";
			cargoes[i].CSC_ChargeableVolume = i + 1;
			cargoes[i].CSC_ChargeableUnitOfVolume = "M3";
			cargoes[i].CSC_IsEmpty = true;
			cargoes[i].CSC_Propulsion = "HYB";
			// cargoes[i].CSC_ReeferSetPoint = i + 1;  // to be done with WI00896094 after deciding which reefer setting to use since we can have more than one
			// cargoes[i].CSC_ReeferTemperatureUnit = "C";
			cargoes[i].CSC_OOGLostSlotsChargeable = i + 1;
			cargoes[i].CSC_CargoType = "CNT";
			cargoes[i].CSC_CargoMovementTypeDestination = "FCL";
			cargoes[i].CSC_CargoMovementTypeOrigin = "FCL";
			cargoes[i].CSC_DeliveryDrayage = "DTB";
			cargoes[i].CSC_ReceiptDrayage = "DTB";
		}
	}

	void AssertCargoes(CarrierShipmentRateCargoDto[] cargoes)
	{
		for (int i = 0; i < cargoes.Length; i++)
		{
			AssertNotNull(cargoes[i]);
			AssertEquals(true, cargoes[i].IsChargeable);
			AssertEquals(i + 1, cargoes[i].PieceCount);
			AssertEquals("20GP", cargoes[i].ContainerType);
			AssertEquals($"EQU{i}", cargoes[i].ContainerNumber);
			AssertEquals($"MOD{i}", cargoes[i].CarModel);
			AssertEquals(Consignor.OH_Code, cargoes[i].CarManufacturer);
			AssertEquals("GLUE", cargoes[i].Commodity);
			AssertEquals("BAG", cargoes[i].PackageType);
			AssertEquals(i + 1m, cargoes[i].GrossWeight);
			AssertEquals("KG", cargoes[i].UnitOfWeight);
			AssertEquals(i + 1m, cargoes[i].RevenueTons);
			AssertEquals(i + 1m, cargoes[i].Length);
			AssertEquals(i + 1m, cargoes[i].Width);
			AssertEquals(i + 1m, cargoes[i].Height);
			AssertEquals("CM", cargoes[i].UnitOfDimension);
			AssertEquals(i + 1m, cargoes[i].Area);
			AssertEquals("M2", cargoes[i].UnitOfArea);
			AssertEquals(i + 1m, cargoes[i].Volume);
			AssertEquals("M3", cargoes[i].UnitOfVolume);
			AssertEquals(true, cargoes[i].IsEmptyContainer);
			AssertEquals("HYB", cargoes[i].Propulsion);
			// AssertEquals(i + 1m, cargoes[i].ReeferTemperature);  // to be done with WI00896094 after deciding which reefer setting to use since we can have more than one
			// AssertEquals("C", cargoes[i].UnitOfReeferTemperature);
			AssertEquals(i + 1m, cargoes[i].LostSlots);
			AssertEquals("CNT", cargoes[i].CargoType);
			AssertEquals("DTB", cargoes[i].DeliveryDrayage);
			AssertEquals("DTB", cargoes[i].ReceiptDrayage);
		}
	}
}
