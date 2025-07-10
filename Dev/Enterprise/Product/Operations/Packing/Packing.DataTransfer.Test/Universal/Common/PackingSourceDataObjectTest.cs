using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	class PackingSourceDataObjectTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var containers = new DataObjectList<Container>() { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CN1" } };
			var packingLines = new DataObjectList<PackingLine>() { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackType = new PackageType() { Code = "PLT" } } };
			var outerPackagePackeType = new PackageType() { Code = "CN1" };
			var totalNoOfPacksPackageType = new PackageType() { Code = "CN2" };
			var unitOfVolume = new UnitOfVolume() { Code = "M3" };
			var unitOfWeight = new UnitOfWeight() { Code = "KG" };
			shipmentDataObject.GoodsDescription = "Goods Description";
			shipmentDataObject.OuterPacks = 1;
			shipmentDataObject.OuterPacksPackageType = outerPackagePackeType;
			shipmentDataObject.TotalNoOfPacks = 100;
			shipmentDataObject.TotalNoOfPacksPackageType = totalNoOfPacksPackageType;
			shipmentDataObject.TotalVolume = 200;
			shipmentDataObject.TotalVolumeUnit = unitOfVolume;
			shipmentDataObject.TotalWeight = 100;
			shipmentDataObject.TotalWeightUnit = unitOfWeight;

			IPackageParentDataObject packageParentDO = new PackingSourceDataObject(containers, packingLines, shipmentDataObject);
			AssertContainsExactElementsInAnyOrder(packageParentDO.ContainerCollection, containers);
			AssertContainsExactElementsInAnyOrder(packageParentDO.PackingLineCollection, packingLines);
			AssertEquals("Goods Description", packageParentDO.GoodsDescription);
			AssertEquals(1, packageParentDO.OuterPacks);
			AssertEquals(outerPackagePackeType, packageParentDO.OuterPacksPackageType);
			AssertEquals(100, packageParentDO.TotalNoOfPacks);
			AssertEquals(totalNoOfPacksPackageType, packageParentDO.TotalNoOfPacksPackageType);
			AssertEquals(200m, packageParentDO.TotalVolume);
			AssertEquals(unitOfVolume, packageParentDO.TotalVolumeUnit);
			AssertEquals(100m, packageParentDO.TotalWeight);
			AssertEquals(unitOfWeight, packageParentDO.TotalWeightUnit);
		}
	}
}
