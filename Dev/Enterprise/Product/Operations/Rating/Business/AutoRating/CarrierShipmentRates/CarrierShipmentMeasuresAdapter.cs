using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.AutoRating.CarrierShipmentRates;

sealed class CarrierShipmentMeasuresAdapter(CarrierShipmentRateQueryBusinessObject rateQuery, CostSell costOrSell, BusinessObjectFactory factory)
{
	public RateableMeasureSet Convert(AdapterType adapterType)
	{
		var measureSet = new RateableMeasureSet(adapterType);

		if (rateQuery == null || rateQuery.Cargo == null || !rateQuery.Cargo.Any())
		{
			return measureSet;
		}

		var cargoDtos = rateQuery.Cargo.ToList();
		if (costOrSell == CostSell.Revenue)
		{
			cargoDtos = cargoDtos.Where(c => c.IsChargeable).ToList();
		}

		AddPackages(measureSet, cargoDtos);

		return measureSet;
	}

	void AddPackages(RateableMeasureSet measureSet, List<CarrierShipmentRateCargoDto> cargoDtos)
	{
		var packages = new RateablePartList
		{
			HasContainerType = true,
			HasCommodity = true,
			HasPackageType = true,
			WeightUnit = Constants.Weight.Kilograms,
			VolumeUnit = Constants.Volume.CubicMetres
		};

		foreach (var cargoDto in cargoDtos)
		{
			var refContainer = GetRefContainer(cargoDto.ContainerType);
			var weight = Constants.Weight.Convert(cargoDto.GrossWeight ?? 0, cargoDto.UnitOfWeight ?? Constants.Weight.Kilograms, Constants.Weight.Kilograms) * (cargoDto.PieceCount ?? 1);
			var volume = Constants.Volume.Convert(cargoDto.Volume ?? 0, cargoDto.UnitOfVolume ?? Constants.Volume.CubicMetres, Constants.Volume.CubicMetres) * (cargoDto.PieceCount ?? 1);

			if (refContainer != null)
			{
				AddContainer(measureSet, packages, cargoDto, weight, volume, refContainer);
			}
			else
			{
				AddNonContainerized(measureSet, packages, cargoDto, weight, volume);
			}
		}

		measureSet.AddPartList(MeasureType.Weight, packages);
		measureSet.AddPartList(MeasureType.Volume, packages);
		measureSet.AddPartList(MeasureType.Package, packages);
		measureSet.AddPartList(MeasureType.LoadingMeters, packages);
		measureSet.AddPartList(MeasureType.Unit, packages);
	}

	void AddNonContainerized(RateableMeasureSet measureSet, RateablePartList packages,
		CarrierShipmentRateCargoDto cargoDto, decimal weight, decimal volume)
	{
		packages.AddPart(new RateablePart
		{
			CommodityCode = cargoDto.Commodity,
			Weight = weight,
			Volume = volume,
			PackageCount = cargoDto.PieceCount,
			UnitCount = cargoDto.PieceCount,
			PackageType = !string.IsNullOrEmpty(cargoDto.PackageType) ? cargoDto.PackageType : cargoDto.CargoType,
			ContainerTypePk = null,
		});

		measureSet.AddLCL(
			cargoDto.Commodity,
			new ZDecimal(weight),
			Constants.Weight.Kilograms,
			new ZDecimal(volume),
			Constants.Volume.CubicMetres,
			cargoDto.PieceCount ?? 1);
	}

	void AddContainer(RateableMeasureSet measureSet, RateablePartList packages,
		CarrierShipmentRateCargoDto cargoDto, decimal weight, decimal volume, RefContainer refContainer)
	{
		packages.HasContainerOwnership = true;
		packages.AddPart(new RateableContainer()
		{
			CommodityCode = cargoDto.Commodity,
			Weight = weight,
			Volume = volume,
			PackageCount = cargoDto.PieceCount,
			ContainerWeightInKG = weight,
			ContainerVolumeInM3 = volume,
			UnitCount = cargoDto.PieceCount,
			ContainerCount = cargoDto.PieceCount ?? 1,
			ContainerPackages = 0,
			ContainerNumber = cargoDto.ContainerNumber,
			ContainerOwnership = cargoDto.ContainerOwnership,
			PackageType = null,
			ContainerTypePk = NullableHelper.ToNullable(refContainer.PK),
			TEU = refContainer?.RC_TEU ?? 0
		});

		var containerInfos = new[]
		{
			new MeasureInfo.ContainerInfo(
				weight,
				Constants.Weight.Kilograms,
				volume,
				Constants.Volume.CubicMetres,
				teu: refContainer.RC_TEU,
				containerNumber: cargoDto.ContainerNumber,
				containerCount: cargoDto.PieceCount ?? 1,
				container: refContainer)
		};

		measureSet.AddContainerGroup(
			refContainer.PK,
			cargoDto.Commodity,
			cargoDto.ContainerOwnership,
			cargoDto.ContainerNumber,
			new ZDecimal(weight),
			new ZDecimal(volume),
			containerInfos);
	}

	RefContainer GetRefContainer(string containerType) => string.IsNullOrEmpty(containerType)
		? null
		: factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
}
