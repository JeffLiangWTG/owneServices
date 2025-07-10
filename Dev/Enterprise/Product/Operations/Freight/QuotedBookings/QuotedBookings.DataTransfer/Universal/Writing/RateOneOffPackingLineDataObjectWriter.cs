using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	internal class RateOneOffPackingLineDataObjectWriter : DataObjectWriter<RateOneOffPackLine, PackingLine>
	{
		internal RateOneOffPackingLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override PackingLine PopulateDataObject(RateOneOffPackLine rateOneOffPackLine)
		{
			var packingLineData = new PackingLine(writeManager.WriterStrategy);
			packingLineData.PackQty = rateOneOffPackLine.TPL_PackLineCount;
			packingLineData.PackType = ListHelper.GetWithDescription<PackageType>(rateOneOffPackLine.TPL_F3_NKPackType, rateOneOffPackLine.Lookups.RefPackTypes);

			packingLineData.Weight = rateOneOffPackLine.TPL_Weight;
			packingLineData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(rateOneOffPackLine.TPL_WeightUQ, rateOneOffPackLine.Lookups.WeightUnits);

			packingLineData.Volume = rateOneOffPackLine.TPL_Volume;
			packingLineData.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(rateOneOffPackLine.TPL_VolumeUQ, rateOneOffPackLine.Lookups.VolumeUnits);

			packingLineData.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(rateOneOffPackLine.TPL_DimensionUQ, rateOneOffPackLine.Lookups.DimensionUnits);
			packingLineData.Length = rateOneOffPackLine.TPL_Length;
			packingLineData.Width = rateOneOffPackLine.TPL_Width;
			packingLineData.Height = rateOneOffPackLine.TPL_Height;

			return packingLineData;
		}
	}
}
