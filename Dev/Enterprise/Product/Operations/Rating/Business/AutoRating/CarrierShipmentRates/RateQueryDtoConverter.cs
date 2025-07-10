using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.OceanCarrier.Business;

namespace Enterprise.Rating.Business;

sealed class RateQueryDtoConverter : IRateQueryDtoConverter
{
	public (CarrierShipmentRateQueryDto rateQueryDto, ICollection<string> errors) Convert(CalculateRatesQueryParameters parameters, IFactory factory)
	{
		ICollection<string> errors = new List<string>();

		var carrierShipmentHeader = factory.Load<CarrierShipmentHeader>(parameters.ShipmentHeaderId);
		if (carrierShipmentHeader == null)
		{
			errors.Add(Res.GetString("3285b4ec-d7ef-467a-a094-9607d77a9093", "Carrier Shipment with ID: '{0}' was not found.", parameters.ShipmentHeaderId));
			return (null, errors);
		}

		var rateQueryDto = new CarrierShipmentRateQueryDto
		{
			Shipment = CreateCarrierShipmentRateShipmentDto(carrierShipmentHeader),
			Cargo = CreateCarrierShipmentRateCargoDto(carrierShipmentHeader),
			RouteLegs = CreateCarrierShipmentRateRouteLegDtoFromRouteSegments(parameters.RouteLegIds, errors, factory)
		};

		rateQueryDto.GetRatesForCosts = parameters.GetRatesForCosts;
		rateQueryDto.GetRatesForSales = parameters.GetRatesForSales;

		return (rateQueryDto, errors);
	}

	List<CarrierShipmentRateRouteLegDto> CreateCarrierShipmentRateRouteLegDtoFromRouteSegments(ICollection<Guid> segmentIds, ICollection<string> errors, IFactory factory)
	{
		var routeLegs = new List<CarrierShipmentRateRouteLegDto>();
		foreach (var segmentId in segmentIds)
		{
			var routeSegment = factory.Load<RouteSegment>(segmentId);
			if (routeSegment == null)
			{
				errors.Add(Res.GetString("12e3df94-5c87-43a3-b1fb-639394c50bbf", "Route segment with ID: '{0}' was not found.", segmentId));
				continue;
			}

			var routeLeg = new CarrierShipmentRateRouteLegDto
			{
				Distance = routeSegment.RSG_Distance,
				FromAddress = routeSegment.FromAddress.OA_RL_NKRelatedPortCode,
				ToAddress = routeSegment.ToAddress.OA_RL_NKRelatedPortCode,
				Service = factory.Load<CarrierService>(routeSegment.RSG_CSV_Service)?.CSV_Code,
				Supplier = routeSegment.TransportOperator.OH_Code,
				UnitOfDistance = routeSegment.RSG_DistanceUnit,
			};

			routeLegs.Add(routeLeg);
		}
		return routeLegs;
	}

	List<CarrierShipmentRateCargoDto> CreateCarrierShipmentRateCargoDto(CarrierShipmentHeader carrierShipmentHeader)
	{
		var cargoList = new List<CarrierShipmentRateCargoDto>();
		foreach (var cargo in carrierShipmentHeader.Cargoes)
		{
			cargoList.Add(MapCargo(cargo));
		}

		return cargoList;
	}

	CarrierShipmentRateCargoDto MapCargo(CarrierShipmentCargo cargo)
	{
		return new CarrierShipmentRateCargoDto
		{
			Area = cargo.CSC_ChargeableArea,
			CarManufacturer = cargo.Manufacturer?.OH_Code,
			CarModel = cargo.CSC_Model,
			CargoType = cargo.CSC_CargoType,
			Commodity = cargo.CSC_RH_NKCommodityCode,
			ContainerNumber = cargo.CSC_EquipmentNo,
			ContainerType = cargo.ChargeableEquipmentType?.RC_Code,
			DeliveryDrayage = cargo.CSC_DeliveryDrayage,
			GrossWeight = cargo.CSC_ChargeableGrossWeight,
			Height = cargo.CSC_ChargeableHeight,
			IsChargeable = cargo.CSC_IsChargeable,
			IsEmptyContainer = cargo.CSC_IsEmpty,
			Length = cargo.CSC_ChargeableLength,
			LostSlots = cargo.CSC_OOGLostSlotsChargeable,
			PackageType = cargo.CSC_F3_NKPackType,
			PieceCount = cargo.CSC_PieceCount,
			Propulsion = cargo.CSC_Propulsion,
			ReceiptDrayage = cargo.CSC_ReceiptDrayage,
			// ReeferTemperature = cargo.CSC_ReeferSetPoint,  // to be done with WI00896094 after deciding which reefer setting to use since we can have more than one
			RevenueTons = cargo.CSC_ChargeableRevenueTons,
			UnitOfArea = cargo.CSC_ChargeableUnitOfArea,
			UnitOfDimension = cargo.CSC_ChargeableUnitOfDimension,
			// UnitOfReeferTemperature = cargo.CSC_ReeferTemperatureUnit,
			UnitOfVolume = cargo.CSC_ChargeableUnitOfVolume,
			UnitOfWeight = cargo.CSC_UnitOfWeight,
			Volume = cargo.CSC_ChargeableVolume,
			Width = cargo.CSC_ChargeableWidth,
			// TODO when OCS supports this field: ContainerOwnership
			// TODO when OCS supports this field: ImdgUNNumber
			// TODO when OCS supports this field: ImdgClass
		};
	}

	CarrierShipmentRateShipmentDto CreateCarrierShipmentRateShipmentDto(CarrierShipmentHeader carrierShipmentHeader)
	{
		return new CarrierShipmentRateShipmentDto
		{
			DeliveryTerm = carrierShipmentHeader.CSH_DeliveryTerm,
			DeliveryType = carrierShipmentHeader.CSH_DeliveryType,
			RateParties = GetParties(carrierShipmentHeader),
			ReadyDate = carrierShipmentHeader.CSH_ReadyDate.ToDateTime(),
			ReceiptType = carrierShipmentHeader.CSH_ReceiptType,
			ReceiptTerm = carrierShipmentHeader.CSH_ReceiptTerm,
			// TODO when OCS supports this field: ContractNumber
		};
	}

	List<CarrierShipmentRateRatePartyDto> GetParties(CarrierShipmentHeader carrierShipmentHeader)
	{
		var organizations = carrierShipmentHeader.DocAddresses.Where(address => address.Organisation != null).Select(address => new CarrierShipmentRateRatePartyDto()
		{
			Code = address.Organisation.OH_Code,
			Role = address.E2_AddressType
		});

		return organizations.ToList();
	}
}
