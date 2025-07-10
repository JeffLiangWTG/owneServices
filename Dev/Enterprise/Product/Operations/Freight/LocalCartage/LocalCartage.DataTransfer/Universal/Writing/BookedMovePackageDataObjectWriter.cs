using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	internal class BookedMovePackageDataObjectWriter : DataObjectWriter<CommonBookedCtgMove, PackingLine>
	{
		internal BookedMovePackageDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override PackingLine PopulateDataObject(CommonBookedCtgMove sourceBO)
		{
			var packlineDataObject = new PackingLine(writeManager.WriterStrategy);

			packlineDataObject.Length = sourceBO.EW_BookedLength;
			packlineDataObject.Width = sourceBO.EW_BookedWidth;
			packlineDataObject.Height = sourceBO.EW_BookedHeight;
			packlineDataObject.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(sourceBO.EW_DimUnit, sourceBO.BindToLists.DimensionUnits);

			packlineDataObject.PackQty = new ZLong(sourceBO.EW_BookedPackCount);
			packlineDataObject.PackType = ListHelper.GetWithDescription<PackageType>(sourceBO.EW_F3_NKPackType, sourceBO.BindToLists.OuterPackTypes);

			packlineDataObject.Volume = sourceBO.EW_BookedVolume;
			packlineDataObject.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(sourceBO.EW_VolumeUQ, sourceBO.BindToLists.VolumeUnits);

			packlineDataObject.Weight = sourceBO.EW_BookedWeight;
			packlineDataObject.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(sourceBO.EW_WeightUQ, sourceBO.BindToLists.WeightUnits);

			packlineDataObject.SetAdditionalServiceCollection(() => ProcessCollection(sourceBO.Services, new AdditionalServiceDataObjectWriter(writeManager)));

			return packlineDataObject;
		}
	}
}
