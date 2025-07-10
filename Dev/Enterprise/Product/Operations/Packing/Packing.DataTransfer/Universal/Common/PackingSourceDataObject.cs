using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PackingSourceDataObject : IPackageParentDataObject
	{
		public PackingSourceDataObject(DataObjectList<Container> containerCollection, DataObjectList<PackingLine> packingLineCollection, IPackageParentDataObject dataObjectWithPacking)
		{
			ContainerCollection = containerCollection;
			PackingLineCollection = packingLineCollection;

			IPackageParentDataObject packageParentDataObject = this;
			packageParentDataObject.GoodsDescription = dataObjectWithPacking.GoodsDescription;
			packageParentDataObject.OuterPacks = dataObjectWithPacking.OuterPacks;
			packageParentDataObject.OuterPacksPackageType = dataObjectWithPacking.OuterPacksPackageType;
			packageParentDataObject.TotalNoOfPacks = dataObjectWithPacking.TotalNoOfPacks;
			packageParentDataObject.TotalNoOfPieces = dataObjectWithPacking.TotalNoOfPieces;
			packageParentDataObject.TotalNoOfPacksPackageType = dataObjectWithPacking.TotalNoOfPacksPackageType;
			packageParentDataObject.TotalVolume = dataObjectWithPacking.TotalVolume;
			packageParentDataObject.TotalVolumeUnit = dataObjectWithPacking.TotalVolumeUnit;
			packageParentDataObject.TotalWeight = dataObjectWithPacking.TotalWeight;
			packageParentDataObject.TotalWeightUnit = dataObjectWithPacking.TotalWeightUnit;
		}

		readonly DataObjectList<Container> ContainerCollection;
		readonly DataObjectList<PackingLine> PackingLineCollection;

		DataObjectList<Container> IPackageParentDataObject.ContainerCollection
		{
			get { return ContainerCollection; }
		}

		DataObjectList<PackingLine> IPackageParentDataObject.PackingLineCollection
		{
			get { return PackingLineCollection; }
		}

		ZString? IPackageParentDataObject.GoodsDescription { get; set; }

		ZInt? IPackageParentDataObject.OuterPacks { get; set; }
		PackageType IPackageParentDataObject.OuterPacksPackageType { get; set; }

		ZInt? IPackageParentDataObject.TotalNoOfPacks { get; set; }
		ZInt? IPackageParentDataObject.TotalNoOfPieces { get; set; }
		PackageType IPackageParentDataObject.TotalNoOfPacksPackageType { get; set; }

		ZDecimal? IPackageParentDataObject.TotalVolume { get; set; }
		UnitOfVolume IPackageParentDataObject.TotalVolumeUnit { get; set; }

		ZDecimal? IPackageParentDataObject.TotalWeight { get; set; }
		UnitOfWeight IPackageParentDataObject.TotalWeightUnit { get; set; }
	}
}
