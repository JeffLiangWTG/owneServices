using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	internal class RateOneOffPackingLineDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestLooseCargoDataExporting()
		{
			AssertLooseCargoData(2, "PLT", "Pallet", 5, "KG", "Kilograms", 0.3m, "M3", "Cubic Meters", 30, 33, 46, "IN", "Inches");
			AssertLooseCargoData(3, "PKG", "Package", 50, "MG", "Milligrams", 0.5m, "CI", "Cubic Inches", 95, 12, 100, "M", "Meters");

			void AssertLooseCargoData(ZShort count, string packTypeCode, string packTypeDesc,
				decimal weight, string weightUnitCode, string weightUnitDesc,
				decimal volume, string volumeUnitCode, string volumeUnitDesc,
				decimal length, decimal width, decimal height, string unitOfDimCode, string unitOfDimDesc)
			{
				var looseCargo = Factory.New<RateOneOffPackLine>();
				looseCargo.TPL_PackLineCount = count;
				looseCargo.TPL_F3_NKPackType = packTypeCode;

				looseCargo.TPL_DimensionUQ = unitOfDimCode;
				looseCargo.TPL_Length = length;
				looseCargo.TPL_Width = width;
				looseCargo.TPL_Height = height;

				looseCargo.TPL_Weight = weight;
				looseCargo.TPL_WeightUQ = weightUnitCode;

				looseCargo.TPL_Volume = volume;
				looseCargo.TPL_VolumeUQ = volumeUnitCode;

				var looseCargoData = new RateOneOffPackingLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CLI, looseCargo))).GetDataObject(looseCargo);
				AssertNotNull("looseCargo", looseCargo);

				CombineAssertions(() =>
				{
					AssertEquals("PackQty", count, (ZShort)looseCargoData.PackQty);
					AssertEquals("PackType.Code", packTypeCode, looseCargoData.PackType.Code);
					AssertEquals("looseCargoData.PackType.Description", packTypeDesc, looseCargoData.PackType.Description);

					AssertEquals("Weight", weight, looseCargoData.Weight);
					AssertEquals("WeightUnit.Code", weightUnitCode, looseCargoData.WeightUnit.Code);
					AssertEquals("WeightUnit.Description", weightUnitDesc, looseCargoData.WeightUnit.Description);

					AssertEquals("Volume", volume, looseCargoData.Volume);
					AssertEquals("VolumeUnit.Code", volumeUnitCode, looseCargoData.VolumeUnit.Code);
					AssertEquals("VolumeUnit.Description", volumeUnitDesc, looseCargoData.VolumeUnit.Description);

					AssertEquals("LengthUnit.Code", unitOfDimCode, looseCargoData.LengthUnit.Code);
					AssertEquals("LengthUnit.Description", unitOfDimDesc, looseCargoData.LengthUnit.Description);
					AssertEquals("Length", length, looseCargoData.Length);
					AssertEquals("Width", width, looseCargoData.Width);
					AssertEquals("Height", height, looseCargoData.Height);
				});
			}
		}
	}
}
