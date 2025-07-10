using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.CarrierShipmentRates;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business.Test.AutoRating.CarrierShipmentRates;

public class CarrierShipmentMeasuresAdapterTest : RatingTestCase
{
	#region Costs

	public void TestGivenShipmentWithNoCargo_WhenCallingConvertForCosts_ThenReturnsEmptyMeasureSet()
	{
		TestNoCargo(CostSell.Cost);
	}

	public void TestGivenShipmentWithContainer_WhenCallingConvertForCosts_ThenReturnsMeasureSetWithRealContainer()
	{
		TestContainer(CostSell.Cost);
	}

	public void TestGivenShipmentWithBreakBulkCargo_WhenCallingConvertForCosts_ThenReturnsMeasureSetWithLCLContainer()
	{
		TestBreakBulk(CostSell.Cost);
	}

	public void TestGivenShipmentWithRoRoCargo_WhenCallingConvertForCosts_ThenReturnsMeasureSetWithLCLContainer()
	{
		TestRoRo(CostSell.Cost);
	}

	public void TestGivenShipmentWithMultipleCargoes_WhenCallingConvertForCosts_ThenReturnsMeasureSetWithAllCargoes()
	{
		var containerDto1 = CreateContainerDto("20GP", 2, 30m, 20m, "GLUE", "CNT1", "OWN1");
		var containerDto2 = CreateContainerDto("20GP", 4, 30m, 20m, "FURN", "CNT2", "OWN2");
		var breakBulkDto1 = CreateBreakBulkDto(6, "BAG", 30m, 20m, "GLUE");
		var breakBulkDto2 = CreateBreakBulkDto(8, "BOT", 30m, 20m, "BEER");
		var roRoDto1 = CreateRoRoDto(10, 30m, 20m);
		var roRoDto2 = CreateRoRoDto(12, 30m, 20m);

		var rateQueryDto = new CarrierShipmentRateQueryDto()
		{
			Cargo = new[] { containerDto1, containerDto2, breakBulkDto1, breakBulkDto2, roRoDto1, roRoDto2 }
		};

		var rateQuery = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "DEHAM");
		var measuresAdapter = new CarrierShipmentMeasuresAdapter(rateQuery, CostSell.Cost, Factory);

		var measureSet = measuresAdapter.Convert(AdapterType.Shipment);

		AssertEquals(6, measureSet.GetPartCount(MeasureType.Weight));
		AssertEquals(6, measureSet.GetPartCount(MeasureType.Volume));
		AssertEquals(6, measureSet.GetPartCount(MeasureType.Package));
		AssertEquals(6, measureSet.GetPartCount(MeasureType.LoadingMeters));
		AssertEquals(6, measureSet.GetPartCount(MeasureType.Unit));

		// RoRos go together as they have the same commodity (null)
		AssertEquals(5, measureSet.GetContainerGroups().Count());

		AssertMeasureSet(measureSet, containerDto1, 0);
		AssertContainerGroup(measureSet, containerDto1, 0);

		AssertMeasureSet(measureSet, containerDto2, 1);
		AssertContainerGroup(measureSet, containerDto2, 1);

		AssertMeasureSet(measureSet, breakBulkDto1, 2);
		AssertContainerGroup(measureSet, breakBulkDto1, 2);

		AssertMeasureSet(measureSet, breakBulkDto2, 3);
		AssertContainerGroup(measureSet, breakBulkDto2, 3);

		AssertMeasureSet(measureSet, roRoDto1, 4);
		AssertMeasureSet(measureSet, roRoDto2, 5);

		var containerGroup = measureSet.GetContainerGroups().ElementAt(4);
		AssertEquals("", containerGroup.Ownership);
		AssertEquals(null, containerGroup.ContainerNumber);
		AssertEquals(MeasureInfo.ContainerInfo.LCL.ToGuid(), containerGroup.ContainerTypePK);
		AssertEquals("", containerGroup.CommodityCode);
		AssertEquals(2, containerGroup.ContainerCount);
	}

	#endregion

	#region Revenues

	public void TestGivenShipmentWithNoCargo_WhenCallingConvertForRevenues_ThenReturnsEmptyMeasureSet()
	{
		TestNoCargo(CostSell.Revenue);
	}

	public void TestGivenShipmentWithContainer_WhenCallingConvertForRevenues_ThenReturnsMeasureSetWithContainerGroup()
	{
		TestContainer(CostSell.Revenue);
	}

	public void TestGivenShipmentWithBreakBulkCargo_WhenCallingConvertForRevenues_ThenReturnsMeasureSetWithPartLists()
	{
		TestBreakBulk(CostSell.Revenue);
	}

	public void TestGivenShipmentWithRoRoCargoAndPackageTypeIsNull_WhenCallingConvertForRevenues_ThenReturnsMeasureSetWithPartLists()
	{
		TestRoRo(CostSell.Revenue);
	}

	public void TestGivenShipmentWithRoRoCargoAndPackageTypeIsEmptyString_WhenCallingConvertForRevenues_ThenReturnsMeasureSetWithPartLists()
	{
		TestRoRo(CostSell.Revenue, "");
	}

	public void
		TestGivenShipmentWithMultipleCargoes_WhenCallingConvertForRevenues_ThenReturnsMeasureSetWithOnlyChargeableCargoes()
	{
		var containerDto1 = CreateContainerDto("20GP", 2, 30m, 20m, "GLUE", "CNT1", "OWN1");
		var containerDto2 = CreateContainerDto("20GP", 4, 30m, 20m, "FURN", "CNT2", "OWN2", false);
		var breakBulkDto1 = CreateBreakBulkDto(6, "BAG", 30m, 20m, "GLUE");
		var breakBulkDto2 = CreateBreakBulkDto(8, "BOT", 30m, 20m, "BEER", false);
		var roRoDto1 = CreateRoRoDto(10, 30m, 20m);
		var roRoDto2 = CreateRoRoDto(12, 30m, 20m, false);

		var rateQueryDto = new CarrierShipmentRateQueryDto()
		{
			Cargo = new[] { containerDto1, containerDto2, breakBulkDto1, breakBulkDto2, roRoDto1, roRoDto2 }
		};

		var rateQuery = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "DEHAM");
		var measuresAdapter = new CarrierShipmentMeasuresAdapter(rateQuery, CostSell.Revenue, Factory);

		var measureSet = measuresAdapter.Convert(AdapterType.Shipment);

		AssertEquals(3, measureSet.GetPartCount(MeasureType.Weight));
		AssertEquals(3, measureSet.GetPartCount(MeasureType.Volume));
		AssertEquals(3, measureSet.GetPartCount(MeasureType.Package));
		AssertEquals(3, measureSet.GetPartCount(MeasureType.LoadingMeters));
		AssertEquals(3, measureSet.GetPartCount(MeasureType.Unit));

		AssertEquals(3, measureSet.GetContainerGroups().Count());

		AssertMeasureSet(measureSet, containerDto1, 0);
		AssertContainerGroup(measureSet, containerDto1, 0);

		AssertMeasureSet(measureSet, breakBulkDto1, 1);
		AssertContainerGroup(measureSet, breakBulkDto1, 1);

		AssertMeasureSet(measureSet, roRoDto1, 2);
		AssertContainerGroup(measureSet, roRoDto1, 2);
	}

	#endregion

	public void TestGivenMeasuresNotInKGAndM3_WhenCallingConvert_ThenMeasuresAreConverted()
	{
		var containerDto = CreateContainerDto("20GP", 2, 30m, 20m, "GLUE");
		containerDto.UnitOfWeight = "T";
		containerDto.UnitOfVolume = "L";
		var rateQueryDto = new CarrierShipmentRateQueryDto() { Cargo = new[] { containerDto } };

		var rateQuery = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "DEHAM");
		var measuresAdapter = new CarrierShipmentMeasuresAdapter(rateQuery, CostSell.Cost, Factory);

		var measureSet = measuresAdapter.Convert(AdapterType.Shipment);

		var rateableContainer = (IRateableContainer)measureSet.GetPartList(MeasureType.Weight)[0];
		AssertEquals(60000m, rateableContainer.WeightMeasure.Actual);
		AssertEquals(0.04m, rateableContainer.VolumeMeasure.Actual);
	}

	void TestNoCargo(CostSell costOrSell)
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto();
		var rateQuery = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "DEHAM");
		var measuresAdapter = new CarrierShipmentMeasuresAdapter(rateQuery, costOrSell, Factory);

		var measureSet = measuresAdapter.Convert(AdapterType.Shipment);

		AssertEquals(0, measureSet.MeasureTypeCount);
		AssertEquals(0, measureSet.GetContainerGroups().Count());
	}

	void TestContainer(CostSell costOrSell)
	{
		var containerDto = CreateContainerDto("20GP", 2, 30m, 20m, "GLUE", "CNT1", "OWN1");
		var rateQueryDto = new CarrierShipmentRateQueryDto() { Cargo = new[] { containerDto } };

		var rateQuery = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "DEHAM");
		var measuresAdapter = new CarrierShipmentMeasuresAdapter(rateQuery, costOrSell, Factory);

		var measureSet = measuresAdapter.Convert(AdapterType.Shipment);

		AssertEquals(1, measureSet.GetPartCount(MeasureType.Weight));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Volume));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Package));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.LoadingMeters));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Unit));
		AssertEquals(1, measureSet.GetContainerGroups().Count());

		AssertMeasureSet(measureSet, containerDto);
		AssertContainerGroup(measureSet, containerDto);
	}

	void TestBreakBulk(CostSell costOrSell)
	{
		var breakBulkDto = CreateBreakBulkDto(2, "BAG", 30m, 20m, "GLUE");
		var rateQueryDto = new CarrierShipmentRateQueryDto() { Cargo = new[] { breakBulkDto } };

		var rateQuery = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "DEHAM");
		var measuresAdapter = new CarrierShipmentMeasuresAdapter(rateQuery, costOrSell, Factory);

		var measureSet = measuresAdapter.Convert(AdapterType.Shipment);

		AssertEquals(1, measureSet.GetPartCount(MeasureType.Weight));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Volume));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Package));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.LoadingMeters));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Unit));
		AssertEquals(1, measureSet.GetContainerGroups().Count());

		AssertMeasureSet(measureSet, breakBulkDto, shouldHaveOwnershipFlag: false);
		AssertContainerGroup(measureSet, breakBulkDto);
	}

	void TestRoRo(CostSell costOrSell, string packageType = null)
	{
		var roRoDto = CreateRoRoDto(2, 30m, 20m);
		roRoDto.PackageType = packageType;
		var rateQueryDto = new CarrierShipmentRateQueryDto() { Cargo = new[] { roRoDto } };

		var rateQuery = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "DEHAM");
		var measuresAdapter = new CarrierShipmentMeasuresAdapter(rateQuery, costOrSell, Factory);

		var measureSet = measuresAdapter.Convert(AdapterType.Shipment);

		AssertEquals(1, measureSet.GetPartCount(MeasureType.Weight));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Volume));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Package));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.LoadingMeters));
		AssertEquals(1, measureSet.GetPartCount(MeasureType.Unit));
		AssertEquals(1, measureSet.GetContainerGroups().Count());

		AssertMeasureSet(measureSet, roRoDto, shouldHaveOwnershipFlag: false);
		AssertContainerGroup(measureSet, roRoDto);
	}

	void AssertMeasureSet(RateableMeasureSet measureSet, CarrierShipmentRateCargoDto cargoDto, int index = 0, bool shouldHaveOwnershipFlag = true)
	{
		var expectedContainerTypePk = string.IsNullOrEmpty(cargoDto.ContainerType)
			? null
			: NullableHelper.ToNullable(Helper.Containers[cargoDto.ContainerType].PK);

		var rateablePart = measureSet.GetPartList(MeasureType.Weight)[index];
		AssertEquals(cargoDto.Commodity, rateablePart.CommodityCode);
		AssertEquals(cargoDto.GrossWeight * (cargoDto.PieceCount ?? 1), rateablePart.WeightMeasure.Actual);
		AssertEquals(cargoDto.Volume * (cargoDto.PieceCount ?? 1), rateablePart.VolumeMeasure.Actual);
		AssertEquals(cargoDto.ContainerNumber, rateablePart.ContainerNumber);
		AssertEquals(!string.IsNullOrEmpty(cargoDto.PackageType) ? cargoDto.PackageType : cargoDto.CargoType, rateablePart.PackageType);
		AssertEquals(expectedContainerTypePk, rateablePart.ContainerTypePk);
		AssertEquals((decimal)cargoDto.PieceCount, rateablePart.PackageCount);
		AssertEquals((decimal)cargoDto.PieceCount, rateablePart.UnitCount);

		if (!string.IsNullOrEmpty(cargoDto.ContainerType))
		{
			var rateableContainer = rateablePart as IRateableContainer;
			AssertEquals(cargoDto.GrossWeight * (cargoDto.PieceCount ?? 1), rateableContainer.ContainerWeightInKG);
			AssertEquals(cargoDto.Volume * (cargoDto.PieceCount ?? 1), rateableContainer.ContainerVolumeInM3);
			AssertEquals(cargoDto.PieceCount, rateableContainer.ContainerCount);
			AssertEquals(0, rateableContainer.ContainerPackages);
			AssertEquals(Helper.Containers[cargoDto.ContainerType].RC_TEU, rateableContainer.TEU);
		}

		AssertEquals(cargoDto.ContainerOwnership, rateablePart.ContainerOwnership);
		AssertEquals(shouldHaveOwnershipFlag, measureSet.ContainerListHasContainerOwnership);
		AssertEquals(rateablePart, measureSet.GetPartList(MeasureType.Volume)[index]);
		AssertEquals(rateablePart, measureSet.GetPartList(MeasureType.Package)[index]);
		AssertEquals(rateablePart, measureSet.GetPartList(MeasureType.LoadingMeters)[index]);
		AssertEquals(rateablePart, measureSet.GetPartList(MeasureType.Unit)[index]);
	}

	void AssertContainerGroup(RateableMeasureSet measureSet, CarrierShipmentRateCargoDto cargoDto, int index = 0)
	{
		var expectedContainerTypePk = string.IsNullOrEmpty(cargoDto.ContainerType)
			? MeasureInfo.ContainerInfo.LCL.ToGuid()
			: Helper.Containers[cargoDto.ContainerType].PK;

		var containerGroup = measureSet.GetContainerGroups().ElementAt(index);
		AssertEquals(cargoDto.ContainerOwnership ?? "", containerGroup.Ownership);
		AssertEquals(cargoDto.ContainerNumber, containerGroup.ContainerNumber);
		AssertEquals(expectedContainerTypePk, containerGroup.ContainerTypePK);
		AssertEquals(cargoDto.Commodity ?? "", containerGroup.CommodityCode);
		AssertEquals(cargoDto.ContainerType != null ? cargoDto.PieceCount : 1, containerGroup.ContainerCount);
	}

	CarrierShipmentRateCargoDto CreateContainerDto(
		string containerType,
		int pieceCount = 1,
		decimal? weight = null,
		decimal? volume = null,
		string commodity = null,
		string containerNumber = null,
		string ownership = null,
		bool isChargeable = true)
	{
		return new CarrierShipmentRateCargoDto()
		{
			ContainerType = containerType,
			PieceCount = pieceCount,
			GrossWeight = weight,
			Volume = volume,
			Commodity = commodity,
			ContainerNumber = containerNumber,
			ContainerOwnership = ownership,
			IsChargeable = isChargeable
		};
	}

	CarrierShipmentRateCargoDto CreateBreakBulkDto(
		int pieceCount = 1,
		string packageType = "PKG",
		decimal? weight = null,
		decimal? volume = null,
		string commodity = null,
		bool isChargeable = true)
	{
		return new CarrierShipmentRateCargoDto()
		{
			PieceCount = pieceCount,
			PackageType = packageType,
			GrossWeight = weight,
			Volume = volume,
			Commodity = commodity,
			IsChargeable = isChargeable
		};
	}

	CarrierShipmentRateCargoDto CreateRoRoDto(
		int pieceCount = 1,
		decimal? weight = null,
		decimal? volume = null,
		bool isChargeable = true)
	{
		return new CarrierShipmentRateCargoDto()
		{
			PieceCount = pieceCount,
			GrossWeight = weight,
			Volume = volume,
			CargoType = "ROR",
			IsChargeable = isChargeable
		};
	}
}
